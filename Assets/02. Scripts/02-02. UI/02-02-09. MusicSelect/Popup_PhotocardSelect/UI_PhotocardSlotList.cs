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
    /// 카드 일러스트 표입니다. 슬롯이 각자 들고 있으면 카드가 바뀔 때마다 다섯 벌을 고쳐야 합니다.
    /// </summary>
    [SerializeField]
    private UI_CardArtBinder _artBinder;

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
                _slotItems[i].SetCard(card, GetIllustration(cardIds[i]));
            }
            else
            {
                _slotItems[i].SetUnknownCard(cardIds[i]);
            }
        }
    }

    /// <summary>
    /// 바인더를 물리지 않은 화면에서도 이름·등급 표시는 그대로 돌아가야 하므로 null을 허용합니다.
    /// </summary>
    private Sprite GetIllustration(string cardId)
    {
        return _artBinder != null ? _artBinder.GetSprite(cardId) : null;
    }
}
