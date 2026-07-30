using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 곡 선택 화면의 곡 목록에서 곡 한 개를 표시하는 행입니다.
/// 풀에서 재사용되므로 표시 상태는 SetItem에서 매번 새로 덮어씁니다.
/// </summary>
public class UI_MusicSelectSongListItem : MonoBehaviour
{
    public Action<int> OnItemClicked;

    private const string UNPLAYED_RANK_TEXT = "-";

    [Header("Selection Colors")]
    [SerializeField]
    private Color _normalColor = new Color(1f, 1f, 1f, 1f);

    [SerializeField]
    private Color _selectedColor = new Color(0.55f, 0.38f, 0.94f, 1f);

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _button;

    [SerializeField]
    private Image _background;

    [SerializeField]
    private Image _coverImage;

    [SerializeField]
    private TMP_Text _songNameText;

    [SerializeField]
    private TMP_Text _rankText;

    [Foldout("Project")]
    [SerializeField]
    private Sprite _placeholderCover;

    private int _itemIndex;

    private void Awake()
    {
        _button.onClick.AddListener(NotifyClicked);
    }

    public void SetItem(int itemIndex, string songTitle, SongRecord bestRecord, bool isInteractable)
    {
        _itemIndex = itemIndex;
        _songNameText.text = songTitle;
        _rankText.text = ReferenceEquals(bestRecord, null) ? UNPLAYED_RANK_TEXT : bestRecord.BestRank.ToString();
        _button.interactable = isInteractable;

        // 커버는 나중에 비동기로 도착하므로, 풀에서 재사용된 행이 이전 곡의 커버를 달고 나오지 않도록 먼저 지웁니다.
        _coverImage.sprite = _placeholderCover;

        SetSelected(false);
    }

    /// <summary>
    /// 커버 이미지는 비동기로 도착하므로 행 표시와 분리해 나중에 채웁니다. 커버가 없는 곡은 기본 이미지를 유지합니다.
    /// </summary>
    public void SetCover(Sprite cover)
    {
        if (cover == null)
        {
            return;
        }

        _coverImage.sprite = cover;
    }

    public void SetSelected(bool isSelected)
    {
        _background.color = isSelected ? _selectedColor : _normalColor;
    }

    /// <summary>
    /// 풀로 반환되기 전에 호출되어, 다음 사용자가 이전 구독자와 표시 상태를 물려받지 않도록 비웁니다.
    /// </summary>
    public void ResetItem()
    {
        OnItemClicked = null;
        _itemIndex = 0;
        _button.interactable = true;
        _rankText.text = UNPLAYED_RANK_TEXT;
        _coverImage.sprite = _placeholderCover;
        SetSelected(false);
    }

    private void NotifyClicked()
    {
        OnItemClicked?.Invoke(_itemIndex);
    }
}
