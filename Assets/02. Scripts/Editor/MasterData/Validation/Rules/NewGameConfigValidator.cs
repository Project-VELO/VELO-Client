using System.Collections.Generic;

/// <summary>
/// 신규 게임 초기 지급 설정을 검사합니다(기획서 3-C-3, 3-H-1).
/// 여기가 어긋나면 새 게임을 시작한 플레이어가 곡 선택 화면에서 곧바로 막히므로 가장 먼저 확인해야 합니다.
/// </summary>
public class NewGameConfigValidator
{
    public void Validate(MasterDataProvider provider, MasterDataValidationReport report)
    {
        NewGameConfigData config = provider.NewGameConfig;

        if (ReferenceEquals(config, null))
        {
            report.AddError($"[신규 게임] {MasterDataPaths.NEW_GAME_CONFIG_FILE_NAME}을 읽지 못했습니다.");
            return;
        }

        foreach (string cardId in config.OwnedCardIds)
        {
            if (!provider.TryGetCard(cardId, out _))
            {
                report.AddError($"[신규 게임] 초기 보유 카드 '{cardId}'를 cards.json에서 찾을 수 없습니다.");
            }
        }

        ValidateStartingDeck(provider, config, report);
        ValidateStartingItems(provider, config, report);
    }

    /// <summary>
    /// 기본 편성이 기획서 3-H-1의 규칙(5장, 멤버당 1장, 중복 없음)을 지키는지 확인합니다.
    /// </summary>
    private void ValidateStartingDeck(MasterDataProvider provider, NewGameConfigData config, MasterDataValidationReport report)
    {
        if (config.SelectedCardIds.Count != MasterDataValidationRule.MEMBER_COUNT)
        {
            report.AddError($"[신규 게임] 기본 편성이 {MasterDataValidationRule.MEMBER_COUNT}장이어야 하는데 {config.SelectedCardIds.Count}장입니다.");
        }

        var usedCharacters = new Dictionary<string, string>();

        foreach (string cardId in config.SelectedCardIds)
        {
            if (!config.OwnedCardIds.Contains(cardId))
            {
                report.AddError($"[신규 게임] 기본 편성 카드 '{cardId}'가 초기 보유 목록에 없습니다.");
            }

            if (!provider.TryGetCard(cardId, out CardData card))
            {
                continue;
            }

            if (usedCharacters.TryGetValue(card.CharacterId, out string owner))
            {
                report.AddError($"[신규 게임] 같은 멤버({card.CharacterId})의 카드가 둘 편성되어 있습니다: {owner}, {cardId}");
                continue;
            }

            usedCharacters[card.CharacterId] = cardId;
        }
    }

    private void ValidateStartingItems(MasterDataProvider provider, NewGameConfigData config, MasterDataValidationReport report)
    {
        if (!provider.TryGetItem(config.StartCostumeId, out ItemData costume))
        {
            report.AddError($"[신규 게임] 기본 의상 '{config.StartCostumeId}'를 items.json에서 찾을 수 없습니다.");
        }
        else if (costume.ItemType != EItemType.COSTUME)
        {
            report.AddError($"[신규 게임] 기본 의상 '{config.StartCostumeId}'의 종류가 {costume.ItemType}입니다.");
        }

        if (!provider.TryGetItem(config.StartAccessoryId, out ItemData accessory))
        {
            report.AddError($"[신규 게임] 기본 악세사리 '{config.StartAccessoryId}'를 items.json에서 찾을 수 없습니다.");
        }
        else if (accessory.ItemType != EItemType.ACCESSORY)
        {
            report.AddError($"[신규 게임] 기본 악세사리 '{config.StartAccessoryId}'의 종류가 {accessory.ItemType}입니다.");
        }
    }
}
