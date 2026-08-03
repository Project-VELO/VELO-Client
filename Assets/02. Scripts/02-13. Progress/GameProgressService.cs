using System.Collections.Generic;

/// <summary>
/// 게임 진행 상태를 바꾸는 유일한 경로입니다. 상태를 바꾼 뒤 저장까지 함께 수행합니다.
///
/// 기획서 3-K-2는 열 곳에서 "즉시 저장"을 요구합니다. 화면마다 저장을 호출하게 하면 반드시 빠뜨리므로,
/// 상태 변경과 저장을 이 파사드 안에 묶어 누락이 구조적으로 불가능하게 만듭니다.
/// 개별 판정과 상태 전환은 각 서비스가 담당하고, 이 클래스는 호출 순서와 저장만 책임집니다.
/// </summary>
public class GameProgressService : POCOSingleton<GameProgressService>
{
    private PlayerData Data => PlayerDataProvider.Instance.Data;

    #region 조회
    public List<ScheduleData> GetTodaySchedules()
    {
        return ScheduleProgressService.GetTodaySchedules(Data);
    }

    public bool IsScheduleCompleted(string scheduleId)
    {
        return Data.Progress.IsScheduleCompleted(scheduleId);
    }

    public bool IsDayCompletable()
    {
        return ScheduleProgressService.IsDayCompletable(Data);
    }

    public EDayViewState GetDayViewState(string weekId, string dayId)
    {
        return DayProgressService.GetDayViewState(Data, weekId, dayId);
    }

    public List<string> GetCurrentWeekDayIds()
    {
        return MasterDataQuery.GetDayIdsByWeek(Data.CurrentWeekId);
    }

    public StoryProgress GetStoryProgress(string storyId)
    {
        return Data.Progress.GetStoryProgress(storyId);
    }
    #endregion

    #region 상태 변경
    /// <summary>
    /// 리듬게임 결과를 오늘의 LIVE 스케줄에 반영합니다. 완료된 스케줄 ID를 돌려줍니다.
    /// 연습실 LIVE와 조건 미충족은 아무것도 완료시키지 않으므로 빈 목록이 돌아옵니다.
    /// </summary>
    public List<string> ApplyLiveResult(LiveResultData result)
    {
        List<string> completedScheduleIds = ScheduleProgressService.CompleteLiveSchedules(Data, result);

        if (completedScheduleIds.Count > 0)
        {
            Save();
        }

        return completedScheduleIds;
    }

    /// <summary>
    /// 스토리를 완료 처리하고, 최초 완료라면 감상 보상까지 지급합니다(기획서 3-J-4).
    /// 대상 스토리를 요구하는 오늘의 스케줄이 있으면 함께 완료 처리합니다.
    /// </summary>
    public bool CompleteStory(string storyId)
    {
        bool isFirstCompletion = StoryProgressService.TryCompleteStory(Data.Progress, storyId);

        if (isFirstCompletion && MasterDataProvider.Instance.TryGetStory(storyId, out StoryData story))
        {
            RewardService.TryGrant(story.RewardId);
        }

        // 바로가기가 아니라 스토리 목록을 통해 완료한 경우에도 스케줄을 완료로 인정합니다(기획서 3-E-2-3).
        List<string> completedScheduleIds = ScheduleProgressService.SyncStorySchedules(Data);

        if (isFirstCompletion || completedScheduleIds.Count > 0)
        {
            Save();
        }

        return isFirstCompletion;
    }

    /// <summary>
    /// 스토리 감상 화면에 진입할 때 호출해 NEW 배지를 내립니다(기획서 3-F-3).
    /// </summary>
    public void ClearStoryNewFlag(string storyId)
    {
        if (StoryProgressService.TryClearNewFlag(Data.Progress, storyId))
        {
            Save();
        }
    }

    /// <summary>
    /// 이미 완료한 스토리에 대응하는 오늘의 스케줄을 완료 처리합니다.
    /// 스케줄이 표시되기 전에 대상 스토리를 미리 완료한 경우를 자동 완료로 흡수합니다(기획서 3-E-2-3).
    /// 홈·사무실 화면에 진입할 때 호출합니다.
    /// </summary>
    public void SyncStorySchedules()
    {
        if (ScheduleProgressService.SyncStorySchedules(Data).Count > 0)
        {
            Save();
        }
    }

    /// <summary>
    /// 하루를 마무리합니다. 필수 스케줄이 남아 있으면 실패를 돌려주며 상태를 바꾸지 않습니다.
    /// 주차의 마지막 날짜였다면 주차 완료와 스토리 해금까지 이어서 처리합니다.
    /// </summary>
    public DayFinishResult FinishDay()
    {
        DayFinishResult result = DayProgressService.TryFinishDay(Data);

        if (result.IsSuccess)
        {
            Save();
        }

        return result;
    }

    /// <summary>
    /// 디버그 도구에서 스케줄을 강제로 완료시킬 때 사용합니다.
    /// 리듬게임을 실제로 클리어하지 않고 진행을 확인하기 위한 경로이므로 완료 조건을 검사하지 않습니다.
    /// </summary>
    public bool ForceCompleteSchedule(string scheduleId)
    {
        if (!ScheduleProgressService.TryCompleteSchedule(Data.Progress, scheduleId))
        {
            return false;
        }

        Save();
        return true;
    }
    #endregion

    private void Save()
    {
        PlayerDataProvider.Instance.Save();
    }
}
