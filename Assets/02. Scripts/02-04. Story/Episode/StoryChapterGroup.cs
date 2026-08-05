using System.Collections.Generic;

/// <summary>
/// 같은 챕터에 속한 스토리를 회차 순서대로 묶은 덩어리입니다.
///
/// 스토리 목록 화면은 챕터 탭 없이 하나의 세로 목록으로 표시하므로(기획서 3-F-3-1),
/// 화면이 필요로 하는 것은 "챕터 하나 = 헤더 + 회차 목록"이라는 단위입니다.
/// 마스터 데이터에는 이 단위가 없고 스토리가 평평한 목록으로만 있어 화면 진입 때 한 번 묶습니다.
/// </summary>
public class StoryChapterGroup
{
    public string ChapterId { get; private set; }

    public int ChapterOrder { get; private set; }

    /// <summary>
    /// 챕터 헤더에 출력할 문자열입니다. 마스터 데이터에 표시명 필드가 없어 ChapterId에서 변환합니다.
    /// </summary>
    public string DisplayName { get; private set; }

    public List<StoryData> Stories { get; private set; }

    public StoryChapterGroup(StoryData firstStory)
    {
        ChapterId = firstStory.ChapterId;
        ChapterOrder = firstStory.ChapterOrder;
        DisplayName = StoryChapterDisplayName.Get(firstStory.ChapterId);
        Stories = new List<StoryData>();
    }

    /// <summary>
    /// 정렬된 목록을 챕터 경계에서 자르는 용도이므로, 여기서 다시 정렬하지 않습니다.
    /// 순서는 MasterDataQuery.GetAllStoriesInDisplayOrder가 이미 보장합니다.
    /// </summary>
    public void Add(StoryData story)
    {
        Stories.Add(story);
    }
}
