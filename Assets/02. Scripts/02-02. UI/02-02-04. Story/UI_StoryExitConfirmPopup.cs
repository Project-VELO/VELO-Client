using System;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스토리 감상 중 뒤로가기를 눌렀을 때 뜨는 종료 확인 팝업입니다(기획서 6.3, 16-5).
///
/// 확인·취소 결과만 전달합니다. 완료 처리나 화면 이동은 재생 컨트롤러가 결정합니다.
/// UI_LivePausePopup과 같은 형태입니다.
///
/// UI_Popup의 닫기 버튼 자리는 비워 두고 두 버튼을 직접 배선합니다.
/// 취소와 확인의 후처리가 서로 달라, 어느 경로로 닫혔는지 구분되어야 하기 때문입니다.
/// </summary>
public class UI_StoryExitConfirmPopup : UI_Popup
{
    public Action OnConfirmed;
    public Action OnCancelled;

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _confirmButton;

    [SerializeField]
    private Button _cancelButton;

    protected override void Awake()
    {
        base.Awake();

        _confirmButton.onClick.AddListener(Confirm);
        _cancelButton.onClick.AddListener(Cancel);
    }

    private void Confirm()
    {
        ClosePopup();
        OnConfirmed?.Invoke();
    }

    private void Cancel()
    {
        ClosePopup();
        OnCancelled?.Invoke();
    }

    /// <summary>
    /// 대리자에 알리기 전에 먼저 닫습니다. 확인이면 화면이 곧 바뀌고, 취소면 팝업이 걸어 둔 입력 차단이
    /// 풀린 상태에서 재개가 진행되어야 합니다.
    /// </summary>
    private void ClosePopup()
    {
        if (UIManager.Instance == null || UIManager.Instance.PopupHandler == null)
        {
            gameObject.SetActive(false);
            return;
        }

        UIManager.Instance.PopupHandler.ClosePopup(this);
    }
}
