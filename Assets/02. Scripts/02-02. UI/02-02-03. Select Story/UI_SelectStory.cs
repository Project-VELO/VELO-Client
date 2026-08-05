using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스토리 목록 화면입니다(기획서 SCREEN-002).
///
/// 카드를 한 번 누르면 선택, 선택된 카드를 다시 누르면 감상 화면으로 넘어갑니다(5.3).
///
/// 진행 상태를 바꾸지 않습니다. NEW 해제와 완료 처리는 감상 화면의 몫이고 여기서는 읽기만 합니다.
/// 화면이 상태를 직접 바꾸면 저장이 함께 일어나지 않습니다.
/// </summary>
public class UI_SelectStory : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("Buttons")]
    [SerializeField]
    private Button _backButton;

    [Foldout("Hierarchy")]
    [Header("Panels")]
    [SerializeField]
    private UI_CurrencyHud _currencyHud;

    [SerializeField]
    private UI_SelectStoryList _list;

    [SerializeField]
    private UI_SelectStoryScroller _scroller;

    [Foldout("Hierarchy")]
    [Header("Popups")]
    [SerializeField]
    private UI_LockedStoryPopup _lockedPopup;

    [SerializeField]
    private UI_StoryRewardPopup _rewardPopup;

    private string _selectedStoryId;

    private void Start()
    {
        _currencyHud.Refresh();

        _list.OnEpisodeClicked = SelectEpisode;
        _list.Build();

        _backButton.onClick.AddListener(MoveToHome);

        EnterScreenAsync().Forget();
    }

    // 씬이 내려갈 때 풀로 되돌리는 처리는 두지 않습니다.
    // PoolRegistrar가 OnDestroy에서 UnregisterPool을 부르고, 그 안의 Pool.Clear가 아직 꺼내 간 오브젝트까지
    // 함께 파괴합니다. 화면이 따로 반환하면 두 OnDestroy의 실행 순서에 따라 이미 사라진 풀에 밀어 넣게 되어
    // "풀이 아직 없어 반환에 실패했습니다" 경고가 뜹니다.
    // 같은 씬 안에서 목록을 다시 그릴 때의 반환은 UI_SelectStoryList.Build가 앞머리에서 처리합니다.

    /// <summary>
    /// 목록을 다 채운 뒤에야 할 수 있는 것들입니다.
    /// 스크롤은 레이아웃이 확정된 다음에 계산할 수 있고, 보상 팝업은 목록이 보이는 상태에서 떠야 합니다.
    /// </summary>
    private async UniTaskVoid EnterScreenAsync()
    {
        await ApplyHighlightAsync(this.GetCancellationTokenOnDestroy());
        TryOpenRewardPopup();
    }

    /// <summary>
    /// 스케줄 바로가기로 들어왔으면 대상 회차까지 스크롤하고 강조합니다(기획서 3-F-3-1).
    /// 홈에서 들어왔으면 최상단부터 보여 줍니다.
    ///
    /// 강조는 선택이 아닙니다. 강조된 카드도 진입하려면 여전히 두 번 눌러야 합니다(5.3).
    /// </summary>
    private async UniTask ApplyHighlightAsync(CancellationToken cancellationToken)
    {
        string highlightStoryId = StoryEntryContext.Instance.HighlightStoryId;
        UI_SelectStoryEpisodeItem target = _list.Find(highlightStoryId);

        if (target == null)
        {
            if (!string.IsNullOrEmpty(highlightStoryId))
            {
                Debug.LogWarning($"[UI_SelectStory] 강조 대상을 목록에서 찾지 못했습니다: {highlightStoryId}");
                StoryEntryContext.Instance.ClearHighlight();
            }

            _scroller.SetTop();
            return;
        }

        target.SetHighlight(true);
        await _scroller.ScrollToAsync(target.transform as RectTransform, cancellationToken);
    }

    /// <summary>
    /// 감상 화면에서 최초 완료하고 돌아온 경우에만 보상을 안내합니다(기획서 3-J-4).
    /// 재감상은 보상이 없으므로 팝업도 뜨지 않습니다.
    /// </summary>
    private void TryOpenRewardPopup()
    {
        StoryEntryContext context = StoryEntryContext.Instance;

        if (!context.HasPendingReward)
        {
            return;
        }

        // 팝업을 못 띄우더라도 소비는 합니다. 남겨 두면 목록에 들어올 때마다 같은 시도를 반복합니다.
        bool isReady = _rewardPopup.SetReward(context.PendingRewardId);
        context.ConsumePendingReward();

        if (isReady)
        {
            UIManager.Instance.PopupHandler.OpenPopup(_rewardPopup);
        }
    }

    /// <summary>
    /// 카드 클릭 처리입니다(기획서 5.3, 5.4).
    /// 잠긴 회차는 선택 상태로도 만들지 않습니다. 안내 팝업만 띄우고 끝냅니다.
    /// </summary>
    private void SelectEpisode(string storyId)
    {
        IStoryProgress progress = GameProgressService.Instance.GetStoryProgress(storyId);

        if (ReferenceEquals(progress, null) || !progress.CanEnter)
        {
            UIManager.Instance.PopupHandler.OpenPopup(_lockedPopup);
            return;
        }

        if (_selectedStoryId == storyId)
        {
            EnterStory(storyId);
            return;
        }

        _selectedStoryId = storyId;
        _list.SetSelection(storyId);
    }

    private void EnterStory(string storyId)
    {
        StoryEntryContext.Instance.SetEntry(storyId);

        // 강조는 이번 진입에서 소임을 다했습니다. 지우지 않으면 감상을 마치고 돌아왔을 때 또 강조됩니다.
        StoryEntryContext.Instance.ClearHighlight();

        LoadScene(ESceneNames.StoryScene);
    }

    /// <summary>
    /// 뒤로가기는 진입 경로와 무관하게 항상 홈입니다(기획서 5.4).
    /// </summary>
    private void MoveToHome()
    {
        StoryEntryContext.Instance.ClearHighlight();
        LoadScene(ESceneNames.HomeScene);
    }

    private void LoadScene(ESceneNames sceneName)
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneAsync(sceneName, this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}
