using System.Collections.Generic;

/// <summary>
/// LIVE 준비 화면에서 변경한 편성(포토카드 5장·의상·악세사리)을 씬 사이에서 임시로 유지하는 싱글톤입니다.
///
/// PlayerData의 편성 값은 "기본 편성"으로서 신규 게임 이후 바뀌지 않고, 여기의 값만 현재 LIVE에
/// 임시 적용됩니다(기획서 3-H-4). 그래서 폐기·복원(준비 화면 뒤로가기, 결과 확인)은 Clear() 한 번으로
/// 끝나고, 다시 플레이하기(3-J-5)의 "직전 편성 유지"는 아무것도 하지 않으면 성립합니다.
/// 저장 파일은 이 클래스와 무관하므로 세이브 스키마도 바뀌지 않습니다.
/// </summary>
public class LiveLoadoutContext : POCOSingleton<LiveLoadoutContext>
{
    private readonly List<string> _cardIds = new List<string>();

    /// <summary>
    /// LIVE 준비 화면이 열려 초기화된 적이 있는지 여부입니다. false면 기본 편성(PlayerData)이 곧 현재 편성입니다.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// 멤버 슬롯 순서(기획서 3-H-2)의 편성 카드입니다. IsInitialized가 false면 비어 있습니다.
    /// </summary>
    public IReadOnlyList<string> CardIds => _cardIds;

    public string CostumeId { get; private set; }
    public string AccessoryId { get; private set; }

    /// <summary>
    /// LIVE 준비 화면이 열릴 때 호출합니다. 이미 초기화되어 있으면 그대로 둡니다 —
    /// 다시 플레이하기를 거쳐 유지 중인 편성이 재진입 초기화로 사라지면 안 되기 때문입니다.
    /// </summary>
    public void InitFromPlayerData()
    {
        if (IsInitialized)
        {
            return;
        }

        ResetPhotocards();
        ResetItems();
        IsInitialized = true;
    }

    /// <summary>
    /// 보유 카드를 클릭했을 때의 즉시 교체입니다(기획서 3-H-3). 카드의 멤버가 정하는 슬롯 하나만 바뀌므로
    /// 중복 편성·슬롯 공백이 생길 경로가 없습니다.
    /// </summary>
    public bool TrySwapCard(string cardId)
    {
        if (!IsInitialized || !LiveLoadoutRule.TryGetSlotIndex(cardId, out int slotIndex))
        {
            return false;
        }

        // 슬롯 수는 멤버 수로 고정이므로(ResetPhotocards) 범위를 벗어날 일이 없습니다.
        // 마스터 데이터가 실행 중에 바뀌는 경우에만 걸리는 방어입니다.
        if (slotIndex >= _cardIds.Count)
        {
            return false;
        }

        _cardIds[slotIndex] = cardId;
        return true;
    }

    /// <summary>
    /// 보유 카드 목록에서 편성 중 표시(반투명·클릭 불가)에 사용합니다.
    /// </summary>
    public bool IsCardEquipped(string cardId)
    {
        return _cardIds.Contains(cardId);
    }

    public void SetCostume(string itemId)
    {
        // 선택 해제는 지원하지 않으므로(기획서 3-H-6) 빈 값 대입은 받지 않습니다.
        if (!string.IsNullOrEmpty(itemId))
        {
            CostumeId = itemId;
        }
    }

    public void SetAccessory(string itemId)
    {
        if (!string.IsNullOrEmpty(itemId))
        {
            AccessoryId = itemId;
        }
    }

    /// <summary>
    /// 초기화 버튼(포토카드 탭)입니다. 슬롯을 비우는 기능이 아니라 기본 편성 복원입니다(기획서 3-H-3-1).
    ///
    /// 저장된 목록을 그대로 옮기지 않고 멤버 슬롯에 배치해 받습니다. 그래야 칸 수가 항상 멤버 수와 같아,
    /// 기본 편성이 모자라거나 순서가 어긋난 세이브에서도 모든 칸을 교체로 채울 수 있습니다.
    /// </summary>
    public void ResetPhotocards()
    {
        _cardIds.Clear();
        _cardIds.AddRange(LiveLoadoutRule.BuildSlotCardIds(PlayerDataProvider.Instance.Data.SelectedCardIds));
    }

    /// <summary>
    /// 초기화 버튼(의상 &amp; 악세사리 탭)입니다. 기본 의상·악세사리로 복원합니다.
    /// </summary>
    public void ResetItems()
    {
        PlayerData data = PlayerDataProvider.Instance.Data;
        CostumeId = data.SelectedCostumeId;
        AccessoryId = data.SelectedAccessoryId;
    }

    /// <summary>
    /// 변경 폐기입니다. 준비 화면 뒤로가기(757)와 결과 확인(759)이 호출하며,
    /// 다음 InitFromPlayerData가 기본 편성을 다시 복사하므로 이것이 곧 "기본 복원"입니다.
    /// </summary>
    public void Clear()
    {
        _cardIds.Clear();
        CostumeId = null;
        AccessoryId = null;
        IsInitialized = false;
    }
}
