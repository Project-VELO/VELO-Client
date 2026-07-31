/// <summary>
/// 결과 화면 진입 시 한 번만 수행해야 하는 뒤처리(보상 지급, 최고 기록 갱신)를 정해진 순서로 묶습니다.
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
    }
}
