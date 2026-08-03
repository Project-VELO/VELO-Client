using UnityEngine;

/// <summary>
/// 보상 테이블의 수치를 플레이어 재화에 반영합니다.
///
/// LIVE 보상은 진입 경로별 비율 계산이 얽혀 있어 LiveRewardService가 따로 처리합니다.
/// 이 클래스는 비율 없이 정해진 수치를 그대로 주는 보상(스토리 감상 보상 등)을 담당합니다.
/// </summary>
public static class RewardService
{
    /// <summary>
    /// 보상을 지급합니다. 중복 지급 방지는 호출부의 책임입니다.
    /// 스토리 감상 보상은 최초 완료 여부로, LIVE 보상은 결과 데이터의 지급 완료 표시로 각각 관문을 둡니다.
    /// </summary>
    public static bool TryGrant(string rewardId)
    {
        if (string.IsNullOrEmpty(rewardId))
        {
            return false;
        }

        if (!MasterDataProvider.Instance.TryGetReward(rewardId, out RewardData reward))
        {
            Debug.LogWarning($"[RewardService] 보상을 찾을 수 없어 지급하지 못했습니다: {rewardId}");
            return false;
        }

        PlayerData data = PlayerDataProvider.Instance.Data;
        data.Money += reward.Money;
        data.Hype += reward.Hype;
        data.Exp += reward.Exp;

        return true;
    }
}
