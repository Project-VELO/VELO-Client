using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 계정 재화, 진행 상태, 세이브 정보를 저장하는 DTO 클래스입니다.
/// </summary>
[Serializable]
public class PlayerData : ISerializationCallbackReceiver
{
    [SerializeField]
    private int _level = 1;

    [SerializeField]
    private int _exp = 0;

    [SerializeField]
    private int _money = 0;

    [SerializeField]
    private int _hype = 0;

    [SerializeField]
    private int _gem = 0;

    [SerializeField]
    private string _currentChapterId = "PROLOGUE";

    [SerializeField]
    private string _currentWeekId = "WEEK_001";

    [SerializeField]
    private string _currentDayId = "DAY_001";

    [SerializeField]
    private string _selectedCostumeId = "COSTUME_001";

    [SerializeField]
    private string _selectedAccessoryId = "ACCESSORY_001";

    [SerializeField]
    private List<string> _ownedCardIds = new List<string>();

    [SerializeField]
    private List<string> _selectedCardIds = new List<string>();

    [SerializeField]
    private List<string> _ownedCostumeIds = new List<string>();

    [SerializeField]
    private List<string> _ownedAccessoryIds = new List<string>();

    [SerializeField]
    private int _dormitoryLevel = 1;

    // 곡 기록은 List로 감싸 직렬화하고 ISerializationCallbackReceiver로 Dictionary와 동기화합니다.
    // 키 조립은 SongRecordKey를 통해서만 수행합니다.
    [SerializeField]
    private List<SongRecordEntry> _songRecordEntries = new List<SongRecordEntry>();

    // JSON 직렬화를 위해 Serializable 딕셔너리 구조가 권장되나,
    // 기본 C# 딕셔너리로 선언하고 로드 시 파싱할 수 있도록 구성합니다.
    // 아래 두 딕셔너리도 _songRecords와 동일한 방식으로 직렬화해야 하나, 세이브 계층 작업 시 함께 정리합니다.
    private Dictionary<string, EStoryStatus> _storyProgresses = new Dictionary<string, EStoryStatus>();
    private Dictionary<string, EScheduleStatus> _scheduleProgresses = new Dictionary<string, EScheduleStatus>();
    private Dictionary<string, SongRecord> _songRecords = new Dictionary<string, SongRecord>();

    public int Level { get => _level; set => _level = value; }
    public int Exp { get => _exp; set => _exp = value; }
    public int Money { get => _money; set => _money = value; }
    public int Hype { get => _hype; set => _hype = value; }
    public int Gem { get => _gem; set => _gem = value; }

    public string CurrentChapterId { get => _currentChapterId; set => _currentChapterId = value; }
    public string CurrentWeekId { get => _currentWeekId; set => _currentWeekId = value; }
    public string CurrentDayId { get => _currentDayId; set => _currentDayId = value; }

    public string SelectedCostumeId { get => _selectedCostumeId; set => _selectedCostumeId = value; }
    public string SelectedAccessoryId { get => _selectedAccessoryId; set => _selectedAccessoryId = value; }
    public int DormitoryLevel { get => _dormitoryLevel; set => _dormitoryLevel = value; }

    public List<string> OwnedCardIds => _ownedCardIds;
    public List<string> SelectedCardIds => _selectedCardIds;
    public List<string> OwnedCostumeIds => _ownedCostumeIds;
    public List<string> OwnedAccessoryIds => _ownedAccessoryIds;

    public Dictionary<string, EStoryStatus> StoryProgresses => _storyProgresses;
    public Dictionary<string, EScheduleStatus> ScheduleProgresses => _scheduleProgresses;
    public Dictionary<string, SongRecord> SongRecords => _songRecords;

    public void OnBeforeSerialize()
    {
        _songRecordEntries.Clear();
        foreach (var pair in _songRecords)
        {
            _songRecordEntries.Add(new SongRecordEntry { Key = pair.Key, Record = pair.Value });
        }
    }

    public void OnAfterDeserialize()
    {
        _songRecords.Clear();
        foreach (var entry in _songRecordEntries)
        {
            _songRecords[entry.Key] = entry.Record;
        }
    }

    /// <summary>
    /// 신규 게임 시작 시 플레이어의 초기 상태 데이터를 생성합니다(기획서 3-C-3).
    /// </summary>
    /// <param name="config">newgame_config.json에서 읽은 초기 지급 설정입니다.</param>
    public void InitPlayerData(NewGameConfigData config)
    {
        // 설정 파일을 읽지 못하면 카드도 편성도 없는 상태가 되어 곡 선택 화면에서 플레이가 막힙니다.
        // 조용히 빈 상태로 시작해 원인을 찾기 어렵게 만드는 대신, 여기서 분명히 알립니다.
        if (ReferenceEquals(config, null))
        {
            Debug.LogError($"[PlayerData] 신규 게임 설정을 읽지 못했습니다. {MasterDataPaths.NEW_GAME_CONFIG_FILE_NAME}을 확인해 주세요.");
            config = new NewGameConfigData();
        }

        _level = config.StartLevel;
        _exp = config.StartExp;
        _money = config.StartMoney;
        _hype = config.StartHype;
        _gem = 0;
        _currentChapterId = config.StartChapterId;
        _currentWeekId = config.StartWeekId;
        _currentDayId = config.StartDayId;
        _selectedCostumeId = config.StartCostumeId;
        _selectedAccessoryId = config.StartAccessoryId;
        _dormitoryLevel = 1;

        _ownedCardIds.Clear();
        _ownedCardIds.AddRange(config.OwnedCardIds);

        _selectedCardIds.Clear();
        _selectedCardIds.AddRange(config.SelectedCardIds);

        // 초기 의상 및 악세서리 보유 설정
        _ownedCostumeIds.Clear();
        _ownedCostumeIds.Add(_selectedCostumeId);
        _ownedAccessoryIds.Clear();
        _ownedAccessoryIds.Add(_selectedAccessoryId);

        _storyProgresses.Clear();
        _scheduleProgresses.Clear();
        _songRecords.Clear();
        _songRecordEntries.Clear();
    }
}
