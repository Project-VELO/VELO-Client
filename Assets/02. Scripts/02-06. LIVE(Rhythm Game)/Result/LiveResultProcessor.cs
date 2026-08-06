using System.Collections.Generic;

/// <summary>
/// 결과 화면 진입 시 한 번만 수행해야 하는 뒤처리를 기획서 3-K-3의 순서대로 묶습니다.
///
/// 결과 확정 → 최고 기록 갱신 여부 확인 → 보상 지급 → 일일 스케줄 완료 조건 확인 → 진행 상태 저장
///
/// 결과 데이터의 지급 완료 표시를 관문으로 삼으므로, 화면을 다시 열어도 두 번 실행되지 않습니다(3-J-3).
/// 보상 계산(구 LiveRewardService)과 최고 기록(구 LiveRecordService)은 이 순서 안에서만 쓰여 병합했습니다.
/// 두 단계를 절반만 수행할 수 없도록 진입점을 ApplyResult 하나로 유지하는 의도적 static입니다(선례: LiveEntryStarter).
/// </summary>
public static class LiveResultProcessor
{
    public static void ApplyResult(LiveResultData result)
    {
        if (ReferenceEquals(result, null) || result.IsRewardClaimed)
        {
            return;
        }

        // 기록을 먼저 갱신하면 이번 판이 최초 CLEAR였는지 알 수 없게 되므로, 보상 비율 판단을 앞에 둡니다.
        bool isFirstClear = IsFirstClear(result.SongId);

        GrantReward(result, isFirstClear);
        result.IsNewBest = TryUpdateRecord(result);
        result.IsRewardClaimed = true;

        // 조건 미충족과 이미 완료된 스케줄은 이 안에서 걸러지므로 여기서 진입 경로를 따로 보지 않습니다.
        // 이번에 완료된 스케줄이 있어야만 확인 버튼이 사무실로 향합니다(기획서 9.4, LiveResultReturnTarget).
        List<string> completedScheduleIds = GameProgressService.Instance.ApplyLiveResult(result);
        result.HasCompletedSchedule = 0 < completedScheduleIds.Count;

        // 보상·최고 기록·스케줄 완료를 한 번에 저장합니다. ApplyLiveResult가 따로 저장하지 않는 것은 이 때문입니다.
        PlayerDataProvider.Instance.Save();
    }

    /// <summary>
    /// 이번 결과를 반영하기 "전" 기준으로, 그 곡을 아직 한 번도 클리어하지 않았는지 여부입니다.
    /// 보상 비율이 최초 CLEAR인지에 따라 갈리므로(3-J-3) 기록을 갱신하기 전에 확인해야 합니다.
    /// </summary>
    private static bool IsFirstClear(string songId)
    {
        SongRecord record = PlayerDataProvider.Instance.GetBestSongRecord(songId);

        return ReferenceEquals(record, null) || record.BestRank == ELiveRank.FAILED;
    }

    /// <summary>
    /// 플레이 결과에 따른 보상을 계산해 플레이어 재화에 반영합니다(기획서 3-J-4).
    /// </summary>
    private static void GrantReward(LiveResultData result, bool isFirstClear)
    {
        // FAILED와 중도 종료는 보상이 없지만, 다시 계산하지 않도록 지급 완료 표시는 동일하게 남깁니다.
        if (!result.IsClear)
        {
            return;
        }

        float rate = LiveRewardRule.GetRewardRate(result.EntryType, isFirstClear);
        LiveRewardRule.GetReward(rate, out int money, out int hype, out int exp);

        PlayerData data = PlayerDataProvider.Instance.Data;
        data.Money += money;
        data.Hype += hype;
        data.Exp += exp;

        result.EarnedMoney = money;
        result.EarnedHype = hype;
        result.EarnedExp = exp;
    }

    /// <summary>
    /// 곡·난이도별 최고 기록 갱신을 시도하고, 갱신되었으면 true를 돌려줍니다. 결과 화면의 NEW BEST 표시에 사용합니다.
    /// FAILED 결과는 최고 기록에 반영하지 않으며, 동점은 갱신으로 보지 않습니다(SongRecord.TryUpdateRecord).
    /// </summary>
    private static bool TryUpdateRecord(LiveResultData result)
    {
        if (!result.IsClear)
        {
            return false;
        }

        PlayerData data = PlayerDataProvider.Instance.Data;
        string key = SongRecordKey.Create(result.SongId, result.Difficulty);

        if (!data.Progress.SongRecords.TryGetValue(key, out SongRecord record))
        {
            record = new SongRecord();
            data.Progress.SongRecords[key] = record;
        }

        return record.TryUpdateRecord(result.Score, result.Rank, result.IsFullCombo);
    }
}
