using System.Collections.Generic;

/// <summary>
/// 캐릭터와 포토카드의 참조를 검사합니다.
/// 카드는 캐릭터를 가리키고 편성 슬롯은 캐릭터가 결정하므로, 두 테이블을 함께 봅니다.
/// </summary>
public class CharacterCardValidator
{
    public void Validate(MasterDataProvider provider, MasterDataValidationReport report)
    {
        ValidateCharacters(provider, report);
        ValidateCards(provider, report);
    }

    private void ValidateCharacters(MasterDataProvider provider, MasterDataValidationReport report)
    {
        var usedOrders = new Dictionary<int, string>();
        int memberCount = 0;

        foreach (KeyValuePair<string, CharacterData> pair in provider.Characters)
        {
            CharacterData character = pair.Value;

            if (!character.IsMember)
            {
                continue;
            }

            memberCount++;

            if (usedOrders.TryGetValue(character.MemberOrder, out string owner))
            {
                report.AddError($"[캐릭터] 멤버 슬롯 {character.MemberOrder}이 중복입니다: {owner}, {character.CharacterId}");
                continue;
            }

            usedOrders[character.MemberOrder] = character.CharacterId;
        }

        // 편성은 정확히 5장이므로(기획서 3-H-1) 멤버가 5명이 아니면 편성 화면이 성립하지 않습니다.
        if (memberCount != MasterDataValidationRule.MEMBER_COUNT)
        {
            report.AddError($"[캐릭터] 편성 멤버가 {MasterDataValidationRule.MEMBER_COUNT}명이어야 하는데 {memberCount}명입니다.");
        }
    }

    private void ValidateCards(MasterDataProvider provider, MasterDataValidationReport report)
    {
        foreach (KeyValuePair<string, CardData> pair in provider.Cards)
        {
            CardData card = pair.Value;

            if (!provider.TryGetCharacter(card.CharacterId, out CharacterData character))
            {
                report.AddError($"[카드] {card.CardId}의 CharacterId '{card.CharacterId}'를 characters.json에서 찾을 수 없습니다.");
                continue;
            }

            if (!character.IsMember)
            {
                report.AddError($"[카드] {card.CardId}가 편성 멤버가 아닌 캐릭터({card.CharacterId})를 가리킵니다.");
            }
        }
    }
}
