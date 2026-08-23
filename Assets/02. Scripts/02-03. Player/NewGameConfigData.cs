using System;
using System.Collections.Generic;

/// <summary>
/// 신규 게임 시작 시 지급할 초기 상태입니다(newgame_config.json, 기획서 3-C-3).
///
/// 원래 ScriptableObject였으나, 개발자 외 직군이 Unity를 사용하지 못해 마스터 데이터를 전부 JSON으로 통일하면서
/// 일반 직렬화 클래스로 바꾸었습니다. 다른 테이블과 달리 항목이 하나뿐이라 MasterDataTable로 감싸지 않고
/// 이 객체 자체가 파일의 최상위입니다.
/// </summary>
[Serializable]
public class NewGameConfigData
{
    public int SchemaVersion = MasterDataSchema.CURRENT_VERSION;

    public int StartLevel = 1;
    public int StartMoney = 1000;
    public int StartHype = 10;
    public int StartExp = 0;

    public string StartChapterId = "PROLOGUE";
    public string StartWeekId = "WEEK_001";
    public string StartDayId = "DAY_001";

    /// <summary>
    /// 극중 달력의 첫날입니다(yyyy-MM-dd). 주간 표의 날짜 표기는 여기서부터 하루씩 더해 만듭니다.
    ///
    /// 반드시 월요일이어야 합니다. 1일차가 주의 시작이고 요일 표기(MON~SUN)는 일차 순번으로만
    /// 정해지므로, 월요일이 아니면 날짜와 요일이 어긋납니다.
    ///
    /// 연도는 화면에 나오지 않지만, 월이 넘어갈 때 며칠까지 있는지 알아야 해서 온전한 날짜로 둡니다.
    /// </summary>
    public string CalendarStartDate = "2022-03-14";

    public string StartCostumeId;
    public string StartAccessoryId;

    /// <summary>
    /// 신규 게임에서 보유한 채로 시작하는 카드 전체입니다. 기획서 3-H-5 기준 10장(기본 5 + 교체 5)입니다.
    /// </summary>
    public List<string> OwnedCardIds = new List<string>();

    /// <summary>
    /// 기본 편성 5장입니다. 멤버 슬롯 순서대로 나열하며, 초기화 버튼은 이 편성으로 되돌립니다(기획서 3-H-3-1).
    /// </summary>
    public List<string> SelectedCardIds = new List<string>();
}
