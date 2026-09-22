using System;
using System.Collections.Generic;

/// <summary>
/// 노트 판정 통계 표의 한 줄입니다. 파일을 불러올 때 한 번 계산해 두고, 창을 다시 그릴 때는 읽기만 합니다.
/// Cells의 순서는 LiveNoteHitStatTable의 열 순서와 같습니다.
/// </summary>
public class LiveNoteHitStatRow
{
    private const string EMPTY_CELL = "-";

    private readonly List<string> _cells = new List<string>();

    public LiveNoteHitStat Stat { get; }
    public bool HasTimingSample { get; }

    /// <summary>
    /// 노트 평균 오차 − 채보 평균 오차(ms)입니다. 오프셋이나 기기 지연처럼 모든 노트에 똑같이 걸린 치우침을 뺀 값이라,
    /// 이 노트를 옮겨야 할 양으로 읽습니다. 음수면 이르게 쳤으므로 노트를 그만큼 앞당깁니다.
    /// </summary>
    public double DeviationMs { get; }

    /// <summary>
    /// 판정 중 GOOD과 BAD가 차지하는 비율(0~1)입니다.
    /// </summary>
    public float ProblemRate { get; }

    public List<string> Cells => _cells;

    public LiveNoteHitStatRow(LiveNoteHitStat stat, string positionText, string stateText, double chartMeanErrorMs)
    {
        Stat = stat;
        HasTimingSample = 0 < stat.TimingSampleCount;

        double meanErrorMs = HasTimingSample ? (double)stat.TimingErrorSumMs / stat.TimingSampleCount : 0.0;
        DeviationMs = HasTimingSample ? meanErrorMs - chartMeanErrorMs : 0.0;
        ProblemRate = 0 < stat.JudgedCount ? (float)(stat.GoodCount + stat.BadCount) / stat.JudgedCount : 0f;

        InitCells(positionText, stateText, meanErrorMs);
    }

    /// <summary>
    /// 분산 = 제곱합 / n − 평균². 부동소수 오차로 아주 작은 음수가 나올 수 있어 0 아래는 잘라 냅니다.
    /// </summary>
    public static double GetStandardDeviation(long squareSum, long sum, int sampleCount)
    {
        if (sampleCount <= 0)
        {
            return 0.0;
        }

        double mean = (double)sum / sampleCount;
        double variance = (double)squareSum / sampleCount - mean * mean;

        return variance <= 0.0 ? 0.0 : Math.Sqrt(variance);
    }

    private void InitCells(string positionText, string stateText, double meanErrorMs)
    {
        LiveNoteHitStat stat = Stat;
        double standardDeviationMs = GetStandardDeviation(stat.TimingErrorSquareSumMs, stat.TimingErrorSumMs, stat.TimingSampleCount);

        _cells.Add(FormatTime(stat.TimeMs));
        _cells.Add(positionText);
        _cells.Add(stat.Lane.ToString());
        _cells.Add(stat.NoteType.ToString());
        _cells.Add(stat.PerfectCount.ToString());
        _cells.Add(stat.GreatCount.ToString());
        _cells.Add(stat.GoodCount.ToString());
        _cells.Add(stat.BadCount.ToString());
        _cells.Add(stat.EarlyBadCount.ToString());
        _cells.Add(stat.MissCount.ToString());
        _cells.Add(stat.HoldBreakCount.ToString());
        _cells.Add(0 < stat.JudgedCount ? $"{ProblemRate * 100f:0}%" : EMPTY_CELL);
        _cells.Add(stat.TimingSampleCount.ToString());
        _cells.Add(HasTimingSample ? FormatSignedMs(meanErrorMs) : EMPTY_CELL);
        _cells.Add(HasTimingSample ? FormatSignedMs(DeviationMs) : EMPTY_CELL);
        _cells.Add(HasTimingSample ? $"{standardDeviationMs:0.0}" : EMPTY_CELL);
        _cells.Add(stateText);
    }

    private static string FormatTime(int timeMs)
    {
        TimeSpan time = TimeSpan.FromMilliseconds(timeMs);
        return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}.{time.Milliseconds:000}";
    }

    private static string FormatSignedMs(double valueMs)
    {
        return valueMs.ToString("+0.0;-0.0;0.0");
    }
}
