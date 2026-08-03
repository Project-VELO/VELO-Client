using System;

/// <summary>
/// 플레이어 데이터를 씬 사이에서 공유하기 위한 싱글톤입니다.
/// 아직 세이브 계층이 없어 디스크 입출력은 하지 않고, 최초 접근 시 신규 게임 상태로 초기화한 값만 들고 있습니다.
/// 저장·로드가 붙는 시점에는 이 클래스만 교체하면 되도록 조회 경로를 이곳으로 모았습니다.
/// </summary>
public class PlayerDataProvider : POCOSingleton<PlayerDataProvider>
{
    private PlayerData _data;

    public PlayerData Data
    {
        get
        {
            if (ReferenceEquals(_data, null))
            {
                InitData();
            }

            return _data;
        }
    }

    /// <summary>
    /// 신규 게임 상태로 초기화합니다. 설정을 넘기지 않으면 마스터 데이터의 newgame_config.json을 사용합니다.
    /// </summary>
    public void InitData(NewGameConfigData config = null)
    {
        MasterDataProvider.Instance.Build();

        _data = new PlayerData();
        _data.InitPlayerData(config ?? MasterDataProvider.Instance.NewGameConfig);
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
        return Data.SongRecords.TryGetValue(key, out SongRecord record) ? record : null;
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
