using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 실시간 키보드 레코딩(1~6)과 마우스 클릭을 처리하여 노트를 배치/선택하는 클래스입니다.
/// New Input System(InputAction) 기반으로 구현되었습니다.
/// </summary>
public class LiveEditorInputHandler : MonoBehaviour
{
    private const int SelectionToleranceMs = 60;

    [SerializeField]
    private LiveEditorController _controller;

    [SerializeField]
    private LiveEditorTimeline _timeline;

    [SerializeField]
    private LiveEditorAudioPlayer _audioPlayer;

    [SerializeField]
    private LiveEditorUndoRedoManager _undoRedoManager;

    [SerializeField]
    private RectTransform _laneAreaRect;

    [SerializeField]
    private Camera _uiCamera;

    private readonly List<InputAction> _laneKeyActions = new List<InputAction>();
    private readonly List<NoteData> _selection = new List<NoteData>();
    private NoteData _pendingLongNoteStart;

    public IReadOnlyList<NoteData> Selection => _selection;

    private void Awake()
    {
        BindLaneKeyActions();
    }

    private void OnEnable()
    {
        foreach (InputAction action in _laneKeyActions)
        {
            action.Enable();
        }
    }

    private void OnDisable()
    {
        foreach (InputAction action in _laneKeyActions)
        {
            action.Disable();
        }
    }

    private void Update()
    {
        HandleMouseClick();
    }

    public void ClearSelection()
    {
        _selection.Clear();
    }

    private void BindLaneKeyActions()
    {
        string[] laneBindings = { "<Keyboard>/1", "<Keyboard>/2", "<Keyboard>/3", "<Keyboard>/4", "<Keyboard>/5", "<Keyboard>/6" };
        for (int i = 0; i < laneBindings.Length; i++)
        {
            int lane = i + 1;
            var action = new InputAction($"Lane{lane}", binding: laneBindings[i]);
            action.performed += _ => RecordNoteOnLane(lane);
            _laneKeyActions.Add(action);
        }
    }

    private void RecordNoteOnLane(int lane)
    {
        if (_controller.State != LiveEditorController.EEditorState.Editing || _controller.CurrentChart == null)
        {
            return;
        }

        AddNoteAtLane(lane, _audioPlayer.CurrentTimeMs);
    }

    private void HandleMouseClick()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (_controller.State != LiveEditorController.EEditorState.Editing || _controller.CurrentChart == null)
        {
            return;
        }

        if (!TryScreenPointToLane(mouse.position.ReadValue(), out int lane, out int rawTimeMs))
        {
            return;
        }

        int snappedTimeMs = LiveEditorBpmTimeConverter.SnapToGrid(_controller.CurrentChart, rawTimeMs, _timeline.SnapDivision);
        bool isLongNoteModifier = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;

        if (isLongNoteModifier && lane != 6)
        {
            HandleLongNoteClick(lane, snappedTimeMs);
            return;
        }

        NoteData existing = FindNoteNear(lane, snappedTimeMs);
        if (existing != null)
        {
            SelectNote(existing);
            return;
        }

        AddNoteAtLane(lane, snappedTimeMs);
    }

    private void SelectNote(NoteData note)
    {
        bool isMultiSelect = Keyboard.current != null && Keyboard.current.ctrlKey.isPressed;
        if (!isMultiSelect)
        {
            _selection.Clear();
        }
        _selection.Add(note);
    }

    private void HandleLongNoteClick(int lane, int timeMs)
    {
        if (_pendingLongNoteStart == null || _pendingLongNoteStart.Lane != lane)
        {
            _pendingLongNoteStart = new NoteData
            {
                NoteId = Guid.NewGuid().ToString(),
                TimeMs = timeMs,
                Lane = lane,
                NoteType = ENoteType.LONG,
                HoldDurationMs = 0,
            };
            _undoRedoManager.PushCommand(new AddNoteCommand(_controller.CurrentChart.Notes, _pendingLongNoteStart));
            return;
        }

        int holdDurationMs = Mathf.Max(0, timeMs - _pendingLongNoteStart.TimeMs);
        _undoRedoManager.PushCommand(new ResizeHoldCommand(_pendingLongNoteStart, _pendingLongNoteStart.HoldDurationMs, holdDurationMs));
        _pendingLongNoteStart = null;
    }

    private void AddNoteAtLane(int lane, int timeMs)
    {
        ENoteType noteType = lane == 6 ? ENoteType.GHOST : ENoteType.NORMAL;

        var note = new NoteData
        {
            NoteId = Guid.NewGuid().ToString(),
            TimeMs = timeMs,
            Lane = lane,
            NoteType = noteType,
        };

        _undoRedoManager.PushCommand(new AddNoteCommand(_controller.CurrentChart.Notes, note));
    }

    private NoteData FindNoteNear(int lane, int timeMs)
    {
        NoteData closest = null;
        int closestDelta = int.MaxValue;

        foreach (NoteData note in _controller.CurrentChart.Notes)
        {
            if (note.Lane != lane)
            {
                continue;
            }

            int delta = Mathf.Abs(note.TimeMs - timeMs);
            if (delta <= SelectionToleranceMs && delta < closestDelta)
            {
                closest = note;
                closestDelta = delta;
            }
        }

        return closest;
    }

    private bool TryScreenPointToLane(Vector2 screenPosition, out int lane, out int timeMs)
    {
        lane = 0;
        timeMs = 0;

        if (_laneAreaRect == null || _timeline.Lanes == null)
        {
            return false;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_laneAreaRect, screenPosition, _uiCamera, out Vector2 localPoint))
        {
            return false;
        }

        Rect rect = _laneAreaRect.rect;
        float verticalRatio = Mathf.Clamp01((localPoint.y - rect.yMin) / rect.height);

        for (int candidateLane = 1; candidateLane <= 6; candidateLane++)
        {
            _timeline.Lanes.GetLaneBoundsAtRatio(candidateLane, verticalRatio, out float leftX, out float rightX);
            if (localPoint.x < leftX || localPoint.x > rightX)
            {
                continue;
            }

            lane = candidateLane;
            timeMs = _timeline.RatioToTimeMs(verticalRatio);
            return true;
        }

        return false;
    }
}
