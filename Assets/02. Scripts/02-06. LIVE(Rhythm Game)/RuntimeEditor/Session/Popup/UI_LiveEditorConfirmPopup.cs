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

    [SerializeField]
    private TMP_Text _cancelButtonLabel;

    protected override void Awake()
    {
        base.Awake();

        _confirmButton.onClick.AddListener(NotifyConfirmed);
        _cancelButton.onClick.AddListener(NotifyCanceled);
    }

    /// <summary>
    /// 두 선택지가 모두 진행을 뜻하는 경우도 있어(예: 기존 채보를 지우거나 그대로 불러오기)
    /// 취소 쪽 문구도 호출하는 곳에서 정할 수 있게 열어 둡니다. 넘기지 않으면 단순 취소로 봅니다.
    /// </summary>
    public void SetMessage(string message, string confirmLabel, string cancelLabel = "취소")
    {
        _messageText.text = message;
        _confirmButtonLabel.text = confirmLabel;

        if (_cancelButtonLabel != null)
        {
            _cancelButtonLabel.text = cancelLabel;
        }
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
