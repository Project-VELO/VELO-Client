/// <summary>
/// PlayerData.SongRecords의 키 형식을 한곳에 고정합니다.
/// 기록은 곡과 난이도 조합별로 관리되므로, 키 조립 규칙이 호출부마다 갈라지면 기록 조회가 조용히 빗나갑니다.
/// </summary>
public static class SongRecordKey
{
    public static string Create(string songId, EDifficulty difficulty)
    {
        return $"{songId}_{difficulty}";
    }
}
