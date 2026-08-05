using UnityEngine;

/// <summary>
/// 진입 컨텍스트와 수록 목록을 대조해, 곡 선택 화면이 처음 열어야 할 챕터와 곡을 정합니다.
/// 지정곡이 있으면 그 곡이 속한 챕터를 열고, 없으면 해금된 첫 챕터의 첫 곡을 자동 선택합니다.
/// </summary>
public static class LiveSongSelectionResolver
{
    public static bool TryResolve(LiveSongCatalog catalog, LiveEntryContext context, out int chapterIndex, out int songIndex)
    {
        chapterIndex = -1;
        songIndex = 0;

        if (context.HasDesignatedSong)
        {
            if (TryResolveDesignated(catalog, context.DesignatedSongId, out chapterIndex, out songIndex))
            {
                return true;
            }

            // 지정 상태를 남겨 두면 곡 목록이 잠긴 채로 열려 자유 선택으로 대체한 의미가 없어집니다.
            context.ClearDesignatedSong();
        }

        chapterIndex = FindFirstUnlockedChapterIndex(catalog);
        songIndex = 0;

        return 0 <= chapterIndex;
    }

    private static bool TryResolveDesignated(LiveSongCatalog catalog, string designatedSongId, out int chapterIndex, out int songIndex)
    {
        chapterIndex = catalog.FindChapterIndex(designatedSongId);
        songIndex = catalog.FindSongIndex(chapterIndex, designatedSongId);

        if (0 <= chapterIndex && 0 <= songIndex)
        {
            return true;
        }

        Debug.LogWarning($"[LiveSongSelectionResolver] 지정곡을 수록 목록에서 찾지 못해 자유 선택으로 대체합니다: {designatedSongId}");

        chapterIndex = -1;
        songIndex = 0;

        return false;
    }

    private static int FindFirstUnlockedChapterIndex(LiveSongCatalog catalog)
    {
        for (int i = 0; i < catalog.Chapters.Count; i++)
        {
            if (LiveChapterUnlockRule.IsUnlocked(catalog.Chapters[i]))
            {
                return i;
            }
        }

        return -1;
    }
}
