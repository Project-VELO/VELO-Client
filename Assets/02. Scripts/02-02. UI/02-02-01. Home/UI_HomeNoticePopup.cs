using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 홈 전용 안내 팝업입니다(문구 + 확인). 이어하기의 "이 회차의 처음부터 시작합니다" 안내가 씁니다(검수 1 답: 안내는 확인 팝업).
///
/// 공용 오류 팝업(UI_ErrorPopup)은 제목이 "오류 안내"라 일반 안내에 쓰지 않습니다.
///
/// 확인 버튼으로 닫을 때만 다음 동작을 잇습니다. Esc는 맨 위 팝업을 닫기만 해야 하므로(기획서 16장),
/// Esc로 닫았는데 회차에 들어가 버리면 사용자가 물러선 동작이 진행으로 바뀝니다.
/// </summary>
public class UI_HomeNoticePopup : UI_Popup
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _messageText;

    [SerializeField]
    private Button _confirmButton;

    private Action _onConfirmed;
    private bool _isConfirmed;

    protected override void Awake()
    {
        base.Awake();
        _confirmButton.onClick.AddListener(Confirm);
    }

    public void SetNotice(string message, Action onConfirmed)
    {
        _messageText.text = UiText.Localize(message);
        _onConfirmed = onConfirmed;
        _isConfirmed = false;
    }

    /// <summary>
    /// 콜백은 실행하기 전에 비웁니다. 팝업이 계속 살아 있어, 남겨 두면 다음 안내를 닫을 때 지난 동작이 한 번 더 실행됩니다.
    /// </summary>
    public override async UniTask CloseAsync()
    {
        Action onConfirmed = _isConfirmed ? _onConfirmed : null;
        _onConfirmed = null;
        _isConfirmed = false;

        await base.CloseAsync();

        onConfirmed?.Invoke();
    }

    private void Confirm()
    {
        _isConfirmed = true;
        RequestClose();
    }
}
