using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 챕터 탭 묶음과 좌우 이동 버튼을 관리합니다.
/// 탭 버튼은 프리팹에 고정 개수로 배치되어 있으므로, 수록된 챕터 수만큼만 노출하고 나머지는 감춥니다.
/// </summary>
public class UI_MusicSelectChapterTabs : MonoBehaviour
{
    public Action<int> OnChapterSelected;

    [Foldout("Hierarchy")]
    [SerializeField]
    private List<UI_MusicSelectChapterTab> _tabs = new List<UI_MusicSelectChapterTab>();

    [SerializeField]
    private Button _previousButton;

    [SerializeField]
    private Button _nextButton;

    private int _chapterCount;
    private int _selectedIndex = -1;

    private void Awake()
    {
        for (int i = 0; i < _tabs.Count; i++)
        {
            _tabs[i].OnTabClicked = NotifyChapterSelected;
        }

        _previousButton.onClick.AddListener(SelectPreviousChapter);
        _nextButton.onClick.AddListener(SelectNextChapter);
    }

    public void RefreshChapters(IReadOnlyList<LiveChapterData> chapters)
    {
        _chapterCount = Mathf.Min(chapters.Count, _tabs.Count);

        if (chapters.Count > _tabs.Count)
        {
            Debug.LogWarning($"[UI_MusicSelectChapterTabs] 수록된 챕터({chapters.Count})가 탭 버튼({_tabs.Count})보다 많아 뒷부분이 표시되지 않습니다.");
        }

        for (int i = 0; i < _tabs.Count; i++)
        {
            bool isUsed = i < _chapterCount;
            _tabs[i].SetVisible(isUsed);

            if (isUsed)
            {
                _tabs[i].SetTab(i, chapters[i].DisplayName, LiveChapterUnlockRule.IsUnlocked(chapters[i]));
            }
        }

        _selectedIndex = -1;
        RefreshArrows();
    }

    public void SetSelectedIndex(int selectedIndex)
    {
        _selectedIndex = selectedIndex;

        for (int i = 0; i < _chapterCount; i++)
        {
            _tabs[i].SetSelected(i == selectedIndex);
        }

        RefreshArrows();
    }

    public bool IsSelectable(int chapterIndex)
    {
        return chapterIndex >= 0 && chapterIndex < _chapterCount && _tabs[chapterIndex].IsUnlocked;
    }

    private void SelectPreviousChapter()
    {
        NotifyChapterSelected(_selectedIndex - 1);
    }

    private void SelectNextChapter()
    {
        NotifyChapterSelected(_selectedIndex + 1);
    }

    private void RefreshArrows()
    {
        _previousButton.interactable = IsSelectable(_selectedIndex - 1);
        _nextButton.interactable = IsSelectable(_selectedIndex + 1);
    }

    private void NotifyChapterSelected(int chapterIndex)
    {
        if (!IsSelectable(chapterIndex) || chapterIndex == _selectedIndex)
        {
            return;
        }

        OnChapterSelected?.Invoke(chapterIndex);
    }
}
