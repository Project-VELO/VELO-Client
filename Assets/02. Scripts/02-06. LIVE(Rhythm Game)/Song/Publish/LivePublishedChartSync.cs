using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 이미 수록된 채보를 작업본과 같은 내용으로 맞추는 것을 전담합니다.
///
/// 채보 에디터는 작업 공간(Charts/)에만 저장하는데 곡 선택 화면과 리듬게임은 수록 공간(LiveSongs/)만 읽으므로,
/// 수록 창(VELO/Live/곡 수록)을 사람이 열지 않으면 찍은 채보가 게임에 반영되지 않습니다.
/// 저장할 때마다 그 창을 여는 데 기대면 반드시 빠뜨리는 날이 오므로, 저장 시점에 여기서 따라붙입니다.
///
/// 이미 수록된 곡의, 이미 수록된 난이도만 갱신합니다. 수록되지 않은 곡과 새 난이도까지 따라가면
/// 작업 중인 채보가 곡 선택 화면에 새어 나가, 두 공간을 나눠 둔 이유가 사라집니다.
/// 이미 수록된 채보는 플레이어에게 이미 노출된 것이라 최신으로 맞추는 데에는 그런 문제가 없습니다.
///
/// 음원과 커버는 건드리지 않습니다. 채보만 바뀌었는데 곡마다 10MB에 가까운 음원을 매 저장마다 다시 쓰게 되고,
/// song_info.json을 통째로 덮으면 수록할 때 정한 표시 순서(_order)와 잠금 상태가 작업본 값으로 밀립니다.
/// 곡 전체를 새로 올리는 것은 지금처럼 수록 창의 몫입니다.
/// </summary>
public class LivePublishedChartSync
{
    private readonly LiveSongPublishWriter _publishWriter = new LiveSongPublishWriter();

    /// <summary>
    /// 저장된 작업본 채보를 수록본에도 반영합니다. 반영 대상이 아니면 아무것도 하지 않고 NotPublished로 답합니다.
    /// </summary>
    public ELivePublishSyncResult SyncChart(SongData song, EDifficulty difficulty, ChartData chart)
    {
        if (ReferenceEquals(song, null) || ReferenceEquals(chart, null))
        {
            return ELivePublishSyncResult.NotPublished;
        }

        if (!TryFindPublishedSongFolder(song.SongId, out string songFolder))
        {
            return ELivePublishSyncResult.NotPublished;
        }

        SongData publishedSong = LoadPublishedSong(songFolder);
        if (ReferenceEquals(publishedSong, null))
        {
            return ELivePublishSyncResult.NotPublished;
        }

        if (!publishedSong.Charts.TryGetValue(difficulty, out ChartMetadata metadata) || ReferenceEquals(metadata, null))
        {
            return ELivePublishSyncResult.NotPublished;
        }

        return WritePublishedChart(song.SongId, difficulty, chart, songFolder, publishedSong, metadata);
    }

    private bool TryFindPublishedSongFolder(string songId, out string songFolder)
    {
        songFolder = null;

        if (!_publishWriter.TryFindPublishedChapterFolder(songId, out string chapterFolder))
        {
            return false;
        }

        songFolder = LiveSongPaths.GetPublishedSongFolder(chapterFolder, songId);
        return true;
    }

    private SongData LoadPublishedSong(string songFolder)
    {
        string songInfoPath = LiveSongPaths.GetPublishedSongInfoPath(songFolder);
        if (!File.Exists(songInfoPath))
        {
            return null;
        }

        return JsonUtility.FromJson<SongData>(File.ReadAllText(songInfoPath));
    }

    /// <summary>
    /// 채보 파일을 복사하고 수록본의 노트 수를 함께 맞춥니다.
    ///
    /// AssetDatabase를 새로 고치지 않습니다. 게임은 이 파일들을 임포트된 에셋이 아니라 디스크에서 직접 읽고,
    /// 덮어쓰는 대상은 이미 메타 파일을 가진 파일이라 새로 고칠 이유가 없습니다.
    /// 채보 에디터는 플레이 모드에서 도는데 그 도중의 새로 고침은 임포트를 유발해 편집을 끊습니다.
    ///
    /// 저장 자체는 이미 끝난 뒤이므로, 갱신이 실패해도 편집 내용을 잃게 해서는 안 됩니다. 예외를 삼키고 알리기만 합니다.
    /// </summary>
    private ELivePublishSyncResult WritePublishedChart(string songId, EDifficulty difficulty, ChartData chart,
        string songFolder, SongData publishedSong, ChartMetadata metadata)
    {
        string workingChartPath = LiveSongPaths.GetWorkingChartPath(songId, difficulty);
        string publishedChartPath = LiveSongPaths.GetPublishedChartPath(songFolder, songId, difficulty);

        try
        {
            File.Copy(workingChartPath, publishedChartPath, true);

            // 난이도 레벨은 기획자가 직접 적는 값이라 두고, 채보에서 다시 세어야 하는 값만 맞춥니다.
            metadata.ChartFilePath = LiveSongPaths.GetChartFileName(songId, difficulty);
            metadata.TotalNoteCount = chart.Notes.Count;

            File.WriteAllText(LiveSongPaths.GetPublishedSongInfoPath(songFolder), JsonUtility.ToJson(publishedSong, true));
        }
        catch (Exception exception)
        {
            Debug.LogError($"[LivePublishedChartSync] 수록본 갱신에 실패했습니다: {publishedChartPath}\n{exception}");
            return ELivePublishSyncResult.Failed;
        }

        return ELivePublishSyncResult.Updated;
    }
}
