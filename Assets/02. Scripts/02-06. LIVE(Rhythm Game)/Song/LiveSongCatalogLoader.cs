using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 수록 공간(StreamingAssets/LiveSongs)을 훑어 챕터와 곡 메타데이터를 읽어들이는 것을 전담합니다.
/// 디스크 접근과 JSON 파싱만 담당하고, 읽어온 결과의 보관과 조회는 LiveSongCatalog가 맡습니다.
/// </summary>
public class LiveSongCatalogLoader
{
    /// <summary>
    /// 수록된 모든 챕터를 표시 순서대로 읽어옵니다. 곡이 하나도 없는 챕터 폴더는 탭을 만들 이유가 없어 제외합니다.
    /// </summary>
    public List<LiveChapterData> LoadChapters()
    {
        var chapters = new List<LiveChapterData>();
        List<string> chapterFolders = LiveSongPaths.GetPublishedChapterFolders();

        foreach (string chapterFolder in chapterFolders)
        {
            LiveChapterData chapter = LoadChapter(chapterFolder);
            if (0 < chapter.Songs.Count)
            {
                chapters.Add(chapter);
            }
        }

        chapters.Sort(CompareChapterOrder);
        return chapters;
    }

    private LiveChapterData LoadChapter(string chapterFolder)
    {
        string folderName = Path.GetFileName(chapterFolder);
        LiveSongPaths.TryParseChapterFolderName(folderName, out int order, out string chapterId);

        var chapter = new LiveChapterData
        {
            Order = order,
            ChapterId = chapterId,
            DisplayName = chapterId,
            UnlockStoryId = string.Empty,
            FolderPath = chapterFolder,
        };

        ApplyChapterInfo(chapter);
        LoadSongs(chapter);

        return chapter;
    }

    /// <summary>
    /// chapter_info.json은 선택 파일이므로, 없거나 값이 비어 있으면 폴더명 기반 기본값을 그대로 둡니다.
    /// </summary>
    private void ApplyChapterInfo(LiveChapterData chapter)
    {
        string infoPath = LiveSongPaths.GetPublishedChapterInfoPath(chapter.FolderPath);
        if (!File.Exists(infoPath))
        {
            return;
        }

        ChapterInfo info = JsonUtility.FromJson<ChapterInfo>(File.ReadAllText(infoPath));
        if (ReferenceEquals(info, null))
        {
            Debug.LogWarning($"[LiveSongCatalogLoader] 챕터 정보 파싱에 실패했습니다: {infoPath}");
            return;
        }

        if (!string.IsNullOrEmpty(info.DisplayName))
        {
            chapter.DisplayName = info.DisplayName;
        }

        if (!string.IsNullOrEmpty(info.UnlockStoryId))
        {
            chapter.UnlockStoryId = info.UnlockStoryId;
        }
    }

    private void LoadSongs(LiveChapterData chapter)
    {
        List<string> songFolders = LiveSongPaths.GetPublishedSongFolders(chapter.FolderPath);

        foreach (string songFolder in songFolders)
        {
            SongData song = LoadSong(songFolder, chapter.ChapterId);
            if (!ReferenceEquals(song, null))
            {
                chapter.AddSong(song);
            }
        }
    }

    private SongData LoadSong(string songFolder, string chapterId)
    {
        string songInfoPath = LiveSongPaths.GetPublishedSongInfoPath(songFolder);
        if (!File.Exists(songInfoPath))
        {
            Debug.LogWarning($"[LiveSongCatalogLoader] {LiveSongPaths.SONG_INFO_FILE_NAME}이 없어 곡을 건너뜁니다: {songFolder}");
            return null;
        }

        SongData song = JsonUtility.FromJson<SongData>(File.ReadAllText(songInfoPath));
        if (ReferenceEquals(song, null) || string.IsNullOrEmpty(song.SongId))
        {
            Debug.LogWarning($"[LiveSongCatalogLoader] 곡 정보 파싱에 실패했습니다: {songInfoPath}");
            return null;
        }

        song.ChapterId = chapterId;
        song.FolderPath = songFolder;

        return song;
    }

    private int CompareChapterOrder(LiveChapterData left, LiveChapterData right)
    {
        int orderComparison = left.Order.CompareTo(right.Order);
        return orderComparison != 0 ? orderComparison : string.CompareOrdinal(left.ChapterId, right.ChapterId);
    }
}
