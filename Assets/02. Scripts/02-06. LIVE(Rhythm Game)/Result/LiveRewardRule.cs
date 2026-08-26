using UnityEngine;

/// <summary>
/// LIVE 보상 규칙입니다(기획서 3-J-3).
/// 홈·스케줄 LIVE의 최초 CLEAR만 기본 보상 전액을 지급하고, 연습실 LIVE와 이미 클리어한 곡의 재플레이는 5%만 지급합니다.
/// </summary>
public static class LiveRewardRule
{
    public const int BASE_MONEY = 1000;
    public const int BASE_HYPE = 10;
    public const int BASE_EXP = 500;

    /// <summary>
    /// 라이브 클리어로 주는 젬입니다. 지급 규칙이 아직 정해지지 않아 0입니다.
    /// 결과 화면이 이미 칸을 그리고 있으므로, 값이 정해지면 여기만 고치면 화면까지 이어집니다.
    /// </summary>
    public const int BASE_GEM = 0;

    private const float FULL_RATE = 1f;
    private const float REPLAY_RATE = 0.05f;

    public static float GetRewardRate(EEntryType entryType, bool isFirstClear)
    {
        if (entryType == EEntryType.PRACTICE_LIVE)
        {
            return REPLAY_RATE;
        }

        return isFirstClear ? FULL_RATE : REPLAY_RATE;
    }

    /// <summary>
    /// 보상 계산에서 소수점이 생기면 버립니다(3-J-3).
    /// </summary>
    public static void GetReward(float rate, out int money, out int hype, out int exp, out int gem)
    {
        money = Mathf.FloorToInt(BASE_MONEY * rate);
        hype = Mathf.FloorToInt(BASE_HYPE * rate);
        exp = Mathf.FloorToInt(BASE_EXP * rate);
        gem = Mathf.FloorToInt(BASE_GEM * rate);
    }
}
