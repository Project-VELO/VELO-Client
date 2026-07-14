/// <summary>
/// 라이브 플레이 결과에 따라 부여되는 정확도 기반 랭크입니다.
/// </summary>
public enum ELiveRank
{
    /// <summary>
    /// 정확도 100% 달성 및 모든 노트 PERFECT 판정인 경우입니다.
    /// </summary>
    PERFECT_S,

    /// <summary>
    /// 정확도 97.00% 이상인 경우입니다.
    /// </summary>
    S,

    /// <summary>
    /// 정확도 95.00% 이상 97.00% 미만인 경우입니다.
    /// </summary>
    A,

    /// <summary>
    /// 정확도 85.00% 이상 95.00% 미만인 경우입니다.
    /// </summary>
    B,

    /// <summary>
    /// 정확도 70.00% 이상 85.00% 미만인 경우입니다.
    /// </summary>
    C,

    /// <summary>
    /// 정확도 70.00% 미만이거나, 귀신 노트 실수(BAD/오입력)가 발생한 경우입니다.
    /// </summary>
    FAILED
}
