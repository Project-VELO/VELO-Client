using System;

/// <summary>
/// 결과 화면 확인용 가짜 플레이 결과입니다.
///
/// 리듬게임을 완주하지 않고 랭크별 결과와 실패 화면을 바로 띄우기 위한 개발용입니다.
/// 판정 개수만 표로 두고 점수·정확도는 실제 규칙(LiveJudgementRule, LiveRankEvaluator)에 넣어 뽑습니다.
/// 숫자를 통째로 박아 두면 밸런스를 고쳤을 때 화면에 규칙과 어긋난 값이 뜨기 때문입니다.
/// </summary>
public static class LiveResultDebugSample
{
    private const int TOTAL_NOTE_COUNT = 400;

    /// <summary>
    /// ELiveRank 선언 순서대로 놓은 판정 구성입니다. 한 줄은 {PERFECT, GREAT, GOOD, BAD, 최대 콤보}입니다.
    /// 각 줄의 점수가 그 랭크의 정확도 구간에 들어가도록 잡았습니다.
    /// </summary>
    private static readonly int[,] JUDGEMENT_TABLE =
    {
        { 400, 0, 0, 0, 400 },      // PERFECT_S : 100.0%
        { 360, 38, 2, 0, 400 },     // S         :  97.9%
        { 340, 50, 8, 2, 210 },     // A         :  96.2%
        { 280, 90, 20, 10, 150 },   // B         :  91.0%
        { 200, 120, 50, 30, 90 },   // C         :  81.5%
        { 120, 100, 80, 100, 45 },  // FAILED    :  62.0%
    };

    public static LiveResultData Create(ELiveRank rank)
    {
        int row = (int)rank;
        int perfectCount = JUDGEMENT_TABLE[row, 0];
        int greatCount = JUDGEMENT_TABLE[row, 1];
        int goodCount = JUDGEMENT_TABLE[row, 2];
        int badCount = JUDGEMENT_TABLE[row, 3];
        int maxCombo = JUDGEMENT_TABLE[row, 4];

        int score = GetScore(perfectCount, greatCount, goodCount);
        bool isClear = LiveRankEvaluator.IsClear(rank);
        LiveEntryContext entry = LiveEntryContext.Instance;

        return new LiveResultData
        {
            PlayResultId = Guid.NewGuid().ToString(),
            EntryType = entry.EntryType,
            ScheduleId = entry.ScheduleId,
            SongId = entry.SelectedSongId,
            Difficulty = entry.SelectedDifficulty,

            IsClear = isClear,
            FailReason = isClear ? null : LiveResultDispatcher.FAIL_REASON_LOW_ACCURACY,

            Score = score,

            // 버튼이 가리키는 랭크를 그대로 씁니다. 판정 구성에서 다시 매기면
            // 경계값을 고쳤을 때 보려던 화면이 아닌 다른 화면이 떠 확인에 쓸 수 없습니다.
            Rank = rank,
            Accuracy = LiveRankEvaluator.GetAccuracy(score, TOTAL_NOTE_COUNT),
            TotalNoteCount = TOTAL_NOTE_COUNT,

            PerfectCount = perfectCount,
            GreatCount = greatCount,
            GoodCount = goodCount,
            BadCount = badCount,

            MaxCombo = maxCombo,
            IsFullCombo = badCount == 0,
        };
    }

    private static int GetScore(int perfectCount, int greatCount, int goodCount)
    {
        return perfectCount * LiveJudgementRule.PERFECT_SCORE
            + greatCount * LiveJudgementRule.GREAT_SCORE
            + goodCount * LiveJudgementRule.GOOD_SCORE;
    }
}
