using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 편성 슬롯 5칸을 일괄 갱신합니다(기획서 3-H-2 멤버 슬롯).
/// 칸 수가 멤버 수로 고정이라 풀링 없이 직렬화 목록으로 둡니다(주간 표 7칸과 같은 방식).
/// </summary>
public class UI_PhotocardSlotList : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<UI_PhotocardSlotItem> _slotItems = new List<UI_PhotocardSlotItem>();

    /// <summary>
    /// 현재 임시 편성(LiveLoadoutContext)을 슬롯 순서대로 그립니다. 컨텍스트 초기화 후에 불러야 합니다.
    /// </summary>
    public void RefreshSlots()
    {
        IReadOnlyList<string> cardIds = LiveLoadoutContext.Instance.CardIds;

        for (int i = 0; i < _slotItems.Count; i++)
        {
            // 기본 편성이 5장 미만인 손상 세이브면 남는 슬롯이 생깁니다. 진입은 어차피 차단되므로 표시만 비웁니다.
            if (cardIds.Count <= i)
            {
                _slotItems[i].SetUnknownCard(string.Empty);
                continue;
            }

            if (MasterDataProvider.Instance.TryGetCard(cardIds[i], out CardData card))
            {
                _slotItems[i].SetCard(card);
            }
            else
            {
                _slotItems[i].SetUnknownCard(cardIds[i]);
            }
        }
    }
}
