using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 보유 카드 목록의 1칸입니다(P_UI_OwnedPhotocard).
/// 클릭하면 카드의 멤버 슬롯으로 즉시 교체됩니다(기획서 3-H-3). 이미 편성 중인 카드는
/// "교체된 카드가 보유 목록으로 돌아간다"의 역상태이므로 배지를 켜고 클릭을 막습니다.
/// </summary>
public class UI_OwnedCardItem : MonoBehaviour
{
    public Action<string> OnClicked;

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _button;

    [SerializeField]
    private TMP_Text _nameText;

    /// <summary>
    /// "편성 중" 배지입니다(TextBlock_Equipped).
    /// </summary>
    [SerializeField]
    private GameObject _equippedRoot;

    private string _cardId;

    private void Awake()
    {
        _button.onClick.AddListener(NotifyClicked);
    }

    public void SetCard(CardData card, bool isEquipped)
    {
        gameObject.SetActive(true);

        _cardId = card.CardId;
        _nameText.text = card.CardName;
        _equippedRoot.SetActive(isEquipped);
        _button.interactable = !isEquipped;
    }

    /// <summary>
    /// 마지막 페이지의 남는 칸입니다. 빈 칸을 클릭 대상으로 남기지 않도록 통째로 숨깁니다.
    /// </summary>
    public void SetEmpty()
    {
        _cardId = null;
        gameObject.SetActive(false);
    }

    private void NotifyClicked()
    {
        if (string.IsNullOrEmpty(_cardId))
        {
            return;
        }

        OnClicked?.Invoke(_cardId);
    }
}
