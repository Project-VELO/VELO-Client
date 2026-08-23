using UnityEngine;

/// <summary>
/// 한 단계 위 등급에 닿는 데 필요한 점수를 구합니다(곡 선택 화면의 Next Goal).
///
/// 등급은 정확도로 갈리고 정확도는 점수에서 나오므로(LiveRankEvaluator.GetAccuracy),
/// 경계 정확도를 점수로 되돌려 목표를 만듭니다. 경계 판정이 "이상"이라 올림으로 맞춥니다.
/// </summary>
public static class LiveRankGoal
{
    /// <summary>
    /// 이미 최고 등급이거나 채보를 몰라 목표를 셀 수 없으면 false를 돌려주고, 호출부가 표시를 비웁니다.
    /// </summary>
    public static bool TryGetGoalScore(ELiveRank currentRank, int totalNoteCount, out int goalScore)
    {
        goalScore = 0;

        if (totalNoteCount <= 0 || currentRank == ELiveRank.PERFECT_S)
        {
            return false;
        }

        int maxScore = totalNoteCount * LiveJudgementRule.MAX_SCORE_PER_NOTE;

        // PERFECT_S만은 정확도 경계가 아니라 "모든 노트가 PERFECT"가 조건이라 만점이 곧 목표입니다.
        if (currentRank == ELiveRank.S)
        {
            goalScore = maxScore;
            return true;
        }

        goalScore = Mathf.CeilToInt(GetGoalAccuracy(currentRank) * maxScore / 100f);
        return true;
    }

    /// <summary>
    /// FAILED와 미기록은 아직 CLEAR도 못 넘긴 상태이므로 C의 경계를 목표로 삼습니다.
    /// </summary>
    private static float GetGoalAccuracy(ELiveRank currentRank)
    {
        switch (currentRank)
        {
            case ELiveRank.A:
                return LiveRankEvaluator.S_ACCURACY;

            case ELiveRank.B:
                return LiveRankEvaluator.A_ACCURACY;

            case ELiveRank.C:
                return LiveRankEvaluator.B_ACCURACY;

            default:
                return LiveRankEvaluator.CLEAR_ACCURACY;
        }
    }
}
