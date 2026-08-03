/// <summary>
/// 날짜 진행 상태의 키 형식을 한곳에 고정합니다(SongRecordKey와 같은 역할).
///
/// 날짜 ID는 주차마다 다시 쓰이므로(1주차와 2주차 모두 DAY_001을 가집니다)
/// 주차와 조합해야 유일해집니다. 조립 규칙이 호출부마다 갈라지면 완료한 날짜 조회가 조용히 빗나갑니다.
/// </summary>
public static class DayProgressKey
{
    public static string Create(string weekId, string dayId)
    {
        return $"{weekId}_{dayId}";
    }
}
