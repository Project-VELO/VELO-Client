using System;

/// <summary>
/// 플레이어 데이터를 씬 사이에서 공유하고, 세이브 파일과의 입출력을 중개하는 싱글톤입니다.
///
/// 최초 접근 시 세이브를 불러오고, 없거나 손상되었으면 신규 게임을 만들어 즉시 저장합니다.
/// 진행 상태를 바꾼 뒤의 저장은 GameProgressService가 호출하므로, 화면 코드가 저장을 신경 쓸 필요는 없습니다.
/// </summary>
public class PlayerDataProvider : POCOSingleton<PlayerDataProvider>
{
    private readonly SaveService _saveService = new SaveService();

    private PlayerData _data;

    public SaveService SaveService => _saveService;

    public PlayerData Data
    {
        get
        {
            if (ReferenceEquals(_data, null))
            {
                Load();
            }

            return _data;
        }
    }

    /// <summary>
    /// 세이브를 불러옵니다. 없거나 손상되어 복구에 실패하면 신규 게임을 만들고 곧바로 저장합니다(기획서 3-K-2, 3-K-4).
    /// </summary>
    public void Load()
    {
        MasterDataProvider.Instance.Build();

        _data = _saveService.Load();

        if (ReferenceEquals(_data, null))
        {
            InitData();
            return;
        }

        // 불러온 세이브에 진행 기록이 없는 스토리를 채웁니다.
        // 스키마 버전이 다른 세이브는 경고만 남기고 통과하고, 진행 데이터가 통째로 없으면 빈 상태로 복구되므로,
        // 이 보정이 없으면 해금 조건이 걸린 챕터가 전부 잠긴 채로 게임이 시작됩니다.
        if (StoryProgressService.SyncStoryProgresses(_data.Progress))
        {
            Save();
        }
    }

    /// <summary>
    /// 신규 게임 상태로 초기화하고 저장합니다. 설정을 넘기지 않으면 마스터 데이터의 newgame_config.json을 사용합니다.
    /// </summary>
    public void InitData(NewGameConfigData config = null)
    {
        MasterDataProvider.Instance.Build();

        _data = new PlayerData();
        _data.InitPlayerData(config ?? MasterDataProvider.Instance.NewGameConfig);

        StoryProgressService.SyncStoryProgresses(_data.Progress);

        Save();
    }

    /// <summary>
    /// Data 프로퍼티가 아니라 _data를 직접 넘깁니다.
    /// Data 게터는 값이 없으면 Load()를 부르고 Load()는 다시 이 메서드에 닿으므로,
    /// 프로퍼티를 쓰면 저장이 호출 순서에 따라 재진입할 수 있는 경로가 생깁니다.
    /// </summary>
    public bool Save()
    {
        return _saveService.Save(_data);
    }

    /// <summary>
    /// 세이브를 지우고 신규 게임으로 되돌립니다. 디버그 도구에서 사용합니다.
    /// </summary>
    public void ResetToNewGame()
    {
        _saveService.Delete();
        InitData();
    }

    /// <summary>
    /// 해당 곡·난이도의 최고 기록입니다. 플레이 기록이 없으면 null을 돌려주므로, 화면에서는 미플레이로 표시합니다.
    /// </summary>
    public SongRecord GetSongRecord(string songId, EDifficulty difficulty)
    {
        if (string.IsNullOrEmpty(songId))
        {
            return null;
        }

        string key = SongRecordKey.Create(songId, difficulty);
        return Data.Progress.SongRecords.TryGetValue(key, out SongRecord record) ? record : null;
    }

    /// <summary>
    /// 난이도를 가리지 않은 그 곡의 최고 랭크 기록입니다. 곡 목록의 행은 난이도 선택과 무관하게 표시되므로,
    /// 난이도를 바꿀 때마다 목록을 다시 그리지 않도록 곡 단위 대표값을 씁니다.
    /// ELiveRank는 좋은 랭크일수록 값이 작으므로 최솟값이 최고 기록입니다.
    /// </summary>
    public SongRecord GetBestSongRecord(string songId)
    {
        SongRecord bestRecord = null;

        foreach (EDifficulty difficulty in Enum.GetValues(typeof(EDifficulty)))
        {
            SongRecord record = GetSongRecord(songId, difficulty);
            if (ReferenceEquals(record, null))
            {
                continue;
            }

            if (ReferenceEquals(bestRecord, null) || record.BestRank < bestRecord.BestRank)
            {
                bestRecord = record;
            }
        }

        return bestRecord;
    }
}
