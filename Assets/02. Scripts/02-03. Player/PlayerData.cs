using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 계정 재화, 진행 상태, 세이브 정보를 저장하는 DTO 클래스입니다.
/// </summary>
[Serializable]
public class PlayerData
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

    // JSON 직렬화를 위해 Serializable 딕셔너리 구조가 권장되나, 
    // 기본 C# 딕셔너리로 선언하고 로드 시 파싱할 수 있도록 구성합니다.
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

    /// <summary>
    /// 신규 게임 시작 시 플레이어의 초기 상태 데이터를 생성합니다.
    /// </summary>
    public void InitPlayerData()
    {
        _level = 1;
        _exp = 0;
        _money = 1000;
        _hype = 10;
        _gem = 0;
        _currentChapterId = "PROLOGUE";
        _currentWeekId = "WEEK_001";
        _currentDayId = "DAY_001";
        _selectedCostumeId = "COSTUME_001"; // 기본 의상: 허름한 제복
        _selectedAccessoryId = "ACCESSORY_001"; // 기본 악세서리: 녹슨 목걸이
        _dormitoryLevel = 1;

        // 초기 카드 지급: 기본 카드 5장, 교체 카드 5장
        _ownedCardIds.Clear();
        for (int i = 1; i <= 10; i++)
        {
            _ownedCardIds.Add($"CARD_{i:D3}");
        }

        // 초기 덱 편성: 기본 카드 5장 자동 배치
        _selectedCardIds.Clear();
        for (int i = 1; i <= 5; i++)
        {
            _selectedCardIds.Add($"CARD_{i:D3}");
        }

        // 초기 의상 및 악세서리 보유 설정
        _ownedCostumeIds.Clear();
        _ownedCostumeIds.Add("COSTUME_001");
        _ownedAccessoryIds.Clear();
        _ownedAccessoryIds.Add("ACCESSORY_001");

        _storyProgresses.Clear();
        _scheduleProgresses.Clear();
        _songRecords.Clear();
    }
}
