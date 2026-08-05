using System.Collections.Generic;

/// <summary>
/// 스토리의 해금·완료·NEW 표시 상태를 다룹니다(기획서 3-F).
///
/// 상태 변경만 수행하고 저장은 하지 않습니다. 저장 시점은 GameProgressService가 관리합니다.
/// </summary>
public static class StoryProgressService
{
    /// <summary>
    /// 마스터 데이터의 스토리 가운데 진행 기록이 없는 것을 채웁니다(기획서 3-C-3).
    /// 해금 조건이 없는 스토리는 열린 상태로, 조건이 걸린 스토리는 잠긴 상태로 시작합니다.
    /// 무언가 채워졌으면 true를 돌려주므로, 호출부는 이때만 저장하면 됩니다.
    ///
    /// 기존 기록은 건드리지 않습니다. 신규 게임뿐 아니라 세이브를 불러온 뒤에도 호출하기 때문입니다.
    /// 기록이 비어 있는 세이브를 그대로 두면 해금 조건이 걸린 챕터가 전부 잠겨 곡 선택 화면에서 진행할 수 없고,
    /// stories.json에 스토리를 추가한 뒤 기존 세이브를 열면 그 스토리가 목록에서 사라집니다.
    /// </summary>
    public static bool SyncStoryProgresses(GameProgressData progress)
    {
        bool isChanged = false;

        foreach (KeyValuePair<string, StoryData> pair in MasterDataProvider.Instance.Stories)
        {
            StoryData story = pair.Value;

            if (!ReferenceEquals(progress.GetStoryProgress(story.StoryId), null))
            {
                continue;
            }

            progress.StoryProgresses[story.StoryId] = new StoryProgress
            {
                Status = story.UnlockType == EUnlockType.NONE ? EStoryStatus.UNLOCKED : EStoryStatus.LOCKED,
                IsNew = false,
            };

            isChanged = true;
        }

        // 이미 마무리한 주차로 열려야 할 스토리를 뒤늦게 추가한 경우를 흡수합니다.
        // 위에서 잠긴 상태로 만들어 두기만 하면 그 주차를 다시 마무리할 방법이 없어 영영 잠깁니다.
        foreach (string weekId in progress.CompletedWeekIds)
        {
            if (0 < UnlockStoriesByWeek(progress, weekId).Count)
            {
                isChanged = true;
            }
        }

        return isChanged;
    }

    /// <summary>
    /// 스토리를 완료 처리합니다. 이번에 처음 완료한 경우에만 true를 돌려주며, 호출부는 이때만 감상 보상을 지급합니다.
    /// 완료한 스토리를 다시 감상해도 보상이 중복 지급되지 않도록 하는 관문입니다(기획서 3-J-4).
    /// </summary>
    public static bool TryCompleteStory(GameProgressData progress, string storyId)
    {
        StoryProgress storyProgress = GetOrCreate(progress, storyId);

        if (ReferenceEquals(storyProgress, null) || storyProgress.IsCompleted)
        {
            return false;
        }

        storyProgress.Status = EStoryStatus.COMPLETED;
        return true;
    }

    /// <summary>
    /// NEW 배지를 내립니다. 스토리 감상 화면에 진입하는 순간 호출합니다(기획서 3-F-3).
    /// </summary>
    public static bool TryClearNewFlag(GameProgressData progress, string storyId)
    {
        StoryProgress storyProgress = progress.GetStoryProgress(storyId);

        if (ReferenceEquals(storyProgress, null) || !storyProgress.IsNew)
        {
            return false;
        }

        storyProgress.IsNew = false;
        return true;
    }

    /// <summary>
    /// 해당 주차 완료로 열리는 스토리를 모두 해금하고, 실제로 열린 스토리 ID를 돌려줍니다(기획서 3-F-2).
    /// 이미 열려 있던 스토리는 건드리지 않으므로 NEW 배지가 다시 켜지지 않습니다.
    /// </summary>
    public static List<string> UnlockStoriesByWeek(GameProgressData progress, string weekId)
    {
        var unlockedStoryIds = new List<string>();

        if (string.IsNullOrEmpty(weekId))
        {
            return unlockedStoryIds;
        }

        foreach (KeyValuePair<string, StoryData> pair in MasterDataProvider.Instance.Stories)
        {
            StoryData story = pair.Value;

            if (story.UnlockType != EUnlockType.WEEK_CLEAR || story.UnlockTargetId != weekId)
            {
                continue;
            }

            StoryProgress storyProgress = GetOrCreate(progress, story.StoryId);

            if (storyProgress.Status != EStoryStatus.LOCKED)
            {
                continue;
            }

            storyProgress.Status = EStoryStatus.UNLOCKED;
            storyProgress.IsNew = true;
            unlockedStoryIds.Add(story.StoryId);
        }

        return unlockedStoryIds;
    }

    public static bool IsCompleted(GameProgressData progress, string storyId)
    {
        StoryProgress storyProgress = progress.GetStoryProgress(storyId);
        return !ReferenceEquals(storyProgress, null) && storyProgress.IsCompleted;
    }

    /// <summary>
    /// 진행 상태를 가져오되 없으면 만들어 둡니다.
    /// 신규 게임 이후에 스토리가 추가된 세이브에서도 조회가 실패하지 않도록 하기 위한 보정입니다.
    /// </summary>
    private static StoryProgress GetOrCreate(GameProgressData progress, string storyId)
    {
        if (string.IsNullOrEmpty(storyId))
        {
            return null;
        }

        StoryProgress storyProgress = progress.GetStoryProgress(storyId);

        if (ReferenceEquals(storyProgress, null))
        {
            storyProgress = new StoryProgress();
            progress.StoryProgresses[storyId] = storyProgress;
        }

        return storyProgress;
    }
}
