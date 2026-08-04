using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// "의상 &amp; 악세사리" 탭의 설정 영역입니다(P_UI_Panel_ItemSetting, 기획서 3-H-6).
///
/// 목록은 items.json 전체를 표시합니다. MVP에는 아이템 획득 경로가 없어 보유 기준으로는 1벌뿐이라
/// "새로 선택하면 교체된다"(16-9)를 검증할 수 없기 때문입니다(계획서 §2-D, 기획 질의 중).
/// 칸 수(각 4)가 목록(각 2)보다 많아 페이징은 두지 않습니다 — 좌/우 버튼은 프리팹에서 비활성 고정.
/// </summary>
public class UI_ItemSettingPanel : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<UI_ItemSlot> _costumeSlots = new List<UI_ItemSlot>();

    [SerializeField]
    private List<UI_ItemSlot> _accessorySlots = new List<UI_ItemSlot>();

    private void Awake()
    {
        foreach (UI_ItemSlot slot in _costumeSlots)
        {
            slot.OnClicked = SetCostume;
        }

        foreach (UI_ItemSlot slot in _accessorySlots)
        {
            slot.OnClicked = SetAccessory;
        }
    }

    /// <summary>
    /// 두 목록과 장착 배지를 현재 임시 편성 기준으로 다시 그립니다. 컨텍스트 초기화 후에 불러야 합니다.
    /// </summary>
    public void RefreshItems()
    {
        LiveLoadoutContext loadout = LiveLoadoutContext.Instance;

        RefreshSlots(_costumeSlots, MasterDataQuery.GetItemsByType(EItemType.COSTUME), loadout.CostumeId);
        RefreshSlots(_accessorySlots, MasterDataQuery.GetItemsByType(EItemType.ACCESSORY), loadout.AccessoryId);
    }

    private static void RefreshSlots(List<UI_ItemSlot> slots, List<ItemData> items, string equippedItemId)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Count)
            {
                slots[i].SetItem(items[i], items[i].ItemId == equippedItemId);
            }
            else
            {
                slots[i].SetEmpty();
            }
        }
    }

    private void SetCostume(string itemId)
    {
        LiveLoadoutContext.Instance.SetCostume(itemId);
        RefreshItems();
    }

    private void SetAccessory(string itemId)
    {
        LiveLoadoutContext.Instance.SetAccessory(itemId);
        RefreshItems();
    }
}
