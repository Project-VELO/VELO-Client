using System;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// AUTO가 켜져 있을 때, 대사가 다 나온 뒤 잠시 기다렸다가 다음 줄로 넘깁니다.
///
/// StoryProgressFlow 안에 넣지 않은 것은 그쪽이 "누르면 무엇이 되는가"를 정하는 상태 기계이고,
/// 이쪽은 "언제 대신 눌러 줄 것인가"를 정하는 시계이기 때문입니다. 시계는 흐름의 공개 동작(Next)만
/// 씁니다. 그래서 사람이 먼저 누르면 시계가 멈추고, 팝업이 열려 있으면 눌러도 무시되는 규칙이
/// 흐름 쪽에 한 벌만 남습니다.
/// </summary>
public class StoryAutoPlay : IDisposable
{
    /// <summary>
    /// 마지막 글자가 찍힌 뒤 다음 줄로 넘기기까지 기다리는 시간입니다(기획: 4초).
    /// </summary>
    private const float DELAY_SECONDS = 4f;

    private readonly StoryProgressFlow _flow;
    private readonly CancellationToken _sceneToken;
    private CancellationTokenSource _waitCts;
    private bool _isEnabled;

    public StoryAutoPlay(StoryProgressFlow flow, CancellationToken sceneToken)
    {
        _flow = flow;
        _sceneToken = sceneToken;
        _flow.OnWaitingNext = OnWaitingNext;
    }

    /// <summary>
    /// 켜고 끕니다. 이미 다 나온 줄에서 켜면 그 자리에서 재기 시작하고, 끄면 재던 시간을 버립니다.
    /// </summary>
    public void SetEnabled(bool isEnabled)
    {
        _isEnabled = isEnabled;

        if (_isEnabled && _flow.IsWaitingNext)
        {
            StartWaiting();
            return;
        }

        Cancel();
    }

    public void Dispose()
    {
        Cancel();
    }

    private void OnWaitingNext()
    {
        if (_isEnabled)
        {
            StartWaiting();
        }
    }

    private void StartWaiting()
    {
        Cancel();
        _waitCts = CancellationTokenSource.CreateLinkedTokenSource(_sceneToken);
        WaitAsync(_waitCts.Token).Forget();
    }

    private void Cancel()
    {
        if (_waitCts == null)
        {
            return;
        }

        _waitCts.Cancel();
        _waitCts.Dispose();
        _waitCts = null;
    }

    /// <summary>
    /// 기다린 뒤에도 아직 같은 줄에서 다음을 기다리고 있을 때만 넘깁니다.
    /// 그 사이에 사람이 넘겼거나 팝업이 열렸다면 흐름의 상태가 바뀌어 있어 아무것도 하지 않습니다.
    /// 팝업을 닫으면 흐름이 OnWaitingNext를 다시 알려 새로 잽니다.
    /// </summary>
    private async UniTaskVoid WaitAsync(CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(DELAY_SECONDS), DelayType.UnscaledDeltaTime,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (_flow.IsWaitingNext)
        {
            _flow.Next();
        }
    }
}
