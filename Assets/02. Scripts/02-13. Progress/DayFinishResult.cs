using System.Collections.Generic;

/// <summary>
/// 하루 마무리 처리의 결과입니다. 사무실 화면이 어떤 연출을 이어서 재생할지 판단하는 근거가 됩니다(기획서 3-E-3-1).
///
/// 마지막 날짜에서는 하루 마무리 연출 뒤에 주차 마무리 연출이 이어지고 다음 날짜로의 하이라이트 이동이 없으므로,
/// 화면이 두 경우를 구분할 수 있어야 합니다.
/// </summary>
public class DayFinishResult
{
    // 해금이 없었을 때 돌려줄 빈 목록입니다. 결과 객체마다 새로 만들 이유가 없어 하나를 공유합니다.
    // Failed보다 먼저 선언해야 합니다. static 초기화는 선언 순서대로 실행되므로,
    // 뒤에 두면 Failed가 만들어지는 시점에 이 값이 아직 null입니다.
    private static readonly List<string> EMPTY_STORY_IDS = new List<string>();

    public static readonly DayFinishResult Failed = new DayFinishResult();

    public bool IsSuccess { get; private set; }

    /// <summary>
    /// 주차의 마지막 날짜를 마무리해 주차까지 완료된 경우입니다.
    /// </summary>
    public bool IsWeekFinished { get; private set; }

    /// <summary>
    /// 이번 마무리로 넘어간 다음 날짜입니다. 주차가 끝났거나 마무리에 실패했다면 비어 있습니다.
    /// 실패와 주차 종료가 서로 다른 값(null과 빈 문자열)을 내면 호출부가 둘을 다르게 다루게 되므로 기본값을 맞춥니다.
    /// </summary>
    public string NextDayId { get; private set; } = string.Empty;

    /// <summary>
    /// 주차 완료로 해금된 스토리입니다. 해금이 없었으면 비어 있습니다.
    /// </summary>
    public IReadOnlyList<string> UnlockedStoryIds { get; private set; } = EMPTY_STORY_IDS;

    public static DayFinishResult ToNextDay(string nextDayId)
    {
        return new DayFinishResult
        {
            IsSuccess = true,
            IsWeekFinished = false,
            NextDayId = nextDayId,
        };
    }

    public static DayFinishResult ToWeekEnd(List<string> unlockedStoryIds)
    {
        return new DayFinishResult
        {
            IsSuccess = true,
            IsWeekFinished = true,
            NextDayId = string.Empty,
            UnlockedStoryIds = unlockedStoryIds ?? EMPTY_STORY_IDS,
        };
    }
}
