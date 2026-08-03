using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 재화·보유물·현재 위치와 게임 진행 상태를 담는 세이브 파일의 최상위 객체입니다.
///
/// 진행 상태는 GameProgressData가 따로 들고 있습니다. 재화·보유물과는 바뀌는 시점이 다르고,
/// 한 클래스에 모으면 Convention의 200줄 제한을 넘기 때문입니다.
/// </summary>
[Serializable]
public class PlayerData
{
    // 기본값을 0으로 두는 것은 의도적입니다. 실제 저장을 거친 데이터만 버전을 갖게 되므로,
    // 빈 JSON처럼 내용이 없는 파일을 유효한 세이브로 오인하지 않는 판별 근거가 됩니다.
    [SerializeField]
    private int _schemaVersion = SaveSchema.UNSAVED_VERSION;

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

    // 실제 값은 newgame_config.json이 채웁니다. 코드에 ID를 박아 두면 마스터 데이터와 어긋날 때 알아채기 어렵습니다.
    [SerializeField]
    private string _selectedCostumeId = string.Empty;

    [SerializeField]
    private string _selectedAccessoryId = string.Empty;

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

    [SerializeField]
    private GameProgressData _progress = new GameProgressData();

    public int SchemaVersion { get => _schemaVersion; set => _schemaVersion = value; }

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

    /// <summary>
    /// 스토리·스케줄·날짜·주차 진행 상태입니다. 값을 바꿀 때는 GameProgressService를 통해야 저장이 함께 이루어집니다.
    /// </summary>
    public GameProgressData Progress
    {
        get
        {
            // 구버전 세이브나 손상된 파일을 읽으면 null이 될 수 있습니다.
            // 진행 상태가 통째로 없다고 게임을 못 켜게 할 이유는 없으므로 빈 상태로 복구합니다.
            if (ReferenceEquals(_progress, null))
            {
                _progress = new GameProgressData();
            }

            return _progress;
        }
    }

    /// <summary>
    /// 신규 게임 시작 시 플레이어의 초기 상태 데이터를 생성합니다(기획서 3-C-3).
    /// 스토리 해금 초기 상태는 마스터 데이터의 해금 조건을 봐야 하므로 여기가 아니라 StoryProgressService가 채웁니다.
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

        _schemaVersion = SaveSchema.CURRENT_VERSION;

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
        // 설정을 읽지 못해 ID가 비어 있을 때 그대로 넣으면 보유 목록에 빈 항목이 남고,
        // 의상 목록 화면에서 마스터 데이터 조회에 실패하는 행이 생깁니다.
        _ownedCostumeIds.Clear();
        _ownedAccessoryIds.Clear();

        if (!string.IsNullOrEmpty(_selectedCostumeId))
        {
            _ownedCostumeIds.Add(_selectedCostumeId);
        }

        if (!string.IsNullOrEmpty(_selectedAccessoryId))
        {
            _ownedAccessoryIds.Add(_selectedAccessoryId);
        }

        Progress.Clear();
    }
}
