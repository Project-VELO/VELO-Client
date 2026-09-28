using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 판정 통계 파일 하나를 조회 창에 보여 줄 요약과 표 줄로 바꿉니다.
///
/// 마디:박과 수정 여부는 작업본 채보(StreamingAssets/Charts)를 읽어 계산합니다. 채보는 에디터에서 작업본을 고치므로,
/// 이미 고친 노트를 표에서 구분하려면 작업본과 비교해야 합니다. 작업본이 없으면 두 칸을 비워 둡니다.
/// </summary>
public class LiveNoteHitStatReport
{
    private const string EMPTY_CELL = "-";
    private const string MOVED_STATE = "수정됨";
    private const string DELETED_STATE = "삭제됨";
    private const double BEAT_ROUNDING = 100.0;

    private readonly List<LiveNoteHitStatRow> _rows = new List<LiveNoteHitStatRow>();

    public LiveChartHitStats Stats { get; private set; }
    public List<LiveNoteHitStatRow> Rows => _rows;
    public bool HasWorkingChart { get; private set; }
    public string OverviewSummary { get; private set; }
    public string JudgementSummary { get; private set; }
    public string TimingSummary { get; private set; }

    public void Init(LiveChartHitStats stats)
    {
        Stats = stats;
        _rows.Clear();

        double chartMeanErrorMs = InitSummaries(stats);
        ChartData workingChart = LoadWorkingChart(stats);
        Dictionary<string, NoteData> workingNotesById = CollectNotesById(workingChart);
        HasWorkingChart = !ReferenceEquals(workingChart, null);

        foreach (LiveNoteHitStat stat in stats.Notes)
        {
            string positionText = GetPositionText(workingChart, stat.TimeMs);
            string stateText = HasWorkingChart ? GetStateText(workingNotesById, stat) : string.Empty;
            _rows.Add(new LiveNoteHitStatRow(stat, positionText, stateText, chartMeanErrorMs));
        }
    }

    /// <summary>
    /// LiveChartLoader는 문법이 깨진 JSON이나 잠긴 파일에서 예외를 던집니다. 작업본은 보조 정보라,
    /// 읽지 못해도 창 전체가 멈추지 않도록 여기서 받아 "작업본 없음"으로 처리합니다.
    /// </summary>
    private static ChartData LoadWorkingChart(LiveChartHitStats stats)
    {
        string path = LiveSongPaths.GetWorkingChartPath(stats.SongId, stats.Difficulty);

        try
        {
            return LiveChartLoader.Load(path);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[LiveNoteHitStatReport] 작업본 채보를 읽지 못했습니다({path}): {exception.Message}");
            return null;
        }
    }

    /// <summary>
    /// 채보 전체의 판정 분포와 오차 평균·표준편차를 요약 문구로 만들고, 편차 계산에 쓸 채보 평균 오차를 돌려줍니다.
    /// 채보 평균은 노트 평균의 평균이 아니라 모든 표본을 한데 모은 평균입니다. 많이 친 노트일수록 무게가 실립니다.
    /// </summary>
    private double InitSummaries(LiveChartHitStats stats)
    {
        int perfectCount = 0;
        int greatCount = 0;
        int goodCount = 0;
        int badCount = 0;
        int earlyBadCount = 0;
        int missCount = 0;
        int holdBreakCount = 0;
        int sampleCount = 0;
        long errorSumMs = 0;
        long errorSquareSumMs = 0;

        foreach (LiveNoteHitStat stat in stats.Notes)
        {
            perfectCount += stat.PerfectCount;
            greatCount += stat.GreatCount;
            goodCount += stat.GoodCount;
            badCount += stat.BadCount;
            earlyBadCount += stat.EarlyBadCount;
            missCount += stat.MissCount;
            holdBreakCount += stat.HoldBreakCount;
            sampleCount += stat.TimingSampleCount;
            errorSumMs += stat.TimingErrorSumMs;
            errorSquareSumMs += stat.TimingErrorSquareSumMs;
        }

        double meanErrorMs = 0 < sampleCount ? (double)errorSumMs / sampleCount : 0.0;
        double standardDeviationMs = LiveNoteHitStatRow.GetStandardDeviation(errorSquareSumMs, errorSumMs, sampleCount);
        int judgedCount = perfectCount + greatCount + goodCount + badCount;

        OverviewSummary = $"판 수 {stats.PlayCount}   노트 {stats.Notes.Count}개   판정 {judgedCount}회";
        JudgementSummary = $"PERFECT {perfectCount}   GREAT {greatCount}   GOOD {goodCount}   BAD {badCount}"
            + $" (이른 BAD {earlyBadCount} · MISS {missCount} · 롱노트 조기 해제 {holdBreakCount})";
        TimingSummary = 0 < sampleCount
            ? $"채보 평균 오차 {meanErrorMs:+0.0;-0.0;0.0}ms   표준편차 {standardDeviationMs:0.0}ms   (표본 {sampleCount}개, ±{LiveJudgementRule.GOOD_WINDOW_MS}ms 안의 입력만)"
            : "오차 표본이 아직 없습니다.";

        return meanErrorMs;
    }

    private static Dictionary<string, NoteData> CollectNotesById(ChartData chart)
    {
        Dictionary<string, NoteData> notesById = new Dictionary<string, NoteData>();

        if (ReferenceEquals(chart, null))
        {
            return notesById;
        }

        foreach (NoteData note in chart.Notes)
        {
            if (!string.IsNullOrEmpty(note.NoteId))
            {
                notesById[note.NoteId] = note;
            }
        }

        return notesById;
    }

    /// <summary>
    /// 채보 에디터의 마디 라벨(bar 1부터)과 같은 기준으로 마디와 박을 셉니다. 박도 1부터 셉니다.
    /// 오프셋보다 앞선 노트는 마디가 없으므로 비워 둡니다.
    ///
    /// 채보 시각은 정수 ms로 저장되어 실제 박 위치와 최대 0.5ms 어긋납니다. 그대로 나누면 마디 첫 박이 43.999처럼
    /// 모자라게 나와 앞 마디의 "5박"으로 찍히므로, 표시 정밀도(1/100박)로 먼저 반올림한 뒤 마디를 나눕니다.
    /// </summary>
    private static string GetPositionText(ChartData chart, int timeMs)
    {
        if (ReferenceEquals(chart, null))
        {
            return EMPTY_CELL;
        }

        double beat = Math.Round(LiveBpmTimeConverter.TimeMsToBeat(chart, timeMs) * BEAT_ROUNDING) / BEAT_ROUNDING;

        if (beat < 0.0)
        {
            return EMPTY_CELL;
        }

        int beatsPerBar = chart.BeatsPerBar;
        int barIndex = (int)Math.Floor(beat / beatsPerBar);
        double beatInBar = beat - barIndex * beatsPerBar + 1.0;

        return $"{barIndex + 1}마디 {beatInBar:0.##}박";
    }

    /// <summary>
    /// 작업본에서 이미 옮기거나 지운 노트를 표시합니다. 이런 노트의 기록은 다음에 리듬게임을 열 때 자동으로 초기화·제거됩니다.
    /// </summary>
    private static string GetStateText(Dictionary<string, NoteData> workingNotesById, LiveNoteHitStat stat)
    {
        if (string.IsNullOrEmpty(stat.NoteId))
        {
            return string.Empty;
        }

        if (!workingNotesById.TryGetValue(stat.NoteId, out NoteData workingNote))
        {
            return DELETED_STATE;
        }

        return stat.IsSameSnapshot(workingNote) ? string.Empty : MOVED_STATE;
    }
}
