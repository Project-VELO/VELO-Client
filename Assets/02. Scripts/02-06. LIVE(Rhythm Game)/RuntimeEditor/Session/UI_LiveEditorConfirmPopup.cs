using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 되돌릴 수 없는 작업을 진행하기 전에 한 번 더 묻는 확인 팝업입니다.
/// 현재는 기존 채보 덮어쓰기 확인에 사용됩니다.
/// </summary>
public class UI_LiveEditorConfirmPopup : UI_Popup
{
    public Action OnConfirmed;
    public Action OnCanceled;

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _messageText;

    [SerializeField]
    private Button _confirmButton;

    [SerializeField]
    private TMP_Text _confirmButtonLabel;

    [SerializeField]
    private Button _cancelButton;

    protected override void Awake()
    {
        base.Awake();

        _confirmButton.onClick.AddListener(NotifyConfirmed);
        _cancelButton.onClick.AddListener(NotifyCanceled);
    }

    public void SetMessage(string message, string confirmLabel)
    {
        _messageText.text = message;
        _confirmButtonLabel.text = confirmLabel;
    }

    private void NotifyConfirmed()
    {
        OnConfirmed?.Invoke();
    }

    private void NotifyCanceled()
    {
        OnCanceled?.Invoke();
    }
}
