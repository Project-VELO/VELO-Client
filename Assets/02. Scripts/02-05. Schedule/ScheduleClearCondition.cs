using System;
using UnityEngine;

/// <summary>
/// 일일 스케줄의 완료 조건입니다(기획서 3-E-2-1).
///
/// 스케줄 유형에 따라 채우는 필드가 다릅니다.
/// LIVE 스케줄은 TargetSongId·RequiredDifficulty·MinimumRank를, 스토리 감상 스케줄은 TargetStoryId를 사용합니다.
/// 유형별로 클래스를 나누지 않은 것은 JsonUtility가 다형성을 직렬화하지 못하기 때문이며,
/// 어느 필드를 읽을지는 ScheduleData.ScheduleType이 결정합니다.
/// </summary>
[Serializable]
public class ScheduleClearCondition : ISerializationCallbackReceiver
{
    /// <summary>
    /// LIVE 스케줄의 지정곡입니다. 이 곡이 아닌 다른 곡을 클리어해도 스케줄은 완료되지 않습니다.
    /// </summary>
    public string TargetSongId;

    /// <summary>
    /// JSON에 적는 요구 난이도 이름입니다("EASY" / "NORMAL" / "HARD"). 1차 MVP는 모두 NORMAL입니다.
    /// </summary>
    public string RequiredDifficultyName;

    /// <summary>
    /// JSON에 적는 최소 랭크 이름입니다("PERFECT_S" / "S" / "A" / "B" / "C"). 1차 MVP는 모두 B입니다.
    /// </summary>
    public string MinimumRankName;

    /// <summary>
    /// 최소 점수 조건입니다. MasterDataSchema.NO_SCORE_REQUIREMENT(0)이면 점수 조건을 보지 않습니다.
    /// 1차 MVP의 모든 LIVE 스케줄이 여기에 해당합니다.
    /// </summary>
    public int MinimumScore = MasterDataSchema.NO_SCORE_REQUIREMENT;

    /// <summary>
    /// 스토리 감상 스케줄의 대상 회차입니다. 이 스토리의 마지막 NEXT를 누른 순간 완료됩니다.
    /// </summary>
    public string TargetStoryId;

    [NonSerialized]
    public EDifficulty RequiredDifficulty;

    /// <summary>
    /// 완료로 인정하는 최소 랭크입니다.
    ///
    /// ELiveRank는 좋은 랭크일수록 값이 작으므로, 판정은 achievedRank &lt;= MinimumRank 형태입니다.
    /// 부등호를 반대로 쓰면 모든 랭크가 조건을 통과합니다.
    /// </summary>
    [NonSerialized]
    public ELiveRank MinimumRank;

    public void OnBeforeSerialize()
    {
        RequiredDifficultyName = MasterDataEnum.ToName(RequiredDifficulty);
        MinimumRankName = MasterDataEnum.ToName(MinimumRank);
    }

    public void OnAfterDeserialize()
    {
        RequiredDifficulty = MasterDataEnum.Parse(RequiredDifficultyName, EDifficulty.NORMAL, $"{nameof(ScheduleClearCondition)}.{nameof(RequiredDifficultyName)}");
        MinimumRank = MasterDataEnum.Parse(MinimumRankName, ELiveRank.B, $"{nameof(ScheduleClearCondition)}.{nameof(MinimumRankName)}");
    }

    /// <summary>
    /// 달성 랭크가 최소 랭크 조건을 만족하는지 판정합니다.
    /// ELiveRank의 부등호 방향을 호출부마다 다시 따지지 않도록 여기에 가둡니다.
    /// </summary>
    public bool IsRankSatisfied(ELiveRank achievedRank)
    {
        return achievedRank <= MinimumRank;
    }
}
