using System;

/// <summary>
/// 플레이 집계와 진입 정보를 합쳐 결과 화면으로 넘길 LiveResultData를 만듭니다.
/// 보상과 최고 기록은 결과 화면 진입 시점에 계산하므로(3-J-4) 여기서는 채우지 않습니다.
/// </summary>
public static class LiveResultBuilder
{
    public const string FAIL_REASON_GHOST_NOTE = "GHOST_NOTE";
    public const string FAIL_REASON_LOW_ACCURACY = "LOW_ACCURACY";

    public static LiveResultData Build(LiveScoreTracker tracker, int totalNoteCount, bool hasGhostFailure)
    {
        LiveEntryContext context = LiveEntryContext.Instance;

        float accuracy = LiveRankEvaluator.GetAccuracy(tracker.Score, totalNoteCount);
        ELiveRank rank = LiveRankEvaluator.Evaluate(accuracy, tracker.PerfectCount, totalNoteCount, hasGhostFailure);
        bool isClear = LiveRankEvaluator.IsClear(rank);

        return new LiveResultData
        {
            PlayResultId = Guid.NewGuid().ToString(),
            EntryType = context.EntryType,
            ScheduleId = context.ScheduleId,
            SongId = context.SelectedSongId,
            Difficulty = context.SelectedDifficulty,

            IsClear = isClear,
            FailReason = GetFailReason(isClear, hasGhostFailure),

            Score = tracker.Score,
            Rank = rank,
            Accuracy = accuracy,
            TotalNoteCount = totalNoteCount,

            PerfectCount = tracker.PerfectCount,
            GreatCount = tracker.GreatCount,
            GoodCount = tracker.GoodCount,
            BadCount = tracker.BadCount,

            MaxCombo = tracker.MaxCombo,
            IsFullCombo = tracker.IsFullCombo,
        };
    }

    private static string GetFailReason(bool isClear, bool hasGhostFailure)
    {
        if (isClear)
        {
            return null;
        }

        return hasGhostFailure ? FAIL_REASON_GHOST_NOTE : FAIL_REASON_LOW_ACCURACY;
    }
}
