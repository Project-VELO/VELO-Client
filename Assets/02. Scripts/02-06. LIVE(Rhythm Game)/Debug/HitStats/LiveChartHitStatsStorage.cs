using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 플레이테스트 판정 통계 파일의 경로와 읽기·쓰기·삭제를 맡습니다. 채보 하나당 파일 하나이며,
/// 이름은 채보 파일과 같은 {songId}_{difficulty}.json입니다.
///
/// StreamingAssets는 빌드에서 쓸 수 없으므로 세이브와 같이 persistentDataPath 아래에 둡니다.
/// Windows에서는 에디터와 PC 개발 빌드가 같은 경로를 쓰므로, 개발 빌드로 친 기록도 에디터 조회 창에서 그대로 보입니다.
/// </summary>
public static class LiveChartHitStatsStorage
{
    private const string FOLDER_NAME = "PlaytestStats";
    private const string FILE_EXTENSION = ".json";

    // 확장자 뒤에 붙여 "*.json" 목록에 임시·백업 파일이 섞이지 않게 합니다.
    private const string TEMP_SUFFIX = ".tmp";
    private const string BACKUP_SUFFIX = ".bak";

    public static string RootPath => Path.Combine(Application.persistentDataPath, FOLDER_NAME);

    public static string GetFilePath(string songId, EDifficulty difficulty)
    {
        return Path.Combine(RootPath, $"{songId}_{difficulty}{FILE_EXTENSION}");
    }

    /// <summary>
    /// 해당 채보의 통계를 불러옵니다. 정식 파일을 쓸 수 없으면 직전 저장본을 시도하고, 둘 다 없으면 null을 돌려줍니다.
    /// </summary>
    public static LiveChartHitStats Load(string songId, EDifficulty difficulty)
    {
        string path = GetFilePath(songId, difficulty);

        if (TryReadChart(path, songId, difficulty, out LiveChartHitStats stats))
        {
            return stats;
        }

        if (TryReadChart(path + BACKUP_SUFFIX, songId, difficulty, out LiveChartHitStats backup))
        {
            Debug.LogWarning($"[LiveChartHitStatsStorage] 판정 통계를 읽지 못해 직전 저장본으로 복구했습니다: {path}");
            return backup;
        }

        return null;
    }

    /// <summary>
    /// 경로로 통계 파일을 직접 읽습니다. 조회 창처럼 곡과 난이도를 모른 채 목록에서 고른 파일을 열 때 씁니다.
    /// </summary>
    public static bool TryRead(string path, out LiveChartHitStats stats)
    {
        stats = null;

        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            stats = JsonUtility.FromJson<LiveChartHitStats>(File.ReadAllText(path));
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[LiveChartHitStatsStorage] 판정 통계 파싱에 실패했습니다({path}): {exception.Message}");
            return false;
        }

        if (ReferenceEquals(stats, null) || stats.SchemaVersion <= 0 || ReferenceEquals(stats.Notes, null))
        {
            Debug.LogWarning($"[LiveChartHitStatsStorage] 판정 통계에 필수 데이터가 없습니다: {path}");
            stats = null;
            return false;
        }

        return true;
    }

    public static bool Save(LiveChartHitStats stats)
    {
        stats.SetSchemaVersion(LiveChartHitStats.CURRENT_SCHEMA_VERSION);

        string path = GetFilePath(stats.SongId, stats.Difficulty);
        string json = JsonUtility.ToJson(stats, true);

        if (AtomicFileWriter.TryWrite(path + TEMP_SUFFIX, path, path + BACKUP_SUFFIX, json, out string error))
        {
            return true;
        }

        Debug.LogWarning($"[LiveChartHitStatsStorage] 판정 통계를 저장하지 못했습니다({path}): {error}");
        return false;
    }

    /// <summary>
    /// 통계 파일과 함께 남은 임시·백업 파일까지 지웁니다. 백업이 남으면 다음 로드에서 지운 기록이 되살아납니다.
    /// </summary>
    public static void Delete(string path)
    {
        DeleteIfExists(path);
        DeleteIfExists(path + TEMP_SUFFIX);
        DeleteIfExists(path + BACKUP_SUFFIX);
    }

    public static List<string> GetAllFilePaths()
    {
        List<string> paths = new List<string>();

        if (!Directory.Exists(RootPath))
        {
            return paths;
        }

        foreach (string path in Directory.GetFiles(RootPath))
        {
            if (path.EndsWith(FILE_EXTENSION, StringComparison.OrdinalIgnoreCase))
            {
                paths.Add(path);
            }
        }

        paths.Sort(StringComparer.OrdinalIgnoreCase);
        return paths;
    }

    /// <summary>
    /// 파일 이름만 믿지 않고 내용 속 곡과 난이도까지 확인합니다. 파일을 손으로 복사하거나 이름을 바꿨을 때
    /// 다른 채보의 통계를 불러와 이 채보의 이름으로 덮어쓰지 않기 위해서입니다.
    /// </summary>
    private static bool TryReadChart(string path, string songId, EDifficulty difficulty, out LiveChartHitStats stats)
    {
        if (!TryRead(path, out stats))
        {
            return false;
        }

        if (stats.SongId == songId && stats.Difficulty == difficulty)
        {
            return true;
        }

        Debug.LogWarning($"[LiveChartHitStatsStorage] 판정 통계의 곡·난이도가 파일과 다릅니다({path}): {stats.SongId}/{stats.Difficulty}");
        stats = null;
        return false;
    }

    private static void DeleteIfExists(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[LiveChartHitStatsStorage] 판정 통계 삭제에 실패했습니다({path}): {exception.Message}");
        }
    }
}
