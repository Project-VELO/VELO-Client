using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 0에서 1까지의 진행도를 시간에 맞춰 흘려 주는 헬퍼입니다.
///
/// 연출 종류마다 보간 대상이 다르지만(위치·배율·색) 시간을 재는 방식은 같아서 여기로 모았습니다.
/// 트윈 라이브러리를 두지 않은 프로젝트라, 이 한 곳만 UniTask로 돌리고 나머지는 값 계산만 합니다.
/// </summary>
public static class StoryEffectTween
{
    /// <summary>
    /// duration 동안 진행도를 0에서 1까지 올리며 apply를 호출합니다. 마지막에는 반드시 1로 한 번 더 부릅니다.
    /// 중간에 취소되면 그 자리에서 멈춥니다. 목표값으로 튀지 않는 것은,
    /// 취소의 원인이 대개 "다음 연출이 들어왔다"이고 그쪽이 곧 자기 값을 쓰기 때문입니다.
    ///
    /// 시간은 unscaledDeltaTime으로 잽니다. 연출이 Time.timeScale에 끌려가면
    /// 나중에 일시정지나 배속을 넣을 때 화면 연출까지 함께 느려집니다.
    /// </summary>
    public static async UniTask LerpAsync(float duration, Action<float> apply, CancellationToken cancellationToken)
    {
        if (duration <= 0f)
        {
            apply(1f);
            return;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            cancellationToken.ThrowIfCancellationRequested();

            elapsed += Time.unscaledDeltaTime;
            apply(Mathf.Clamp01(elapsed / duration));

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        apply(1f);
    }

    /// <summary>
    /// 취소될 때까지 매 프레임 apply를 호출합니다. 경과 시간을 함께 넘겨 떨림 위상을 만들 수 있게 합니다.
    /// 지속 흔들림처럼 끝이 정해지지 않은 연출에 씁니다.
    /// </summary>
    public static async UniTask RepeatAsync(Action<float> apply, CancellationToken cancellationToken)
    {
        float elapsed = 0f;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            elapsed += Time.unscaledDeltaTime;
            apply(elapsed);

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }
    }
}
