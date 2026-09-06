using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_Popup _popupNotification;

    [SerializeField]
    private UI_Popup _popupMailbox;

    [SerializeField]
    private UI_Popup _popupSetting;

    /// <summary>
    /// 씬 전환 중에만 켜지는 전체화면 로딩 패널입니다(기획서 3-L "화면 로딩 중 입력 | 입력 비활성화").
    ///
    /// 로딩 씬을 따로 두지 않는 것은 이 프로젝트가 서브 씬을 Additive로 겹쳐 올리는 구조이기 때문입니다.
    /// 로딩 씬을 끼우면 동시 로드 씬이 셋이 되고 SceneTransitionManager의 서브 씬 추적이 그것을 서브 씬으로 오인합니다.
    ///
    /// 패널이 로딩 표시와 입력 차단을 겸합니다.
    /// InputHandler.BlockInput()은 bool 플래그일 뿐이라 uGUI 버튼을 막지 못합니다.
    /// </summary>
    [SerializeField]
    private UI_LoadingPanel _loadingPanel;

    /// <summary>
    /// 공용 오류 안내 팝업입니다(SCREEN-011). 곡 데이터 없음·스토리 대사 없음·저장 실패가 모두 이 하나를 씁니다.
    /// 저장 실패는 어느 화면에서든 날 수 있어 화면 전용 팝업 원칙의 예외로 여기에 둡니다.
    /// </summary>
    [SerializeField]
    private UI_ErrorPopup _errorPopup;

    /// <summary>
    /// 팝업 스택은 외부에 노출하지 않습니다. 바깥에서는 아래의 위임 메서드만 쓰게 해,
    /// 화면 스크립트가 스택 내부 구조에 접근하는 2단 체인이 다시 생기지 않게 막습니다.
    /// </summary>
    private UI_PopupHandler _popupHandler;

    private PendingErrorPresenter _errorPresenter;

    /// <summary>
    /// 구독한 시점의 참조를 기억해 두었다가 그 참조로 해제합니다. 종료 순서에 따라 해제 시점에
    /// Instance 조회가 파괴된 매니저를 새로 찾으려 들 수 있기 때문입니다.
    /// </summary>
    private SceneTransitionManager _subscribedTransitionManager;

    public bool HasPopups => _popupHandler != null && _popupHandler.HasPopups;

    protected override void Awake()
    {
        base.Awake();
        _popupHandler = new UI_PopupHandler();
        _errorPresenter = new PendingErrorPresenter(_errorPopup, OpenPopup, this.GetCancellationTokenOnDestroy());
    }

    private void OnEnable()
    {
        _subscribedTransitionManager = SceneTransitionManager.Instance;
        if (_subscribedTransitionManager != null)
        {
            _subscribedTransitionManager.OnTransitionStarted += HandleTransitionStarted;
            _subscribedTransitionManager.OnTransitionFinished += HandleTransitionFinished;
        }
    }

    private void OnDisable()
    {
        if (_subscribedTransitionManager != null)
        {
            _subscribedTransitionManager.OnTransitionStarted -= HandleTransitionStarted;
            _subscribedTransitionManager.OnTransitionFinished -= HandleTransitionFinished;
            _subscribedTransitionManager = null;
        }
    }

    private void Update()
    {
        // 키보드가 없는 플랫폼에서는 Keyboard.current가 null이므로 매 프레임 존재 여부를 확인합니다.
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleEscapeInput();
        }
    }

    private void HandleEscapeInput()
    {
        // 이번 ESC가 방금 연 팝업은 같은 입력으로 닫지 않습니다(UI_PopupHandler.IsPopupOpenedThisFrame 참고).
        if (HasPopups && !_popupHandler.IsPopupOpenedThisFrame)
        {
            CloseLatestPopup();
        }
    }

    private void HandleTransitionStarted()
    {
        // 전환으로 강제 정리되는 팝업은 닫힘 통지가 오지 않으므로, 이월될 대기 오류를 먼저 폐기합니다.
        _errorPresenter.ClearPendingErrors();
        ClearAllPopups();

        // 로딩이 끝날 때까지 화면 전체를 덮습니다. 떠나는 화면의 버튼이 그대로 살아 있으면
        // 연속 클릭이 이미 시작된 전환 위에 또 다른 동작을 얹습니다(기획서 3-L "화면 로딩 중 입력").
        SetLoadingActive(true);
    }

    private void HandleTransitionFinished()
    {
        SetLoadingActive(false);
    }

    public void OpenPopup(UI_Popup popup)
    {
        _popupHandler.OpenPopup(popup);
    }

    public void CloseLatestPopup()
    {
        if (_popupHandler != null)
        {
            _popupHandler.CloseLatestPopup();
        }
    }

    public void ClearAllPopups()
    {
        if (_popupHandler != null)
        {
            _popupHandler.ClearAllPopups();
        }
    }

    public void OpenNotificationPopup()
    {
        if (_popupNotification != null)
        {
            OpenPopup(_popupNotification);
        }
    }

    public void OpenMailboxPopup()
    {
        if (_popupMailbox != null)
        {
            OpenPopup(_popupMailbox);
        }
    }

    public void OpenSettingPopup()
    {
        if (_popupSetting != null)
        {
            OpenPopup(_popupSetting);
        }
    }

    public void OpenProfileSettingPopup(UI_ProfileSettingPopup popup)
    {
        if (popup != null)
        {
            OpenPopup(popup);
        }
    }

    public void OpenErrorPopup(string message, Action onConfirmed = null)
    {
        _errorPresenter.OpenErrorPopup(message, onConfirmed);
    }

    /// <summary>
    /// 로딩 패널을 페이드로 켜고 끕니다. 씬 전환은 SceneTransitionManager가 중복 진입을 막으므로
    /// 중첩 요청이 없어, 참조 카운트 없이 그대로 켜고 끄면 됩니다.
    ///
    /// 페이드가 끝나기를 기다리지 않습니다. 입력 차단은 패널이 알아서 즉시 시작하므로
    /// 전환을 페이드 시간만큼 늦출 이유가 없습니다.
    /// </summary>
    public void SetLoadingActive(bool isActive)
    {
        if (_loadingPanel == null)
        {
            return;
        }

        if (isActive)
        {
            // 팝업이 열려 있는 동안 전환이 시작될 수 있으므로, 켤 때마다 맨 앞으로 올려 항상 최상단을 보장합니다.
            _loadingPanel.transform.SetAsLastSibling();
            _loadingPanel.Show();
            return;
        }

        _loadingPanel.Hide();
    }
}
