using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스토리 목록의 회차 카드 한 장입니다(기획서 5.2 "스토리 카드 = 회차 명, 제목, 상태 표시").
///
/// 풀에서 재사용되므로 표시 상태는 SetItem에서 매번 새로 덮어씁니다.
/// 목적지·팝업·진행 상태 변경은 이 클래스가 하지 않고 클릭만 위로 던집니다.
/// 카드가 목적지를 알면 잠금 판정과 진입 규칙이 카드마다 갈라집니다.
/// </summary>
public class UI_SelectStoryEpisodeItem : MonoBehaviour
{
    public Action<string> OnItemClicked;

    private const string EPISODE_FORMAT = "{0}화";
    private const string LOCKED_LABEL = "LOCKED";

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _button;

    [SerializeField]
    private TMP_Text _episodeNumberText;

    [SerializeField]
    private TMP_Text _titleText;

    /// <summary>
    /// 상태 문구를 담은 루트입니다. UNLOCKED에는 표시할 문구가 없어 통째로 끕니다(기획서 5.5).
    /// </summary>
    [SerializeField]
    private GameObject _statusRoot;

    [SerializeField]
    private TMP_Text _statusText;

    /// <summary>
    /// 자물쇠 표시입니다. 기획서 5.5가 LOCKED에만 요구합니다.
    /// </summary>
    [SerializeField]
    private GameObject _lockIcon;

    /// <summary>
    /// 감상을 마친 회차 표시입니다. 완료는 문구 없이 이 아이콘으로만 알립니다.
    /// </summary>
    [SerializeField]
    private GameObject _clearIcon;

    [SerializeField]
    private GameObject _newBadge;

    /// <summary>
    /// 현재 선택된 카드 표시입니다(기획서 5.2 "선택 표시").
    /// </summary>
    [SerializeField]
    private GameObject _selectionOutline;

    /// <summary>
    /// 스케줄 바로가기로 진입했을 때의 대상 표시입니다(기획서 3-F-3-1).
    /// 선택 표시와 별개입니다. 강조됐다고 해서 선택된 것은 아니며 진입에는 여전히 2회 클릭이 필요합니다.
    /// </summary>
    [SerializeField]
    private GameObject _highlight;

    private string _storyId;

    public string StoryId => _storyId;

    private void Awake()
    {
        _button.onClick.AddListener(NotifyClicked);
    }

    public void SetItem(StoryData story, IStoryProgress progress)
    {
        _storyId = story.StoryId;
        _episodeNumberText.text = string.Format(EPISODE_FORMAT, story.EpisodeNumber);
        _titleText.text = string.IsNullOrEmpty(story.Title) ? story.StoryId : story.Title;

        SetProgress(progress);
        SetSelected(false);
        SetHighlight(false);
    }

    /// <summary>
    /// 잠긴 카드도 클릭은 받습니다. 기획서 5.4가 잠긴 스토리를 클릭하면 안내 팝업을 띄우라고 규정하므로,
    /// 버튼을 비활성으로 만들면 그 팝업을 띄울 방법이 없어집니다.
    /// </summary>
    private void SetProgress(IStoryProgress progress)
    {
        bool isLocked = ReferenceEquals(progress, null) || !progress.CanEnter;
        bool isCompleted = !ReferenceEquals(progress, null) && progress.IsCompleted;

        _lockIcon.SetActive(isLocked);
        _clearIcon.SetActive(!isLocked && isCompleted);
        _newBadge.SetActive(!ReferenceEquals(progress, null) && progress.IsNew);

        if (isLocked)
        {
            SetStatusLabel(LOCKED_LABEL);
            return;
        }

        // 완료는 아이콘 하나로 충분해 문구를 띄우지 않습니다. 잠금만 문구로 알립니다.
        _statusRoot.SetActive(false);
    }

    private void SetStatusLabel(string label)
    {
        _statusRoot.SetActive(true);
        _statusText.text = label;
    }

    public void SetSelected(bool isSelected)
    {
        _selectionOutline.SetActive(isSelected);
    }

    public void SetHighlight(bool isHighlighted)
    {
        _highlight.SetActive(isHighlighted);
    }

    /// <summary>
    /// 풀로 반환되기 전에 호출해, 다음 사용자가 이전 구독자와 표시 상태를 물려받지 않게 합니다.
    /// </summary>
    public void ResetItem()
    {
        OnItemClicked = null;
        _storyId = null;
        _statusRoot.SetActive(false);
        _lockIcon.SetActive(false);
        _clearIcon.SetActive(false);
        _newBadge.SetActive(false);
        SetSelected(false);
        SetHighlight(false);
    }

    private void NotifyClicked()
    {
        if (string.IsNullOrEmpty(_storyId))
        {
            return;
        }

        OnItemClicked?.Invoke(_storyId);
    }
}
