using System.Collections.Generic;

/// <summary>
/// 노트의 추가·이동·삭제 커맨드 생성을 한곳에서 담당합니다.
/// 키보드 레코딩과 마우스 편집이 같은 규칙으로 노트를 다루도록, 배치 가능 여부 판정도 여기서 처리합니다.
///
/// 유니티 생명주기를 쓰지 않으므로 컴포넌트가 아니며, LiveEditorNoteEditing이 생성해 소유합니다.
/// </summary>
public class LiveEditorNoteWriter
{
    // 6번 레인은 고스트 노트 전용이며, 나머지 레인에는 고스트 노트를 놓을 수 없습니다.
    public const int GHOST_LANE = 6;

    private readonly LiveEditorController _controller;
    private readonly LiveEditorUndoRedoManager _undoRedoManager;

    public LiveEditorNoteWriter(LiveEditorController controller, LiveEditorUndoRedoManager undoRedoManager)
    {
        _controller = controller;
        _undoRedoManager = undoRedoManager;
    }

    public NoteData AddNote(int lane, int timeMs, ENoteType noteType)
    {
        var note = new NoteData
        {
            NoteId = System.Guid.NewGuid().ToString(),
            TimeMs = timeMs,
            Lane = lane,
            NoteType = noteType,
        };

        _undoRedoManager.PushCommand(new AddNoteCommand(_controller.CurrentChart.Notes, note));
        return note;
    }

    public NoteData AddNote(int lane, int timeMs)
    {
        return AddNote(lane, timeMs, GetNoteTypeForLane(lane));
    }

    /// <summary>
    /// 드래그 도중에는 미리보기를 위해 노트 값을 직접 바꿔 두므로, 놓는 시점에 원래 위치만 커맨드로 남깁니다.
    /// </summary>
    public void MoveNote(NoteData note, int previousLane, int previousTimeMs)
    {
        _undoRedoManager.PushCommand(new MoveNoteCommand(note, previousTimeMs, previousLane, note.TimeMs, note.Lane));
    }

    public void ResizeHold(NoteData note, int previousHoldDurationMs, int newHoldDurationMs)
    {
        _undoRedoManager.PushCommand(new ResizeHoldCommand(note, previousHoldDurationMs, newHoldDurationMs));
    }

    public void DeleteNote(NoteData note)
    {
        _undoRedoManager.PushCommand(new DeleteNoteCommand(_controller.CurrentChart.Notes, new List<NoteData> { note }));
    }

    public void DeleteNotes(List<NoteData> notes)
    {
        _undoRedoManager.PushCommand(new DeleteNoteCommand(_controller.CurrentChart.Notes, notes));
    }

    /// <summary>
    /// 고스트 레인 규칙을 어기거나 같은 레인의 다른 노트와 시각이 겹치는 자리는 거부합니다.
    /// 저장 시점의 유효성 검사에서 걸리기 전에 편집 단계에서 막아 줍니다.
    /// </summary>
    public bool CanPlaceAt(NoteData note, int lane, int timeMs)
    {
        bool isGhostNote = note.NoteType == ENoteType.GHOST;
        if (isGhostNote != (lane == GHOST_LANE))
        {
            return false;
        }

        foreach (NoteData other in _controller.CurrentChart.Notes)
        {
            if (ReferenceEquals(other, note))
            {
                continue;
            }

            if (other.Lane == lane && other.TimeMs == timeMs)
            {
                return false;
            }
        }

        return true;
    }

    public static ENoteType GetNoteTypeForLane(int lane)
    {
        return lane == GHOST_LANE ? ENoteType.GHOST : ENoteType.NORMAL;
    }
}
