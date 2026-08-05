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
    /// 시작 타이밍에 맞춰 누른 뒤 HoldDurationMs 동안 유지해야 하는 롱노트입니다.
    /// 판정은 LiveHoldTracker에 구현되어 있으나 현재 채보에서는 쓰지 않기로 하여
    /// 에디터의 배치 경로가 잠겨 있습니다(LiveEditorInputHandler.IS_LONG_NOTE_PLACEMENT_ENABLED).
    /// </summary>
    LONG,
}
