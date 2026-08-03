/// <summary>
/// 스토리 관련 화면 사이에서 값을 옮기는 싱글톤입니다.
/// LiveEntryContext와 같은 형태로, MonoBehaviour가 아니어서 서브씬이 언로드되어도 값이 남고
/// PersistentScene에 오브젝트를 두지 않아도 됩니다.
///
/// 세 방향의 전달을 담당합니다.
///   ① 홈·사무실의 스토리 스케줄 바로가기 → 스토리 목록 (강조할 회차)
///   ② 스토리 목록 → 스토리 감상 (감상할 회차)
///   ③ 스토리 감상 → 스토리 목록 (완료 결과와 보상)
///
/// ③이 필요한 것은 보상 안내 팝업이 감상 화면이 아니라 목록 화면에서 뜨기 때문입니다
/// (기획서.txt:1372, 1381, 2711).
/// </summary>
public class StoryEntryContext : POCOSingleton<StoryEntryContext>
{
    #region ① 바로가기 → 스토리 목록
    /// <summary>
    /// 스케줄 바로가기로 들어왔을 때 목록에서 자동 스크롤하고 강조할 회차입니다.
    /// 비어 있으면 강조 없이 평소대로 엽니다.
    /// </summary>
    public string HighlightStoryId { get; private set; }

    /// <summary>
    /// 스토리 목록 화면으로 이동하기 직전에 호출합니다.
    /// </summary>
    public void SetShortcutEntry(string storyId)
    {
        HighlightStoryId = storyId;
    }

    /// <summary>
    /// 목록 화면이 강조를 적용한 뒤 호출합니다.
    /// 지우지 않으면 다음에 홈에서 스토리 버튼으로 들어와도 지난 회차가 계속 강조됩니다.
    /// </summary>
    public void ClearHighlight()
    {
        HighlightStoryId = null;
    }
    #endregion

    #region ② 스토리 목록 → 스토리 감상
    /// <summary>
    /// 감상 화면이 대본을 읽을 대상입니다.
    /// </summary>
    public string SelectedStoryId { get; private set; }

    /// <summary>
    /// 목록에서 회차를 확정해 감상 화면으로 넘어가기 직전에 호출합니다.
    /// </summary>
    public void SetEntry(string storyId)
    {
        SelectedStoryId = storyId;
    }
    #endregion

    #region ③ 스토리 감상 → 스토리 목록
    /// <summary>
    /// 마지막까지 읽어 완료한 회차입니다. 중간에 나왔다면 비어 있습니다(기획서.txt:919-923).
    /// </summary>
    public string LastCompletedStoryId { get; private set; }

    /// <summary>
    /// 이번에 최초 완료해 보상이 지급되었는지 여부입니다.
    /// 재감상은 보상을 주지 않으므로 팝업도 띄우지 않습니다(기획서.txt:1375-1376, 2714).
    /// </summary>
    public bool HasPendingReward { get; private set; }

    public string PendingRewardId { get; private set; }

    /// <summary>
    /// 감상 화면이 완료 처리를 마친 뒤 호출합니다.
    /// isFirstCompletion은 GameProgressService.CompleteStory가 돌려준 값을 그대로 넘깁니다.
    /// </summary>
    public void SetCompletionResult(string storyId, bool isFirstCompletion, string rewardId)
    {
        LastCompletedStoryId = storyId;
        HasPendingReward = isFirstCompletion && !string.IsNullOrEmpty(rewardId);
        PendingRewardId = isFirstCompletion ? rewardId : null;
    }

    /// <summary>
    /// 목록 화면이 보상 팝업을 띄운 뒤 호출합니다.
    /// 지우지 않으면 목록에 들어올 때마다 같은 팝업이 반복해서 뜹니다.
    /// </summary>
    public void ConsumePendingReward()
    {
        HasPendingReward = false;
        PendingRewardId = null;
    }
    #endregion

    public void Clear()
    {
        HighlightStoryId = null;
        SelectedStoryId = null;
        LastCompletedStoryId = null;
        HasPendingReward = false;
        PendingRewardId = null;
    }
}
