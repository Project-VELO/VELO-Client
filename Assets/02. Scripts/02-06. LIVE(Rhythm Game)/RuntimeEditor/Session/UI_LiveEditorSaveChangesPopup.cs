using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 저장하지 않은 편집이 남은 채로 다른 채보로 넘어가려 할 때, 저장 여부를 묻는 팝업입니다.
/// 되돌릴 수 없는 작업이므로 "저장 안 함"과 "취소"를 분리해 실수로 작업을 잃지 않게 합니다.
/// </summary>
public class UI_LiveEditorSaveChangesPopup : UI_Popup
{
    public Action OnSaveClicked;
    public Action OnDiscardClicked;
    public Action OnCancelClicked;

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _messageText;

    [SerializeField]
    private Button _saveButton;

    [SerializeField]
    private Button _discardButton;

    [SerializeField]
    private Button _cancelButton;

    protected override void Awake()
    {
        base.Awake();

        _saveButton.onClick.AddListener(NotifySave);
        _discardButton.onClick.AddListener(NotifyDiscard);
        _cancelButton.onClick.AddListener(NotifyCancel);
    }

    public void SetMessage(string message)
    {
        _messageText.text = message;
    }

    private void NotifySave()
    {
        OnSaveClicked?.Invoke();
    }

    private void NotifyDiscard()
    {
        OnDiscardClicked?.Invoke();
    }

    private void NotifyCancel()
    {
        OnCancelClicked?.Invoke();
    }
}
