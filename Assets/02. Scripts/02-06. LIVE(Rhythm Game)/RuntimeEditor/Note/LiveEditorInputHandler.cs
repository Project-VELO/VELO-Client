using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

/// <summary>
/// 실시간 키보드 레코딩(1~6)과 마우스 클릭을 처리하여 노트를 배치/선택하는 클래스입니다.
/// 배치 위치는 항상 화면에 그려진 격자 셀에서 직접 산출하므로, 보이는 격자와 실제 노트 시각이 어긋나지 않습니다.
/// </summary>
public class LiveEditorInputHandler : MonoBehaviour
{
    private const int GHOST_LANE = 6;

    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorController _controller;

    [SerializeField]
    private LiveEditorTimeline _timeline;

    [SerializeField]
    private LiveEditorAudioPlayer _audioPlayer;

    [SerializeField]
    private LiveEditorUndoRedoManager _undoRedoManager;

    [SerializeField]
    private LiveEditorTrackPointer _trackPointer;

    private readonly List<InputAction> _laneKeyActions = new List<InputAction>();
    private readonly LiveEditorNoteSelection _selection = new LiveEditorNoteSelection();

    private NoteData _pendingLongNoteStart;

    public IReadOnlyList<NoteData> Selection => _selection.Notes;

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

    private void Update()
    {
        HandleMouseClick();
    }

    private void OnDisable()
    {
        foreach (InputAction action in _laneKeyActions)
        {
            action.Disable();
        }
    }

    private void OnDestroy()
    {
        foreach (InputAction action in _laneKeyActions)
        {
            action.Dispose();
        }

        _laneKeyActions.Clear();
    }

    public void ClearSelection()
    {
        _selection.Clear();
    }

    private void BindLaneKeyActions()
    {
        var laneBindings = new List<string> { "<Keyboard>/1", "<Keyboard>/2", "<Keyboard>/3", "<Keyboard>/4", "<Keyboard>/5", "<Keyboard>/6" };

        for (int i = 0; i < laneBindings.Count; i++)
        {
            int lane = i + 1;
            var action = new InputAction($"Lane{lane}", binding: laneBindings[i]);
            action.performed += _ => RecordNoteOnLane(lane);
            _laneKeyActions.Add(action);
        }
    }

    /// <summary>
    /// 재생 중 눌린 키를 현재 재생 위치에서 가장 가까운 격자 셀로 스냅해 배치합니다.
    /// </summary>
    private void RecordNoteOnLane(int lane)
    {
        if (!CanEdit())
        {
            return;
        }

        double barPosition = _timeline.BarLayout.GetBarPosition(_audioPlayer.CurrentTimeMs);

        if (!_timeline.BarLayout.TryGetCellAtBarPosition(barPosition, _timeline.SnapDivision, out int barIndex, out int cellIndex))
        {
            return;
        }

        AddNote(lane, _timeline.GetCellTimeMs(barIndex, cellIndex));
    }

    private void HandleMouseClick()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (!CanEdit())
        {
            return;
        }

        if (!_trackPointer.TryGetCell(mouse.position.ReadValue(), out int lane, out int barIndex, out int cellIndex))
        {
            return;
        }

        int cellTimeMs = _timeline.GetCellTimeMs(barIndex, cellIndex);
        bool isLongNoteModifier = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;

        if (isLongNoteModifier && lane != GHOST_LANE)
        {
            HandleLongNoteClick(lane, cellTimeMs);
            return;
        }

        NoteData existing = _selection.FindNoteNear(_controller.CurrentChart, lane, cellTimeMs);

        if (existing != null)
        {
            bool isMultiSelect = Keyboard.current != null && Keyboard.current.ctrlKey.isPressed;
            _selection.Select(existing, isMultiSelect);
            return;
        }

        AddNote(lane, cellTimeMs);
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

    private void AddNote(int lane, int timeMs)
    {
        var note = new NoteData
        {
            NoteId = Guid.NewGuid().ToString(),
            TimeMs = timeMs,
            Lane = lane,
            NoteType = lane == GHOST_LANE ? ENoteType.GHOST : ENoteType.NORMAL,
        };

        _undoRedoManager.PushCommand(new AddNoteCommand(_controller.CurrentChart.Notes, note));
    }

    private bool CanEdit()
    {
        // 정지 상태에서 원하는 마디에 노트를 놓는 것이 기본 작업 흐름이므로, 편집은 테스트 플레이와 팝업 표시 중에만 막습니다.
        return !InputHandler.IsInputBlocked
            && _controller.State != LiveEditorController.EEditorState.TestPlay
            && !ReferenceEquals(_controller.CurrentChart, null)
            && _timeline.BarLayout.IsBuilt;
    }
}
