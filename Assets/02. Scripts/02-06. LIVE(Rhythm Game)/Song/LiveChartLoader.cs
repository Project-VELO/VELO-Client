using System.IO;
using UnityEngine;

/// <summary>
/// 채보 JSON 파일을 ChartData로 읽어 오는 단일 창구입니다.
/// 채보 에디터, 곡 선택 화면, 리듬게임이 같은 파싱과 방어 코드를 각자 반복하지 않도록 여기로 모았습니다.
/// 노트 목록이 없는 파일은 열 수 있어도 플레이할 수 없으므로, 파싱 실패와 동일하게 null로 답합니다.
/// </summary>
public static class LiveChartLoader
{
    public static ChartData Load(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return null;
        }

        ChartData chart = JsonUtility.FromJson<ChartData>(File.ReadAllText(path));

        if (ReferenceEquals(chart, null) || ReferenceEquals(chart.Notes, null))
        {
            Debug.LogWarning($"[LiveChartLoader] 채보 파싱에 실패했습니다: {path}");
            return null;
        }

        return chart;
    }

    /// <summary>
    /// 수록 공간(LiveSongs)에 놓인 채보를 읽습니다. 곡 선택 화면과 리듬게임이 사용합니다.
    /// </summary>
    public static ChartData LoadPublished(SongData song, EDifficulty difficulty)
    {
        if (ReferenceEquals(song, null))
        {
            return null;
        }

        return Load(LiveSongPaths.GetPublishedChartPath(song.FolderPath, song.SongId, difficulty));
    }
}
