using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 스토리 목록의 챕터 한 덩어리입니다. 챕터 헤더와 그 아래 회차 카드를 담습니다
/// (기획서 5.2 "챕터 구분 영역", 3-F-3-1).
///
/// 진행 상태 조회는 카드가 아니라 여기서 합니다. 카드는 자기 표시만 책임지고,
/// 어느 데이터로 채울지는 목록을 만드는 쪽이 정합니다.
/// </summary>
public class UI_SelectStoryChapterSection : MonoBehaviour
{
    public Action<string> OnEpisodeClicked;

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _chapterNameText;

    /// <summary>
    /// 회차 카드가 들어갈 자리입니다.
    /// </summary>
    [SerializeField]
    private RectTransform _episodeRoot;

    private readonly List<UI_SelectStoryEpisodeItem> _items = new List<UI_SelectStoryEpisodeItem>();

    public IReadOnlyList<UI_SelectStoryEpisodeItem> Items => _items;

    public void SetChapter(StoryChapterGroup group)
    {
        ReleaseItems();

        _chapterNameText.text = group.DisplayName;

        for (int i = 0; i < group.Stories.Count; i++)
        {
            UI_SelectStoryEpisodeItem item = AcquireItem();
            if (item == null)
            {
                break;
            }

            StoryData story = group.Stories[i];
            item.SetItem(story, GetProgress(story.StoryId));
            item.OnItemClicked = NotifyEpisodeClicked;
        }
    }

    /// <summary>
    /// 기록이 없는 것은 정상 상태가 아니라 데이터 오류입니다. StoryProgressService.SyncStoryProgresses가
    /// 신규 게임과 세이브 로드 양쪽에서 모든 스토리의 기록을 채우기 때문입니다.
    /// 조용히 넘어가면 잠긴 것처럼 보이는 이유를 찾기 어려우므로 로그를 남깁니다.
    /// </summary>
    private IStoryProgress GetProgress(string storyId)
    {
        IStoryProgress progress = GameProgressService.Instance.GetStoryProgress(storyId);

        if (ReferenceEquals(progress, null))
        {
            Debug.LogWarning($"[UI_SelectStoryChapterSection] 진행 기록이 없는 스토리입니다. 잠금으로 표시합니다: {storyId}");
        }

        return progress;
    }

    private UI_SelectStoryEpisodeItem AcquireItem()
    {
        GameObject go = PoolManager.Instance.Pop(EPoolable.SelectStoryEpisodeItem);
        if (go == null)
        {
            return null;
        }

        var item = go.GetComponent<UI_SelectStoryEpisodeItem>();
        item.transform.SetParent(_episodeRoot, false);
        item.transform.SetAsLastSibling();
        _items.Add(item);

        return item;
    }

    /// <summary>
    /// 섹션 자신이 풀로 돌아가기 전에 반드시 먼저 호출해야 합니다.
    /// 카드를 남긴 채 섹션만 반환하면 카드는 풀이 모르는 곳에 매달린 채 남습니다.
    /// </summary>
    public void ReleaseItems()
    {
        foreach (UI_SelectStoryEpisodeItem item in _items)
        {
            item.ResetItem();
            PoolManager.Instance.Push(EPoolable.SelectStoryEpisodeItem, item.gameObject);
        }

        _items.Clear();
    }

    private void NotifyEpisodeClicked(string storyId)
    {
        OnEpisodeClicked?.Invoke(storyId);
    }
}
