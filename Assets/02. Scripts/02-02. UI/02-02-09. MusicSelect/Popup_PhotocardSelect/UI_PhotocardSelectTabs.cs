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

    [Foldout("Hierarchy")]
    [Header("탭 배경")]
    /// <summary>
    /// 탭 버튼의 배경입니다. 선택 여부에 따라 그림을 갈아 끼웁니다.
    /// </summary>
    [SerializeField]
    private Image _photocardTabImage;

    [SerializeField]
    private Image _itemTabImage;

    [Foldout("Project")]
    [Header("탭 배경 그림")]
    [SerializeField]
    private Sprite _photocardSelectedSprite;

    [SerializeField]
    private Sprite _photocardNormalSprite;

    [SerializeField]
    private Sprite _itemSelectedSprite;

    [SerializeField]
    private Sprite _itemNormalSprite;

    public EPhotocardSelectTab SelectedTab { get; private set; } = EPhotocardSelectTab.PHOTOCARD;

    private void Awake()
    {
        _photocardTabButton.onClick.AddListener(() => NotifyTabSelected(EPhotocardSelectTab.PHOTOCARD));
        _itemTabButton.onClick.AddListener(() => NotifyTabSelected(EPhotocardSelectTab.ITEM));
    }

    /// <summary>
    /// 선택된 탭의 패널만 남기고 나머지를 끕니다.
    ///
    /// 탭 배경은 선택·비선택 두 장을 갈아 끼웁니다. 탭 이름이 그림에 들어 있어 색만 바꾸는 것으로는
    /// 선택 상태가 드러나지 않습니다. 보고 있는 화면의 탭을 누를 수 없게 두는 것은 그대로 둡니다.
    /// </summary>
    public void SetSelectedTab(EPhotocardSelectTab tab)
    {
        SelectedTab = tab;

        _photocardPanel.SetActive(tab == EPhotocardSelectTab.PHOTOCARD);
        _itemPanel.SetActive(tab == EPhotocardSelectTab.ITEM);

        _photocardTabButton.interactable = tab != EPhotocardSelectTab.PHOTOCARD;
        _itemTabButton.interactable = tab != EPhotocardSelectTab.ITEM;

        bool isPhotocard = tab == EPhotocardSelectTab.PHOTOCARD;
        SetSprite(_photocardTabImage, isPhotocard ? _photocardSelectedSprite : _photocardNormalSprite);
        SetSprite(_itemTabImage, isPhotocard ? _itemNormalSprite : _itemSelectedSprite);
    }

    /// <summary>
    /// 아트가 아직 안 들어온 탭은 비워 두어도 됩니다. 빈 스프라이트를 대입하면 흰 사각형이
    /// 남아, 배경이 없는 것보다 더 어색해집니다.
    /// </summary>
    private static void SetSprite(Image target, Sprite sprite)
    {
        if (target == null || sprite == null)
        {
            return;
        }

        target.sprite = sprite;
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
