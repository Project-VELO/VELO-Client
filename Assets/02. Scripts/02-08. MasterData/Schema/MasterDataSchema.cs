/// <summary>
/// 마스터 데이터 JSON 스키마의 버전과 공용 센티널 값을 한곳에 모읍니다.
/// </summary>
public static class MasterDataSchema
{
    /// <summary>
    /// 현재 스키마 버전입니다. 필드를 추가하거나 의미를 바꿀 때 올립니다.
    /// </summary>
    public const int CURRENT_VERSION = 1;

    /// <summary>
    /// 편성 슬롯을 갖지 않는 캐릭터(P, 무명, 임원 등)의 슬롯 순서 값입니다.
    /// JsonUtility가 Nullable을 지원하지 않아 "값 없음"을 센티널로 표현합니다.
    /// </summary>
    public const int NOT_MEMBER_ORDER = -1;

    /// <summary>
    /// 최소 점수 조건을 두지 않는 스케줄의 값입니다. 현재 모든 LIVE 스케줄이 여기에 해당합니다(기획서 3-E-2-1).
    /// </summary>
    public const int NO_SCORE_REQUIREMENT = 0;
}
