/// <summary>
/// 플레이 도중의 점수·콤보·판정 개수를 누적합니다. 기획서 3-I-3(콤보)과 3-I-4(점수)를 따릅니다.
/// 콤보 보너스와 카드 보너스는 아직 적용하지 않으므로, 최종 점수는 판정 점수의 합에서 귀신 노트 감점을 뺀 값입니다.
/// </summary>
public class LiveScoreTracker
{
    public int Score { get; private set; }
    public int Combo { get; private set; }
    public int MaxCombo { get; private set; }

    public int PerfectCount { get; private set; }
    public int GreatCount { get; private set; }
    public int GoodCount { get; private set; }
    public int BadCount { get; private set; }

    /// <summary>
    /// 귀신 레인 오입력 횟수입니다. 채보에 없는 입력이라 노트 판정 개수와 뒤섞이면 정확도의 분모와
    /// 결과 화면의 BAD 개수가 실제 노트 수보다 부풀려지므로, BadCount와 분리해 따로 셉니다(3-I-6-1).
    /// </summary>
    public int GhostMisinputCount { get; private set; }

    /// <summary>
    /// 판정이 끝난 노트 수입니다. 귀신 노트 오입력은 채보에 없는 입력이므로 포함하지 않습니다(3-I-6-1).
    /// </summary>
    public int JudgedNoteCount => PerfectCount + GreatCount + GoodCount + BadCount;

    public bool IsFullCombo => BadCount == 0;

    public void Clear()
    {
        Score = 0;
        Combo = 0;
        MaxCombo = 0;
        PerfectCount = 0;
        GreatCount = 0;
        GoodCount = 0;
        BadCount = 0;
        GhostMisinputCount = 0;
    }

    public void Apply(EJudgement judgement)
    {
        Score += LiveJudgementRule.GetScore(judgement);
        AddJudgementCount(judgement);
        RefreshCombo(judgement);
    }

    /// <summary>
    /// 귀신 노트 오입력을 기록합니다. 점수도 주지 않고 노트 판정 개수에도 넣지 않으며, 콤보만 끊습니다(3-I-6-1).
    /// </summary>
    public void AddGhostMisinput()
    {
        GhostMisinputCount++;
        Combo = 0;
    }

    /// <summary>
    /// 귀신 노트를 놓쳤을 때의 감점입니다(3-I-6). 판정 개수와 콤보는 BAD로 이미 Apply에서 처리되므로 점수만 건드립니다.
    /// </summary>
    public void ApplyGhostMissPenalty()
    {
        Score = LiveJudgementRule.ApplyGhostMissPenalty(Score);
    }

    private void AddJudgementCount(EJudgement judgement)
    {
        switch (judgement)
        {
            case EJudgement.PERFECT:
                PerfectCount++;
                break;

            case EJudgement.GREAT:
                GreatCount++;
                break;

            case EJudgement.GOOD:
                GoodCount++;
                break;

            default:
                BadCount++;
                break;
        }
    }

    private void RefreshCombo(EJudgement judgement)
    {
        if (LiveJudgementRule.BreaksCombo(judgement))
        {
            Combo = 0;
            return;
        }

        // GOOD은 콤보를 올리지도 끊지도 않으므로 여기서 걸러집니다.
        if (!LiveJudgementRule.IncreasesCombo(judgement))
        {
            return;
        }

        Combo++;

        if (MaxCombo < Combo)
        {
            MaxCombo = Combo;
        }
    }
}
