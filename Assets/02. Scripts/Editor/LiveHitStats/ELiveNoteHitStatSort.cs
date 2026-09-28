/// <summary>
/// 노트 판정 통계 표의 정렬 기준입니다. 순서는 조회 창 정렬 버튼의 순서와 같습니다.
/// </summary>
public enum ELiveNoteHitStatSort
{
    /// <summary>
    /// GOOD과 BAD가 판정에서 차지하는 비율이 높은 노트부터 보여 줍니다.
    /// </summary>
    PROBLEM_RATE,

    /// <summary>
    /// 채보 평균 대비 편차가 큰 노트부터 보여 줍니다. 오차 표본이 없는 노트는 맨 뒤로 보냅니다.
    /// </summary>
    DEVIATION,

    TIME,
}
