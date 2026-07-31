/// <summary>
/// 곡·난이도별 최고 기록을 다룹니다(기획서 3-J-2).
/// FAILED 결과는 최고 기록에 반영하지 않으며, 동점은 갱신으로 보지 않습니다(SongRecord.TryUpdateRecord).
/// </summary>
public static class LiveRecordService
{
    /// <summary>
    /// 이번 결과를 반영하기 "전" 기준으로, 그 곡을 아직 한 번도 클리어하지 않았는지 여부입니다.
    /// 보상 비율이 최초 CLEAR인지에 따라 갈리므로(3-J-3) 기록을 갱신하기 전에 확인해야 합니다.
    /// </summary>
    public static bool IsFirstClear(string songId)
    {
        SongRecord record = PlayerDataProvider.Instance.GetBestSongRecord(songId);

        return ReferenceEquals(record, null) || record.BestRank == ELiveRank.FAILED;
    }

    /// <summary>
    /// 최고 기록 갱신을 시도하고, 갱신되었으면 true를 돌려줍니다. 결과 화면의 NEW BEST 표시에 사용합니다.
    /// </summary>
    public static bool TryUpdateRecord(LiveResultData result)
    {
        if (!result.IsClear)
        {
            return false;
        }

        PlayerData data = PlayerDataProvider.Instance.Data;
        string key = SongRecordKey.Create(result.SongId, result.Difficulty);

        if (!data.SongRecords.TryGetValue(key, out SongRecord record))
        {
            record = new SongRecord();
            data.SongRecords[key] = record;
        }

        return record.TryUpdateRecord(result.Score, result.Rank, result.IsFullCombo);
    }
}
