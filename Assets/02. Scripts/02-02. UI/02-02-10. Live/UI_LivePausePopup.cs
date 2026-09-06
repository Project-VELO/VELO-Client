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
    /// 세 버튼 중 하나로 닫히는 중인지 여부입니다. 선택 없이 닫힌 경우를 가려내는 데 씁니다.
    /// </summary>
    private bool _isChoiceMade;

    protected override void Awake()
    {
        base.Awake();

        _resumeButton.onClick.AddListener(RequestResume);
        _restartButton.onClick.AddListener(RequestRestart);
        _quitButton.onClick.AddListener(RequestQuit);
    }

    public override async UniTask OpenAsync()
    {
        _isChoiceMade = false;

        await base.OpenAsync();
    }

    /// <summary>
    /// 선택 없이 닫힌 경우를 "계속하기"로 간주해 재개를 알립니다.
    /// ESC나 닫기 버튼으로 닫으면 팝업만 사라지고 게임은 멈춘 채 남아, 재개할 방법이 없어지기 때문입니다.
    ///
    /// 씬 전환으로 정리되는 팝업은 UI_PopupHandler.ClearAllPopups가 이 경로를 거치지 않고 바로 끄므로,
    /// 떠나는 화면을 되살리지 않습니다.
    /// </summary>
    public override async UniTask CloseAsync()
    {
        await base.CloseAsync();

        if (_isChoiceMade)
        {
            return;
        }

        OnResumeRequested?.Invoke();
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
    /// 컨트롤러에 알리기 전에 먼저 닫아, 팝업이 걸어 둔 UI 입력 모드가 풀린 상태에서 선택이 진행되게 합니다.
    /// </summary>
    private void CloseWithChoice(Action onChosen)
    {
        _isChoiceMade = true;

        ClosePopup();
        onChosen?.Invoke();
    }

    /// <summary>
    /// 팝업 스택 없이 열린 단독 실행에서는 스택 정리가 없으므로 바로 끕니다.
    /// </summary>
    private void ClosePopup()
    {
        if (OnCloseRequested == null)
        {
            gameObject.SetActive(false);
            return;
        }

        RequestClose();
    }
}
