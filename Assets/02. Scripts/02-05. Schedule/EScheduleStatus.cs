/// <summary>
/// 스케줄의 진행 상태입니다.
///
/// 세이브 데이터에 저장됩니다. JsonUtility가 enum을 선언 순서에 따른 정수로 직렬화하므로
/// 새 값은 반드시 맨 뒤에만 추가합니다.
/// </summary>
public enum EScheduleStatus
{
    NOT_STARTED,
    IN_PROGRESS,
    COMPLETED
}
