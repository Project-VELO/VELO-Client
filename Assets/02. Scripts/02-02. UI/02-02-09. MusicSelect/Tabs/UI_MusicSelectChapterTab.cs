using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 곡 선택 화면의 챕터 탭 버튼 하나입니다. 잠긴 챕터는 선택 강조 대신 잠금 표시를 유지합니다.
/// </summary>
public class UI_MusicSelectChapterTab : MonoBehaviour
{
    public Action<int> OnTabClicked;

    /// <summary>
    /// 잠긴 챕터는 전용 아트가 없어, 미선택 그림을 어둡게 눌러 구분합니다.
    /// </summary>
    [Header("Tab Colors")]
    [SerializeField]
    private Color _lockedColor = new Color(0.45f, 0.45f, 0.5f, 1f);

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _button;

    [SerializeField]
    private Image _background;

    [SerializeField]
    private TMP_Text _label;

    [Foldout("Project")]
    [SerializeField]
    private Sprite _normalSprite;

    [SerializeField]
    private Sprite _selectedSprite;

    private int _tabIndex;
    private bool _isUnlocked = true;

    public bool IsUnlocked => _isUnlocked;

    private void Awake()
    {
        _button.onClick.AddListener(NotifyClicked);
    }

    public void SetTab(int tabIndex, string displayName, bool isUnlocked)
    {
        _tabIndex = tabIndex;
        _label.text = displayName;
        _isUnlocked = isUnlocked;
        _button.interactable = isUnlocked;

        SetSelected(false);
    }

    /// <summary>
    /// 선택 여부를 색이 아니라 그림으로 구분합니다. 두 상태의 테두리와 광원이 서로 달라
    /// 한 장을 틴트하는 것으로는 시안이 나오지 않습니다.
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        if (!_isUnlocked)
        {
            _background.sprite = _normalSprite;
            _background.color = _lockedColor;
            return;
        }

        _background.sprite = isSelected ? _selectedSprite : _normalSprite;
        _background.color = Color.white;
    }

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    private void NotifyClicked()
    {
        OnTabClicked?.Invoke(_tabIndex);
    }
}
