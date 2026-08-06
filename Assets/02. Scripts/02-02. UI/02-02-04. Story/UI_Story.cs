using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스토리 감상 화면의 부품을 한데 묶어 내주는 컴포저입니다(기획서 SCREEN-003).
///
/// 스스로 판단하는 것이 없습니다. 진행 순서와 상태 전이는 StoryPlaybackController가 정하고,
/// 이 클래스는 어느 오브젝트가 무슨 역할인지만 인스펙터로 고정합니다.
/// UI_Live가 리듬게임 화면에 대해 하는 일과 같습니다.
/// </summary>
public class UI_Story : MonoBehaviour
{
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
    public Button NextButton => _nextButton;
    public Button SkipTypeWriterButton => _skipTypeWriterButton;
    public Button LogButton => _logButton;
    public Button BackButton => _backButton;
    public UI_StoryLogPopup LogPopup => _logPopup;
    public UI_StoryExitConfirmPopup ExitConfirmPopup => _exitConfirmPopup;
}
