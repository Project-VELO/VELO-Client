/// <summary>
/// 주차·날짜 표시 문구를 만듭니다.
///
/// 마스터 데이터에 달력 날짜가 없어 주차 순번과 일차 번호로만 적습니다(기획 확정 전 잠정 표기).
/// 홈과 사무실이 같은 스케줄 목록을 보여주므로, 화면마다 문구가 갈라지지 않도록 여기 하나로 모읍니다.
/// </summary>
public static class ScheduleDayLabel
{
    private const string UNKNOWN_DAY = "-";

    /// <summary>
    /// dayNumber가 0이면 주차 목록에서 날짜를 찾지 못한 경우입니다.
    /// 그대로 적으면 "0일차"라는 없는 날짜가 화면에 뜨므로 빈 표시로 대신합니다.
    /// </summary>
    public static string Format(int weekOrder, int dayNumber)
    {
        if (dayNumber <= 0)
        {
            return UNKNOWN_DAY;
        }

        return $"{weekOrder}주차 {dayNumber}일차";
    }
}
