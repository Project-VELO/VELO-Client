/// <summary>
/// 리듬게임의 노트 입력 유형을 정의하는 열거형입니다.
/// </summary>
public enum ENoteType
{
    /// <summary>
    /// 일반 레인(1~5번)에서 처리하는 기본 단타형 노트입니다.
    /// </summary>
    NORMAL,

    /// <summary>
    /// 6번 레인에서 처리하며, 입력 실패(BAD) 시 즉시 FAILED가 유발되는 귀신 노트입니다.
    /// </summary>
    GHOST,

    /// <summary>
    /// 길게 누르고 있어야 하는 롱노트입니다. (추후 확장용)
    /// </summary>
    LONG,

    /// <summary>
    /// 드래그로 입력하는 슬라이드 노트입니다. (추후 확장용)
    /// </summary>
    SLIDE,

    /// <summary>
    /// 튕겨서 입력하는 플릭 노트입니다. (추후 확장용)
    /// </summary>
    FLICK
}
