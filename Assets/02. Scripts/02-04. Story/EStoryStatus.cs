/// <summary>
/// 스토리의 진행 및 해금 상태를 정의하는 열거형입니다.
///
/// 이 값은 세이브 데이터에 저장됩니다. JsonUtility는 enum을 선언 순서에 따른 정수로 직렬화하므로,
/// 새 값은 반드시 맨 뒤에만 추가하고 기존 값의 순서를 바꾸거나 중간에 끼워 넣지 않습니다.
/// 순서를 바꾸면 기존 세이브의 COMPLETED가 다른 상태로 조용히 뒤바뀝니다.
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
