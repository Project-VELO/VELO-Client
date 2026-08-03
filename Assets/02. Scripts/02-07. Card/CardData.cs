using System;

/// <summary>
/// 포토카드 한 장의 기획 메타데이터입니다(cards.json).
///
/// 편성 슬롯은 이 카드가 아니라 CharacterId가 가리키는 CharacterData.MemberOrder가 결정합니다.
/// 각 멤버 슬롯에는 해당 멤버의 카드만 배치할 수 있으므로(기획서 3-H-2) 슬롯 정보를 카드에 중복해 두지 않습니다.
///
/// 필드가 public인 것은 의도적입니다. 사람이 직접 편집하는 JSON이라
/// [SerializeField] private 방식의 밑줄 접두사가 키에 노출되면 곤란합니다(Convention 1-b-(2)).
/// </summary>
[Serializable]
public class CardData
{
    public string CardId;

    /// <summary>
    /// 이 카드가 대표하는 캐릭터입니다. characters.json의 CharacterId를 가리킵니다.
    /// </summary>
    public string CharacterId;

    public string CardName;
    public string ImagePath;

    /// <summary>
    /// 카드 등급(★)입니다. 1차 MVP에서는 표시에만 사용하며 점수에 반영하지 않습니다.
    /// </summary>
    public int Grade = 1;

    /// <summary>
    /// 카드 능력치입니다. 1차 MVP에서는 점수·판정에 반영하지 않으나(기획서 3-H-1) 스키마는 미리 둡니다.
    /// </summary>
    public CardStats Stats = new CardStats();

    /// <summary>
    /// 카드 고유 스킬입니다. 1차 MVP 미사용입니다.
    /// </summary>
    public string SkillId;
}
