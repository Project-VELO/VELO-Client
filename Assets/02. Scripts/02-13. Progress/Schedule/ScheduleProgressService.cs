using System.Collections.Generic;

/// <summary>
/// 일일 스케줄의 완료 상태를 다룹니다(기획서 3-E-2).
///
/// 상태 변경만 수행하고 저장은 하지 않습니다. 저장 시점은 GameProgressService가 관리합니다.
/// </summary>
public static class ScheduleProgressService
{
    public static List<ScheduleData> GetTodaySchedules(PlayerData data)
    {
        return MasterDataQuery.GetSchedulesByDay(data.CurrentWeekId, data.CurrentDayId);
    }

    /// <summary>
    /// 오늘의 필수 스케줄이 모두 완료되었는지 여부입니다. 하루 마무리를 허용하는 조건입니다(기획서 3-E-3).
    /// 스케줄이 하나도 없는 날짜는 마무리할 수 없는 것으로 봅니다. 데이터 누락을 완료로 오인하지 않기 위해서입니다.
    /// </summary>
    public static bool IsDayCompletable(PlayerData data)
    {
        List<ScheduleData> schedules = GetTodaySchedules(data);

        if (schedules.Count == 0)
        {
            return false;
        }

        foreach (ScheduleData schedule in schedules)
        {
            if (schedule.IsRequired && !data.Progress.IsScheduleCompleted(schedule.ScheduleId))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 스케줄을 완료 처리합니다. 이미 완료된 스케줄은 다시 완료 처리하지 않습니다(기획서 3-E-6).
    /// </summary>
    public static bool TryCompleteSchedule(GameProgressData progress, string scheduleId)
    {
        if (string.IsNullOrEmpty(scheduleId) || progress.IsScheduleCompleted(scheduleId))
        {
            return false;
        }

        progress.ScheduleProgresses[scheduleId] = EScheduleStatus.COMPLETED;
        return true;
    }

    /// <summary>
    /// 리듬게임 결과로 완료되는 오늘의 LIVE 스케줄을 처리하고, 완료된 스케줄 ID를 돌려줍니다.
    ///
    /// 진입 경로가 스케줄 LIVE였는지 홈 LIVE였는지는 보지 않고 오늘의 미완료 스케줄을 모두 대조합니다.
    /// 홈 화면의 일반 LIVE로 진입해도 조건이 맞으면 완료로 인정해야 하기 때문입니다(기획서 3-E-2 일일퀘스트 7번).
    /// </summary>
    public static List<string> CompleteLiveSchedules(PlayerData data, LiveResultData result)
    {
        var completedScheduleIds = new List<string>();

        if (ReferenceEquals(result, null) || !ScheduleCompletionRule.ReflectsToSchedule(result.EntryType))
        {
            return completedScheduleIds;
        }

        foreach (ScheduleData schedule in GetTodaySchedules(data))
        {
            if (data.Progress.IsScheduleCompleted(schedule.ScheduleId))
            {
                continue;
            }

            if (!ScheduleCompletionRule.IsSatisfiedBy(schedule, result))
            {
                continue;
            }

            if (TryCompleteSchedule(data.Progress, schedule.ScheduleId))
            {
                completedScheduleIds.Add(schedule.ScheduleId);
            }
        }

        return completedScheduleIds;
    }

    /// <summary>
    /// 대상 스토리를 이미 완료한 스토리 감상 스케줄을 완료 처리하고, 완료된 스케줄 ID를 돌려줍니다.
    ///
    /// 기획서 3-E-2-3은 두 가지를 요구합니다. 바로가기가 아니라 스토리 목록을 통해 완료해도 인정할 것,
    /// 그리고 스케줄이 표시되기 전에 이미 완료했다면 자동으로 완료 처리할 것.
    /// 둘 다 "대상 스토리가 완료 상태인가"만 보면 되므로 한 곳에서 처리합니다.
    /// </summary>
    public static List<string> SyncStorySchedules(PlayerData data)
    {
        var completedScheduleIds = new List<string>();

        foreach (ScheduleData schedule in GetTodaySchedules(data))
        {
            if (schedule.ScheduleType != EScheduleType.STORY || data.Progress.IsScheduleCompleted(schedule.ScheduleId))
            {
                continue;
            }

            if (!StoryProgressService.IsCompleted(data.Progress, schedule.ClearCondition.TargetStoryId))
            {
                continue;
            }

            if (TryCompleteSchedule(data.Progress, schedule.ScheduleId))
            {
                completedScheduleIds.Add(schedule.ScheduleId);
            }
        }

        return completedScheduleIds;
    }
}
