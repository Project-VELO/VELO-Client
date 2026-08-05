/// <summary>
/// 마스터 데이터 검사가 기준으로 삼는 기획 수치입니다.
/// 검사 코드 여기저기에 숫자를 흩어 두면 기획이 바뀔 때 일부만 고치게 되므로 한곳에 모읍니다.
/// </summary>
public static class MasterDataValidationRule
{
    /// <summary>
    /// LIVE 편성 인원이자 멤버 수입니다(기획서 3-H-1, 3-H-2).
    /// </summary>
    public const int MEMBER_COUNT = 5;

    /// <summary>
    /// 하루에 배치되는 필수 스케줄 수입니다(기획서 3-C-2).
    /// </summary>
    public const int REQUIRED_SCHEDULES_PER_DAY = 3;
}
