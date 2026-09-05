using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 무대가 지금 어디에 어떤 배율로 서 있는지를 들고, 흔들림·줌·시점 이동을 겁니다.
///
/// 덮개(암전·필터)와 떼어 놓은 이유는 되돌아갈 자리를 아는 주체가 달라서입니다.
/// 흔들림은 "떨기 전 자리"로 돌아와야 하고 그 자리는 앞선 시점 이동이 정합니다.
/// 덮개에는 그런 기준점이 없어 목표 색만 있으면 됩니다.
/// </summary>
public class StoryStagePose
{
    private readonly UI_StoryEffectLayer _layer;

    /// <summary>
    /// 시점 이동으로 눌러앉은 오프셋입니다. 흔들림은 이 값을 기준으로 떨립니다.
    /// 흔들림이 끝나면 0이 아니라 여기로 돌아와야 시점 이동이 유지됩니다.
    /// </summary>
    private Vector2 _settledOffset;

    public StoryStagePose(UI_StoryEffectLayer layer)
    {
        _layer = layer;
    }

    /// <summary>
    /// 평소 자리와 평소 배율로 돌립니다.
    /// </summary>
    public void Reset()
    {
        _settledOffset = Vector2.zero;
        _layer.ResetStage();
    }

    /// <summary>
    /// 지속 흔들림은 취소될 때까지 떨고, 단발은 진폭을 줄여 가며 제자리로 돌아옵니다.
    /// 단발이 갑자기 멈추면 화면이 튀어 보입니다.
    /// </summary>
    public async UniTask ShakeAsync(StoryEffectData effect, CancellationToken cancellationToken)
    {
        // 떨 자리를 먼저 확보합니다. 진폭이 작아 확대도 얼마 되지 않습니다.
        EnsureSlackFor(new Vector2(effect.Strength, effect.Strength));

        if (effect.IsLooping)
        {
            await StoryEffectTween.RepeatAsync(_ => ApplyShake(effect.Strength), cancellationToken);
            return;
        }

        await StoryEffectTween.LerpAsync(effect.DurationSeconds,
            progress => ApplyShake(effect.Strength * (1f - progress)), cancellationToken);

        _layer.SetStageOffset(_settledOffset);
    }

    public async UniTask ZoomAsync(StoryEffectData effect, CancellationToken cancellationToken)
    {
        float from = _layer.GetStageScale();

        await StoryEffectTween.LerpAsync(effect.DurationSeconds,
            progress => _layer.SetStageScale(Mathf.Lerp(from, effect.Strength, progress)), cancellationToken);
    }

    /// <summary>
    /// 시점을 옮깁니다. 미는 동안 필요한 만큼 함께 확대해, 가장자리가 비지 않으면서도
    /// 움직이지 않는 줄에서는 그림이 잘리지 않게 합니다.
    ///
    /// 훑기로 지정된 이동은 반대편 끝에서 시작합니다. 시점이 늘 가운데에 있어,
    /// 그러지 않으면 한 줄로는 그림의 절반까지밖에 못 갑니다(StoryEffectData.IsSweep).
    /// </summary>
    public async UniTask PanAsync(StoryEffectData effect, CancellationToken cancellationToken)
    {
        var to = new Vector2(effect.Strength, effect.StrengthY);
        Vector2 from = effect.IsSweep ? -to : _layer.GetStageOffset();

        float fromScale = _layer.GetStageScale();
        _settledOffset = to;

        // 이미 더 확대되어 있으면 그대로 둡니다. 줌인 중에 시점을 옮기는 줄에서 화면이 뒤로 물러나면 안 됩니다.
        float toScale = Mathf.Max(fromScale, _layer.GetRequiredScale(to));

        // 훑기는 시작점도 화면 끝이라, 출발 전에 그만큼 확대해 두지 않으면 첫 프레임이 빕니다.
        if (effect.IsSweep)
        {
            fromScale = toScale;
            _layer.SetStageScale(toScale);
            _layer.SetStageOffset(from);
        }

        await StoryEffectTween.LerpAsync(effect.DurationSeconds, progress =>
        {
            _layer.SetStageScale(Mathf.Lerp(fromScale, toScale, progress));
            _layer.SetStageOffset(Vector2.Lerp(from, to, progress));
        }, cancellationToken);
    }

    /// <summary>
    /// 이만큼 밀 수 있도록 배율을 올려 둡니다. 이미 충분하면 아무것도 하지 않습니다.
    /// </summary>
    private void EnsureSlackFor(Vector2 offset)
    {
        float required = _layer.GetRequiredScale(_settledOffset + offset);

        if (_layer.GetStageScale() < required)
        {
            _layer.SetStageScale(required);
        }
    }

    private void ApplyShake(float amplitude)
    {
        var jitter = new Vector2(
            Random.Range(-amplitude, amplitude),
            Random.Range(-amplitude, amplitude));

        _layer.SetStageOffset(_settledOffset + jitter);
    }
}
