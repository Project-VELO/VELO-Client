using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 스토리 목록의 본체입니다. 챕터 섹션을 풀에서 꺼내 채우고 되돌리는 일을 전담합니다.
///
/// 화면 클래스에서 떼어낸 이유는 목록 구성과 화면 진행이 서로 다른 일이기 때문입니다.
/// 화면은 "어느 회차를 눌렀을 때 무엇을 하는가"를, 이 클래스는 "무엇을 어떤 순서로 그리는가"를 맡습니다.
/// </summary>
public class UI_SelectStoryList : MonoBehaviour
{
    public Action<string> OnEpisodeClicked;

    /// <summary>
    /// 챕터 섹션이 들어갈 자리입니다.
    /// </summary>
    [Foldout("Hierarchy")]
    [SerializeField]
    private RectTransform _sectionRoot;

    private readonly List<UI_SelectStoryChapterSection> _sections = new List<UI_SelectStoryChapterSection>();

    public void Build()
    {
        Release();

        foreach (StoryChapterGroup group in BuildGroups())
        {
            UI_SelectStoryChapterSection section = AcquireSection();
            if (section == null)
            {
                break;
            }

            section.SetChapter(group);
            section.OnEpisodeClicked = NotifyEpisodeClicked;
        }
    }

    /// <summary>
    /// 정렬된 스토리 목록을 챕터 경계에서 자릅니다.
    /// 정렬은 GetAllStoriesInDisplayOrder가 챕터 순서 → 회차 순서로 이미 보장합니다.
    /// </summary>
    private List<StoryChapterGroup> BuildGroups()
    {
        var groups = new List<StoryChapterGroup>();
        StoryChapterGroup current = null;

        foreach (StoryData story in MasterDataQuery.GetAllStoriesInDisplayOrder())
        {
            if (current == null || current.ChapterId != story.ChapterId)
            {
                current = new StoryChapterGroup(story);
                groups.Add(current);
            }

            current.Add(story);
        }

        return groups;
    }

    private UI_SelectStoryChapterSection AcquireSection()
    {
        GameObject go = PoolManager.Instance.Pop(EPoolable.SelectStoryChapterSection);
        if (go == null)
        {
            return null;
        }

        var section = go.GetComponent<UI_SelectStoryChapterSection>();
        section.transform.SetParent(_sectionRoot, false);
        section.transform.SetAsLastSibling();
        _sections.Add(section);

        return section;
    }

    /// <summary>
    /// 섹션보다 카드를 먼저 반환합니다. 순서가 바뀌면 카드가 풀 밖에 남습니다.
    /// </summary>
    public void Release()
    {
        foreach (UI_SelectStoryChapterSection section in _sections)
        {
            section.OnEpisodeClicked = null;
            section.ReleaseItems();
            PoolManager.Instance.Push(EPoolable.SelectStoryChapterSection, section.gameObject);
        }

        _sections.Clear();
    }

    /// <summary>
    /// 목록 전체에서 하나만 선택됩니다. 다른 챕터의 카드를 눌러도 이전 선택이 풀립니다.
    /// </summary>
    public void SetSelection(string storyId)
    {
        foreach (UI_SelectStoryChapterSection section in _sections)
        {
            foreach (UI_SelectStoryEpisodeItem item in section.Items)
            {
                item.SetSelected(item.StoryId == storyId);
            }
        }
    }

    public UI_SelectStoryEpisodeItem Find(string storyId)
    {
        if (string.IsNullOrEmpty(storyId))
        {
            return null;
        }

        foreach (UI_SelectStoryChapterSection section in _sections)
        {
            foreach (UI_SelectStoryEpisodeItem item in section.Items)
            {
                if (item.StoryId == storyId)
                {
                    return item;
                }
            }
        }

        return null;
    }

    private void NotifyEpisodeClicked(string storyId)
    {
        OnEpisodeClicked?.Invoke(storyId);
    }
}
