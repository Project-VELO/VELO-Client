using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 미리듣기 음량을 시작에서 올리고 끝에서 내립니다.
///
/// BgmManager에서 떼어 낸 것은 재생 목록을 다루는 일과 음량 곡선을 그리는 일이 성격이
/// 다르기 때문입니다. 곡선을 손보는 동안 재생 쪽을 건드리지 않게 됩니다.
/// </summary>
public class BgmPreviewFade
{
    private readonly AudioSource _source;

    /// <summary>
    /// 차오르고 잦아드는 데 각각 걸리는 시간입니다. 0이면 봉투 없이 그대로 웁니다.
    /// </summary>
    private readonly float _fadeSeconds;

    private CancellationTokenSource _cts;

    public BgmPreviewFade(AudioSource source, float fadeSeconds)
    {
        _source = source;
        _fadeSeconds = fadeSeconds;
    }

    /// <summary>
    /// 음량을 0에서 시작해 곡이 흐르는 동안 계속 다시 계산합니다.
    /// </summary>
    public void Begin(float targetVolume, CancellationToken cancellationToken)
    {
        Stop();

        _source.volume = 0f;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        RunAsync(targetVolume, _cts.Token).Forget();
    }

    /// <summary>
    /// 곡선을 끊습니다. 음량은 부르는 쪽이 다시 정합니다.
    /// </summary>
    public void Stop()
    {
        if (_cts == null)
        {
            return;
        }

        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }

    private async UniTaskVoid RunAsync(float targetVolume, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                if (_source == null || _source.clip == null)
                {
                    return;
                }

                _source.volume = targetVolume * GetEnvelope();
            }
        }
        catch (OperationCanceledException)
        {
            // 다른 곡을 고르거나 화면을 떠났습니다.
        }
    }

    /// <summary>
    /// 지금 지점의 음량 배수(0~1)입니다.
    ///
    /// 흐른 시간과 남은 시간을 함께 보는 이유는 반복 재생 때문입니다. 흐른 시간만 보면
    /// 두 바퀴째부터는 이미 다 차오른 상태라 잦아들 자리를 놓칩니다. AudioSource.time이
    /// 한 바퀴마다 0으로 돌아오므로 이 모양이 매 바퀴 그대로 반복됩니다.
    /// </summary>
    private float GetEnvelope()
    {
        if (_fadeSeconds <= 0f)
        {
            return 1f;
        }

        float played = _source.time;
        float remaining = _source.clip.length - played;

        return Mathf.Min(
            Mathf.Clamp01(played / _fadeSeconds),
            Mathf.Clamp01(remaining / _fadeSeconds));
    }
}
