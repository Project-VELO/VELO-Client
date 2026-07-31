/// <summary>
/// 노트 하나에 대한 판정 결과입니다. 기획서 3-I-2의 네 단계를 그대로 따릅니다.
/// </summary>
public enum EJudgement
{
    /// <summary>
    /// 입력 오차가 45ms 이하인 경우입니다.
    /// </summary>
    PERFECT,

    /// <summary>
    /// 입력 오차가 45ms 초과 85ms 이하인 경우입니다.
    /// </summary>
    GREAT,

    /// <summary>
    /// 입력 오차가 85ms 초과 125ms 이하인 경우입니다.
    /// </summary>
    GOOD,

    /// <summary>
    /// 입력 오차가 125ms를 넘었거나 아예 입력하지 않은 경우입니다.
    /// </summary>
    BAD,
}
