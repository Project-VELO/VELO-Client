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

    [Header("Tab Colors")]
    [SerializeField]
    private Color _normalColor = new Color(0.16f, 0.15f, 0.24f, 1f);

    [SerializeField]
    private Color _selectedColor = new Color(0.55f, 0.38f, 0.94f, 1f);

    [SerializeField]
    private Color _lockedColor = new Color(0.25f, 0.25f, 0.28f, 1f);

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _button;

    [SerializeField]
    private Image _background;

    [SerializeField]
    private TMP_Text _label;

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

    public void SetSelected(bool isSelected)
    {
        if (!_isUnlocked)
        {
            _background.color = _lockedColor;
            return;
        }

        _background.color = isSelected ? _selectedColor : _normalColor;
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
