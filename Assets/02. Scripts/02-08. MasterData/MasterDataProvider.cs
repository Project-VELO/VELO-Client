using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 마스터 데이터를 보관하고 단건 조회를 제공하는 싱글톤입니다(LiveSongCatalog와 같은 역할).
/// 씬을 넘나들며 다시 읽지 않아도 되도록 POCO 싱글톤으로 둡니다.
///
/// 조건에 따른 목록 질의는 MasterDataQuery가 담당합니다.
/// </summary>
public class MasterDataProvider : POCOSingleton<MasterDataProvider>
{
    private readonly MasterDataLoader _loader = new MasterDataLoader();

    private readonly Dictionary<string, CharacterData> _characters = new Dictionary<string, CharacterData>();
    private readonly Dictionary<string, CardData> _cards = new Dictionary<string, CardData>();
    private readonly Dictionary<string, ItemData> _items = new Dictionary<string, ItemData>();
    private readonly Dictionary<string, RewardData> _rewards = new Dictionary<string, RewardData>();
    private readonly Dictionary<string, StoryData> _stories = new Dictionary<string, StoryData>();
    private readonly Dictionary<string, ScheduleData> _schedules = new Dictionary<string, ScheduleData>();
    private readonly Dictionary<string, StoryEffectData> _effects = new Dictionary<string, StoryEffectData>();

    public IReadOnlyDictionary<string, CharacterData> Characters => _characters;
    public IReadOnlyDictionary<string, CardData> Cards => _cards;
    public IReadOnlyDictionary<string, ItemData> Items => _items;
    public IReadOnlyDictionary<string, RewardData> Rewards => _rewards;
    public IReadOnlyDictionary<string, StoryData> Stories => _stories;
    public IReadOnlyDictionary<string, ScheduleData> Schedules => _schedules;
    public IReadOnlyDictionary<string, StoryEffectData> Effects => _effects;

    public NewGameConfigData NewGameConfig { get; private set; }

    public bool IsBuilt { get; private set; }

    /// <summary>
    /// 마스터 데이터를 읽습니다. 이미 읽어 두었으면 다시 읽지 않습니다.
    /// </summary>
    public void Build()
    {
        if (IsBuilt)
        {
            return;
        }

        Rebuild();
    }

    /// <summary>
    /// JSON을 수정한 뒤 등 내용이 바뀐 경우 강제로 다시 읽습니다.
    /// </summary>
    public void Rebuild()
    {
        SetIndex(_characters, _loader.LoadTable<CharacterData>(MasterDataPaths.CHARACTERS_FILE_NAME), item => item.CharacterId, "CharacterId");
        SetIndex(_cards, _loader.LoadTable<CardData>(MasterDataPaths.CARDS_FILE_NAME), item => item.CardId, "CardId");
        SetIndex(_items, _loader.LoadTable<ItemData>(MasterDataPaths.ITEMS_FILE_NAME), item => item.ItemId, "ItemId");
        SetIndex(_rewards, _loader.LoadTable<RewardData>(MasterDataPaths.REWARDS_FILE_NAME), item => item.RewardId, "RewardId");
        SetIndex(_stories, _loader.LoadTable<StoryData>(MasterDataPaths.STORIES_FILE_NAME), item => item.StoryId, "StoryId");
        SetIndex(_schedules, _loader.LoadTable<ScheduleData>(MasterDataPaths.SCHEDULES_FILE_NAME), item => item.ScheduleId, "ScheduleId");
        SetIndex(_effects, _loader.LoadTable<StoryEffectData>(MasterDataPaths.EFFECTS_FILE_NAME), item => item.EffectId, "EffectId");

        NewGameConfig = _loader.LoadNewGameConfig();

        IsBuilt = true;
    }

    public bool TryGetCharacter(string characterId, out CharacterData character)
    {
        return TryGet(_characters, characterId, out character);
    }

    public bool TryGetCard(string cardId, out CardData card)
    {
        return TryGet(_cards, cardId, out card);
    }

    public bool TryGetItem(string itemId, out ItemData item)
    {
        return TryGet(_items, itemId, out item);
    }

    public bool TryGetReward(string rewardId, out RewardData reward)
    {
        return TryGet(_rewards, rewardId, out reward);
    }

    public bool TryGetStory(string storyId, out StoryData story)
    {
        return TryGet(_stories, storyId, out story);
    }

    public bool TryGetSchedule(string scheduleId, out ScheduleData schedule)
    {
        return TryGet(_schedules, scheduleId, out schedule);
    }

    public bool TryGetEffect(string effectId, out StoryEffectData effect)
    {
        return TryGet(_effects, effectId, out effect);
    }

    /// <summary>
    /// 캐릭터의 표시명입니다. 등록되지 않은 화자여도 대사를 못 읽게 만들 이유는 없으므로 ID를 그대로 돌려줍니다.
    /// </summary>
    public string GetCharacterDisplayName(string characterId)
    {
        return TryGetCharacter(characterId, out CharacterData character) ? character.DisplayName : characterId;
    }

    private static bool TryGet<T>(Dictionary<string, T> source, string key, out T value)
    {
        value = default;

        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        return source.TryGetValue(key, out value);
    }

    /// <summary>
    /// 목록을 ID로 찾을 수 있게 펼칩니다.
    /// ID가 비었거나 중복되면 조회가 조용히 빗나가므로, 덮어쓰기 전에 반드시 알립니다.
    /// </summary>
    private static void SetIndex<T>(Dictionary<string, T> target, List<T> items, Func<T, string> keySelector, string keyName)
    {
        target.Clear();

        foreach (T item in items)
        {
            string key = keySelector(item);

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"[MasterDataProvider] {keyName}가 비어 있는 항목을 건너뜁니다: {typeof(T).Name}");
                continue;
            }

            if (target.ContainsKey(key))
            {
                Debug.LogWarning($"[MasterDataProvider] {keyName}가 중복되어 나중 항목이 앞선 항목을 덮어씁니다: {key}");
            }

            target[key] = item;
        }
    }
}
