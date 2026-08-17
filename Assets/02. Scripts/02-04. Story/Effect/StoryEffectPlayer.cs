using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 대사 한 줄에 딸린 화면 연출을 재생합니다(연출표의 [화면 이펙트] 열).
///
/// 연출은 두 성격으로 갈립니다. 암전·비네팅·줌처럼 "지시가 있을 때까지 그대로 있는" 것과,
/// 섬광·단발 흔들림처럼 "한 번 지나가는" 것입니다. 그래서 지나가는 연출은
/// 눌러앉은 상태 위에 얹었다가 되돌립니다. 암전 위에 섬광을 쳐도 암전이 남습니다.
///
/// 줄이 넘어갈 때 진행 중인 연출을 끊는 것은 StoryLinePlayer가 타이핑을 끊는 것과 같은 이유입니다.
/// 앞 줄의 흔들림이 다음 줄까지 물고 늘어지면 어느 지시가 화면을 만지는지 알 수 없게 됩니다.
///
/// 무대의 자리와 배율은 StoryStagePose가 들고 있고, 여기서는 덮개와 수명만 다룹니다.
/// </summary>
public class StoryEffectPlayer : IDisposable
{
    private readonly UI_StoryEffectLayer _layer;
    private readonly StoryStagePose _pose;
    private readonly CancellationToken _sceneToken;

    private CancellationTokenSource _effectCts;

    public StoryEffectPlayer(UI_StoryEffectLayer layer, CancellationToken sceneToken)
    {
        _layer = layer;
        _pose = new StoryStagePose(layer);
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
            _pose.Reset();
            return;
        }

        _effectCts = CancellationTokenSource.CreateLinkedTokenSource(_sceneToken);
        RunAsync(effect, _effectCts.Token).Forget();
    }

    /// <summary>
    /// 화면을 아무 연출도 걸리지 않은 상태로 되돌립니다. 장면이 바뀔 때 부릅니다.
    ///
    /// 이 지시가 필요한 이유는, 연출표가 암전을 푸는 방법을 "배경 노출"로만 적기 때문입니다.
    /// "화면 암전" 다음 줄이 "배경 서서히 노출"인데, 후자는 이펙트가 아니라 배경 교체로 표현돼 있습니다.
    /// 그래서 덮개를 걷는 지시가 데이터에 없고, 걷지 않으면 회차 내내 캄캄한 화면이 남습니다.
    ///
    /// 새 장면이 앞 장면의 줌과 시점까지 물려받을 이유도 없어 무대도 함께 제자리로 돌립니다.
    /// </summary>
    public void ResetForNewScene()
    {
        Stop();

        _pose.Reset();
        _layer.SetOverlayColor(EStoryEffectTarget.OVERLAY, Color.clear);
        _layer.SetOverlayColor(EStoryEffectTarget.VIGNETTE, Color.clear);
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
                    await _pose.ShakeAsync(effect, cancellationToken);
                    break;
                case EStoryEffectKind.ZOOM:
                    await _pose.ZoomAsync(effect, cancellationToken);
                    break;
                case EStoryEffectKind.PAN:
                    await _pose.PanAsync(effect, cancellationToken);
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
