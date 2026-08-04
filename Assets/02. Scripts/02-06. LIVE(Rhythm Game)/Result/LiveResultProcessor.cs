using System.Collections.Generic;

/// <summary>
/// 결과 화면 진입 시 한 번만 수행해야 하는 뒤처리를 기획서 3-K-3의 순서대로 묶습니다.
///
/// 결과 확정 → 최고 기록 갱신 여부 확인 → 보상 지급 → 일일 스케줄 완료 조건 확인 → 진행 상태 저장
///
/// 결과 데이터의 지급 완료 표시를 관문으로 삼으므로, 화면을 다시 열어도 두 번 실행되지 않습니다(3-J-3).
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
        bool isFirstClear = LiveRecordService.IsFirstClear(result.SongId);

        LiveRewardService.GrantReward(result, isFirstClear);
        result.IsNewBest = LiveRecordService.TryUpdateRecord(result);
        result.IsRewardClaimed = true;

        // 조건 미충족과 이미 완료된 스케줄은 이 안에서 걸러지므로 여기서 진입 경로를 따로 보지 않습니다.
        // 이번에 완료된 스케줄이 있어야만 확인 버튼이 사무실로 향합니다(기획서 9.4, LiveResultReturnTarget).
        List<string> completedScheduleIds = GameProgressService.Instance.ApplyLiveResult(result);
        result.HasCompletedSchedule = completedScheduleIds.Count > 0;

        // 보상·최고 기록·스케줄 완료를 한 번에 저장합니다. ApplyLiveResult가 따로 저장하지 않는 것은 이 때문입니다.
        PlayerDataProvider.Instance.Save();
    }
}
