using System.Collections.Generic;

/// <summary>
/// 마스터 데이터에서 조건에 맞는 목록을 뽑아냅니다.
/// 단건 조회(ID → 데이터)는 MasterDataProvider가, 조건 질의는 이 클래스가 맡습니다.
///
/// 반환 목록은 호출할 때마다 새로 만듭니다. 화면 진입처럼 드물게 호출되는 지점에서만 사용하고,
/// 매 프레임 도는 코드에서는 결과를 캐싱해 두고 쓰십시오.
/// </summary>
public static class MasterDataQuery
{
    /// <summary>
    /// 해당 날짜의 일일 스케줄을 진행 순서대로 돌려줍니다(기획서 4.2, 9.2의 "일일 스케줄 3개").
    /// </summary>
    public static List<ScheduleData> GetSchedulesByDay(string weekId, string dayId)
    {
        var schedules = new List<ScheduleData>();

        if (string.IsNullOrEmpty(weekId) || string.IsNullOrEmpty(dayId))
        {
            return schedules;
        }

        foreach (KeyValuePair<string, ScheduleData> pair in MasterDataProvider.Instance.Schedules)
        {
            ScheduleData schedule = pair.Value;
            if (schedule.WeekId == weekId && schedule.DayId == dayId)
            {
                schedules.Add(schedule);
            }
        }

        schedules.Sort(CompareScheduleOrder);
        return schedules;
    }

    /// <summary>
    /// 해당 주차의 날짜 ID를 진행 순서대로 돌려줍니다. 주간 스케줄 표의 7칸을 만드는 데 사용합니다.
    /// </summary>
    public static List<string> GetDayIdsByWeek(string weekId)
    {
        var dayIds = new List<string>();
        var dayOrders = new Dictionary<string, int>();

        if (string.IsNullOrEmpty(weekId))
        {
            return dayIds;
        }

        foreach (KeyValuePair<string, ScheduleData> pair in MasterDataProvider.Instance.Schedules)
        {
            ScheduleData schedule = pair.Value;
            if (schedule.WeekId != weekId || dayOrders.ContainsKey(schedule.DayId))
            {
                continue;
            }

            dayOrders[schedule.DayId] = schedule.DayOrder;
            dayIds.Add(schedule.DayId);
        }

        dayIds.Sort((left, right) => dayOrders[left].CompareTo(dayOrders[right]));
        return dayIds;
    }

    /// <summary>
    /// 해당 챕터의 스토리를 회차 순서대로 돌려줍니다.
    /// </summary>
    public static List<StoryData> GetStoriesByChapter(string chapterId)
    {
        var stories = new List<StoryData>();

        if (string.IsNullOrEmpty(chapterId))
        {
            return stories;
        }

        foreach (KeyValuePair<string, StoryData> pair in MasterDataProvider.Instance.Stories)
        {
            if (pair.Value.ChapterId == chapterId)
            {
                stories.Add(pair.Value);
            }
        }

        stories.Sort(CompareEpisodeNumber);
        return stories;
    }

    /// <summary>
    /// 모든 스토리를 챕터 순서 → 회차 순서로 정렬해 돌려줍니다.
    /// 스토리 목록 화면은 챕터 탭 없이 하나의 세로 목록으로 표시하므로(기획서 3-F-3-1) 이 순서를 그대로 씁니다.
    /// </summary>
    public static List<StoryData> GetAllStoriesInDisplayOrder()
    {
        var stories = new List<StoryData>();

        foreach (KeyValuePair<string, StoryData> pair in MasterDataProvider.Instance.Stories)
        {
            stories.Add(pair.Value);
        }

        stories.Sort(CompareChapterThenEpisode);
        return stories;
    }

    /// <summary>
    /// 편성 대상 멤버를 슬롯 순서대로 돌려줍니다(기획서 3-H-2).
    /// </summary>
    public static List<CharacterData> GetMembersInSlotOrder()
    {
        var members = new List<CharacterData>();

        foreach (KeyValuePair<string, CharacterData> pair in MasterDataProvider.Instance.Characters)
        {
            if (pair.Value.IsMember)
            {
                members.Add(pair.Value);
            }
        }

        members.Sort((left, right) => left.MemberOrder.CompareTo(right.MemberOrder));
        return members;
    }

    /// <summary>
    /// 특정 멤버의 보유 가능 카드를 돌려줍니다. 각 멤버 슬롯에는 해당 멤버의 카드만 배치할 수 있습니다.
    /// </summary>
    public static List<CardData> GetCardsByCharacter(string characterId)
    {
        var cards = new List<CardData>();

        if (string.IsNullOrEmpty(characterId))
        {
            return cards;
        }

        foreach (KeyValuePair<string, CardData> pair in MasterDataProvider.Instance.Cards)
        {
            if (pair.Value.CharacterId == characterId)
            {
                cards.Add(pair.Value);
            }
        }

        return cards;
    }

    /// <summary>
    /// 의상 또는 악세사리 목록을 돌려줍니다.
    /// </summary>
    public static List<ItemData> GetItemsByType(EItemType itemType)
    {
        var items = new List<ItemData>();

        foreach (KeyValuePair<string, ItemData> pair in MasterDataProvider.Instance.Items)
        {
            if (pair.Value.ItemType == itemType)
            {
                items.Add(pair.Value);
            }
        }

        return items;
    }

    private static int CompareScheduleOrder(ScheduleData left, ScheduleData right)
    {
        return left.ScheduleOrder.CompareTo(right.ScheduleOrder);
    }

    private static int CompareEpisodeNumber(StoryData left, StoryData right)
    {
        return left.EpisodeNumber.CompareTo(right.EpisodeNumber);
    }

    private static int CompareChapterThenEpisode(StoryData left, StoryData right)
    {
        int chapterComparison = left.ChapterOrder.CompareTo(right.ChapterOrder);
        return chapterComparison != 0 ? chapterComparison : left.EpisodeNumber.CompareTo(right.EpisodeNumber);
    }
}
