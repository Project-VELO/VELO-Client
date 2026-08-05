using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// LIVE 곡·채보 파일의 경로를 한곳에서 만들어 주는 정적 헬퍼입니다.
/// 제작 중인 파일을 두는 "작업 공간"(Songs/, Charts/)과 곡 선택 화면에 노출할 완성본을 두는
/// "수록 공간"(LiveSongs/)을 구분하며, 수록 공간에서는 챕터를 폴더로 구분합니다.
/// </summary>
public static class LiveSongPaths
{
    public const string SONG_INFO_FILE_NAME = "song_info.json";
    public const string CHAPTER_INFO_FILE_NAME = "chapter_info.json";
    public const string COVER_FILE_NAME = "cover.png";

    private const string WORKING_SONGS_FOLDER = "Songs";
    private const string WORKING_CHARTS_FOLDER = "Charts";
    private const string PUBLISHED_FOLDER = "LiveSongs";

    // 챕터 폴더명은 "00_PROLOGUE"처럼 표시 순서를 접두사로 갖습니다.
    // 접두사가 없으면 폴더명 정렬에 끌려가 PROLOGUE가 CHAPTER01 뒤로 밀리므로, 순서를 폴더명에 새겨 둡니다.
    private const char CHAPTER_ORDER_SEPARATOR = '_';

    public static string WorkingSongsRoot => Path.Combine(Application.streamingAssetsPath, WORKING_SONGS_FOLDER);
    public static string WorkingChartsRoot => Path.Combine(Application.streamingAssetsPath, WORKING_CHARTS_FOLDER);
    public static string PublishedRoot => Path.Combine(Application.streamingAssetsPath, PUBLISHED_FOLDER);

    public static string GetChartFileName(string songId, EDifficulty difficulty)
    {
        return $"{songId}_{difficulty}.json";
    }

    #region 작업 공간 (채보 에디터 전용)
    public static string GetWorkingSongFolder(string songId)
    {
        return Path.Combine(WorkingSongsRoot, songId);
    }

    public static string GetWorkingSongInfoPath(string songId)
    {
        return Path.Combine(GetWorkingSongFolder(songId), SONG_INFO_FILE_NAME);
    }

    public static string GetWorkingAudioPath(string songId, string audioFileName)
    {
        return Path.Combine(GetWorkingSongFolder(songId), audioFileName);
    }

    public static string GetWorkingChartPath(string songId, EDifficulty difficulty)
    {
        return Path.Combine(WorkingChartsRoot, GetChartFileName(songId, difficulty));
    }
    #endregion

    #region 수록 공간 (곡 선택 화면 전용)
    public static string GetPublishedChapterFolder(int order, string chapterId)
    {
        return Path.Combine(PublishedRoot, $"{order:D2}{CHAPTER_ORDER_SEPARATOR}{chapterId}");
    }

    public static string GetPublishedChapterInfoPath(string chapterFolder)
    {
        return Path.Combine(chapterFolder, CHAPTER_INFO_FILE_NAME);
    }

    public static string GetPublishedSongFolder(string chapterFolder, string songId)
    {
        return Path.Combine(chapterFolder, songId);
    }

    public static string GetPublishedSongInfoPath(string songFolder)
    {
        return Path.Combine(songFolder, SONG_INFO_FILE_NAME);
    }

    public static string GetPublishedAudioPath(string songFolder, string audioFileName)
    {
        return Path.Combine(songFolder, audioFileName);
    }

    public static string GetPublishedCoverPath(string songFolder)
    {
        return Path.Combine(songFolder, COVER_FILE_NAME);
    }

    public static string GetPublishedChartPath(string songFolder, string songId, EDifficulty difficulty)
    {
        return Path.Combine(songFolder, GetChartFileName(songId, difficulty));
    }

    /// <summary>
    /// 수록 공간의 챕터 폴더를 표시 순서대로 돌려줍니다.
    /// Directory.GetDirectories의 반환 순서는 보장되지 않으므로 폴더명으로 직접 정렬합니다.
    /// </summary>
    public static List<string> GetPublishedChapterFolders()
    {
        var chapterFolders = new List<string>();

        if (!Directory.Exists(PublishedRoot))
        {
            return chapterFolders;
        }

        chapterFolders.AddRange(Directory.GetDirectories(PublishedRoot));
        chapterFolders.Sort((left, right) => string.CompareOrdinal(Path.GetFileName(left), Path.GetFileName(right)));

        return chapterFolders;
    }

    public static List<string> GetPublishedSongFolders(string chapterFolder)
    {
        var songFolders = new List<string>();

        if (!Directory.Exists(chapterFolder))
        {
            return songFolders;
        }

        songFolders.AddRange(Directory.GetDirectories(chapterFolder));
        songFolders.Sort((left, right) => string.CompareOrdinal(Path.GetFileName(left), Path.GetFileName(right)));

        return songFolders;
    }

    /// <summary>
    /// "00_PROLOGUE" 형태의 폴더명을 표시 순서와 챕터 ID로 나눕니다.
    /// 접두사가 없거나 숫자가 아니면 폴더명 전체를 챕터 ID로 보고 표시 순서를 맨 뒤로 미룹니다.
    /// </summary>
    public static bool TryParseChapterFolderName(string folderName, out int order, out string chapterId)
    {
        order = int.MaxValue;
        chapterId = folderName;

        if (string.IsNullOrEmpty(folderName))
        {
            return false;
        }

        int separatorIndex = folderName.IndexOf(CHAPTER_ORDER_SEPARATOR);
        if (separatorIndex <= 0 || folderName.Length - 1 <= separatorIndex)
        {
            return true;
        }

        if (int.TryParse(folderName.Substring(0, separatorIndex), out int parsedOrder))
        {
            order = parsedOrder;
            chapterId = folderName.Substring(separatorIndex + 1);
        }

        return true;
    }
    #endregion
}
