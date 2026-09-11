using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

/// <summary>
/// 트랙 위 마우스 조작으로 노트를 배치·선택·이동·삭제하는 입력을 전담합니다.
/// 좌클릭은 빈 칸이면 배치, 이미 노트가 있으면 선택과 동시에 드래그 이동을 시작하고, 우클릭은 즉시 삭제합니다.
/// Shift+좌클릭은 롱노트를 시작하고, 같은 레인의 뒤쪽을 한 번 더 눌러 길이를 정합니다.
/// 키보드 레코딩은 LiveEditorLaneKeyRecorder가 담당합니다.
/// </summary>
public class LiveEditorInputHandler : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorNoteEditing _noteEditing;

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
            _noteEditing.LongNotePlacer.Cancel();
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
        if (!_noteEditing.TrackPointer.TryGetCell(mouse.position.ReadValue(), out int lane, out int barIndex, out int cellIndex))
        {
            return;
        }

        int timeMs = _noteEditing.EditContext.GetCellTimeMs(barIndex, cellIndex);
        NoteData existing = _noteEditing.Selection.FindNoteNear(lane, timeMs);

        if (existing == null)
        {
            PlaceNote(lane, barIndex, cellIndex, timeMs);
            return;
        }

        // 이미 놓인 롱노트의 머리를 Shift+클릭하면 길이를 다시 정하는 상태로 되돌아갑니다.
        // 다음 Shift+클릭 자리가 곧 새 꼬리이므로, 지웠다 두 번 다시 찍지 않고 늘이거나 줄일 수 있습니다.
        if (IsLongNoteModifierPressed() && existing.NoteType == ENoteType.LONG)
        {
            _noteEditing.LongNotePlacer.Reopen(existing);
            return;
        }

        _noteEditing.LongNotePlacer.Cancel();

        bool isMultiSelect = Keyboard.current != null && Keyboard.current.ctrlKey.isPressed;
        _noteEditing.Selection.Select(existing, isMultiSelect);
        BeginDrag(existing);
    }

    /// <summary>
    /// 빈 칸을 눌렀을 때의 배치입니다. 이미 노트가 있는 칸은 여기까지 오지 않으므로 같은 자리에 두 장이 겹치지 않습니다.
    ///
    /// 롱노트는 Shift를 누른 두 번의 클릭이 곧바로 이어질 때만 한 쌍으로 봅니다.
    /// 사이에 다른 조작이 끼면 진행 상태를 버리므로, 한참 전에 찍어 둔 시작점에 길이가 붙어
    /// 여러 마디를 가로지르는 롱노트가 만들어지는 일이 없습니다.
    /// </summary>
    private void PlaceNote(int lane, int barIndex, int cellIndex, int timeMs)
    {
        if (IsLongNoteModifierPressed() && lane != LiveLane.GHOST)
        {
            _noteEditing.LongNotePlacer.Place(lane, barIndex, cellIndex, timeMs);
            return;
        }

        _noteEditing.LongNotePlacer.Cancel();
        _noteEditing.NoteWriter.AddNote(lane, timeMs);
    }

    private static bool IsLongNoteModifierPressed()
    {
        return Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
    }

    private void HandleRightPress(Mouse mouse)
    {
        if (!_noteEditing.TrackPointer.TryGetCellTime(mouse.position.ReadValue(), out int lane, out int timeMs))
        {
            return;
        }

        _noteEditing.LongNotePlacer.Cancel();

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
}
