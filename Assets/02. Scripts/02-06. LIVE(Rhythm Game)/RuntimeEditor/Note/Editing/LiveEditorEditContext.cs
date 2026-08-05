/// <summary>
/// 편집 가능 여부와 격자 셀 시각 계산처럼 여러 입력 처리기가 공통으로 묻는 조건을 한곳에 모읍니다.
/// 같은 판정을 여러 클래스에 흩어 두면 조건이 어긋나기 쉬우므로 이 클래스만 보도록 합니다.
///
/// 유니티 생명주기를 쓰지 않는 순수 판정이므로 컴포넌트가 아니며, LiveEditorNoteEditing이 생성해 소유합니다.
/// </summary>
public class LiveEditorEditContext
{
    private readonly LiveEditorController _controller;
    private readonly LiveEditorTimeline _timeline;

    /// <summary>
    /// 정지 상태에서 원하는 마디에 노트를 놓는 것이 기본 작업 흐름이므로,
    /// 편집은 테스트 플레이와 팝업 표시 중에만 막습니다.
    /// </summary>
    public bool CanEdit
    {
        get
        {
            return !InputHandler.IsInputBlocked
                && _controller.State != LiveEditorController.EEditorState.TestPlay
                && !ReferenceEquals(_controller.CurrentChart, null)
                && _timeline.BarLayout.IsBuilt;
        }
    }

    public LiveEditorEditContext(LiveEditorController controller, LiveEditorTimeline timeline)
    {
        _controller = controller;
        _timeline = timeline;
    }

    public int GetCellTimeMs(int barIndex, int cellIndex)
    {
        return _timeline.GetCellTimeMs(barIndex, cellIndex);
    }

    /// <summary>
    /// 임의의 시각을 현재 스냅 단위에서 가장 가까운 격자 셀의 시각으로 바꿉니다.
    /// </summary>
    public bool TryGetCellAtTime(int rawTimeMs, out int timeMs)
    {
        timeMs = 0;

        double barPosition = _timeline.BarLayout.GetBarPosition(rawTimeMs);
        if (!_timeline.BarLayout.TryGetCellAtBarPosition(barPosition, _timeline.SnapDivision, out int barIndex, out int cellIndex))
        {
            return false;
        }

        timeMs = _timeline.GetCellTimeMs(barIndex, cellIndex);
        return true;
    }
}
