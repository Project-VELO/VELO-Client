using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 일시정지 팝업 상단의 "일시정지" / "설정" 탭을 관리합니다.
/// 두 화면은 같은 자리를 쓰므로, 선택된 탭의 패널만 켜고 나머지는 끕니다.
/// </summary>
public class UI_LivePauseTabs : MonoBehaviour
{
    public Action<ELivePauseTab> OnTabSelected;

    [Foldout("Hierarchy")]
    [Header("Pause")]
    [SerializeField]
    private Button _pauseTabButton;

    [SerializeField]
    private GameObject _pausePanel;

    [Foldout("Hierarchy")]
    [Header("Setting")]
    [SerializeField]
    private Button _settingTabButton;

    [SerializeField]
    private GameObject _settingPanel;

    [Foldout("Hierarchy")]
    [Header("탭 배경")]
    /// <summary>
    /// 탭 버튼의 배경입니다. 선택 여부에 따라 그림을 갈아 끼웁니다.
    /// </summary>
    [SerializeField]
    private Image _pauseTabImage;

    /// <summary>
    /// 탭 글자입니다. 그림에 구워 두면 언어를 바꿔도 한글이 남아 밖으로 빼냈습니다.
    /// </summary>
    [SerializeField]
    private TMP_Text _pauseTabLabel;

    [SerializeField]
    private Image _settingTabImage;

    [SerializeField]
    private TMP_Text _settingTabLabel;

    [Foldout("Project")]
    [Header("탭 배경 그림")]
    [SerializeField]
    private Sprite _selectedSprite;

    [SerializeField]
    private Sprite _normalSprite;

    [Foldout("Settings")]
    [Header("탭 글자색")]
    [SerializeField]
    private Color _selectedTextColor = Color.white;

    [SerializeField]
    private Color _normalTextColor = new Color(0.19f, 0.14f, 0.45f, 1f);

    public ELivePauseTab SelectedTab { get; private set; } = ELivePauseTab.PAUSE;

    private void Awake()
    {
        _pauseTabButton.onClick.AddListener(SelectPauseTab);
        _settingTabButton.onClick.AddListener(SelectSettingTab);
    }

    private void OnDestroy()
    {
        _pauseTabButton.onClick.RemoveListener(SelectPauseTab);
        _settingTabButton.onClick.RemoveListener(SelectSettingTab);
    }

    /// <summary>
    /// 선택된 탭의 패널만 남기고 나머지를 끕니다.
    /// 탭 이름이 그림에 들어 있지 않으므로 배경과 글자색을 함께 바꿔 선택 상태를 드러냅니다.
    /// </summary>
    public void SetSelectedTab(ELivePauseTab tab)
    {
        SelectedTab = tab;

        bool isPause = tab == ELivePauseTab.PAUSE;

        _pausePanel.SetActive(isPause);
        _settingPanel.SetActive(!isPause);

        // 보고 있는 화면의 탭을 다시 누르는 것은 아무 일도 하지 않으므로 눌리지 않게 둡니다.
        _pauseTabButton.interactable = !isPause;
        _settingTabButton.interactable = isPause;

        SetSprite(_pauseTabImage, isPause ? _selectedSprite : _normalSprite);
        SetSprite(_settingTabImage, isPause ? _normalSprite : _selectedSprite);

        SetLabelColor(_pauseTabLabel, isPause);
        SetLabelColor(_settingTabLabel, !isPause);
    }

    private void SelectPauseTab()
    {
        NotifyTabSelected(ELivePauseTab.PAUSE);
    }

    private void SelectSettingTab()
    {
        NotifyTabSelected(ELivePauseTab.SETTING);
    }

    private void NotifyTabSelected(ELivePauseTab tab)
    {
        if (SelectedTab == tab)
        {
            return;
        }

        OnTabSelected?.Invoke(tab);
    }

    private void SetLabelColor(TMP_Text target, bool isSelected)
    {
        if (target == null)
        {
            return;
        }

        target.color = isSelected ? _selectedTextColor : _normalTextColor;
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
}
