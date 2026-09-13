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
    private readonly LiveEditorController _controller;
    private readonly LiveEditorUndoRedoManager _undoRedoManager;

    public LiveEditorNoteWriter(LiveEditorController controller, LiveEditorUndoRedoManager undoRedoManager)
    {
        _controller = controller;
        _undoRedoManager = undoRedoManager;
    }

    public NoteData AddNote(int lane, int timeMs, ENoteType noteType)
    {
        return AddNote(lane, timeMs, noteType, 0);
    }

    public NoteData AddNote(int lane, int timeMs)
    {
        return AddNote(lane, timeMs, GetNoteTypeForLane(lane), 0);
    }

    /// <summary>
    /// 길이가 0인 롱노트는 저장 시 검증에서 거부되므로(LiveEditorChartValidator), 놓는 순간부터 길이를 함께 받습니다.
    /// </summary>
    public NoteData AddHoldNote(int lane, int timeMs, int holdDurationMs)
    {
        return AddNote(lane, timeMs, ENoteType.LONG, holdDurationMs);
    }

    private NoteData AddNote(int lane, int timeMs, ENoteType noteType, int holdDurationMs)
    {
        var note = new NoteData
        {
            NoteId = System.Guid.NewGuid().ToString(),
            TimeMs = timeMs,
            Lane = lane,
            NoteType = noteType,
            HoldDurationMs = holdDurationMs,
        };

        _undoRedoManager.PushCommand(new AddNoteCommand(_controller.CurrentChart.Notes, note));
        return note;
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
        if (isGhostNote != (lane == LiveLane.GHOST))
        {
            return false;
        }

        return !HasNoteAt(lane, timeMs, note);
    }

    /// <summary>
    /// 같은 레인·같은 시각에 이미 노트가 있는지 봅니다.
    /// 클릭 선택에 쓰는 LiveEditorNoteSelection.FindNoteNear는 시간차를 넉넉히 봐주므로, 배치 판정에 쓰면
    /// 칸 간격이 그보다 좁은 분박(빠른 곡의 32분박 등)에서 옆 칸의 노트까지 걸려 빈 칸에 놓지 못합니다.
    /// </summary>
    public bool HasNoteAt(int lane, int timeMs)
    {
        return HasNoteAt(lane, timeMs, null);
    }

    private bool HasNoteAt(int lane, int timeMs, NoteData ignoredNote)
    {
        foreach (NoteData other in _controller.CurrentChart.Notes)
        {
            if (ReferenceEquals(other, ignoredNote))
            {
                continue;
            }

            if (other.Lane == lane && other.TimeMs == timeMs)
            {
                return true;
            }
        }

        return false;
    }

    public static ENoteType GetNoteTypeForLane(int lane)
    {
        return lane == LiveLane.GHOST ? ENoteType.GHOST : ENoteType.NORMAL;
    }
}
