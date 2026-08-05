/// <summary>
/// 노트 하나에 대한 판정 결과입니다. 기획서 3-I-2의 네 단계를 그대로 따릅니다.
/// 각 단계의 입력 오차 구간은 LiveJudgementRule이 정합니다.
/// </summary>
public enum EJudgement
{
    PERFECT,
    GREAT,
    GOOD,

    /// <summary>
    /// 오차가 판정 구간을 넘었거나 아예 입력하지 않은 경우입니다.
    /// </summary>
    BAD,
}
