using System;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 리듬게임 일시정지 팝업입니다(기획서 3-I-9).
/// 음악·노트·타이머를 멈추는 것은 LiveGameController의 몫이고, 이 팝업은 세 가지 선택지를 전달만 합니다.
/// </summary>
public class UI_LivePausePopup : UI_Popup
{
    public Action OnResumeRequested;
    public Action OnRestartRequested;
    public Action OnQuitRequested;

    [Foldout("Hierarchy")]
    [Header("Buttons")]
    [SerializeField]
    private Button _resumeButton;

    [SerializeField]
    private Button _restartButton;

    [SerializeField]
    private Button _quitButton;

    protected override void Awake()
    {
        base.Awake();

        _resumeButton.onClick.AddListener(RequestResume);
        _restartButton.onClick.AddListener(RequestRestart);
        _quitButton.onClick.AddListener(RequestQuit);
    }

    private void RequestResume()
    {
        ClosePopup();
        OnResumeRequested?.Invoke();
    }

    private void RequestRestart()
    {
        ClosePopup();
        OnRestartRequested?.Invoke();
    }

    private void RequestQuit()
    {
        ClosePopup();
        OnQuitRequested?.Invoke();
    }

    /// <summary>
    /// 컨트롤러에 알리기 전에 먼저 닫아, 팝업이 걸어 둔 UI 입력 모드가 풀린 상태에서 재개가 진행되게 합니다.
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
