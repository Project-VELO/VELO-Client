using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 신규 게임 시작 시 플레이어의 초기 지급 데이터(재화, 의상, 악세서리, 카드 등) 기획 데이터를 정의하는 ScriptableObject 클래스입니다.
/// </summary>
[CreateAssetMenu(fileName = "NewGameConfig", menuName = "Config/NewGameConfig")]
public class NewGameConfig : ScriptableObject
{
    [SerializeField]
    private int _startLevel = 1;

    [SerializeField]
    private int _startMoney = 1000;

    [SerializeField]
    private int _startHype = 10;

    [SerializeField]
    private string _startChapterId = "PROLOGUE";

    [SerializeField]
    private string _startWeekId = "WEEK_001";

    [SerializeField]
    private string _startDayId = "DAY_001";

    [SerializeField]
    private string _startCostumeId = "COSTUME_001";

    [SerializeField]
    private string _startAccessoryId = "ACCESSORY_001";

    [SerializeField]
    private List<string> _initialCardIds = new List<string>()
    {
        "CARD_001", "CARD_002", "CARD_003", "CARD_004", "CARD_005",
        "CARD_006", "CARD_007", "CARD_008", "CARD_009", "CARD_010"
    };

    [SerializeField]
    private List<string> _initialSelectedCardIds = new List<string>()
    {
        "CARD_001", "CARD_002", "CARD_003", "CARD_004", "CARD_005"
    };

    public int StartLevel => _startLevel;
    public int StartMoney => _startMoney;
    public int StartHype => _startHype;
    public string StartChapterId => _startChapterId;
    public string StartWeekId => _startWeekId;
    public string StartDayId => _startDayId;
    public string StartCostumeId => _startCostumeId;
    public string StartAccessoryId => _startAccessoryId;
    public List<string> InitialCardIds => _initialCardIds;
    public List<string> InitialSelectedCardIds => _initialSelectedCardIds;
}
