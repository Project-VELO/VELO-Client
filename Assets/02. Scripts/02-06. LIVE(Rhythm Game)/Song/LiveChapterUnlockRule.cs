/// <summary>
/// 챕터 탭의 해금 여부를 판정합니다.
/// 챕터 탭은 스토리 챕터 오픈과 연동되므로, chapter_info.json에 적힌 스토리의 상태를 기준으로 삼습니다.
/// </summary>
public static class LiveChapterUnlockRule
{
    public static bool IsUnlocked(LiveChapterData chapter)
    {
        if (ReferenceEquals(chapter, null))
        {
            return false;
        }

        if (!chapter.HasUnlockCondition)
        {
            return true;
        }

        // 스토리 진행 데이터를 채우는 계층이 아직 없어, 기록이 없는 스토리는 잠긴 것으로 단정하지 않습니다.
        // 그렇지 않으면 해금 조건을 적어 둔 챕터가 영구히 잠긴 것처럼 보입니다.
        if (!PlayerDataProvider.Instance.Data.StoryProgresses.TryGetValue(chapter.UnlockStoryId, out EStoryStatus status))
        {
            return true;
        }

        return status != EStoryStatus.LOCKED;
    }
}
