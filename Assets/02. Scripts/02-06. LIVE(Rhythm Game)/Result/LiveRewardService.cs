/// <summary>
/// 플레이 결과에 따른 보상을 계산해 플레이어 재화에 반영합니다(기획서 3-J-4).
/// 지급 여부는 결과 데이터에 기록되므로, 결과 화면을 다시 열거나 확인 버튼을 여러 번 눌러도 중복 지급되지 않습니다.
/// </summary>
public static class LiveRewardService
{
    public static void GrantReward(LiveResultData result, bool isFirstClear)
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
}
