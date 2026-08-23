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
    /// 하루에 배치할 수 있는 필수 스케줄 수의 한계입니다.
    ///
    /// 상한은 화면이 정합니다. 홈과 사무실의 스케줄 줄이 프리팹에 세 개씩 고정으로 놓여 있어,
    /// 네 개째부터는 그릴 자리가 없습니다.
    ///
    /// 하한이 1인 것은 주간 시안이 요일마다 개수를 다르게 두기 때문입니다(토요일은 한 건).
    /// 하루가 비어 있으면 마무리할 것이 없어 날짜가 넘어가지 않으므로 0은 허용하지 않습니다.
    /// </summary>
    public const int MIN_REQUIRED_SCHEDULES_PER_DAY = 1;

    public const int MAX_REQUIRED_SCHEDULES_PER_DAY = 3;
}
