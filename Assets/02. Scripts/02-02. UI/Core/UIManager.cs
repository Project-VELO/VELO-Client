using System;
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

    public UI_PopupHandler PopupHandler { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        PopupHandler = new UI_PopupHandler();
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
        InputHandler.TriggerCancelEvent();
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

    public void FadeInLoadingPanel(Action callback)
    {
        callback?.Invoke();
    }

    public void FadeOutLoadingPanel(Action callback)
    {
        callback?.Invoke();
    }
}
