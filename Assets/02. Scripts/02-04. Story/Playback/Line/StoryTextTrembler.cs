using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 대사 글자를 제자리에서 떨게 합니다(연출표의 [글자 떨림]).
///
/// 글자마다 정점을 흔들지 않고 텍스트 오브젝트를 통째로 흔듭니다. 타이핑이 maxVisibleCharacters로
/// 한 글자씩 메시를 다시 만들기 때문에, 정점을 직접 건드리면 다음 글자가 찍힐 때마다 흔들림이
/// 원점으로 되돌아가 떨림이 끊깁니다.
///
/// 흔들림은 난수가 아니라 펄린 노이즈로 만듭니다. 매 프레임 난수를 뽑으면 값이 튀어 지지직거리는데,
/// 노이즈는 이웃한 시간끼리 값이 이어져 손이 떨리는 것처럼 보입니다.
/// </summary>
public class StoryTextTrembler
{
    /// <summary>
    /// 가로와 세로가 같은 값으로 흔들리지 않도록 노이즈를 뽑는 자리를 떼어 놓습니다.
    /// 같은 자리에서 뽑으면 대각선으로만 왕복합니다.
    /// </summary>
    private const float VERTICAL_NOISE_OFFSET = 37.4f;

    private CancellationTokenSource _cancellation;

    private RectTransform _target;

    /// <summary>
    /// 떨기 전의 자리입니다. 멈출 때 여기로 되돌립니다.
    /// </summary>
    private Vector2 _origin;

    /// <summary>
    /// 떨림을 시작합니다. 이미 떨고 있으면 앞의 것을 멈추고 그 자리를 되돌린 뒤 새로 시작합니다.
    /// 되돌리지 않으면 흔들린 지점이 다음 줄의 기준 자리가 되어 대사 상자가 조금씩 밀립니다.
    /// </summary>
    public void Play(RectTransform target, float amplitude, float frequency, CancellationToken cancellationToken)
    {
        Stop();

        if (target == null || amplitude <= 0f)
        {
            return;
        }

        _target = target;
        _origin = target.anchoredPosition;

        _cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        TrembleAsync(amplitude, frequency, _cancellation.Token).Forget();
    }

    /// <summary>
    /// 떨림을 멈추고 원래 자리로 되돌립니다. 떨고 있지 않았다면 아무 일도 하지 않습니다.
    /// </summary>
    public void Stop()
    {
        if (_cancellation != null)
        {
            _cancellation.Cancel();
            _cancellation.Dispose();
            _cancellation = null;
        }

        if (_target != null)
        {
            _target.anchoredPosition = _origin;
            _target = null;
        }
    }

    private async UniTaskVoid TrembleAsync(float amplitude, float frequency, CancellationToken cancellationToken)
    {
        try
        {
            await StoryEffectTween.RepeatAsync(elapsed => ApplyTremble(elapsed, amplitude, frequency), cancellationToken);
        }
        catch (System.OperationCanceledException)
        {
            // 다음 줄로 넘어갔거나 화면을 떠난 것뿐이라 알릴 것이 없습니다. 자리 되돌리기는 Stop이 이미 했습니다.
        }
    }

    private void ApplyTremble(float elapsed, float amplitude, float frequency)
    {
        if (_target == null)
        {
            return;
        }

        float phase = elapsed * frequency;
        float x = (Mathf.PerlinNoise(phase, 0f) - 0.5f) * 2f * amplitude;
        float y = (Mathf.PerlinNoise(0f, phase + VERTICAL_NOISE_OFFSET) - 0.5f) * 2f * amplitude;

        _target.anchoredPosition = _origin + new Vector2(x, y);
    }
}
