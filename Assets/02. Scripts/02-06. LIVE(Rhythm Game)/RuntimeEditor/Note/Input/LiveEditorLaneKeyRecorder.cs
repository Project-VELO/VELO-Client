using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

/// <summary>
/// 1~6 키를 눌러 현재 재생 위치에 노트를 실시간으로 찍는 레코딩 입력을 전담합니다.
/// 눌린 순간의 시각을 가장 가까운 격자 셀로 스냅하므로, 손이 조금 밀려도 박자에 정렬됩니다.
/// </summary>
public class LiveEditorLaneKeyRecorder : MonoBehaviour
{
    private static readonly List<string> LaneKeyBindings = new List<string>
    {
        "<Keyboard>/1", "<Keyboard>/2", "<Keyboard>/3", "<Keyboard>/4", "<Keyboard>/5", "<Keyboard>/6",
    };

    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorNoteEditing _noteEditing;

    [SerializeField]
    private LiveAudioPlayer _audioPlayer;

    private readonly List<InputAction> _laneKeyActions = new List<InputAction>();

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

    private void OnDestroy()
    {
        foreach (InputAction action in _laneKeyActions)
        {
            action.Dispose();
        }

        _laneKeyActions.Clear();
    }

    private void BindLaneKeyActions()
    {
        for (int i = 0; i < LaneKeyBindings.Count; i++)
        {
            int lane = i + 1;
            var action = new InputAction($"Lane{lane}", binding: LaneKeyBindings[i]);
            action.performed += _ => RecordNoteOnLane(lane);
            _laneKeyActions.Add(action);
        }
    }

    /// <summary>
    /// 정지 상태에서 같은 키를 두 번 누르면 같은 셀로 스냅되므로, 이미 그 자리에 있는 노트를 먼저 걸러 냅니다.
    /// 시각까지 똑같이 겹친 노트는 화면에서 한 장으로 보여 알아차리기 어렵습니다.
    /// </summary>
    private void RecordNoteOnLane(int lane)
    {
        if (!_noteEditing.EditContext.CanEdit)
        {
            return;
        }

        if (!_noteEditing.EditContext.TryGetCellAtTime(_audioPlayer.CurrentTimeMs, out int timeMs))
        {
            return;
        }

        if (_noteEditing.Selection.FindNoteNear(lane, timeMs) != null)
        {
            return;
        }

        _noteEditing.NoteWriter.AddNote(lane, timeMs);
    }
}
