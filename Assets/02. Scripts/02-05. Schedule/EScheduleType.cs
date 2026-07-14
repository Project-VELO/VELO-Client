/// <summary>
/// 스케줄의 활동 유형을 정의하는 열거형입니다.
/// </summary>
public enum EScheduleType
{
    /// <summary>
    /// 스토리 감상 스케줄입니다.
    /// </summary>
    STORY,

    /// <summary>
    /// 리듬게임 플레이 스케줄입니다.
    /// </summary>
    LIVE,

    /// <summary>
    /// 하루 마무리, 주차 완료 연출 등 시스템에 의해 정의된 스케줄입니다. (확장성 고려)
    /// </summary>
    SYSTEM
}
