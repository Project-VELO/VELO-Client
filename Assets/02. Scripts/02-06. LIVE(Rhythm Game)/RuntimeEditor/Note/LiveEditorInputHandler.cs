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
    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorEditContext _editContext;

    [SerializeField]
    private LiveEditorTrackPointer _trackPointer;

    [SerializeField]
    private LiveEditorNoteSelection _selection;

    [SerializeField]
    private LiveEditorNoteWriter _noteWriter;

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

        if (!_editContext.CanEdit)
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
        if (!_trackPointer.TryGetCellTime(mouse.position.ReadValue(), out int lane, out int timeMs))
        {
            return;
        }

        bool isLongNoteModifier = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
        if (isLongNoteModifier && lane != LiveEditorNoteWriter.GHOST_LANE)
        {
            HandleLongNoteClick(lane, timeMs);
            return;
        }

        NoteData existing = _selection.FindNoteNear(lane, timeMs);
        if (existing == null)
        {
            _noteWriter.AddNote(lane, timeMs);
            return;
        }

        bool isMultiSelect = Keyboard.current != null && Keyboard.current.ctrlKey.isPressed;
        _selection.Select(existing, isMultiSelect);
        BeginDrag(existing);
    }

    private void HandleRightPress(Mouse mouse)
    {
        if (!_trackPointer.TryGetCellTime(mouse.position.ReadValue(), out int lane, out int timeMs))
        {
            return;
        }

        NoteData existing = _selection.FindNoteNear(lane, timeMs);
        if (existing == null)
        {
            return;
        }

        _selection.Remove(existing);
        _noteWriter.DeleteNote(existing);
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

        if (!_trackPointer.TryGetCellTime(mouse.position.ReadValue(), out int lane, out int timeMs))
        {
            return;
        }

        if (_draggingNote.Lane == lane && _draggingNote.TimeMs == timeMs)
        {
            return;
        }

        if (!_noteWriter.CanPlaceAt(_draggingNote, lane, timeMs))
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

        _noteWriter.MoveNote(note, _dragOriginLane, _dragOriginTimeMs);
    }

    private void HandleLongNoteClick(int lane, int timeMs)
    {
        if (_pendingLongNoteStart == null || _pendingLongNoteStart.Lane != lane)
        {
            _pendingLongNoteStart = _noteWriter.AddNote(lane, timeMs, ENoteType.LONG);
            return;
        }

        int holdDurationMs = Mathf.Max(0, timeMs - _pendingLongNoteStart.TimeMs);
        _noteWriter.ResizeHold(_pendingLongNoteStart, _pendingLongNoteStart.HoldDurationMs, holdDurationMs);
        _pendingLongNoteStart = null;
    }
}
