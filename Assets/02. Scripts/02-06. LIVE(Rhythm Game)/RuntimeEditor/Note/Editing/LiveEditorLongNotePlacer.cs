/// <summary>
/// Shift+클릭 두 번으로 롱노트를 놓는 두 단계 상태를 전담합니다.
/// 마우스 입력 처리기가 단타 배치·선택·드래그와 이 상태를 함께 들면 성격이 다른 진행 상태가 한곳에 섞이므로 떼어 냈습니다.
///
/// 유니티 생명주기를 쓰지 않으므로 컴포넌트가 아니며, LiveEditorNoteEditing이 생성해 소유합니다.
/// </summary>
public class LiveEditorLongNotePlacer
{
    private readonly LiveEditorEditContext _editContext;
    private readonly LiveEditorNoteWriter _noteWriter;

    private NoteData _pendingNote;

    public LiveEditorLongNotePlacer(LiveEditorEditContext editContext, LiveEditorNoteWriter noteWriter)
    {
        _editContext = editContext;
        _noteWriter = noteWriter;
    }

    /// <summary>
    /// 첫 클릭에서 한 칸짜리 롱노트를 놓고, 같은 레인의 뒤쪽 칸을 다시 클릭하면 그 자리까지 길이를 늘립니다.
    ///
    /// 시작만 해 둔 상태에도 길이를 주는 이유는 길이가 0인 롱노트가 저장 시 검증에서 거부되기 때문입니다
    /// (LiveEditorChartValidator). 두 번째 클릭 없이 다른 작업으로 넘어가도 채보에는 유효한 롱노트만 남습니다.
    /// 다른 레인이나 앞쪽 칸을 클릭하면 늘리는 대신 그 자리에서 새로 시작하며, 잘못 놓은 것은 우클릭으로 지웁니다.
    /// </summary>
    public void Place(int lane, int barIndex, int cellIndex, int timeMs)
    {
        if (TryExtendPendingNote(timeMs, lane))
        {
            return;
        }

        int nextCellTimeMs = _editContext.GetCellTimeMs(barIndex, cellIndex + 1);
        _pendingNote = _noteWriter.AddHoldNote(lane, timeMs, nextCellTimeMs - timeMs);
    }

    /// <summary>
    /// 이미 놓인 롱노트를 다시 길이 대기 상태로 되돌립니다. 머리를 Shift+클릭한 뒤 원하는 칸을 Shift+클릭하면
    /// 꼬리가 그 자리로 옮겨집니다. 선택 상태가 화면에 드러나지 않으므로, 되돌릴 대상은 눈에 보이는 머리로만 지정합니다.
    /// </summary>
    public void Reopen(NoteData note)
    {
        _pendingNote = note;
    }

    /// <summary>
    /// 두 번의 Shift+클릭 사이에 다른 조작이 끼거나 편집이 막히면 진행 상태를 버립니다.
    /// 시작점이 지워졌거나 한참 전에 잊고 둔 것에 길이가 붙는 일을 막습니다.
    /// </summary>
    public void Cancel()
    {
        _pendingNote = null;
    }

    private bool TryExtendPendingNote(int timeMs, int lane)
    {
        if (_pendingNote == null || _pendingNote.Lane != lane || timeMs <= _pendingNote.TimeMs)
        {
            return false;
        }

        _noteWriter.ResizeHold(_pendingNote, _pendingNote.HoldDurationMs, timeMs - _pendingNote.TimeMs);
        _pendingNote = null;

        return true;
    }
}
