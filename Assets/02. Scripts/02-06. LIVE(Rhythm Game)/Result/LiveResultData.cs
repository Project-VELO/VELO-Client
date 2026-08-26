using System;
using UnityEngine;

/// <summary>
/// 라이브 완료 시 생성되는 플레이 결과 정보를 전달하는 DTO 클래스입니다.
/// </summary>
[Serializable]
public class LiveResultData
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
    private ELiveRank _rank = ELiveRank.FAILED;

    [SerializeField]
    private float _accuracy = 0f;

    [SerializeField]
    private int _totalNoteCount = 0;

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

    [SerializeField]
    private int _earnedGem = 0;

    public string PlayResultId { get => _playResultId; set => _playResultId = value; }
    public EEntryType EntryType { get => _entryType; set => _entryType = value; }
    public string ScheduleId { get => _scheduleId; set => _scheduleId = value; }
    public string SongId { get => _songId; set => _songId = value; }
    public EDifficulty Difficulty { get => _difficulty; set => _difficulty = value; }
    
    public bool IsClear { get => _isClear; set => _isClear = value; }
    public string FailReason { get => _failReason; set => _failReason = value; }
    
    public int Score { get => _score; set => _score = value; }
    public ELiveRank Rank { get => _rank; set => _rank = value; }

    /// <summary>
    /// 백분율 정확도입니다. 랭크 판정의 근거이자 결과 화면 표시 값입니다.
    /// </summary>
    public float Accuracy { get => _accuracy; set => _accuracy = value; }

    /// <summary>
    /// 채보의 전체 노트 개수입니다. 귀신 노트 오입력은 포함하지 않습니다.
    /// </summary>
    public int TotalNoteCount { get => _totalNoteCount; set => _totalNoteCount = value; }
    
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
    public int EarnedGem { get => _earnedGem; set => _earnedGem = value; }

    /// <summary>
    /// 이번 결과 처리로 일일 스케줄이 하나라도 완료되었는지 여부입니다.
    /// 결과 화면의 확인 버튼이 사무실로 갈지 곡 선택으로 갈지 이 값으로 정합니다(기획서 9.4).
    ///
    /// 위 필드들과 달리 백킹 필드 없이 둡니다. 결과 화면을 다시 여는 동안만 의미가 있는 세션 값이라
    /// 직렬화 대상에서 빼기 위해서입니다. 자동 프로퍼티는 JsonUtility와 인스펙터 어느 쪽에도 잡히지 않습니다.
    /// </summary>
    public bool HasCompletedSchedule { get; set; }
}
