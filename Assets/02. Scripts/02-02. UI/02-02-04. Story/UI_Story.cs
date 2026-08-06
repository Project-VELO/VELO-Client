using System;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스토리 감상 화면의 부품을 한데 묶어 내주는 컴포저입니다(기획서 SCREEN-003).
///
/// 스스로 판단하는 것이 없습니다. 버튼 클릭을 의도 단위 통지로 바꿔 내보낼 뿐,
/// 진행 순서와 상태 전이는 StoryPlaybackController가 정합니다.
/// UI_Live가 리듬게임 화면에 대해 하는 일과 같습니다.
/// </summary>
public class UI_Story : MonoBehaviour
{
    /// <summary>
    /// 컨트롤러가 버튼 오브젝트까지 내려와 직접 배선하면 화면 구조 변경이 곧 컨트롤러 수정이 되므로,
    /// 자기 부품의 배선은 화면이 맡고 밖으로는 의도만 알립니다.
    /// </summary>
    public Action OnNextRequested;
    public Action OnSkipTypeWriterRequested;
    public Action OnLogRequested;
    public Action OnBackRequested;

    [Foldout("Hierarchy")]
    [Header("Parts")]
    [SerializeField]
    private UI_StoryStage _stage;

    [SerializeField]
    private UI_StoryDialogBox _dialogBox;

    [Foldout("Hierarchy")]
    [Header("Buttons")]
    [SerializeField]
    private Button _nextButton;

    [SerializeField]
    private Button _skipTypeWriterButton;

    [SerializeField]
    private Button _logButton;

    [SerializeField]
    private Button _backButton;

    [Foldout("Hierarchy")]
    [Header("Popups")]
    [SerializeField]
    private UI_StoryLogPopup _logPopup;

    [SerializeField]
    private UI_StoryExitConfirmPopup _exitConfirmPopup;

    public UI_StoryStage Stage => _stage;
    public UI_StoryDialogBox DialogBox => _dialogBox;
    public UI_StoryLogPopup LogPopup => _logPopup;
    public UI_StoryExitConfirmPopup ExitConfirmPopup => _exitConfirmPopup;

    private void Awake()
    {
        _nextButton.onClick.AddListener(RequestNext);
        _skipTypeWriterButton.onClick.AddListener(RequestSkipTypeWriter);
        _logButton.onClick.AddListener(RequestLog);
        _backButton.onClick.AddListener(RequestBack);
    }

    private void RequestNext()
    {
        OnNextRequested?.Invoke();
    }

    private void RequestSkipTypeWriter()
    {
        OnSkipTypeWriterRequested?.Invoke();
    }

    private void RequestLog()
    {
        OnLogRequested?.Invoke();
    }

    private void RequestBack()
    {
        OnBackRequested?.Invoke();
    }
}
