/// <summary>
/// 정확도를 계산하고 랭크를 부여합니다. 기획서 3-I-5(CLEAR 조건)와 3-I-7(랭크)을 따릅니다.
/// 전체 노트 개수에는 일반 노트와 귀신 노트를 모두 포함하며, 귀신 노트 오입력은 포함하지 않습니다.
/// </summary>
public static class LiveRankEvaluator
{
    public const float S_ACCURACY = 97f;
    public const float A_ACCURACY = 95f;
    public const float B_ACCURACY = 85f;

    /// <summary>
    /// CLEAR로 인정되는 최소 정확도입니다. 이 아래는 랭크가 FAILED가 됩니다.
    /// </summary>
    public const float CLEAR_ACCURACY = 70f;

    public static float GetAccuracy(int score, int totalNoteCount)
    {
        if (totalNoteCount <= 0)
        {
            return 0f;
        }

        return (float)score / (totalNoteCount * LiveJudgementRule.MAX_SCORE_PER_NOTE) * 100f;
    }

    /// <summary>
    /// 귀신 노트 실패는 정확도와 무관하게 곧바로 FAILED이며, 모든 노트가 PERFECT인 경우를 가장 먼저 검사합니다.
    /// </summary>
    public static ELiveRank Evaluate(float accuracy, int perfectCount, int totalNoteCount, bool hasGhostFailure)
    {
        if (hasGhostFailure)
        {
            return ELiveRank.FAILED;
        }

        if (0 < totalNoteCount && perfectCount == totalNoteCount)
        {
            return ELiveRank.PERFECT_S;
        }

        if (S_ACCURACY <= accuracy)
        {
            return ELiveRank.S;
        }

        if (A_ACCURACY <= accuracy)
        {
            return ELiveRank.A;
        }

        if (B_ACCURACY <= accuracy)
        {
            return ELiveRank.B;
        }

        if (CLEAR_ACCURACY <= accuracy)
        {
            return ELiveRank.C;
        }

        return ELiveRank.FAILED;
    }

    public static bool IsClear(ELiveRank rank)
    {
        return rank != ELiveRank.FAILED;
    }
}
