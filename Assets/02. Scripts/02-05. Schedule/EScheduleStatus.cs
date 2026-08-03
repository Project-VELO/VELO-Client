/// <summary>
/// 스케줄의 진행 상태를 정의하는 열거형입니다.
///
/// 이 값은 세이브 데이터에 저장됩니다. JsonUtility는 enum을 선언 순서에 따른 정수로 직렬화하므로,
/// 새 값은 반드시 맨 뒤에만 추가하고 기존 값의 순서를 바꾸거나 중간에 끼워 넣지 않습니다.
/// </summary>
public enum EScheduleStatus
{
    /// <summary>
    /// 스케줄이 아직 시작되지 않은 상태입니다.
    /// </summary>
    NOT_STARTED,

    /// <summary>
    /// 플레이어가 스케줄 진행을 시도 중이거나 대기 중인 상태입니다.
    /// </summary>
    IN_PROGRESS,

    /// <summary>
    /// 스케줄을 완료한 상태입니다.
    /// </summary>
    COMPLETED
}
