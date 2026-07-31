using System;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 포토카드 선택 팝업 상단의 "1. 포토카드 세팅" / "2. 의상 &amp; 악세서리" 탭을 관리합니다.
/// 두 편성 화면은 같은 자리를 쓰므로, 선택된 탭의 패널만 켜고 나머지는 끕니다.
/// </summary>
public class UI_PhotocardSelectTabs : MonoBehaviour
{
    public Action<EPhotocardSelectTab> OnTabSelected;

    [Foldout("Hierarchy")]
    [Header("Photocard")]
    [SerializeField]
    private Button _photocardTabButton;

    [SerializeField]
    private GameObject _photocardPanel;

    [Foldout("Hierarchy")]
    [Header("Item")]
    [SerializeField]
    private Button _itemTabButton;

    [SerializeField]
    private GameObject _itemPanel;

    public EPhotocardSelectTab SelectedTab { get; private set; } = EPhotocardSelectTab.PHOTOCARD;

    private void Awake()
    {
        _photocardTabButton.onClick.AddListener(() => NotifyTabSelected(EPhotocardSelectTab.PHOTOCARD));
        _itemTabButton.onClick.AddListener(() => NotifyTabSelected(EPhotocardSelectTab.ITEM));
    }

    /// <summary>
    /// 선택된 탭의 패널만 남기고 나머지를 끕니다.
    /// 두 탭 버튼은 색 대비가 없어 눌린 상태가 드러나지 않으므로,
    /// 이미 보고 있는 화면의 탭은 누를 수 없게 두어 선택 상태를 표시합니다.
    /// </summary>
    public void SetSelectedTab(EPhotocardSelectTab tab)
    {
        SelectedTab = tab;

        _photocardPanel.SetActive(tab == EPhotocardSelectTab.PHOTOCARD);
        _itemPanel.SetActive(tab == EPhotocardSelectTab.ITEM);

        _photocardTabButton.interactable = tab != EPhotocardSelectTab.PHOTOCARD;
        _itemTabButton.interactable = tab != EPhotocardSelectTab.ITEM;
    }

    private void NotifyTabSelected(EPhotocardSelectTab tab)
    {
        if (SelectedTab == tab)
        {
            return;
        }

        OnTabSelected?.Invoke(tab);
    }
}
