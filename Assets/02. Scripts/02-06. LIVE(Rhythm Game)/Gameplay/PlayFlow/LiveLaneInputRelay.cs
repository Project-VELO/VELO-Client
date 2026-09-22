/// <summary>
/// 리듬게임 씬에서 레인을 누르고 뗀 입력을 판정기로 넘깁니다. 판정은 오프셋이 반영된 JudgementTimeMs로 합니다.
///
/// 언제 입력을 받을지는 LiveGameController가 상태 전환에 맞춰 SetRelaying으로 정합니다.
/// 입력 장치의 수락 여부와 이 클래스의 전달 여부를 한 번에 바꾸므로, 플레이 중이 아닌 때의 입력이 판정기에 닿지 않습니다.
///
/// 유니티 이벤트 메서드를 쓰지 않으므로 컴포넌트가 아니라 컨트롤러가 생성해 쓰는 일반 클래스입니다.
/// </summary>
public class LiveLaneInputRelay
{
    private readonly LivePlayInput _playInput;
    private readonly LiveConductor _conductor;
    private readonly LiveJudgementProcessor _judgementProcessor;
    private readonly UI_Live _liveUI;

    private bool _isRelaying;

    public LiveLaneInputRelay(LivePlayInput playInput, LiveConductor conductor,
        LiveJudgementProcessor judgementProcessor, UI_Live liveUI)
    {
        _playInput = playInput;
        _conductor = conductor;
        _judgementProcessor = judgementProcessor;
        _liveUI = liveUI;

        _playInput.OnLanePressed += PressLane;
        _playInput.OnLaneReleased += ReleaseLane;
    }

    public void SetRelaying(bool isRelaying)
    {
        _isRelaying = isRelaying;
        _playInput.SetAcceptingInput(isRelaying);
    }

    public void Release()
    {
        _playInput.OnLanePressed -= PressLane;
        _playInput.OnLaneReleased -= ReleaseLane;
    }

    private void PressLane(int lane)
    {
        if (!_isRelaying)
        {
            return;
        }

        if (_liveUI.LaneFeedback != null)
        {
            _liveUI.LaneFeedback.RefreshLanePress(lane);
        }

        _judgementProcessor.PressLane(lane, _conductor.JudgementTimeMs);
    }

    private void ReleaseLane(int lane)
    {
        if (!_isRelaying)
        {
            return;
        }

        _judgementProcessor.ReleaseLane(lane, _conductor.JudgementTimeMs);
    }
}
