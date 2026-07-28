using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// StreamingAssets/Charts 아래의 채보(ChartData) 파일 입출력과 삭제를 전담하는 클래스입니다.
/// 저장 직전 LiveEditorChartValidator로 유효성 검사를 수행하며, 검사에 실패하면 저장하지 않습니다.
/// 곡 메타데이터는 LiveEditorSongIO가 다룹니다.
/// </summary>
public class LiveEditorChartIO
{
    private readonly LiveEditorChartValidator _validator = new LiveEditorChartValidator();

    public string GetChartPath(string songId, EDifficulty difficulty)
    {
        return Path.Combine(Application.streamingAssetsPath, "Charts", $"{songId}_{difficulty}.json");
    }

    public bool SaveChart(string path, ChartData chart, SongData song, out List<string> errors)
    {
        errors = _validator.Validate(chart, song);
        if (errors.Count > 0)
        {
            return false;
        }

        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonUtility.ToJson(chart, true);
        File.WriteAllText(path, json);
        return true;
    }

    public ChartData LoadChart(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<ChartData>(json);
    }

    /// <summary>
    /// 채보 파일을 삭제합니다. Unity가 함께 만들어 둔 메타 파일이 남으면 다음 리프레시에서 경고가 뜨므로 같이 지웁니다.
    /// </summary>
    public bool DeleteChart(string songId, EDifficulty difficulty)
    {
        string path = GetChartPath(songId, difficulty);
        if (!File.Exists(path))
        {
            return false;
        }

        File.Delete(path);

        string metaPath = path + ".meta";
        if (File.Exists(metaPath))
        {
            File.Delete(metaPath);
        }

        return true;
    }

    public bool HasChart(string songId, EDifficulty difficulty)
    {
        return File.Exists(GetChartPath(songId, difficulty));
    }

    /// <summary>
    /// 해당 곡에 대해 이미 저장된 채보가 존재하는 난이도만 추려서 돌려줍니다.
    /// </summary>
    public List<EDifficulty> GetSavedDifficulties(string songId)
    {
        var savedDifficulties = new List<EDifficulty>();

        foreach (EDifficulty difficulty in Enum.GetValues(typeof(EDifficulty)))
        {
            if (HasChart(songId, difficulty))
            {
                savedDifficulties.Add(difficulty);
            }
        }

        return savedDifficulties;
    }

    /// <summary>
    /// 주어진 곡들 중 저장된 채보가 하나 이상 있는 곡만 추려서 돌려줍니다. 채보 불러오기 목록에 사용됩니다.
    /// </summary>
    public List<string> GetSongIdsWithCharts(List<string> songIds)
    {
        var songIdsWithCharts = new List<string>();

        foreach (string songId in songIds)
        {
            if (GetSavedDifficulties(songId).Count > 0)
            {
                songIdsWithCharts.Add(songId);
            }
        }

        return songIdsWithCharts;
    }
}
