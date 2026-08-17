using System.Collections.Generic;

/// <summary>
/// 스토리·스케줄·날짜·주차 진행 상태를 바꾸는 창구입니다. 상태를 바꾼 뒤 저장까지 함께 수행합니다.
///
/// 기획서 3-K-2는 열 곳에서 "즉시 저장"을 요구합니다. 화면마다 저장을 호출하게 하면 반드시 빠뜨리므로,
/// 상태 변경과 저장을 이 파사드 안에 묶어 누락이 구조적으로 불가능하게 만듭니다.
///
/// 리듬게임 결과 처리만 예외입니다. 보상·기록·스케줄 완료가 한 번의 저장으로 함께 남아야 하므로
/// LiveResultProcessor가 저장을 맡습니다.
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

    /// <summary>
    /// 특정 날짜의 스케줄입니다. 오늘이 아닌 날짜도 조회할 수 있어 주간 스케줄 표가 씁니다.
    /// </summary>
    public List<ScheduleData> GetSchedulesByDay(string weekId, string dayId)
    {
        return MasterDataQuery.GetSchedulesByDay(weekId, dayId);
    }

    /// <summary>
    /// 내일 예정된 스케줄입니다. 주차의 마지막 날짜라면 빈 목록입니다.
    /// </summary>
    public List<ScheduleData> GetTomorrowSchedules()
    {
        return ScheduleProgressService.GetTomorrowSchedules(Data);
    }

    /// <summary>
    /// 현재 주차의 순번입니다. WeekData 마스터 테이블이 따로 없어 오늘 스케줄의 WeekOrder에서 얻습니다.
    /// 스케줄이 하나도 없으면 1주차로 봅니다.
    /// </summary>
    public int GetCurrentWeekOrder()
    {
        List<ScheduleData> schedules = GetTodaySchedules();

        return 0 < schedules.Count ? schedules[0].WeekOrder : 1;
    }

    public int GetCurrentDayNumber()
    {
        return DayProgressService.GetDayNumber(Data, Data.CurrentDayId);
    }

    public int GetTomorrowDayNumber()
    {
        return DayProgressService.GetDayNumber(Data, DayProgressService.GetNextDayId(Data));
    }

    /// <summary>
    /// 읽기 전용 인터페이스로 돌려줍니다. 화면이 상태를 직접 바꾸면 저장이 함께 이루어지지 않아
    /// 게임을 다시 켰을 때 값이 되돌아갑니다. 기록이 없으면 null입니다.
    /// </summary>
    public IStoryProgress GetStoryProgress(string storyId)
    {
        return Data.Progress.GetStoryProgress(storyId);
    }
    #endregion

    #region 상태 변경
    /// <summary>
    /// 리듬게임 결과를 오늘의 LIVE 스케줄에 반영합니다. 완료된 스케줄 ID를 돌려줍니다.
    /// 연습실 LIVE와 조건 미충족은 아무것도 완료시키지 않으므로 빈 목록이 돌아옵니다.
    ///
    /// 이 메서드만 저장하지 않습니다. 호출부인 LiveResultProcessor가 보상과 최고 기록까지 바꾼 뒤
    /// 한 번에 저장하므로, 여기서 저장하면 같은 데이터를 두 번 쓰게 됩니다.
    /// </summary>
    public List<string> ApplyLiveResult(LiveResultData result)
    {
        return ScheduleProgressService.CompleteLiveSchedules(Data, result);
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
            GrantReward(story.RewardId);
        }

        // 바로가기가 아니라 스토리 목록을 통해 완료한 경우에도 스케줄을 완료로 인정합니다(기획서 3-E-2-3).
        List<string> completedScheduleIds = ScheduleProgressService.SyncStorySchedules(Data);

        if (isFirstCompletion || 0 < completedScheduleIds.Count)
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
        if (0 < ScheduleProgressService.SyncStorySchedules(Data).Count)
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

    /// <summary>
    /// 보상 테이블의 수치를 재화에 반영합니다(스토리 감상 보상 등). 중복 지급 방지는 호출부의 책임입니다.
    /// 비율 계산이 얽힌 LIVE 보상은 예외로 LiveResultProcessor가 처리합니다(클래스 주석 참고).
    /// </summary>
    private void GrantReward(string rewardId)
    {
        if (string.IsNullOrEmpty(rewardId))
        {
            return;
        }

        if (!MasterDataProvider.Instance.TryGetReward(rewardId, out RewardData reward))
        {
            UnityEngine.Debug.LogWarning($"[GameProgressService] 보상을 찾을 수 없어 지급하지 못했습니다: {rewardId}");
            return;
        }

        Data.Money += reward.Money;
        Data.Gem += reward.Gem;
        Data.Hype += reward.Hype;
        Data.Exp += reward.Exp;
    }

    private void Save()
    {
        PlayerDataProvider.Instance.Save();
    }
}
