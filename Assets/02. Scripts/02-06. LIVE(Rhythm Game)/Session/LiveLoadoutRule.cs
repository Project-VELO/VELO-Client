using System.Collections.Generic;

/// <summary>
/// LIVE 편성 규칙입니다(기획서 3-H-1·3-H-2).
/// 카드가 들어갈 멤버 슬롯을 정하고, 현재 편성이 플레이 가능한 상태인지 검사합니다.
/// 준비 화면의 카드 교체와 곡 선택 화면의 진입 차단(기획서 16-15)이 같은 판정을 공유합니다.
/// </summary>
public static class LiveLoadoutRule
{
    /// <summary>
    /// 편성 슬롯 수입니다. 멤버 5명에게 각 1장씩입니다(기획서 3-H-1).
    /// </summary>
    public const int REQUIRED_CARD_COUNT = 5;

    /// <summary>
    /// 카드가 배치될 멤버 슬롯 인덱스를 얻습니다. 각 멤버 슬롯에는 해당 멤버의 카드만 배치할 수 있으므로(3-H-2)
    /// 슬롯은 선택이 아니라 카드의 소속 멤버가 결정합니다.
    /// </summary>
    public static bool TryGetSlotIndex(string cardId, out int slotIndex)
    {
        return TryGetSlotIndex(MasterDataQuery.GetMembersInSlotOrder(), cardId, out slotIndex);
    }

    /// <summary>
    /// 저장된 편성 목록을 멤버 슬롯에 배치한 작업용 목록으로 만듭니다. 길이는 항상 멤버 수와 같고,
    /// 각 칸에는 그 멤버의 카드만 들어가며, 대응되는 카드가 없는 칸은 빈 값으로 둡니다.
    ///
    /// 목록을 위치 그대로 복사하지 않는 것은 "N번째 항목이 N번째 멤버"라는 보장이 저장 데이터에 없기 때문입니다.
    /// 길이가 모자란 세이브를 그대로 옮기면 뒤쪽 슬롯이 아예 존재하지 않게 되어, 교체로도 채울 수 없는 칸이 생깁니다.
    /// </summary>
    public static List<string> BuildSlotCardIds(IReadOnlyList<string> sourceCardIds)
    {
        List<CharacterData> members = MasterDataQuery.GetMembersInSlotOrder();
        var slotCardIds = new List<string>(members.Count);

        for (int i = 0; i < members.Count; i++)
        {
            slotCardIds.Add(string.Empty);
        }

        if (ReferenceEquals(sourceCardIds, null))
        {
            return slotCardIds;
        }

        for (int i = 0; i < sourceCardIds.Count; i++)
        {
            if (TryGetSlotIndex(members, sourceCardIds[i], out int slotIndex))
            {
                slotCardIds[slotIndex] = sourceCardIds[i];
            }
        }

        return slotCardIds;
    }

    /// <summary>
    /// 지금 LIVE에 적용될 편성(임시 편성이 있으면 그것, 없으면 기본 편성)이 플레이 가능한지 검사합니다.
    /// 미달이면 리듬게임 진입을 차단하고 안내 팝업을 띄워야 합니다(기획서 16-15, SCREEN-011).
    ///
    /// 기본 편성은 슬롯 배치를 거친 뒤에 봅니다. 준비 화면이 실제로 적용할 편성이 그것이므로,
    /// 순서만 어긋난 세이브를 열어 보지도 못하게 막지 않기 위해서입니다.
    ///
    /// 편성과 세이브는 호출자가 넘깁니다. 규칙이 LiveLoadoutContext.Instance를 도로 읽으면
    /// 컨텍스트가 규칙을 쓰고 규칙이 컨텍스트를 쓰는 순환이 생기기 때문입니다.
    /// </summary>
    public static bool IsCurrentLoadoutPlayable(LiveLoadoutContext loadout, PlayerData playerData)
    {
        if (loadout.IsInitialized)
        {
            return IsLoadoutValid(loadout.CardIds, playerData);
        }

        return IsLoadoutValid(BuildSlotCardIds(playerData.SelectedCardIds), playerData);
    }

    private static bool TryGetSlotIndex(List<CharacterData> members, string cardId, out int slotIndex)
    {
        slotIndex = -1;

        if (!MasterDataProvider.Instance.TryGetCard(cardId, out CardData card))
        {
            return false;
        }

        for (int i = 0; i < members.Count; i++)
        {
            if (members[i].CharacterId == card.CharacterId)
            {
                slotIndex = i;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 편성이 규칙(5장·슬롯별 멤버 일치·보유·카드 ID 중복 없음)을 모두 지키는지 검사합니다.
    /// 멤버당 1장(3-H-1)은 슬롯-멤버 일치 검사로 함께 보장됩니다.
    /// </summary>
    private static bool IsLoadoutValid(IReadOnlyList<string> cardIds, PlayerData playerData)
    {
        List<CharacterData> members = MasterDataQuery.GetMembersInSlotOrder();

        if (ReferenceEquals(cardIds, null) || cardIds.Count != REQUIRED_CARD_COUNT || members.Count != REQUIRED_CARD_COUNT)
        {
            return false;
        }

        List<string> ownedCardIds = playerData.OwnedCardIds;

        for (int i = 0; i < cardIds.Count; i++)
        {
            string cardId = cardIds[i];

            if (string.IsNullOrEmpty(cardId) || !ownedCardIds.Contains(cardId))
            {
                return false;
            }

            if (!MasterDataProvider.Instance.TryGetCard(cardId, out CardData card) || card.CharacterId != members[i].CharacterId)
            {
                return false;
            }

            for (int j = 0; j < i; j++)
            {
                if (cardIds[j] == cardId)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
