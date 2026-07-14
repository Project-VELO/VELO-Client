using System;
using UnityEngine;

/// <summary>
/// 리듬게임 완료 시 생성되는 플레이 결과 정보를 전달하는 DTO 클래스입니다.
/// </summary>
[Serializable]
public class RhythmResultData
{
    [SerializeField]
    private string _playResultId;

    [SerializeField]
    private EEntryType _entryType = EEntryType.HOME_LIVE;

    [SerializeField]
    private string _scheduleId;

    [SerializeField]
    private string _songId;

    [SerializeField]
    private EDifficulty _difficulty = EDifficulty.NORMAL;

    [SerializeField]
    private bool _isClear = false;

    [SerializeField]
    private string _failReason;

    [SerializeField]
    private int _score = 0;

    [SerializeField]
    private ERhythmRank _rank = ERhythmRank.FAILED;

    [SerializeField]
    private int _perfectCount = 0;

    [SerializeField]
    private int _greatCount = 0;

    [SerializeField]
    private int _goodCount = 0;

    [SerializeField]
    private int _badCount = 0;

    [SerializeField]
    private int _maxCombo = 0;

    [SerializeField]
    private bool _isFullCombo = false;

    [SerializeField]
    private bool _isNewBest = false;

    [SerializeField]
    private bool _isRewardClaimed = false;

    [SerializeField]
    private int _comboBonusScore = 0;

    [SerializeField]
    private int _cardBonusScore = 0;

    [SerializeField]
    private int _earnedMoney = 0;

    [SerializeField]
    private int _earnedHype = 0;

    [SerializeField]
    private int _earnedExp = 0;

    public string PlayResultId { get => _playResultId; set => _playResultId = value; }
    public EEntryType EntryType { get => _entryType; set => _entryType = value; }
    public string ScheduleId { get => _scheduleId; set => _scheduleId = value; }
    public string SongId { get => _songId; set => _songId = value; }
    public EDifficulty Difficulty { get => _difficulty; set => _difficulty = value; }
    
    public bool IsClear { get => _isClear; set => _isClear = value; }
    public string FailReason { get => _failReason; set => _failReason = value; }
    
    public int Score { get => _score; set => _score = value; }
    public ERhythmRank Rank { get => _rank; set => _rank = value; }
    
    public int PerfectCount { get => _perfectCount; set => _perfectCount = value; }
    public int GreatCount { get => _greatCount; set => _greatCount = value; }
    public int GoodCount { get => _goodCount; set => _goodCount = value; }
    public int BadCount { get => _badCount; set => _badCount = value; }
    
    public int MaxCombo { get => _maxCombo; set => _maxCombo = value; }
    public bool IsFullCombo { get => _isFullCombo; set => _isFullCombo = value; }
    public bool IsNewBest { get => _isNewBest; set => _isNewBest = value; }
    public bool IsRewardClaimed { get => _isRewardClaimed; set => _isRewardClaimed = value; }

    public int ComboBonusScore { get => _comboBonusScore; set => _comboBonusScore = value; }
    public int CardBonusScore { get => _cardBonusScore; set => _cardBonusScore = value; }
    public int EarnedMoney { get => _earnedMoney; set => _earnedMoney = value; }
    public int EarnedHype { get => _earnedHype; set => _earnedHype = value; }
    public int EarnedExp { get => _earnedExp; set => _earnedExp = value; }
}
