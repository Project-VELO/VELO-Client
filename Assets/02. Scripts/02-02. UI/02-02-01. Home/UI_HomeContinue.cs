using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 이어하기 버튼(H02)입니다. 카드에 적힌 회차를 목록을 거치지 않고 처음부터 엽니다(개발 기획서 1장 이어하기 공통 규칙).
///
/// 버튼 글은 개발 기획서 1장 기준안을 따릅니다: 완료한 회차가 없으면 "시작하기", 전부 완료했으면 "다시보기",
/// 그 밖에는 시안 글 "이어하기". 들어갔다가 끝내지 않은 회차면 "이 회차의 처음부터 시작합니다"를 확인받고 들어갑니다.
///
/// 들어갈 회차는 스토리 카드의 NextStory를 그대로 씁니다. 따로 계산하면 카드에 적힌 회차와 어긋날 수 있습니다.
/// </summary>
public class UI_HomeContinue : MonoBehaviour
{
    /// <summary>
    /// 이어갈 회차가 없을 때 회차 목록으로 보내 달라고 알립니다. 목록 이동(H03)은 홈 화면이 맡습니다.
    /// </summary>
    public Action OnStoryListRequested;

    private const string START_LABEL = "시작하기";
    private const string CONTINUE_LABEL = "이어하기";
    private const string REPLAY_LABEL = "다시보기";
    private const string RESTART_NOTICE = "이 회차의 처음부터 시작합니다";

    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_HomeStoryBox _storyBox;

    [SerializeField]
    private Button _continueButton;

    [SerializeField]
    private TMP_Text _labelText;

    /// <summary>
    /// 씬에 둔 안내 팝업을 씬의 홈 인스턴스에서 연결합니다(UI_Home의 종료 팝업과 같은 방식).
    /// </summary>
    [SerializeField]
    private UI_HomeNoticePopup _noticePopup;

    /// <summary>
    /// 전부 완료했을 때의 안내 문구입니다(기획자 입력). 비어 있으면 안내 없이 바로 회차 목록으로 갑니다.
    /// </summary>
    [EndFoldout]
    [Header("Planner Input")]
    [SerializeField]
    private string _allCompletedNotice = string.Empty;

    private void Awake()
    {
        _continueButton.onClick.AddListener(ContinueStory);
    }

    public void Refresh()
    {
        _labelText.text = UiText.Localize(GetLabel());
    }

    private static string GetLabel()
    {
        List<StoryData> stories = MasterDataQuery.GetAllStoriesInDisplayOrder();
        int completedCount = 0;

        for (int i = 0; i < stories.Count; i++)
        {
            IStoryProgress progress = GameProgressService.Instance.GetStoryProgress(stories[i].StoryId);

            if (!ReferenceEquals(progress, null) && progress.IsCompleted)
            {
                completedCount++;
            }
        }

        if (completedCount == 0)
        {
            return START_LABEL;
        }

        return completedCount == stories.Count ? REPLAY_LABEL : CONTINUE_LABEL;
    }

    /// <summary>
    /// 잠겨 있으면 막힌 이유를 볼 수 있는 회차 목록으로, 전부 완료했으면 다시볼 회차를 고르는 회차 목록으로 보냅니다.
    /// 안내가 떠 있는 동안 다시 눌러도 안내를 겹쳐 열지 않습니다.
    /// </summary>
    private void ContinueStory()
    {
        if (_noticePopup.gameObject.activeSelf)
        {
            return;
        }

        StoryData story = _storyBox.NextStory;
        IStoryProgress progress = ReferenceEquals(story, null) ? null : GameProgressService.Instance.GetStoryProgress(story.StoryId);

        if (ReferenceEquals(progress, null) || !progress.CanEnter)
        {
            OnStoryListRequested?.Invoke();
            return;
        }

        if (progress.IsCompleted)
        {
            ShowAllCompleted();
            return;
        }

        // 카드의 "진행 중" 배지와 같은 판정입니다(UI_HomeStoryBox). 들어간 기록이 있고 끝내지 않은 회차입니다.
        if (progress.IsInProgress)
        {
            ShowNotice(RESTART_NOTICE, () => EnterStory(story));
            return;
        }

        EnterStory(story);
    }

    private void ShowAllCompleted()
    {
        if (string.IsNullOrEmpty(_allCompletedNotice))
        {
            OnStoryListRequested?.Invoke();
            return;
        }

        ShowNotice(_allCompletedNotice, () => OnStoryListRequested?.Invoke());
    }

    /// <summary>
    /// 팝업 스택은 PersistentScene의 UIManager가 들고 있으므로, 이 화면만 단독으로 열면 없을 수 있습니다.
    /// 그때는 안내를 건너뛰고 바로 잇습니다.
    /// </summary>
    private void ShowNotice(string message, Action onConfirmed)
    {
        if (UIManager.Instance == null)
        {
            onConfirmed();
            return;
        }

        _noticePopup.SetNotice(message, onConfirmed);
        UIManager.Instance.OpenPopup(_noticePopup);
    }

    private void EnterStory(StoryData story)
    {
        StoryEntryContext.Instance.SetEntry(story.StoryId);
        StoryEntryContext.Instance.ClearHighlight();

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneAsync(ESceneNames.StoryScene, this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}
