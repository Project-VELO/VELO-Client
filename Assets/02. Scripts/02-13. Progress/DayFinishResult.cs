using System.Collections.Generic;

/// <summary>
/// 하루 마무리 처리의 결과입니다. 사무실 화면이 어떤 연출을 이어서 재생할지 판단하는 근거가 됩니다(기획서 3-E-3-1).
///
/// 마지막 날짜에서는 하루 마무리 연출 뒤에 주차 마무리 연출이 이어지고 다음 날짜로의 하이라이트 이동이 없으므로,
/// 화면이 두 경우를 구분할 수 있어야 합니다.
/// </summary>
public class DayFinishResult
{
    public static readonly DayFinishResult Failed = new DayFinishResult();

    public bool IsSuccess { get; private set; }

    /// <summary>
    /// 주차의 마지막 날짜를 마무리해 주차까지 완료된 경우입니다.
    /// </summary>
    public bool IsWeekFinished { get; private set; }

    /// <summary>
    /// 이번 마무리로 넘어간 다음 날짜입니다. 주차가 끝났다면 비어 있습니다.
    /// </summary>
    public string NextDayId { get; private set; }

    /// <summary>
    /// 주차 완료로 해금된 스토리입니다. 해금이 없었으면 비어 있습니다.
    /// </summary>
    public IReadOnlyList<string> UnlockedStoryIds { get; private set; } = new List<string>();

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
            UnlockedStoryIds = unlockedStoryIds,
        };
    }
}
