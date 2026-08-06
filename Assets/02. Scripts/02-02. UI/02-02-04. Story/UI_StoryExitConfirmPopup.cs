using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스토리 감상 중 뒤로가기를 눌렀을 때 뜨는 종료 확인 팝업입니다(기획서 6.3, 16-5).
///
/// 확인·취소 결과만 전달합니다. 완료 처리나 화면 이동은 재생 컨트롤러가 결정합니다.
/// UI_LivePausePopup과 같은 형태입니다.
///
/// UI_Popup의 닫기 버튼 자리는 비워 두고 확인 버튼만 직접 배선합니다.
/// 확인과 취소의 후처리가 달라 어느 경로로 닫혔는지 구분되어야 하기 때문입니다.
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

    /// <summary>
    /// 이번에 확인 버튼으로 닫히는 중인지 여부입니다. 닫힘 처리를 CloseAsync 한 곳으로 모으면서,
    /// 확인과 취소를 구분하기 위해 둡니다.
    /// </summary>
    private bool _isConfirming;

    protected override void Awake()
    {
        base.Awake();

        _confirmButton.onClick.AddListener(Confirm);
        _cancelButton.onClick.AddListener(Close);
    }

    /// <summary>
    /// 취소 통지는 버튼이 아니라 여기서 냅니다.
    ///
    /// 취소 버튼 외에도 닫히는 경로가 있습니다. ESC를 누르면 UIManager가 맨 위 팝업을 닫고
    /// (UIManager.cs의 HandleEscapeInput), 씬을 전환하면 SceneTransitionManager가 팝업을 모두 지웁니다.
    /// 버튼 콜백에서만 통지하면 그 경로로 닫혔을 때 재생이 멈춘 채로 남아 화면이 아무 입력도 받지 않게 됩니다.
    /// 모든 닫힘이 CloseAsync를 거치므로 여기서 통지하면 경로가 하나로 모입니다.
    /// </summary>
    public override async UniTask CloseAsync()
    {
        await base.CloseAsync();

        if (_isConfirming)
        {
            _isConfirming = false;
            OnConfirmed?.Invoke();
            return;
        }

        OnCancelled?.Invoke();
    }

    private void Confirm()
    {
        _isConfirming = true;
        Close();
    }

    private void Close()
    {
        if (OnCloseRequested == null)
        {
            // 팝업 스택 없이 열린 단독 실행입니다. CloseAsync가 불리지 않으므로 여기서 직접 통지합니다.
            gameObject.SetActive(false);

            if (_isConfirming)
            {
                _isConfirming = false;
                OnConfirmed?.Invoke();
                return;
            }

            OnCancelled?.Invoke();
            return;
        }

        RequestClose();
    }
}
