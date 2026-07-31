using System;
using System.Collections.Generic;

/// <summary>
/// 수록된 채보 파일에서 곡 선택 화면 표시에 필요한 값(노트 수, 난이도 레벨)을 뽑아냅니다.
/// 노트 수는 채보 파일을 실제로 읽어 세므로 song_info.json의 메타데이터가 낡아 있어도 화면 값이 어긋나지 않습니다.
/// 곡·난이도 조합별로 캐시해 같은 곡을 다시 선택할 때 재파싱하지 않습니다.
/// </summary>
public class LiveChartSummaryReader
{
    private readonly Dictionary<string, LiveChartSummary> _summaries = new Dictionary<string, LiveChartSummary>();

    public LiveChartSummary GetSummary(SongData song, EDifficulty difficulty)
    {
        if (ReferenceEquals(song, null))
        {
            return new LiveChartSummary();
        }

        string cacheKey = $"{song.SongId}_{difficulty}";
        if (_summaries.TryGetValue(cacheKey, out LiveChartSummary cached))
        {
            return cached;
        }

        LiveChartSummary summary = ReadSummary(song, difficulty);
        _summaries[cacheKey] = summary;

        return summary;
    }

    /// <summary>
    /// 채보 파일이 실제로 존재하는 난이도만 추려 돌려줍니다. 난이도 버튼의 클릭 가능 여부를 이 결과로 정합니다.
    /// </summary>
    public List<EDifficulty> GetAvailableDifficulties(SongData song)
    {
        var availableDifficulties = new List<EDifficulty>();

        if (ReferenceEquals(song, null))
        {
            return availableDifficulties;
        }

        foreach (EDifficulty difficulty in Enum.GetValues(typeof(EDifficulty)))
        {
            if (GetSummary(song, difficulty).HasChart)
            {
                availableDifficulties.Add(difficulty);
            }
        }

        return availableDifficulties;
    }

    public void Clear()
    {
        _summaries.Clear();
    }

    private LiveChartSummary ReadSummary(SongData song, EDifficulty difficulty)
    {
        var summary = new LiveChartSummary
        {
            Level = GetAuthoredLevel(song, difficulty),
        };

        // 읽을 수 없는 채보를 선택 가능으로 두면 난이도 버튼이 열린 채 플레이로 넘어가므로, 파일이 있어도 없는 것으로 답합니다.
        ChartData chart = LiveChartLoader.LoadPublished(song, difficulty);
        if (ReferenceEquals(chart, null))
        {
            return summary;
        }

        summary.HasChart = true;
        summary.NoteCount = chart.Notes.Count;

        return summary;
    }

    /// <summary>
    /// 난이도 레벨은 자동으로 계산할 수 없는 기획 값이므로 song_info.json에 적힌 값을 그대로 씁니다.
    /// 값이 없으면 0으로 두고, 화면에서는 미표기로 처리합니다.
    /// </summary>
    private int GetAuthoredLevel(SongData song, EDifficulty difficulty)
    {
        if (song.Charts.TryGetValue(difficulty, out ChartMetadata metadata) && !ReferenceEquals(metadata, null))
        {
            return metadata.Level;
        }

        return 0;
    }
}
