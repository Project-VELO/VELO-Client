using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 곡/난이도 목록 팝업에서 항목 하나를 표시하는 버튼입니다.
/// 풀에서 재사용되므로 표시 상태는 SetItem에서 매번 새로 덮어씁니다.
/// </summary>
public class UI_LiveEditorListItem : MonoBehaviour
{
    public Action<int> OnItemClicked;

    [Header("Selection Colors")]
    [SerializeField]
    private Color _normalColor = new Color(0.93f, 0.93f, 0.95f, 1f);

    [SerializeField]
    private Color _selectedColor = new Color(0.29f, 0.56f, 0.96f, 1f);

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _button;

    [SerializeField]
    private Image _background;

    [SerializeField]
    private TMP_Text _label;

    private int _itemIndex;

    private void Awake()
    {
        _button.onClick.AddListener(NotifyClicked);
    }

    public void SetItem(int itemIndex, string label, bool isInteractable)
    {
        _itemIndex = itemIndex;
        _label.text = label;
        _button.interactable = isInteractable;
        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        _background.color = isSelected ? _selectedColor : _normalColor;
    }

    /// <summary>
    /// 풀로 반환되기 전에 호출되어, 다음 사용자가 이전 구독자를 물려받지 않도록 상태를 비웁니다.
    /// </summary>
    public void ResetItem()
    {
        OnItemClicked = null;
        _itemIndex = 0;
        _button.interactable = true;
        SetSelected(false);
    }

    private void NotifyClicked()
    {
        OnItemClicked?.Invoke(_itemIndex);
    }
}
