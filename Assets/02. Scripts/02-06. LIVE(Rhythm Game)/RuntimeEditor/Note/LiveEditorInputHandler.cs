using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

/// <summary>
/// 트랙 위 마우스 조작으로 노트를 배치·선택·이동·삭제하는 입력을 전담합니다.
/// 좌클릭은 빈 칸이면 배치, 이미 노트가 있으면 선택과 동시에 드래그 이동을 시작하고, 우클릭은 즉시 삭제합니다.
/// 키보드 레코딩은 LiveEditorLaneKeyRecorder가 담당합니다.
/// </summary>
public class LiveEditorInputHandler : MonoBehaviour
{
    /// <summary>
    /// 롱노트 배치(Shift+클릭) 허용 여부입니다.
    /// 판정은 LiveHoldTracker에 구현되어 있지만 1차 MVP 채보에는 롱노트를 쓰지 않기로 했으므로,
    /// 실수로 배치되어 의도와 다르게 플레이되는 것을 막기 위해 입력 단계에서만 잠가 둡니다.
    /// 롱노트를 쓰기로 하면 이 값만 true로 되돌리면 됩니다.
    /// </summary>
    private const bool IS_LONG_NOTE_PLACEMENT_ENABLED = false;

    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorNoteEditing _noteEditing;

    private NoteData _pendingLongNoteStart;
    private NoteData _draggingNote;
    private int _dragOriginLane;
    private int _dragOriginTimeMs;

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (!_noteEditing.EditContext.CanEdit)
        {
            CancelDrag();
            return;
        }

        if (_draggingNote != null)
        {
            UpdateDrag(mouse);
            return;
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            HandleRightPress(mouse);
            return;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            HandleLeftPress(mouse);
        }
    }

    private void HandleLeftPress(Mouse mouse)
    {
        if (!_noteEditing.TrackPointer.TryGetCellTime(mouse.position.ReadValue(), out int lane, out int timeMs))
        {
            return;
        }

        bool isLongNoteModifier = IS_LONG_NOTE_PLACEMENT_ENABLED && Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
        if (isLongNoteModifier && lane != LiveLane.GHOST)
        {
            HandleLongNoteClick(lane, timeMs);
            return;
        }

        NoteData existing = _noteEditing.Selection.FindNoteNear(lane, timeMs);
        if (existing == null)
        {
            _noteEditing.NoteWriter.AddNote(lane, timeMs);
            return;
        }

        bool isMultiSelect = Keyboard.current != null && Keyboard.current.ctrlKey.isPressed;
        _noteEditing.Selection.Select(existing, isMultiSelect);
        BeginDrag(existing);
    }

    private void HandleRightPress(Mouse mouse)
    {
        if (!_noteEditing.TrackPointer.TryGetCellTime(mouse.position.ReadValue(), out int lane, out int timeMs))
        {
            return;
        }

        NoteData existing = _noteEditing.Selection.FindNoteNear(lane, timeMs);
        if (existing == null)
        {
            return;
        }

        _noteEditing.Selection.Remove(existing);
        _noteEditing.NoteWriter.DeleteNote(existing);
    }

    private void BeginDrag(NoteData note)
    {
        _draggingNote = note;
        _dragOriginLane = note.Lane;
        _dragOriginTimeMs = note.TimeMs;
    }

    /// <summary>
    /// 드래그 도중에는 커맨드를 쌓지 않고 노트 값만 바꿔 실시간 미리보기를 보여 주고,
    /// 버튼을 놓는 시점에 원래 위치를 담은 이동 커맨드 하나만 남깁니다.
    /// </summary>
    private void UpdateDrag(Mouse mouse)
    {
        if (!mouse.leftButton.isPressed)
        {
            EndDrag();
            return;
        }

        if (!_noteEditing.TrackPointer.TryGetCellTime(mouse.position.ReadValue(), out int lane, out int timeMs))
        {
            return;
        }

        if (_draggingNote.Lane == lane && _draggingNote.TimeMs == timeMs)
        {
            return;
        }

        if (!_noteEditing.NoteWriter.CanPlaceAt(_draggingNote, lane, timeMs))
        {
            return;
        }

        _draggingNote.Lane = lane;
        _draggingNote.TimeMs = timeMs;
    }

    /// <summary>
    /// 드래그 도중 편집이 막히면(팝업 표시, 채보 닫힘 등) 이동 커맨드를 남길 수 없으므로,
    /// 커맨드 없이 옮겨진 값이 그대로 굳지 않도록 원래 위치로 되돌립니다.
    /// </summary>
    private void CancelDrag()
    {
        if (_draggingNote == null)
        {
            return;
        }

        _draggingNote.Lane = _dragOriginLane;
        _draggingNote.TimeMs = _dragOriginTimeMs;
        _draggingNote = null;
    }

    private void EndDrag()
    {
        NoteData note = _draggingNote;
        _draggingNote = null;

        bool isMoved = note.Lane != _dragOriginLane || note.TimeMs != _dragOriginTimeMs;
        if (!isMoved)
        {
            return;
        }

        _noteEditing.NoteWriter.MoveNote(note, _dragOriginLane, _dragOriginTimeMs);
    }

    private void HandleLongNoteClick(int lane, int timeMs)
    {
        if (_pendingLongNoteStart == null || _pendingLongNoteStart.Lane != lane)
        {
            _pendingLongNoteStart = _noteEditing.NoteWriter.AddNote(lane, timeMs, ENoteType.LONG);
            return;
        }

        int holdDurationMs = Mathf.Max(0, timeMs - _pendingLongNoteStart.TimeMs);
        _noteEditing.NoteWriter.ResizeHold(_pendingLongNoteStart, _pendingLongNoteStart.HoldDurationMs, holdDurationMs);
        _pendingLongNoteStart = null;
    }
}
