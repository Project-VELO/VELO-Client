using System;
using UnityEngine;
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

    private void Start()
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
        if (Input.GetKeyDown(KeyCode.Escape))
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

    public void FadeInLoadingPanel(Action callback)
    {
        callback?.Invoke();
    }

    public void FadeOutLoadingPanel(Action callback)
    {
        callback?.Invoke();
    }
}
