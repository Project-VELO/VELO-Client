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
    /// 노트가 입력을 받아 줄 수 있는 늦은 쪽 최대 오차입니다. 이 시간을 넘겨 지나간 노트는 BAD로 확정됩니다.
    /// 이른 쪽은 EARLY_CONSUME_WINDOW_MS가 따로 잡으므로 입력 허용 구간은 판정선을 기준으로 비대칭입니다.
    /// </summary>
    public const int JUDGEABLE_WINDOW_MS = GOOD_WINDOW_MS;

    /// <summary>
    /// 판정선에 닿기 전 입력이 노트를 소비할 수 있는 가장 이른 시점입니다.
    /// 이 구간이 없으면 GOOD 창보다 이른 입력은 아무 대가 없이 무시되고 노트가 살아남아 다시 칠 수 있습니다.
    ///
    /// "화면에 보이는 노트"를 기준으로 삼지 않는 이유는, 가시 범위가 4/HiSpeed 마디여서(LiveScrollMapper)
    /// 소비 구간이 하이스피드와 BPM에 따라 수 초까지 벌어지고 판정이 표시 설정에 종속되기 때문입니다.
    /// </summary>
    public const int EARLY_CONSUME_WINDOW_MS = GOOD_WINDOW_MS * 2;

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
    /// 귀신 노트를 놓쳤을 때 깎는 점수입니다(3-I-6). 즉시 실패를 대신하는 처벌이므로,
    /// 무시할 수 없되 한 번으로 판을 뒤집지는 않도록 PERFECT 두 개 분량으로 잡았습니다.
    /// </summary>
    public const int GHOST_MISS_PENALTY_SCORE = PERFECT_SCORE * 2;

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
    /// 귀신 노트를 놓친 감점을 반영한 점수를 돌려줍니다. 남은 점수보다 감점이 크면 0에서 멈춥니다.
    /// 정확도가 점수에서 파생되므로(LiveRankEvaluator.GetAccuracy) 음수를 허용하면 정확도까지 음수가 됩니다.
    /// </summary>
    public static int ApplyGhostMissPenalty(int score)
    {
        int penalizedScore = score - GHOST_MISS_PENALTY_SCORE;

        return penalizedScore < 0 ? 0 : penalizedScore;
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
