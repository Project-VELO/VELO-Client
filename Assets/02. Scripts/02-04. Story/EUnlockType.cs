/// <summary>
/// 스토리 및 곡의 해금 조건 유형을 정의하는 열거형입니다.
/// </summary>
public enum EUnlockType
{
    /// <summary>
    /// 조건 없이 항상 해금되어 있는 상태입니다.
    /// </summary>
    NONE,

    /// <summary>
    /// 특정 주차를 완료하면 해금되는 조건입니다.
    /// </summary>
    WEEK_CLEAR,

    /// <summary>
    /// 특정 선행 스토리를 완료하면 해금되는 조건입니다.
    /// </summary>
    PREVIOUS_STORY_CLEAR
}
