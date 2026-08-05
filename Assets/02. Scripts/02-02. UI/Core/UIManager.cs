using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_NotificationPopup _popupNotification;

    [SerializeField]
    private UI_MailboxPopup _popupMailbox;

    [SerializeField]
    private UI_SettingPopup _popupSetting;

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
    /// 안내가 떠 있는 동안 들어온 다음 오류들입니다. 팝업이 하나뿐이라 차례로 보여 줍니다.
    /// </summary>
    private readonly Queue<(string Message, Action OnConfirmed)> _pendingErrors = new Queue<(string, Action)>();

    public UI_PopupHandler PopupHandler { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        PopupHandler = new UI_PopupHandler();

        if (_errorPopup != null)
        {
            _errorPopup.OnClosed = ShowNextPendingError;
        }
    }

    private void OnEnable()
    {
        if (PopupHandler != null)
        {
            PopupHandler.Init();
        }
    }

    private void OnDisable()
    {
        if (PopupHandler != null)
        {
            PopupHandler.Dispose();
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
        if (PopupHandler != null && PopupHandler.HasPopups)
        {
            PopupHandler.CloseLatestPopup();
        }
    }

    public void OpenNotificationPopup()
    {
        if (_popupNotification != null)
        {
            PopupHandler.OpenPopup(_popupNotification);
        }
    }

    public void OpenMailboxPopup()
    {
        if (_popupMailbox != null)
        {
            PopupHandler.OpenPopup(_popupMailbox);
        }
    }

    public void OpenSettingPopup()
    {
        if (_popupSetting != null)
        {
            PopupHandler.OpenPopup(_popupSetting);
        }
    }

    public void OpenProfileSettingPopup(UI_ProfileSettingPopup popup)
    {
        if (popup != null)
        {
            PopupHandler.OpenPopup(popup);
        }
    }

    /// <summary>
    /// 오류를 안내합니다(기획서 3-L). onConfirmed는 안내를 닫은 뒤에 이어서 할 일이며,
    /// "오류 팝업 후 스토리 목록 복귀"처럼 복귀가 뒤따르는 경우에 넘깁니다.
    ///
    /// 팝업이 없으면(PersistentScene 없이 단독 실행) 안내를 건너뛰더라도 뒤처리는 그대로 진행합니다.
    /// 그러지 않으면 대본을 읽지 못한 감상 화면에 갇힙니다.
    /// </summary>
    public void OpenErrorPopup(string message, Action onConfirmed = null)
    {
        if (_errorPopup == null)
        {
            Debug.LogWarning($"[UIManager] 오류 안내 팝업이 없어 문구를 표시하지 못했습니다: {message}");
            onConfirmed?.Invoke();
            return;
        }

        // 이미 떠 있는 안내를 덮어쓰지 않고 차례를 기다립니다. 덮어쓰면 문구만 바뀌는 것이 아니라
        // 앞선 요청의 콜백까지 사라져, "오류 팝업 후 스토리 목록 복귀"처럼 닫기에 매인 뒤처리가 끊깁니다.
        // (감상 화면에서 대본을 읽지 못한 안내가 떠 있는 동안 저장 실패가 겹치는 경우)
        if (_errorPopup.gameObject.activeSelf)
        {
            _pendingErrors.Enqueue((message, onConfirmed));
            return;
        }

        ShowError(message, onConfirmed);
    }

    private void ShowError(string message, Action onConfirmed)
    {
        _errorPopup.SetError(message, onConfirmed);
        PopupHandler.OpenPopup(_errorPopup);
    }

    /// <summary>
    /// 밀려 있던 다음 안내를 엽니다. 이 통지는 UI_ErrorPopup.CloseAsync 안에서 오므로,
    /// 같은 프레임에 다시 열면 아직 끝나지 않은 닫기 뒤처리(UI_PopupHandler의 형제 순서 복원)가
    /// 새로 연 팝업을 도로 뒤로 밀어냅니다. 그래서 한 프레임을 기다린 뒤 엽니다.
    /// </summary>
    private void ShowNextPendingError()
    {
        if (_pendingErrors.Count == 0)
        {
            return;
        }

        ShowNextPendingErrorAsync().Forget();
    }

    private async UniTaskVoid ShowNextPendingErrorAsync()
    {
        await UniTask.NextFrame(this.GetCancellationTokenOnDestroy());

        if (!_pendingErrors.TryDequeue(out (string Message, Action OnConfirmed) request))
        {
            return;
        }

        ShowError(request.Message, request.OnConfirmed);
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
