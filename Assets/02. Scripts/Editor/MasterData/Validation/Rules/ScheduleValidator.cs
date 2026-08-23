using System.Collections.Generic;

/// <summary>
/// 일일 스케줄의 완료 조건이 실제로 달성 가능한지 검사합니다.
/// 지정곡이나 대상 스토리가 없으면 그 스케줄은 영원히 완료할 수 없고, 하루가 넘어가지 않아 진행이 막힙니다.
/// </summary>
public class ScheduleValidator
{
    public void Validate(MasterDataProvider provider, MasterDataValidationReport report)
    {
        foreach (KeyValuePair<string, ScheduleData> pair in provider.Schedules)
        {
            ScheduleData schedule = pair.Value;

            if (!string.IsNullOrEmpty(schedule.RewardId) && !provider.TryGetReward(schedule.RewardId, out _))
            {
                report.AddError($"[스케줄] {schedule.ScheduleId}의 RewardId '{schedule.RewardId}'를 rewards.json에서 찾을 수 없습니다.");
            }

            if (schedule.ScheduleType == EScheduleType.LIVE)
            {
                ValidateLiveSchedule(schedule, report);
            }
            else if (schedule.ScheduleType == EScheduleType.STORY)
            {
                ValidateStorySchedule(provider, schedule, report);
            }
        }

        ValidateDailyScheduleCounts(provider, report);
    }

    private void ValidateLiveSchedule(ScheduleData schedule, MasterDataValidationReport report)
    {
        string songId = schedule.ClearCondition.TargetSongId;

        if (string.IsNullOrEmpty(songId))
        {
            report.AddError($"[스케줄] {schedule.ScheduleId}가 LIVE인데 지정곡이 비어 있습니다.");
            return;
        }

        if (!LiveSongCatalog.Instance.TryGetSong(songId, out SongData song))
        {
            report.AddError($"[스케줄] {schedule.ScheduleId}의 지정곡 '{songId}'가 수록 목록에 없습니다.");
            return;
        }

        // 곡은 수록되어 있어도 해당 난이도 채보가 없으면 플레이에 진입할 수 없습니다(기획서 3-L).
        if (!song.Charts.ContainsKey(schedule.ClearCondition.RequiredDifficulty))
        {
            report.AddError($"[스케줄] {schedule.ScheduleId}의 지정곡 '{songId}'에 {schedule.ClearCondition.RequiredDifficulty} 채보가 수록되어 있지 않습니다.");
        }
    }

    private void ValidateStorySchedule(MasterDataProvider provider, ScheduleData schedule, MasterDataValidationReport report)
    {
        string storyId = schedule.ClearCondition.TargetStoryId;

        if (string.IsNullOrEmpty(storyId))
        {
            report.AddError($"[스케줄] {schedule.ScheduleId}가 스토리 감상인데 대상 스토리가 비어 있습니다.");
            return;
        }

        if (!provider.TryGetStory(storyId, out _))
        {
            report.AddError($"[스케줄] {schedule.ScheduleId}의 대상 스토리 '{storyId}'를 stories.json에서 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 하루에 필수 스케줄이 몇 개인지 검사합니다.
    ///
    /// 요일마다 개수가 달라도 됩니다. 다만 화면의 스케줄 줄이 세 개 고정이라 그보다 많으면
    /// 그릴 자리가 없고, 하나도 없으면 마무리할 것이 없어 날짜가 넘어가지 않습니다.
    /// </summary>
    private void ValidateDailyScheduleCounts(MasterDataProvider provider, MasterDataValidationReport report)
    {
        var requiredCounts = new Dictionary<string, int>();

        foreach (KeyValuePair<string, ScheduleData> pair in provider.Schedules)
        {
            ScheduleData schedule = pair.Value;

            if (!schedule.IsRequired)
            {
                continue;
            }

            string dayKey = $"{schedule.WeekId}/{schedule.DayId}";
            requiredCounts.TryGetValue(dayKey, out int count);
            requiredCounts[dayKey] = count + 1;
        }

        foreach (KeyValuePair<string, int> pair in requiredCounts)
        {
            if (pair.Value < MasterDataValidationRule.MIN_REQUIRED_SCHEDULES_PER_DAY
                || MasterDataValidationRule.MAX_REQUIRED_SCHEDULES_PER_DAY < pair.Value)
            {
                report.AddWarning($"[스케줄] {pair.Key}의 필수 스케줄이 {pair.Value}개입니다. "
                    + $"{MasterDataValidationRule.MIN_REQUIRED_SCHEDULES_PER_DAY}~{MasterDataValidationRule.MAX_REQUIRED_SCHEDULES_PER_DAY}개여야 합니다.");
            }
        }
    }
}
