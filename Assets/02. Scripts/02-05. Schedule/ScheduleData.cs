using System;
using UnityEngine;

/// <summary>
/// 일일 스케줄 하나의 기획 메타데이터입니다(schedules.json).
///
/// 완료 여부는 플레이어마다 다르므로 여기가 아니라 세이브 데이터에 있습니다.
///
/// 필드가 public인 것은 의도적입니다. 사람이 직접 편집하는 JSON이라
/// [SerializeField] private 방식의 밑줄 접두사가 키에 노출되면 곤란합니다(Convention 1-b-(2)).
/// </summary>
[Serializable]
public class ScheduleData : ISerializationCallbackReceiver
{
    public string ScheduleId;
    public string ChapterId;

    public string WeekId;

    /// <summary>
    /// 주차의 진행 순서입니다(1주차 = 1). 문자열 ID만으로는 다음 주차를 계산할 수 없습니다.
    /// </summary>
    public int WeekOrder;

    public string DayId;

    /// <summary>
    /// 주차 안에서 날짜의 진행 순서입니다(1일차 = 1).
    /// 하루 마무리 후 다음 날짜로 넘어가는 판정과 주간 스케줄 표의 배치 순서에 사용합니다(기획서 3-E-3-1).
    /// </summary>
    public int DayOrder;

    /// <summary>
    /// 같은 날짜 안에서 스케줄을 나열하는 순서입니다.
    /// 1차 MVP에서는 세 스케줄 모두 처음부터 진행할 수 있으므로 표시 순서일 뿐 선행 조건이 아닙니다(기획서 3-E-2-2).
    /// </summary>
    public int ScheduleOrder;

    /// <summary>
    /// JSON에 적는 스케줄 유형 이름입니다("LIVE" / "STORY" / "SYSTEM").
    /// </summary>
    public string ScheduleTypeName;

    public string Title;
    public string Description;

    /// <summary>
    /// 하루 완료 판정에 포함되는 필수 스케줄인지 여부입니다. 1차 MVP의 세 스케줄은 모두 필수입니다.
    /// </summary>
    public bool IsRequired = true;

    /// <summary>
    /// 완료 조건입니다. 어느 필드를 읽을지는 ScheduleType이 결정합니다.
    /// </summary>
    public ScheduleClearCondition ClearCondition = new ScheduleClearCondition();

    public string RewardId;

    /// <summary>
    /// ScheduleTypeName을 변환한 값입니다. 역직렬화 직후 채워집니다.
    /// </summary>
    [NonSerialized]
    public EScheduleType ScheduleType;

    public void OnBeforeSerialize()
    {
        ScheduleTypeName = MasterDataEnum.ToName(ScheduleType);
    }

    public void OnAfterDeserialize()
    {
        ScheduleType = MasterDataEnum.Parse(ScheduleTypeName, EScheduleType.LIVE, $"{nameof(ScheduleData)}({ScheduleId}).{nameof(ScheduleTypeName)}");
    }
}
