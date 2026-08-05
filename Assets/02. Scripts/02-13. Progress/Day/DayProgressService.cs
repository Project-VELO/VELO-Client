using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 하루 마무리와 주차 마무리를 처리합니다(기획서 3-E-3, 3-E-4).
///
/// LIVE 결과만으로 날짜가 넘어가지 않는다는 것이 이 계층의 핵심 규칙입니다.
/// 필수 스케줄을 모두 마쳐도 플레이어가 날짜를 직접 클릭해야 다음 날짜로 넘어갑니다(기획서 3-K-3, 16-7).
///
/// 상태 변경만 수행하고 저장은 하지 않습니다. 저장 시점은 GameProgressService가 관리합니다.
/// </summary>
public static class DayProgressService
{
    /// <summary>
    /// 현재 날짜를 마무리합니다. 필수 스케줄이 남아 있으면 아무것도 하지 않고 실패를 돌려줍니다.
    /// 이미 완료한 날짜를 다시 마무리하려는 경우도 막으므로, 날짜를 연속으로 클릭해도 두 날짜가 넘어가지 않습니다.
    /// </summary>
    public static DayFinishResult TryFinishDay(PlayerData data)
    {
        if (!ScheduleProgressService.IsDayCompletable(data))
        {
            return DayFinishResult.Failed;
        }

        GameProgressData progress = data.Progress;

        if (progress.IsDayCompleted(data.CurrentWeekId, data.CurrentDayId))
        {
            return DayFinishResult.Failed;
        }

        // 날짜 위치를 상태를 바꾸기 전에 확인합니다.
        // "목록에 없는 날짜"와 "주차의 마지막 날짜"를 같이 취급하면, 데이터 조회에 실패했을 때
        // 주차가 마무리되고 스토리가 잘못 해금됩니다. 날짜 순서는 문자열 ID로 알 수 없으므로
        // 마스터 데이터의 DayOrder 정렬 결과를 따릅니다.
        List<string> dayIds = MasterDataQuery.GetDayIdsByWeek(data.CurrentWeekId);
        int currentIndex = dayIds.IndexOf(data.CurrentDayId);

        if (currentIndex < 0)
        {
            Debug.LogWarning($"[DayProgressService] 현재 날짜를 주차 목록에서 찾지 못해 하루를 마무리하지 않습니다: {data.CurrentWeekId} / {data.CurrentDayId}");
            return DayFinishResult.Failed;
        }

        progress.CompletedDayKeys.Add(DayProgressKey.Create(data.CurrentWeekId, data.CurrentDayId));

        bool hasNextDay = currentIndex < dayIds.Count - 1;

        if (hasNextDay)
        {
            string nextDayId = dayIds[currentIndex + 1];
            data.CurrentDayId = nextDayId;
            return DayFinishResult.ToNextDay(nextDayId);
        }

        return FinishWeek(data);
    }

    /// <summary>
    /// 주차 안에서 이 날짜가 몇 번째인지입니다(1일차 = 1). 목록에 없으면 0입니다.
    /// 마스터 데이터에 달력 날짜가 없어 DayOrder 정렬 목록의 위치로 대신합니다.
    /// </summary>
    public static int GetDayNumber(PlayerData data, string dayId)
    {
        List<string> dayIds = MasterDataQuery.GetDayIdsByWeek(data.CurrentWeekId);
        int index = dayIds.IndexOf(dayId);

        return index < 0 ? 0 : index + 1;
    }

    /// <summary>
    /// 다음 날짜의 ID입니다. 주차의 마지막 날짜이거나 목록에 없으면 빈 문자열입니다.
    /// TryFinishDay가 실제로 넘어갈 날짜를 고르는 것과 같은 목록·같은 순서를 씁니다.
    /// 둘이 갈라지면 홈 화면이 예고한 내일과 실제로 넘어가는 날짜가 달라집니다.
    /// </summary>
    public static string GetNextDayId(PlayerData data)
    {
        List<string> dayIds = MasterDataQuery.GetDayIdsByWeek(data.CurrentWeekId);
        int index = dayIds.IndexOf(data.CurrentDayId);

        if (index < 0 || dayIds.Count <= index + 1)
        {
            return string.Empty;
        }

        return dayIds[index + 1];
    }

    /// <summary>
    /// 날짜 한 칸의 화면 상태입니다. 화면마다 같은 판정을 다시 짜면 홈과 사무실의 표시가 어긋나므로 여기로 모읍니다.
    /// </summary>
    public static EDayViewState GetDayViewState(PlayerData data, string weekId, string dayId)
    {
        if (data.Progress.IsDayCompleted(weekId, dayId))
        {
            return EDayViewState.COMPLETED;
        }

        if (weekId != data.CurrentWeekId || dayId != data.CurrentDayId)
        {
            return EDayViewState.LOCKED;
        }

        return ScheduleProgressService.IsDayCompletable(data) ? EDayViewState.COMPLETABLE : EDayViewState.IN_PROGRESS;
    }

    /// <summary>
    /// 주차를 완료 처리하고 연결된 스토리를 해금합니다.
    /// 완료한 주차를 기록해 두므로 같은 주차에서 해금이 두 번 실행되지 않습니다(기획서 16-7).
    /// </summary>
    private static DayFinishResult FinishWeek(PlayerData data)
    {
        GameProgressData progress = data.Progress;

        if (progress.IsWeekCompleted(data.CurrentWeekId))
        {
            return DayFinishResult.ToWeekEnd(new List<string>());
        }

        progress.CompletedWeekIds.Add(data.CurrentWeekId);

        List<string> unlockedStoryIds = StoryProgressService.UnlockStoriesByWeek(progress, data.CurrentWeekId);

        return DayFinishResult.ToWeekEnd(unlockedStoryIds);
    }
}
