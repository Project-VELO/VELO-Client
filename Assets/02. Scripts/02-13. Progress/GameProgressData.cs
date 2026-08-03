using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 게임 진행 상태입니다. PlayerData가 소유하며 같은 세이브 파일에 함께 저장됩니다.
///
/// 재화·보유물과 분리한 이유는 두 가지입니다. 바뀌는 시점이 서로 다르고,
/// 한 클래스에 모으면 Convention의 200줄 제한을 넘습니다.
///
/// 이 클래스는 상태를 담기만 하고 판정하지 않습니다. 완료 조건과 전환 규칙은 진행 서비스가 가집니다.
/// </summary>
[Serializable]
public class GameProgressData
{
    [SerializeField]
    private SerializableDictionary<string, StoryProgress> _storyProgresses = new SerializableDictionary<string, StoryProgress>();

    [SerializeField]
    private SerializableDictionary<string, EScheduleStatus> _scheduleProgresses = new SerializableDictionary<string, EScheduleStatus>();

    // 곡·난이도 조합별 최고 기록입니다. 키 조립은 SongRecordKey를 통해서만 수행합니다.
    [SerializeField]
    private SerializableDictionary<string, SongRecord> _songRecords = new SerializableDictionary<string, SongRecord>();

    // 하루 마무리를 마친 날짜입니다. 키 조립은 DayProgressKey를 통해서만 수행합니다.
    [SerializeField]
    private List<string> _completedDayKeys = new List<string>();

    // 마무리를 마친 주차입니다. 스토리 해금이 같은 주차에서 두 번 실행되지 않도록 하는 관문입니다(기획서 16-7).
    [SerializeField]
    private List<string> _completedWeekIds = new List<string>();

    public SerializableDictionary<string, StoryProgress> StoryProgresses => _storyProgresses;
    public SerializableDictionary<string, EScheduleStatus> ScheduleProgresses => _scheduleProgresses;
    public SerializableDictionary<string, SongRecord> SongRecords => _songRecords;

    public List<string> CompletedDayKeys => _completedDayKeys;
    public List<string> CompletedWeekIds => _completedWeekIds;

    public bool IsScheduleCompleted(string scheduleId)
    {
        if (string.IsNullOrEmpty(scheduleId))
        {
            return false;
        }

        return _scheduleProgresses.TryGetValue(scheduleId, out EScheduleStatus status) && status == EScheduleStatus.COMPLETED;
    }

    public bool IsDayCompleted(string weekId, string dayId)
    {
        return _completedDayKeys.Contains(DayProgressKey.Create(weekId, dayId));
    }

    public bool IsWeekCompleted(string weekId)
    {
        return _completedWeekIds.Contains(weekId);
    }

    /// <summary>
    /// 해당 스토리의 진행 상태입니다. 기록이 없으면 null을 돌려주므로, 호출부는 마스터 데이터의 해금 조건으로 판단합니다.
    /// </summary>
    public StoryProgress GetStoryProgress(string storyId)
    {
        if (string.IsNullOrEmpty(storyId))
        {
            return null;
        }

        return _storyProgresses.TryGetValue(storyId, out StoryProgress progress) ? progress : null;
    }

    public void Clear()
    {
        _storyProgresses.Clear();
        _scheduleProgresses.Clear();
        _songRecords.Clear();
        _completedDayKeys.Clear();
        _completedWeekIds.Clear();
    }
}
