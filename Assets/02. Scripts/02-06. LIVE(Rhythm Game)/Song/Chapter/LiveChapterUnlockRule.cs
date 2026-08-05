using UnityEngine;

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

        IStoryProgress progress = GameProgressService.Instance.GetStoryProgress(chapter.UnlockStoryId);

        // 신규 게임을 만들 때와 세이브를 불러올 때 stories.json의 모든 스토리에 진행 상태를 채우므로,
        // 기록이 없다는 것은 chapter_info.json이 존재하지 않는 스토리를 가리킨다는 뜻입니다.
        // 조용히 열어 두면 오타를 발견하지 못하므로 잠근 채로 알립니다.
        if (ReferenceEquals(progress, null))
        {
            Debug.LogWarning($"[LiveChapterUnlockRule] 해금 조건 스토리를 찾을 수 없어 챕터를 잠급니다: {chapter.UnlockStoryId}");
            return false;
        }

        return progress.CanEnter;
    }
}
