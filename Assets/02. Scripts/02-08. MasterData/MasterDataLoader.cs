using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 마스터 데이터 JSON 파일을 읽어 목록으로 돌려주는 것을 전담합니다(LiveSongCatalogLoader와 같은 역할).
/// 디스크 접근과 파싱만 담당하고, 읽어온 결과의 보관과 조회는 MasterDataProvider가 맡습니다.
///
/// 디스크 접근을 이 클래스와 StoryScriptLoader에만 두는 이유가 있습니다.
/// StreamingAssets를 File API로 읽는 방식은 Android에서 동작하지 않으므로(APK 내부 경로라 UnityWebRequest가 필요합니다),
/// 모바일로 확장할 때 교체 지점이 이 두 파일로만 모이도록 격리합니다.
/// 1차 MVP는 키보드 6키와 마우스 휠을 쓰는 PC 게임이라 지금은 File API로 충분합니다.
/// </summary>
public class MasterDataLoader
{
    /// <summary>
    /// 테이블 파일 하나를 읽습니다. 파일이 없거나 파싱에 실패하면 빈 목록을 돌려주고 경고만 남깁니다.
    /// 데이터 한 종류 때문에 게임 전체가 뜨지 않는 상황을 만들지 않기 위해서입니다.
    /// </summary>
    public List<T> LoadTable<T>(string fileName)
    {
        string path = MasterDataPaths.GetTablePath(fileName);

        if (!TryReadText(path, out string json))
        {
            return new List<T>();
        }

        MasterDataTable<T> table = JsonUtility.FromJson<MasterDataTable<T>>(json);

        if (ReferenceEquals(table, null) || ReferenceEquals(table.Items, null))
        {
            Debug.LogWarning($"[MasterDataLoader] 테이블 파싱에 실패했습니다: {path}");
            return new List<T>();
        }

        if (table.SchemaVersion != MasterDataSchema.CURRENT_VERSION)
        {
            Debug.LogWarning($"[MasterDataLoader] 스키마 버전이 다릅니다(파일 {table.SchemaVersion} / 코드 {MasterDataSchema.CURRENT_VERSION}): {path}");
        }

        return table.Items;
    }

    /// <summary>
    /// 신규 게임 설정을 읽습니다. 항목이 하나뿐이라 테이블로 감싸지 않고 객체 자체가 파일의 최상위입니다.
    /// </summary>
    public NewGameConfigData LoadNewGameConfig()
    {
        string path = MasterDataPaths.GetTablePath(MasterDataPaths.NEW_GAME_CONFIG_FILE_NAME);

        if (!TryReadText(path, out string json))
        {
            return null;
        }

        NewGameConfigData config = JsonUtility.FromJson<NewGameConfigData>(json);

        if (ReferenceEquals(config, null))
        {
            Debug.LogWarning($"[MasterDataLoader] 신규 게임 설정 파싱에 실패했습니다: {path}");
        }

        return config;
    }

    private bool TryReadText(string path, out string json)
    {
        json = null;

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[MasterDataLoader] 파일이 없습니다: {path}");
            return false;
        }

        json = File.ReadAllText(path);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"[MasterDataLoader] 파일이 비어 있습니다: {path}");
            return false;
        }

        return true;
    }
}
