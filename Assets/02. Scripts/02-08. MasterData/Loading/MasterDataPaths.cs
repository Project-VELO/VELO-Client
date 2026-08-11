using System.IO;
using UnityEngine;

/// <summary>
/// 마스터 데이터 파일의 경로를 한곳에서 만들어 주는 정적 헬퍼입니다(LiveSongPaths와 같은 역할).
///
/// 마스터 데이터는 모든 플레이어에게 동일한 기획 데이터이므로 빌드에 포함되는 StreamingAssets에 둡니다.
/// 플레이어마다 달라지는 세이브 데이터는 이곳이 아니라 Application.persistentDataPath에 저장해야 합니다.
/// StreamingAssets는 빌드 산출물 안에 들어가 런타임 쓰기가 불가능합니다.
/// </summary>
public static class MasterDataPaths
{
    public const string CHARACTERS_FILE_NAME = "characters.json";
    public const string CARDS_FILE_NAME = "cards.json";
    public const string ITEMS_FILE_NAME = "items.json";
    public const string REWARDS_FILE_NAME = "rewards.json";
    public const string STORIES_FILE_NAME = "stories.json";
    public const string SCHEDULES_FILE_NAME = "schedules.json";
    public const string NEW_GAME_CONFIG_FILE_NAME = "newgame_config.json";

    private const string MASTER_DATA_FOLDER = "MasterData";
    private const string STORY_SCRIPTS_FOLDER = "StoryScripts";
    private const string STORY_BACKGROUNDS_FOLDER = "StoryBackgroundImage";
    private const string STORY_BACKGROUND_EXTENSION = ".png";

    public static string MasterDataRoot => Path.Combine(Application.streamingAssetsPath, MASTER_DATA_FOLDER);

    public static string StoryScriptsRoot => Path.Combine(MasterDataRoot, STORY_SCRIPTS_FOLDER);

    public static string StoryBackgroundsRoot => Path.Combine(MasterDataRoot, STORY_BACKGROUNDS_FOLDER);

    public static string GetTablePath(string fileName)
    {
        return Path.Combine(MasterDataRoot, fileName);
    }

    /// <summary>
    /// 회차별 대본 파일의 경로입니다. 파일명을 StoryId에서 유도하므로 StoryData에 경로 필드를 두지 않습니다.
    /// </summary>
    public static string GetStoryScriptPath(string storyId)
    {
        return Path.Combine(StoryScriptsRoot, $"{storyId}.json");
    }

    /// <summary>
    /// 감상 화면 배경 이미지의 경로입니다. 파일명이 곧 대본의 BackgroundId입니다("2-A.png").
    ///
    /// 배경을 스프라이트 에셋이 아니라 StreamingAssets에 두는 이유는 대본과 같습니다.
    /// 프리팹이 30여 장을 전부 참조하면 감상 화면을 열 때마다 쓰지도 않을 회차의 배경까지 함께 올라옵니다.
    /// </summary>
    public static string GetStoryBackgroundPath(string backgroundId)
    {
        return Path.Combine(StoryBackgroundsRoot, $"{backgroundId}{STORY_BACKGROUND_EXTENSION}");
    }
}
