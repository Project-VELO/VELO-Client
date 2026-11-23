using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 홈 화면입니다(기획서 SCREEN-001, 개발 기획서 v1.16 2장 H01~H09).
///
/// 이 클래스는 화면 이동 버튼을 배선하고 진입, 언어 변경, 프로필 저장 때 화면 조각들을 다시 그리게 합니다.
/// 그리는 일은 위치별 컴포넌트가 나눠 맡습니다: 프로필(H01) UI_HomeProfile, 이어하기(H02) UI_HomeContinue,
/// 가운데 캐릭터(H06) UI_HomeCharacter, 대사창(H07) UI_HomeDialog.
/// </summary>
public class UI_Home : MonoBehaviour, ILanguageRefreshable
{
    [Foldout("Hierarchy")]
    [Header("Story")]
    [SerializeField]
    private UI_HomeStoryBox _storyBox;

    [SerializeField]
    private UI_HomeContinue _continue;

    [SerializeField]
    private Button _episodeListButton;

    [Foldout("Hierarchy")]
    [Header("Character")]
    [SerializeField]
    private UI_HomeProfile _profile;

    [SerializeField]
    private UI_HomeCharacter _homeCharacter;

    [SerializeField]
    private UI_HomeDialog _dialog;

    [Foldout("Hierarchy")]
    [Header("Menu")]
    [SerializeField]
    private Button _collectionButton;

    [SerializeField]
    private Button _practiceButton;

    [SerializeField]
    private Button _settingButton;

    [SerializeField]
    private Button _exitButton;

    /// <summary>
    /// 씬에 둔 팝업을 씬의 이 화면 인스턴스에서 연결합니다(UI_MusicSelect의 포토카드 팝업과 같은 방식).
    /// </summary>
    [Foldout("Hierarchy")]
    [Header("Popups")]
    [SerializeField]
    private UI_QuitConfirmPopup _quitConfirmPopup;

    private void Start()
    {
        // 홈에는 더 이상 스케줄 목록이 없지만, 홈 진입 때 하던 스케줄 자동 완료 반영은 그대로 둡니다.
        // 사무실을 거치지 않고 홈만 오가는 동안에도 스케줄 진행이 지금까지와 같은 시점에 저장되게 하기 위해서입니다.
        GameProgressService.Instance.SyncStorySchedules();

        RefreshScreen();
        InitButtons();
    }

    private void OnDestroy()
    {
        _continue.OnStoryListRequested -= MoveToStoryList;
        _profile.OnProfileApplied -= RefreshScreen;
    }

    /// <summary>
    /// 언어를 고른 자리에서 부릅니다. 챕터 이름이 마스터 데이터에서 오므로
    /// 글자만 훑어서는 바뀌지 않습니다.
    /// </summary>
    public void RefreshLanguage()
    {
        RefreshScreen();
    }

    /// <summary>
    /// 대사창의 화자는 가운데 홈 캐릭터입니다(검수 1 답, 개발 기획서 16장 권고). 그래서 캐릭터를 먼저 바꾸고 대사창을 그립니다.
    /// </summary>
    private void RefreshScreen()
    {
        _storyBox.Refresh();
        _continue.Refresh();
        _profile.Refresh();
        _homeCharacter.SetCharacter(HomeProfileService.Instance.HomeCharacterId);
        _dialog.SetSpeaker(_homeCharacter.CurrentEntry);
    }

    private void InitButtons()
    {
        _continue.OnStoryListRequested += MoveToStoryList;
        _profile.OnProfileApplied += RefreshScreen;

        _episodeListButton.onClick.AddListener(MoveToStoryList);
        _collectionButton.onClick.AddListener(MoveToCollection);
        _practiceButton.onClick.AddListener(StartFreeLive);

        // 설정 화면이 아직 없어 설정과 나가기가 같은 팝업을 엽니다. 이 팝업이 언어 선택·데이터 초기화와
        // 게임 종료를 함께 들고 있는 임시 설정 창이라, 다른 화면의 설정 버튼(UI_TopRightMenu)과도 같은 배선입니다.
        _settingButton.onClick.AddListener(OpenQuitConfirm);
        _exitButton.onClick.AddListener(OpenQuitConfirm);
    }

    /// <summary>
    /// 회차 목록(H03)입니다. 스케줄 바로가기로 들어온 것이 아니므로 강조 대상을 지웁니다.
    /// 지우지 않으면 이전에 스케줄로 진입했을 때의 회차가 계속 강조됩니다.
    /// </summary>
    private void MoveToStoryList()
    {
        StoryEntryContext.Instance.ClearHighlight();
        LoadScene(ESceneNames.SelectStoryScene);
    }

    /// <summary>
    /// 컬렉션(H04)입니다. 컬렉션 화면 안의 규칙은 이 작업의 범위 밖이라 이동만 합니다(검수 1 답).
    /// </summary>
    private void MoveToCollection()
    {
        LoadScene(ESceneNames.CollectionScene);
    }

    /// <summary>
    /// 연습실(H05)입니다. 연습실 화면이 들어오기 전까지는 지금의 곡 선택 화면으로 갑니다.
    /// 지정곡 없이 진입하므로 곡 선택 화면에서 첫 곡이 자동 선택되고 다른 곡으로 바꿀 수 있습니다.
    /// </summary>
    private void StartFreeLive()
    {
        LiveEntryStarter.StartLive(EEntryType.HOME_LIVE, cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    /// <summary>
    /// 팝업 스택은 PersistentScene 의 UIManager 가 들고 있으므로, 이 화면만 단독으로 열어
    /// 확인할 때는 없을 수 있습니다.
    /// </summary>
    private void OpenQuitConfirm()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.OpenPopup(_quitConfirmPopup);
    }

    private void LoadScene(ESceneNames sceneName)
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneAsync(sceneName, this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}
