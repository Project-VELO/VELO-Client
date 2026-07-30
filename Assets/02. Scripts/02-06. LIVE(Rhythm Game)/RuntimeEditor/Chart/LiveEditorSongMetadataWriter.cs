using UnityEngine;

/// <summary>
/// 채보를 편집·저장하는 과정에서 알게 된 곡 메타데이터를 song_info.json에 되돌려 적는 것을 전담합니다.
/// 곡 길이와 노트 수는 편집 중에만 알 수 있는 값인데, 곡 선택 화면은 song_info.json만 읽으므로
/// 여기서 남겨 두지 않으면 화면에 표시할 수 없습니다.
/// </summary>
public class LiveEditorSongMetadataWriter
{
    private readonly LiveEditorSongIO _songIO;

    public LiveEditorSongMetadataWriter(LiveEditorSongIO songIO)
    {
        _songIO = songIO;
    }

    /// <summary>
    /// 오디오 클립 로드로 처음 알게 된 곡 길이를 파일에 반영합니다. 이미 같은 값이면 불필요한 쓰기를 건너뜁니다.
    /// </summary>
    public void WriteDuration(SongData song, float durationSeconds)
    {
        if (ReferenceEquals(song, null) || Mathf.Approximately(song.Duration, durationSeconds))
        {
            return;
        }

        song.Duration = durationSeconds;
        _songIO.SaveSong(_songIO.GetSongInfoPath(song.SongId), song);
    }

    /// <summary>
    /// 채보 저장 직후 해당 난이도의 메타데이터를 갱신합니다.
    /// 난이도 레벨은 기획자가 직접 적는 값이므로 기존 값을 보존합니다.
    /// </summary>
    public void WriteChartMetadata(SongData song, EDifficulty difficulty, ChartData chart)
    {
        if (ReferenceEquals(song, null) || ReferenceEquals(chart, null))
        {
            return;
        }

        if (!song.Charts.TryGetValue(difficulty, out ChartMetadata metadata) || ReferenceEquals(metadata, null))
        {
            metadata = new ChartMetadata();
            song.Charts[difficulty] = metadata;
        }

        // 절대 경로를 남기면 다른 작업자의 PC에서 무의미해지므로 파일명만 기록합니다.
        metadata.ChartFilePath = LiveSongPaths.GetChartFileName(song.SongId, difficulty);
        metadata.TotalNoteCount = chart.Notes.Count;

        _songIO.SaveSong(_songIO.GetSongInfoPath(song.SongId), song);
    }
}
