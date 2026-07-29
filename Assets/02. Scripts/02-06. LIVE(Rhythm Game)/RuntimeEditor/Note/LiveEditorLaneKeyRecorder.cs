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
    private LiveEditorEditContext _editContext;

    [SerializeField]
    private LiveEditorAudioPlayer _audioPlayer;

    [SerializeField]
    private LiveEditorNoteWriter _noteWriter;

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

    private void RecordNoteOnLane(int lane)
    {
        if (!_editContext.CanEdit)
        {
            return;
        }

        if (!_editContext.TryGetCellAtTime(_audioPlayer.CurrentTimeMs, out int timeMs))
        {
            return;
        }

        _noteWriter.AddNote(lane, timeMs);
    }
}
