using System;

/// <summary>
/// 노트 판정 통계 표의 정렬 기준을 비교 함수로 바꿔 줍니다. 기준이 같은 줄은 시각 → 레인 순으로 둡니다.
/// </summary>
public static class LiveNoteHitStatSorter
{
    public static Comparison<LiveNoteHitStatRow> GetComparison(ELiveNoteHitStatSort sort)
    {
        switch (sort)
        {
            case ELiveNoteHitStatSort.PROBLEM_RATE:
                return CompareByProblemRate;
            case ELiveNoteHitStatSort.DEVIATION:
                return CompareByDeviation;
            default:
                return CompareByTime;
        }
    }

    // 비율이 같으면 많이 친 노트를 앞에 둡니다. 1회 중 1회보다 10회 중 10회가 더 확실한 신호입니다.
    private static int CompareByProblemRate(LiveNoteHitStatRow left, LiveNoteHitStatRow right)
    {
        int order = right.ProblemRate.CompareTo(left.ProblemRate);

        if (order == 0)
        {
            order = right.Stat.JudgedCount.CompareTo(left.Stat.JudgedCount);
        }

        return order != 0 ? order : CompareByTime(left, right);
    }

    private static int CompareByDeviation(LiveNoteHitStatRow left, LiveNoteHitStatRow right)
    {
        if (left.HasTimingSample != right.HasTimingSample)
        {
            return left.HasTimingSample ? -1 : 1;
        }

        int order = Math.Abs(right.DeviationMs).CompareTo(Math.Abs(left.DeviationMs));

        return order != 0 ? order : CompareByTime(left, right);
    }

    private static int CompareByTime(LiveNoteHitStatRow left, LiveNoteHitStatRow right)
    {
        int order = left.Stat.TimeMs.CompareTo(right.Stat.TimeMs);

        return order != 0 ? order : left.Stat.Lane.CompareTo(right.Stat.Lane);
    }
}
