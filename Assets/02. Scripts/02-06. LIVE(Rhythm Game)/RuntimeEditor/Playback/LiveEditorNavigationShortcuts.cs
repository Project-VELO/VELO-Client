using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

/// <summary>
/// 재생/일시정지와 마디·분박 단위 탐색 입력을 처리하는 클래스입니다.
/// 트랙에서 위쪽이 미래 시점이므로, 위 방향 입력(PageUp, 휠 위)을 곡 진행 방향으로 매핑합니다.
/// 실제 재생 위치 변경과 스크롤 갱신은 LiveEditorController가 수행합니다.
/// </summary>
public class LiveEditorNavigationShortcuts : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorPlaybackControl _playbackControl;

    private readonly List<InputAction> _actions = new List<InputAction>();

    private void Awake()
    {
        BindAction("PlayPause", "<Keyboard>/space", _ => _playbackControl.TogglePlayPause());
        BindAction("SeekBackward", "<Keyboard>/leftArrow", _ => _playbackControl.SeekByGridStep(-1));
        BindAction("SeekForward", "<Keyboard>/rightArrow", _ => _playbackControl.SeekByGridStep(1));
        BindAction("PreviousBar", "<Keyboard>/pageDown", _ => _playbackControl.SeekByBar(-1));
        BindAction("NextBar", "<Keyboard>/pageUp", _ => _playbackControl.SeekByBar(1));
        BindAction("SeekToFirstBar", "<Keyboard>/home", _ => _playbackControl.SetPlaybackBar(0));
        BindAction("WheelSeek", "<Mouse>/scroll/y", context => SeekByWheel(context.ReadValue<float>()));
    }

    private void OnEnable()
    {
        foreach (InputAction action in _actions)
        {
            action.Enable();
        }
    }

    private void OnDisable()
    {
        foreach (InputAction action in _actions)
        {
            action.Disable();
        }
    }

    private void OnDestroy()
    {
        foreach (InputAction action in _actions)
        {
            action.Dispose();
        }

        _actions.Clear();
    }

    /// <summary>
    /// 팝업이 떠 있는 동안에는 InputHandler가 입력을 차단하므로, 모든 단축키를 한곳에서 함께 막습니다.
    /// </summary>
    private void BindAction(string actionName, string binding, Action<InputAction.CallbackContext> callback)
    {
        var action = new InputAction(actionName, binding: binding);
        action.performed += context =>
        {
            if (InputHandler.IsInputBlocked)
            {
                return;
            }

            callback(context);
        };
        _actions.Add(action);
    }

    /// <summary>
    /// 휠 델타는 플랫폼마다 크기가 달라 값 자체는 쓰지 않고 방향만 사용해 한 칸씩 이동합니다.
    /// </summary>
    private void SeekByWheel(float scrollDelta)
    {
        if (Mathf.Approximately(scrollDelta, 0f))
        {
            return;
        }

        _playbackControl.SeekByGridStep(0f < scrollDelta ? 1 : -1);
    }
}
