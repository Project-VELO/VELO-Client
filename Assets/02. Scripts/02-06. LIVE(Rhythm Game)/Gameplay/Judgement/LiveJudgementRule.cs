/// <summary>
/// 기획서 3-I-2(판정 기준)와 3-I-4(점수)를 코드로 옮긴 규칙표입니다.
/// 판정 창과 점수는 이 클래스에만 존재하므로, 밸런스를 조정할 때 여기만 고치면 됩니다.
/// </summary>
public static class LiveJudgementRule
{
    public const int PERFECT_WINDOW_MS = 45;
    public const int GREAT_WINDOW_MS = 85;
    public const int GOOD_WINDOW_MS = 125;

    /// <summary>
    /// 노트가 입력을 받아 줄 수 있는 최대 오차입니다. 이 시간을 넘겨 지나간 노트는 BAD로 확정됩니다.
    /// </summary>
    public const int JUDGEABLE_WINDOW_MS = GOOD_WINDOW_MS;

    /// <summary>
    /// 롱노트를 끝까지 유지한 것으로 인정하는 조기 해제 허용치입니다.
    /// 종료 시각에 정확히 맞춰 떼는 것은 사람이 할 수 없으므로, 단타와 같은 유효 입력 폭만큼 앞서 떼도 인정합니다.
    /// </summary>
    public const int HOLD_RELEASE_TOLERANCE_MS = GOOD_WINDOW_MS;

    public const int PERFECT_SCORE = 100;
    public const int GREAT_SCORE = 80;
    public const int GOOD_SCORE = 60;
    public const int BAD_SCORE = 0;

    /// <summary>
    /// 정확도 계산의 분모가 되는, 노트 하나가 받을 수 있는 최대 점수입니다.
    /// </summary>
    public const int MAX_SCORE_PER_NOTE = PERFECT_SCORE;

    public static EJudgement Judge(int errorMs)
    {
        int absErrorMs = errorMs < 0 ? -errorMs : errorMs;

        if (absErrorMs <= PERFECT_WINDOW_MS)
        {
            return EJudgement.PERFECT;
        }

        if (absErrorMs <= GREAT_WINDOW_MS)
        {
            return EJudgement.GREAT;
        }

        if (absErrorMs <= GOOD_WINDOW_MS)
        {
            return EJudgement.GOOD;
        }

        return EJudgement.BAD;
    }

    public static int GetScore(EJudgement judgement)
    {
        switch (judgement)
        {
            case EJudgement.PERFECT:
                return PERFECT_SCORE;

            case EJudgement.GREAT:
                return GREAT_SCORE;

            case EJudgement.GOOD:
                return GOOD_SCORE;

            default:
                return BAD_SCORE;
        }
    }

    /// <summary>
    /// 콤보를 1 올리는 판정인지 여부입니다.
    /// GOOD은 콤보를 올리지도 끊지도 않고 그대로 유지하므로(3-I-3) 여기에 포함되지 않습니다.
    /// </summary>
    public static bool IncreasesCombo(EJudgement judgement)
    {
        return judgement == EJudgement.PERFECT || judgement == EJudgement.GREAT;
    }

    public static bool BreaksCombo(EJudgement judgement)
    {
        return judgement == EJudgement.BAD;
    }
}
