/// <summary>
/// 정확도를 계산하고 랭크를 부여합니다. 기획서 3-I-5(CLEAR 조건)와 3-I-7(랭크)을 따릅니다.
/// 전체 노트 개수에는 일반 노트와 귀신 노트를 모두 포함합니다.
/// </summary>
public static class LiveRankEvaluator
{
    public const float S_ACCURACY = 95f;
    public const float A_ACCURACY = 85f;
    public const float B_ACCURACY = 70f;

    /// <summary>
    /// CLEAR로 인정되는 최소 정확도이자 C랭크의 경계입니다. 이 아래는 랭크가 FAILED가 됩니다.
    /// </summary>
    public const float CLEAR_ACCURACY = 50f;

    public static float GetAccuracy(int score, int totalNoteCount)
    {
        if (totalNoteCount <= 0)
        {
            return 0f;
        }

        return (float)score / (totalNoteCount * LiveJudgementRule.MAX_SCORE_PER_NOTE) * 100f;
    }

    /// <summary>
    /// 모든 노트가 PERFECT인 경우를 가장 먼저 검사한 뒤 정확도 구간을 훑습니다.
    ///
    /// PERFECT_S의 조건은 "정확도 100% + ALL PERFECT"이지만 ALL PERFECT 하나만 봅니다.
    /// 귀신 노트를 놓치면 BAD로 판정되므로, 모든 노트가 PERFECT면 감점도 없어 정확도는 저절로 100%입니다.
    ///
    /// 귀신 노트를 놓치면 점수가 깎이고(LiveJudgementRule.GHOST_MISS_PENALTY_SCORE) 그 손실이 정확도를 통해
    /// 여기까지 전해지므로, 귀신 실패를 따로 받아 FAILED로 못 박지 않습니다.
    /// </summary>
    public static ELiveRank Evaluate(float accuracy, int perfectCount, int totalNoteCount)
    {
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
