/// <summary>
/// 라이브 플레이 결과에 따라 부여되는 정확도 기반 랭크입니다.
/// 각 랭크의 경계 정확도는 LiveRankEvaluator가 정합니다.
///
/// 선언 순서가 곧 우열입니다. 좋은 랭크일수록 정수 값이 작으므로,
/// "B랭크 이상" 같은 최소 조건 비교는 rank &lt;= ELiveRank.B 형태가 됩니다(부등호 방향 주의).
///
/// 세이브의 최고 기록에 저장되며 JsonUtility가 정수로 직렬화하므로, 순서를 바꾸거나 중간에 끼워 넣지 않습니다.
/// </summary>
public enum ELiveRank
{
    /// <summary>
    /// 정확도 100%이면서 모든 노트가 PERFECT인 경우입니다. 정확도만으로는 도달할 수 없습니다.
    /// </summary>
    PERFECT_S,

    S,
    A,
    B,
    C,

    /// <summary>
    /// 최저 경계에 미달했거나, 귀신 노트 실수(BAD/오입력)가 발생한 경우입니다.
    /// </summary>
    FAILED
}
