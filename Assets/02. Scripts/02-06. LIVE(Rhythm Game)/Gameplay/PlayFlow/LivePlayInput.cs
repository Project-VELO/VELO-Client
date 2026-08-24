using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

/// <summary>
/// 리듬게임의 레인 입력을 담당합니다. 액션 에셋의 Live 액션맵(Lane1~Lane6)을 켜고 끄며,
/// 키가 눌린 순간을 레인 번호로 바꿔 알립니다. 판정은 구독자가 수행합니다.
///
/// 입력을 받지 않아야 하는 구간이 두 가지라 게이트도 둘입니다.
/// 팝업이 열린 동안은 InputHandler가 UI 모드로 바꿔 주므로 그 플래그를 그대로 따르고,
/// 팝업 없이 진행되는 시작·재개 카운트다운은 컨트롤러가 SetAcceptingInput으로 직접 잠급니다.
/// </summary>
public class LivePlayInput : MonoBehaviour
{
    public Action<int> OnLanePressed;

    /// <summary>
    /// 키를 뗀 순간을 알립니다. 롱노트의 유지 판정에만 쓰이며, 단타 노트는 이 통지를 무시합니다.
    /// </summary>
    public Action<int> OnLaneReleased;

    /// <summary>
    /// 일시정지 키가 눌린 것을 알립니다. 실제로 멈추는 것은 구독자(LivePauseController)가 판단합니다.
    /// </summary>
    public Action OnPauseRequested;

    private const string LIVE_ACTION_MAP_NAME = "Live";
    private const string LANE_ACTION_NAME_PREFIX = "Lane";
    private const string PAUSE_ACTION_NAME = "Pause";

    [Foldout("Project")]
    [SerializeField]
    private InputActionAsset _inputActions;

    private readonly InputAction[] _laneActions = new InputAction[LiveLane.COUNT];
    private readonly Action<InputAction.CallbackContext>[] _pressCallbacks = new Action<InputAction.CallbackContext>[LiveLane.COUNT];
    private readonly Action<InputAction.CallbackContext>[] _releaseCallbacks = new Action<InputAction.CallbackContext>[LiveLane.COUNT];

    private InputActionMap _liveActionMap;
    private InputAction _pauseAction;
    private Action<InputAction.CallbackContext> _pauseCallback;
    private bool _isAcceptingInput;

    private void Awake()
    {
        InitLaneActions();
        InitPauseAction();
    }

    private void OnEnable()
    {
        if (_liveActionMap == null)
        {
            return;
        }

        for (int i = 0; i < LiveLane.COUNT; i++)
        {
            if (_laneActions[i] != null)
            {
                _laneActions[i].performed += _pressCallbacks[i];
                _laneActions[i].canceled += _releaseCallbacks[i];
            }
        }

        if (_pauseAction != null)
        {
            _pauseAction.performed += _pauseCallback;
        }

        _liveActionMap.Enable();
    }

    private void OnDisable()
    {
        if (_liveActionMap == null)
        {
            return;
        }

        for (int i = 0; i < LiveLane.COUNT; i++)
        {
            if (_laneActions[i] != null)
            {
                _laneActions[i].performed -= _pressCallbacks[i];
                _laneActions[i].canceled -= _releaseCallbacks[i];
            }
        }

        if (_pauseAction != null)
        {
            _pauseAction.performed -= _pauseCallback;
        }

        // 액션 에셋은 프로젝트 전역에서 공유되므로, 이 씬이 내려갈 때 맵을 반드시 다시 꺼 둡니다.
        _liveActionMap.Disable();
    }

    public void SetAcceptingInput(bool isAccepting)
    {
        _isAcceptingInput = isAccepting;
    }

    private void InitLaneActions()
    {
        if (_inputActions == null)
        {
            Debug.LogError("[LivePlayInput] InputActionAsset이 할당되지 않아 레인 입력을 받을 수 없습니다.");
            return;
        }

        _liveActionMap = _inputActions.FindActionMap(LIVE_ACTION_MAP_NAME, throwIfNotFound: false);
        if (_liveActionMap == null)
        {
            Debug.LogError($"[LivePlayInput] 액션 에셋에 '{LIVE_ACTION_MAP_NAME}' 액션맵이 없습니다.");
            return;
        }

        for (int i = 0; i < LiveLane.COUNT; i++)
        {
            int lane = LiveLane.FIRST + i;
            InputAction action = _liveActionMap.FindAction($"{LANE_ACTION_NAME_PREFIX}{lane}", throwIfNotFound: false);

            if (action == null)
            {
                Debug.LogError($"[LivePlayInput] '{LANE_ACTION_NAME_PREFIX}{lane}' 액션을 찾지 못했습니다.");
                continue;
            }

            _laneActions[i] = action;

            // 구독 해제를 위해 델리게이트 인스턴스를 그대로 보관합니다. 매번 새로 만들면 -=가 아무것도 지우지 못합니다.
            _pressCallbacks[i] = _ => PressLane(lane);
            _releaseCallbacks[i] = _ => ReleaseLane(lane);
        }
    }

    private void InitPauseAction()
    {
        if (_liveActionMap == null)
        {
            return;
        }

        _pauseAction = _liveActionMap.FindAction(PAUSE_ACTION_NAME, throwIfNotFound: false);

        if (_pauseAction == null)
        {
            Debug.LogError($"[LivePlayInput] 액션 에셋에 '{PAUSE_ACTION_NAME}' 액션이 없어 키보드로 일시정지할 수 없습니다.");
            return;
        }

        _pauseCallback = _ => RequestPause();
    }

    /// <summary>
    /// 레인 입력 게이트(SetAcceptingInput)와 따로 두어 화면의 Pause 버튼과 같은 조건으로 동작합니다.
    /// 실제로 멈출 수 있는지는 LiveGameController.TryPause가 정하며 플레이 중에만 통과하므로,
    /// 카운트다운 동안 누른 ESC는 팝업을 열지 않고 그대로 흘려보냅니다.
    /// 팝업이 이미 열려 있으면 InputHandler가 입력을 막아 두므로 두 번 열리지 않습니다.
    /// </summary>
    private void RequestPause()
    {
        if (InputHandler.Instance.IsInputBlocked)
        {
            return;
        }

        OnPauseRequested?.Invoke();
    }

    private void PressLane(int lane)
    {
        if (!IsInputAccepted())
        {
            return;
        }

        OnLanePressed?.Invoke(lane);
    }

    /// <summary>
    /// 뗀 입력도 누른 입력과 같은 조건으로 막습니다. 일시정지 도중 뗀 것을 뒤늦게 통지하면
    /// 멈춰 있는 재생 시각으로 롱노트가 실패 처리되기 때문입니다.
    /// </summary>
    private void ReleaseLane(int lane)
    {
        if (!IsInputAccepted())
        {
            return;
        }

        OnLaneReleased?.Invoke(lane);
    }

    private bool IsInputAccepted()
    {
        return _isAcceptingInput && !InputHandler.Instance.IsInputBlocked;
    }
}
