/// <summary>
/// 리듬게임 결과가 LIVE 일일 스케줄의 완료 조건을 만족하는지 판정합니다(기획서 3-E-2-1).
///
/// 상태를 바꾸지 않는 순수 판정만 담습니다. 어떤 스케줄에 적용할지와 완료 처리는 ScheduleProgressService가 맡습니다.
/// </summary>
public static class ScheduleCompletionRule
{
    /// <summary>
    /// 이 진입 경로의 결과를 일일 스케줄 완료에 반영해도 되는지 여부입니다(기획서 3-C-4).
    ///
    /// 연습실 LIVE는 같은 곡을 같은 조건으로 클리어해도 일일 스케줄에 반영하지 않습니다.
    /// 홈 LIVE와 스케줄 LIVE는 진입 위치만 다를 뿐 완료 판정이 동일합니다.
    /// </summary>
    public static bool ReflectsToSchedule(EEntryType entryType)
    {
        return entryType != EEntryType.PRACTICE_LIVE;
    }

    /// <summary>
    /// 결과가 해당 스케줄의 완료 조건을 모두 만족하는지 판정합니다.
    ///
    /// 홈 화면의 일반 LIVE로 진입해 지정곡을 고르지 않았더라도, 곡·난이도·랭크가 일치하면 완료로 인정합니다
    /// (기획서 3-E-2 일일퀘스트 7번). 그래서 진입 시 지정곡이었는지는 보지 않고 결과 값만 대조합니다.
    /// </summary>
    public static bool IsSatisfiedBy(ScheduleData schedule, LiveResultData result)
    {
        if (ReferenceEquals(schedule, null) || ReferenceEquals(result, null))
        {
            return false;
        }

        if (schedule.ScheduleType != EScheduleType.LIVE)
        {
            return false;
        }

        if (!result.IsClear)
        {
            return false;
        }

        ScheduleClearCondition condition = schedule.ClearCondition;

        if (string.IsNullOrEmpty(condition.TargetSongId) || condition.TargetSongId != result.SongId)
        {
            return false;
        }

        if (condition.RequiredDifficulty != result.Difficulty)
        {
            return false;
        }

        // ELiveRank는 좋은 랭크일수록 값이 작아 부등호를 직접 쓰면 실수하기 쉬우므로 조건 객체에 판정을 맡깁니다.
        if (!condition.IsRankSatisfied(result.Rank))
        {
            return false;
        }

        // 최소 점수는 1차 MVP에서 사용하지 않지만(NO_SCORE_REQUIREMENT), 조건이 걸리면 함께 검사합니다.
        return condition.MinimumScore <= MasterDataSchema.NO_SCORE_REQUIREMENT || condition.MinimumScore <= result.Score;
    }
}
