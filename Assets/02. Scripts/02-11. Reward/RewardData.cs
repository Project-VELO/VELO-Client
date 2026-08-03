using System;

/// <summary>
/// 보상 한 건의 지급 수치입니다(rewards.json).
/// 스토리 감상 보상과 스케줄 완료 보상이 이 테이블을 참조합니다(기획서 3-J-3).
///
/// LIVE 보상은 진입 경로별 비율(100% / 5%) 계산이 얽혀 있어 아직 LiveRewardRule의 상수를 사용합니다.
/// 이 테이블로 옮기는 작업은 LIVE 스케줄 연동 시점에 함께 처리합니다.
/// </summary>
[Serializable]
public class RewardData
{
    public string RewardId;
    public int Money;
    public int Hype;
    public int Exp;
}
