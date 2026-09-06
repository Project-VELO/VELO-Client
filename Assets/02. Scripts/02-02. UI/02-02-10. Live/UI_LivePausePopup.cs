using System;
using Cysharp.Threading.Tasks;
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

    /// <summary>
    /// 세 버튼 중 하나로 고른 선택입니다. 닫힘 연출이 끝난 뒤에 실행하려고 들고 있습니다.
    /// 비어 있으면 선택 없이 닫힌 것으로 봅니다.
    /// </summary>
    private Action _pendingChoice;

    protected override void Awake()
    {
        base.Awake();

        _resumeButton.onClick.AddListener(RequestResume);
        _restartButton.onClick.AddListener(RequestRestart);
        _quitButton.onClick.AddListener(RequestQuit);
    }

    public override async UniTask OpenAsync()
    {
        _pendingChoice = null;

        await base.OpenAsync();
    }

    /// <summary>
    /// 닫는 연출까지 끝난 뒤에 선택을 실행합니다.
    ///
    /// 씬 전환으로 정리되는 팝업은 UI_PopupHandler.ClearAllPopups가 이 경로를 거치지 않고 바로 끄므로,
    /// 떠나는 화면을 되살리지 않습니다.
    /// </summary>
    public override async UniTask CloseAsync()
    {
        await base.CloseAsync();

        InvokeChoice();
    }

    private void RequestResume()
    {
        CloseWithChoice(OnResumeRequested);
    }

    private void RequestRestart()
    {
        CloseWithChoice(OnRestartRequested);
    }

    private void RequestQuit()
    {
        CloseWithChoice(OnQuitRequested);
    }

    /// <summary>
    /// 선택을 적어 두기만 하고 닫기를 요청합니다.
    /// UI_PopupHandler의 닫기는 연출이 끝날 때까지 이어지는 비동기 작업이라, 여기서 곧바로 실행하면
    /// 팝업이 사라지고 입력 모드가 풀리기도 전에 재개·재시작·씬 전환이 시작됩니다.
    /// </summary>
    private void CloseWithChoice(Action onChosen)
    {
        _pendingChoice = onChosen;

        ClosePopup();
    }

    /// <summary>
    /// 팝업 스택 없이 열린 단독 실행에서는 스택 정리도 CloseAsync 호출도 없으므로 바로 끄고 선택까지 처리합니다.
    /// </summary>
    private void ClosePopup()
    {
        if (OnCloseRequested == null)
        {
            gameObject.SetActive(false);
            InvokeChoice();
            return;
        }

        RequestClose();
    }

    /// <summary>
    /// 골라 둔 선택을 실행합니다. 선택 없이 닫힌 경우는 "계속하기"로 간주해 재개를 알립니다.
    /// ESC나 닫기 버튼으로 닫으면 팝업만 사라지고 게임은 멈춘 채 남아, 재개할 방법이 없어지기 때문입니다.
    /// </summary>
    private void InvokeChoice()
    {
        Action chosen = _pendingChoice ?? OnResumeRequested;
        _pendingChoice = null;

        chosen?.Invoke();
    }
}
