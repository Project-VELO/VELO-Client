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
    private UI_RankIcon _rankIcon;

    [Foldout("Project")]
    [SerializeField]
    private Sprite _normalSprite;

    [SerializeField]
    private Sprite _selectedSprite;

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
        _button.interactable = isInteractable;

        SetRank(bestRecord);

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

    /// <summary>
    /// 한 번도 클리어하지 않은 곡은 등급 자리를 비웁니다(시안에 미기록 표시가 따로 없습니다).
    /// </summary>
    private void SetRank(SongRecord bestRecord)
    {
        if (ReferenceEquals(bestRecord, null))
        {
            _rankIcon.Clear();
            return;
        }

        _rankIcon.RefreshRank(bestRecord.BestRank);
    }

    /// <summary>
    /// 선택 여부를 색이 아니라 그림으로 구분합니다. 선택된 행은 테두리와 광원이 따로 그려져 있어
    /// 한 장을 틴트하는 것으로는 시안이 나오지 않습니다.
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        _background.sprite = isSelected ? _selectedSprite : _normalSprite;
    }

    /// <summary>
    /// 풀로 반환되기 전에 호출되어, 다음 사용자가 이전 구독자와 표시 상태를 물려받지 않도록 비웁니다.
    /// </summary>
    public void ResetItem()
    {
        OnItemClicked = null;
        _itemIndex = 0;
        _button.interactable = true;
        _rankIcon.Clear();
        _coverImage.sprite = _placeholderCover;
        SetSelected(false);
    }

    private void NotifyClicked()
    {
        OnItemClicked?.Invoke(_itemIndex);
    }
}
