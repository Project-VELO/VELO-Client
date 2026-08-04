using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 보유 카드 목록입니다(P_UI_Panel_OwnedPhotocards). 고정 5칸 + 좌/우 페이징으로 보유 10장을 탐색합니다.
///
/// 기획서 11.2의 "멤버 리스트(캐릭터별 카드 필터)"는 시안 프리팹에 대응 요소가 없어
/// 시안 그대로 페이징으로 진행합니다(계획서 §2-B, 기획 질의 중).
/// </summary>
public class UI_OwnedCardList : MonoBehaviour
{
    public Action<string> OnCardClicked;

    [Foldout("Hierarchy")]
    [SerializeField]
    private List<UI_OwnedCardItem> _cardItems = new List<UI_OwnedCardItem>();

    [SerializeField]
    private Button _leftButton;

    [SerializeField]
    private Button _rightButton;

    private readonly List<CardData> _ownedCards = new List<CardData>();
    private int _pageIndex;

    private void Awake()
    {
        _leftButton.onClick.AddListener(() => MovePage(-1));
        _rightButton.onClick.AddListener(() => MovePage(1));

        foreach (UI_OwnedCardItem item in _cardItems)
        {
            item.OnClicked = NotifyCardClicked;
        }
    }

    /// <summary>
    /// 팝업이 열릴 때 첫 페이지로 되돌립니다. 페이지는 컴포넌트에 남으므로,
    /// 이전에 보던 페이지가 다음 열람에 그대로 나오지 않게 합니다. 교체 후 갱신은 페이지를 유지해야 하므로 따로 둡니다.
    /// </summary>
    public void ResetPage()
    {
        _pageIndex = 0;
    }

    /// <summary>
    /// 보유 목록을 다시 읽어 현재 페이지를 그립니다. 편성 교체 후에도 호출해 배지를 맞춥니다.
    /// 목록 순서는 보유 목록(newgame_config) 순서 그대로입니다.
    /// </summary>
    public void RefreshCards()
    {
        _ownedCards.Clear();

        foreach (string cardId in PlayerDataProvider.Instance.Data.OwnedCardIds)
        {
            // 마스터에 없는 보유 카드는 표시할 방법이 없으므로 건너뜁니다. 진입 판정과는 무관합니다.
            if (MasterDataProvider.Instance.TryGetCard(cardId, out CardData card))
            {
                _ownedCards.Add(card);
            }
        }

        _pageIndex = Mathf.Clamp(_pageIndex, 0, GetMaxPageIndex());
        RefreshPage();
    }

    private void RefreshPage()
    {
        LiveLoadoutContext loadout = LiveLoadoutContext.Instance;
        int startIndex = _pageIndex * _cardItems.Count;

        for (int i = 0; i < _cardItems.Count; i++)
        {
            int cardIndex = startIndex + i;

            if (cardIndex < _ownedCards.Count)
            {
                CardData card = _ownedCards[cardIndex];
                _cardItems[i].SetCard(card, loadout.IsCardEquipped(card.CardId));
            }
            else
            {
                _cardItems[i].SetEmpty();
            }
        }

        _leftButton.interactable = _pageIndex > 0;
        _rightButton.interactable = _pageIndex < GetMaxPageIndex();
    }

    private void MovePage(int direction)
    {
        _pageIndex = Mathf.Clamp(_pageIndex + direction, 0, GetMaxPageIndex());
        RefreshPage();
    }

    private int GetMaxPageIndex()
    {
        if (_ownedCards.Count == 0 || _cardItems.Count == 0)
        {
            return 0;
        }

        return (_ownedCards.Count - 1) / _cardItems.Count;
    }

    private void NotifyCardClicked(string cardId)
    {
        OnCardClicked?.Invoke(cardId);
    }
}
