using System;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public Action OnLogRequested;
    public Action OnBackRequested;

    [Foldout("Hierarchy")]
    [Header("Parts")]
    [SerializeField]
    private UI_StoryStage _stage;

    [SerializeField]
    private UI_StoryIllustration _illustration;

    [SerializeField]
    private UI_StoryDialogBox _dialogBox;

    [SerializeField]
    private UI_StoryEffectLayer _effectLayer;

    [SerializeField]
    private StoryAudioBinder _audioBinder;

    [Foldout("Hierarchy")]
    [Header("Buttons")]
    /// <summary>
    /// 대사 상자 전체를 덮는 버튼입니다. 출력 중이면 즉시 완성, 끝났으면 다음 대사로 넘깁니다(기획서 6.3).
    /// 넘기기 전용 버튼을 따로 두지 않는 것은, 상자 어디를 눌러도 진행되는 편이 읽는 흐름을 끊지 않기 때문입니다.
    /// </summary>
    [SerializeField]
    private Button _nextButton;

    [SerializeField]
    private Button _logButton;

    [SerializeField]
    private Button _backButton;

    /// <summary>
    /// 대사 상자 우상단의 AUTO 버튼입니다. 켜고 끄는 일만 하고, 실제 자동 진행은 컨트롤러가 StoryAutoPlay로 돌립니다.
    /// </summary>
    [SerializeField]
    private UI_StoryAutoToggle _autoToggle;

    [Foldout("Hierarchy")]
    [Header("Popups")]
    [SerializeField]
    private UI_StoryLogPopup _logPopup;

    [SerializeField]
    private UI_StoryExitConfirmPopup _exitConfirmPopup;

    public UI_StoryStage Stage => _stage;
    public UI_StoryIllustration Illustration => _illustration;
    public UI_StoryDialogBox DialogBox => _dialogBox;
    public UI_StoryEffectLayer EffectLayer => _effectLayer;
    public StoryAudioBinder AudioBinder => _audioBinder;
    public UI_StoryLogPopup LogPopup => _logPopup;
    public UI_StoryExitConfirmPopup ExitConfirmPopup => _exitConfirmPopup;
    public UI_StoryAutoToggle AutoToggle => _autoToggle;

    private void Awake()
    {
        _nextButton.onClick.AddListener(RequestNext);
        _logButton.onClick.AddListener(RequestLog);
        _backButton.onClick.AddListener(RequestBack);
    }

    /// <summary>
    /// Enter 키도 대사 상자 클릭과 같은 NEXT입니다(대사 상자의 "클릭 or Enter" 안내).
    /// 팝업이 떠 있을 때 눌린 Enter는 진행 흐름이 PAUSED에서 무시하므로 여기서 가리지 않습니다.
    /// 키보드가 없는 플랫폼에서는 Keyboard.current가 null이라 매 프레임 확인합니다.
    /// </summary>
    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            RequestNext();
        }
    }

    private void RequestNext()
    {
        OnNextRequested?.Invoke();
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
