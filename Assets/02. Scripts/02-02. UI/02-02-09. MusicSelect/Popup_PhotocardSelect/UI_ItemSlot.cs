using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 의상·악세사리 목록의 1칸입니다(P_UI_Item).
/// 클릭하면 기존 선택을 이 아이템으로 교체합니다(기획서 3-H-6). 선택 해제가 없으므로
/// 이미 선택 중인 칸은 배지를 켜고 클릭을 막는 것이 곧 장착 상태 표시입니다.
/// </summary>
public class UI_ItemSlot : MonoBehaviour
{
    public Action<string> OnClicked;

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _button;

    [SerializeField]
    private TMP_Text _nameText;

    /// <summary>
    /// "장착 중" 배지입니다.
    /// </summary>
    [SerializeField]
    private GameObject _equippedRoot;

    private string _itemId;

    private void Awake()
    {
        _button.onClick.AddListener(NotifyClicked);
    }

    public void SetItem(ItemData item, bool isEquipped)
    {
        gameObject.SetActive(true);

        _itemId = item.ItemId;
        _nameText.text = item.DisplayName;
        _equippedRoot.SetActive(isEquipped);
        _button.interactable = !isEquipped;
    }

    /// <summary>
    /// 아이템 수보다 칸이 많을 때의 남는 칸입니다. 빈 칸을 클릭 대상으로 남기지 않도록 통째로 숨깁니다.
    /// </summary>
    public void SetEmpty()
    {
        _itemId = null;
        gameObject.SetActive(false);
    }

    private void NotifyClicked()
    {
        if (string.IsNullOrEmpty(_itemId))
        {
            return;
        }

        OnClicked?.Invoke(_itemId);
    }
}
