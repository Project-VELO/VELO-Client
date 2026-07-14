using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개별 스케줄(활동 및 미션)에 대한 기획 메타데이터를 저장하는 DTO 클래스입니다.
/// </summary>
[Serializable]
public class ScheduleData
{
    [SerializeField]
    private string _scheduleId;

    [SerializeField]
    private string _chapterId;

    [SerializeField]
    private string _weekId;

    [SerializeField]
    private string _dayId;

    [SerializeField]
    private int _scheduleOrder;

    [SerializeField]
    private EScheduleType _scheduleType;

    [SerializeField]
    private string _title;

    [SerializeField]
    private string _description;

    [SerializeField]
    private string _linkedSongId;

    [SerializeField]
    private EDifficulty _requiredDifficulty = EDifficulty.NORMAL;

    [SerializeField]
    private bool _isRequired = true;

    [SerializeField]
    private string _clearCondition;

    [SerializeField]
    private string _rewardId;

    [SerializeField]
    private List<string> _requiredPrerequisiteIds = new List<string>();

    public string ScheduleId { get => _scheduleId; set => _scheduleId = value; }
    public string ChapterId { get => _chapterId; set => _chapterId = value; }
    public string WeekId { get => _weekId; set => _weekId = value; }
    public string DayId { get => _dayId; set => _dayId = value; }
    public int ScheduleOrder { get => _scheduleOrder; set => _scheduleOrder = value; }
    public EScheduleType ScheduleType { get => _scheduleType; set => _scheduleType = value; }
    public string Title { get => _title; set => _title = value; }
    public string Description { get => _description; set => _description = value; }
    public string LinkedSongId { get => _linkedSongId; set => _linkedSongId = value; }
    public EDifficulty RequiredDifficulty { get => _requiredDifficulty; set => _requiredDifficulty = value; }
    public bool IsRequired { get => _isRequired; set => _isRequired = value; }
    public string ClearCondition { get => _clearCondition; set => _clearCondition = value; }
    public string RewardId { get => _rewardId; set => _rewardId = value; }
    public List<string> RequiredPrerequisiteIds => _requiredPrerequisiteIds;
}
