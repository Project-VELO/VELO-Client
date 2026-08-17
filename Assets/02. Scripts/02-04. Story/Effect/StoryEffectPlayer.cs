using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 대사 한 줄에 딸린 화면 연출을 재생합니다(연출표의 [화면 이펙트] 열).
///
/// 연출은 두 성격으로 갈립니다. 암전·비네팅·줌처럼 "지시가 있을 때까지 그대로 있는" 것과,
/// 섬광·단발 흔들림처럼 "한 번 지나가는" 것입니다. 그래서 이 클래스가 눌러앉은 상태를 들고 있고,
/// 지나가는 연출은 그 상태 위에 얹었다가 되돌립니다.
///
/// 줄이 넘어갈 때 진행 중인 연출을 끊는 것은 StoryLinePlayer가 타이핑을 끊는 것과 같은 이유입니다.
/// 앞 줄의 흔들림이 다음 줄까지 물고 늘어지면 어느 지시가 화면을 만지는지 알 수 없게 됩니다.
/// </summary>
public class StoryEffectPlayer : IDisposable
{
    private readonly UI_StoryEffectLayer _layer;
    private readonly CancellationToken _sceneToken;

    private CancellationTokenSource _effectCts;

    /// <summary>
    /// 시점 이동으로 눌러앉은 오프셋입니다. 흔들림은 이 값을 기준으로 떨립니다.
    /// 흔들림이 끝나면 0이 아니라 여기로 돌아와야 시점 이동이 유지됩니다.
    /// </summary>
    private Vector2 _settledOffset;

    private float _settledScale = 1f;

    public StoryEffectPlayer(UI_StoryEffectLayer layer, CancellationToken sceneToken)
    {
        _layer = layer;
        _sceneToken = sceneToken;
    }

    /// <summary>
    /// 이 줄의 연출을 시작합니다. 값이 비어 있으면 앞 줄의 상태를 그대로 둡니다.
    /// 빈 칸을 "연출 해제"로 보지 않는 것은, 대본이 바뀌는 줄에만 이펙트를 적는 방식이기 때문입니다.
    /// </summary>
    public void Play(string effectId)
    {
        if (string.IsNullOrEmpty(effectId))
        {
            return;
        }

        if (!MasterDataProvider.Instance.TryGetEffect(effectId, out StoryEffectData effect))
        {
            Debug.LogWarning($"[StoryEffectPlayer] 이펙트 '{effectId}'를 effects.json에서 찾을 수 없습니다.");
            return;
        }

        Stop();

        if (effect.Kind == EStoryEffectKind.STOP)
        {
            _settledOffset = Vector2.zero;
            _settledScale = 1f;
            _layer.ResetStage();
            return;
        }

        _effectCts = CancellationTokenSource.CreateLinkedTokenSource(_sceneToken);
        RunAsync(effect, _effectCts.Token).Forget();
    }

    /// <summary>
    /// 진행 중인 연출만 끊습니다. 눌러앉은 상태는 그대로 둡니다.
    /// </summary>
    public void Stop()
    {
        if (_effectCts == null)
        {
            return;
        }

        _effectCts.Cancel();
        _effectCts.Dispose();
        _effectCts = null;
    }

    public void Dispose()
    {
        Stop();
    }

    private async UniTaskVoid RunAsync(StoryEffectData effect, CancellationToken cancellationToken)
    {
        try
        {
            switch (effect.Kind)
            {
                case EStoryEffectKind.SHAKE:
                    await ShakeAsync(effect, cancellationToken);
                    break;
                case EStoryEffectKind.ZOOM:
                    await ZoomAsync(effect, cancellationToken);
                    break;
                case EStoryEffectKind.PAN:
                    await PanAsync(effect, cancellationToken);
                    break;
                case EStoryEffectKind.TINT:
                    await TintAsync(effect, cancellationToken);
                    break;
                case EStoryEffectKind.FLASH:
                    await FlashAsync(effect, cancellationToken);
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            // 다음 줄이 들어왔거나 씬이 내려갔습니다. 상태는 다음 연출이나 씬 정리가 맡습니다.
        }
    }

    /// <summary>
    /// 지속 흔들림은 취소될 때까지 떨고, 단발은 진폭을 줄여 가며 제자리로 돌아옵니다.
    /// 단발이 갑자기 멈추면 화면이 튀어 보입니다.
    /// </summary>
    private async UniTask ShakeAsync(StoryEffectData effect, CancellationToken cancellationToken)
    {
        if (effect.IsLooping)
        {
            await StoryEffectTween.RepeatAsync(_ => ApplyShake(effect.Strength), cancellationToken);
            return;
        }

        await StoryEffectTween.LerpAsync(effect.DurationSeconds,
            progress => ApplyShake(effect.Strength * (1f - progress)), cancellationToken);

        _layer.SetStageOffset(_settledOffset);
    }

    private void ApplyShake(float amplitude)
    {
        var jitter = new Vector2(
            UnityEngine.Random.Range(-amplitude, amplitude),
            UnityEngine.Random.Range(-amplitude, amplitude));

        _layer.SetStageOffset(_settledOffset + jitter);
    }

    private async UniTask ZoomAsync(StoryEffectData effect, CancellationToken cancellationToken)
    {
        float from = _layer.GetStageScale();
        _settledScale = effect.Strength;

        await StoryEffectTween.LerpAsync(effect.DurationSeconds,
            progress => _layer.SetStageScale(Mathf.Lerp(from, effect.Strength, progress)), cancellationToken);
    }

    private async UniTask PanAsync(StoryEffectData effect, CancellationToken cancellationToken)
    {
        Vector2 from = _layer.GetStageOffset();
        var to = new Vector2(effect.Strength, effect.StrengthY);
        _settledOffset = to;

        await StoryEffectTween.LerpAsync(effect.DurationSeconds,
            progress => _layer.SetStageOffset(Vector2.Lerp(from, to, progress)), cancellationToken);
    }

    private async UniTask TintAsync(StoryEffectData effect, CancellationToken cancellationToken)
    {
        Color from = _layer.GetOverlayColor(effect.Target);
        Color to = effect.Color;
        to.a = effect.Strength;

        await StoryEffectTween.LerpAsync(effect.DurationSeconds,
            progress => _layer.SetOverlayColor(effect.Target, Color.Lerp(from, to, progress)), cancellationToken);
    }

    /// <summary>
    /// 절반 동안 올리고 절반 동안 내립니다. 시작 색으로 되돌리므로 암전 위에 섬광을 얹어도 암전이 유지됩니다.
    /// </summary>
    private async UniTask FlashAsync(StoryEffectData effect, CancellationToken cancellationToken)
    {
        Color from = _layer.GetOverlayColor(effect.Target);
        Color peak = effect.Color;
        peak.a = Mathf.Max(from.a, effect.Strength);

        float half = Mathf.Max(effect.DurationSeconds, 0.02f) * 0.5f;

        await StoryEffectTween.LerpAsync(half,
            progress => _layer.SetOverlayColor(effect.Target, Color.Lerp(from, peak, progress)), cancellationToken);
        await StoryEffectTween.LerpAsync(half,
            progress => _layer.SetOverlayColor(effect.Target, Color.Lerp(peak, from, progress)), cancellationToken);
    }
}
