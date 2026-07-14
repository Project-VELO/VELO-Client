/// <summary>
/// 스토리의 진행 및 해금 상태를 정의하는 열거형입니다.
/// </summary>
public enum EStoryStatus
{
    /// <summary>
    /// 스토리가 잠겨 있어 감상할 수 없는 상태입니다.
    /// </summary>
    LOCKED,

    /// <summary>
    /// 스토리의 잠금이 해제되어 감상 가능한 상태입니다.
    /// </summary>
    UNLOCKED,

    /// <summary>
    /// 스토리를 이미 끝까지 완료한 상태입니다.
    /// </summary>
    COMPLETED
}
