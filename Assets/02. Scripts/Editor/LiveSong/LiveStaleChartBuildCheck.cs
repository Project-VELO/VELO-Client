using System.Collections.Generic;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 빌드 직전에 수록본 채보가 작업본과 같은지 확인하고, 다르면 빌드를 멈춥니다.
///
/// 채보를 고친 뒤 수록을 빠뜨리면 게임에는 옛 채보가 그대로 나가는데 화면 어디에도 그 사실이 드러나지 않습니다.
/// 실제로 11_59와 TILL_THE_END가 그렇게 갈라진 채 남아, 기획자가 찍은 노트의 절반 이상이 게임에서 사라져 있었습니다.
/// 저장 시 자동 갱신(LivePublishedChartSync)이 이 상황을 대부분 막지만, 파일을 손으로 옮긴 경우처럼
/// 자동 갱신이 닿지 않는 경로가 남아 마지막으로 여기서 봅니다.
///
/// 경고가 아니라 빌드 중단으로 둔 이유는, 이 문제 자체가 아무도 눈치채지 못한 채 지나가서 생겼기 때문입니다.
/// </summary>
public class LiveStaleChartBuildCheck : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        List<string> staleCharts = CollectStaleCharts();
        if (staleCharts.Count == 0)
        {
            return;
        }

        throw new BuildFailedException(
            "수록본 채보가 작업본과 다릅니다. VELO/Live/곡 수록(Publish)에서 아래 채보를 다시 수록한 뒤 빌드해 주세요.\n"
            + string.Join("\n", staleCharts));
    }

    private List<string> CollectStaleCharts()
    {
        var staleCharts = new List<string>();

        foreach (string chapterFolder in LiveSongPaths.GetPublishedChapterFolders())
        {
            foreach (string songFolder in LiveSongPaths.GetPublishedSongFolders(chapterFolder))
            {
                CollectStaleChartsOfSong(songFolder, staleCharts);
            }
        }

        return staleCharts;
    }

    private void CollectStaleChartsOfSong(string songFolder, List<string> staleCharts)
    {
        string songInfoPath = LiveSongPaths.GetPublishedSongInfoPath(songFolder);
        if (!File.Exists(songInfoPath))
        {
            return;
        }

        SongData song = JsonUtility.FromJson<SongData>(File.ReadAllText(songInfoPath));
        if (ReferenceEquals(song, null) || string.IsNullOrEmpty(song.SongId))
        {
            return;
        }

        foreach (KeyValuePair<EDifficulty, ChartMetadata> pair in song.Charts)
        {
            if (IsChartStale(songFolder, song.SongId, pair.Key))
            {
                staleCharts.Add($"- {song.SongId} / {pair.Key}");
            }
        }
    }

    /// <summary>
    /// 수록과 자동 갱신 모두 파일을 그대로 복사하므로, 맞춰진 한 쌍은 바이트까지 같습니다.
    /// 작업본이 없는 경우는 비교할 대상이 없는 것이므로 낡음으로 보지 않습니다.
    /// </summary>
    private bool IsChartStale(string songFolder, string songId, EDifficulty difficulty)
    {
        string publishedPath = LiveSongPaths.GetPublishedChartPath(songFolder, songId, difficulty);
        string workingPath = LiveSongPaths.GetWorkingChartPath(songId, difficulty);

        if (!File.Exists(publishedPath) || !File.Exists(workingPath))
        {
            return false;
        }

        byte[] published = File.ReadAllBytes(publishedPath);
        byte[] working = File.ReadAllBytes(workingPath);

        if (published.Length != working.Length)
        {
            return true;
        }

        for (int i = 0; i < published.Length; i++)
        {
            if (published[i] != working[i])
            {
                return true;
            }
        }

        return false;
    }
}
