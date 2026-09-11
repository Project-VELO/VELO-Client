using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 작업 공간(Songs/, Charts/)의 결과물을 수록 공간(LiveSongs/)의 챕터 폴더로 복사하는 것을 전담합니다.
///
/// 하는 일이 전부 파일 복사라 에디터 전용일 이유가 없어 런타임에 둡니다. 그래야 채보 에디터 씬에서도
/// 곡을 수록할 수 있고, 기획자가 채보를 만들다 말고 Unity 메뉴를 따로 열지 않아도 됩니다.
/// 에셋 데이터베이스 갱신이나 곡 길이 측정처럼 에디터에서만 되는 뒷정리는 LiveSongPublisher가 맡습니다.
/// </summary>
public class LiveSongPublishWriter
{
    private readonly LiveEditorSongIO _songIO = new LiveEditorSongIO();
    private readonly LiveEditorChartIO _chartIO = new LiveEditorChartIO();

    /// <summary>
    /// 곡 메타데이터·음원·선택한 난이도의 채보를 챕터 폴더로 복사합니다. 이미 수록되어 있으면 덮어씁니다.
    /// </summary>
    public bool Publish(string songId, string chapterFolder, List<EDifficulty> difficulties, out string error)
    {
        error = null;

        SongData song = _songIO.LoadSong(_songIO.GetSongInfoPath(songId));
        if (ReferenceEquals(song, null))
        {
            error = $"{LiveSongPaths.SONG_INFO_FILE_NAME}을 읽지 못했습니다: {songId}";
            return false;
        }

        if (difficulties.Count == 0)
        {
            error = $"수록할 난이도를 하나 이상 선택해야 합니다: {songId}";
            return false;
        }

        // 복사를 시작한 뒤에 실패하면 이전 수록본과 새 파일이 뒤섞이므로, 필요한 원본이 모두 있는지 먼저 확인합니다.
        if (!ValidateSources(song, difficulties, out error))
        {
            return false;
        }

        string songFolder = LiveSongPaths.GetPublishedSongFolder(chapterFolder, songId);
        Directory.CreateDirectory(songFolder);

        CopyAudio(song, songFolder);
        CopyCharts(songId, songFolder, difficulties);
        CopyCover(song, songFolder);
        WriteSongInfo(song, songFolder);

        return true;
    }

    /// <summary>
    /// 곡이 이미 수록되어 있다면 그 챕터 폴더를 돌려줍니다.
    /// 곡이 어느 챕터에 속하는지는 폴더 구조에만 남아 있어(SongData.ChapterId는 직렬화되지 않습니다) 훑어 찾습니다.
    /// </summary>
    public bool TryFindPublishedChapterFolder(string songId, out string chapterFolder)
    {
        chapterFolder = null;

        if (string.IsNullOrEmpty(songId))
        {
            return false;
        }

        foreach (string candidate in LiveSongPaths.GetPublishedChapterFolders())
        {
            if (Directory.Exists(LiveSongPaths.GetPublishedSongFolder(candidate, songId)))
            {
                chapterFolder = candidate;
                return true;
            }
        }

        return false;
    }

    public List<string> GetPublishedSongIds(string chapterFolder)
    {
        var songIds = new List<string>();

        foreach (string songFolder in LiveSongPaths.GetPublishedSongFolders(chapterFolder))
        {
            songIds.Add(Path.GetFileName(songFolder));
        }

        return songIds;
    }

    /// <summary>
    /// 수록에 필요한 원본이 모두 있는지 한 번에 확인합니다. 한 파일이라도 없으면 수록 폴더를 건드리지 않고 중단합니다.
    /// </summary>
    private bool ValidateSources(SongData song, List<EDifficulty> difficulties, out string error)
    {
        error = null;

        string sourceAudioPath = LiveSongPaths.GetWorkingAudioPath(song.SongId, song.AudioFilePath);
        if (!File.Exists(sourceAudioPath))
        {
            error = $"음원 파일을 찾지 못했습니다: {sourceAudioPath}";
            return false;
        }

        foreach (EDifficulty difficulty in difficulties)
        {
            string sourceChartPath = _chartIO.GetChartPath(song.SongId, difficulty);
            if (!File.Exists(sourceChartPath))
            {
                error = $"채보 파일을 찾지 못했습니다: {sourceChartPath}";
                return false;
            }
        }

        return true;
    }

    private void CopyAudio(SongData song, string songFolder)
    {
        string sourceAudioPath = LiveSongPaths.GetWorkingAudioPath(song.SongId, song.AudioFilePath);
        File.Copy(sourceAudioPath, LiveSongPaths.GetPublishedAudioPath(songFolder, song.AudioFilePath), true);
    }

    /// <summary>
    /// 커버가 없는 곡도 정상이므로, 원본이 없으면 수록을 막지 않고 건너뜁니다(SongCoverLoader와 같은 판단).
    /// 그래서 ValidateSources가 아니라 여기서 확인합니다.
    /// </summary>
    private void CopyCover(SongData song, string songFolder)
    {
        string fileName = string.IsNullOrEmpty(song.CoverImagePath) ? LiveSongPaths.COVER_FILE_NAME : song.CoverImagePath;
        string sourcePath = Path.Combine(LiveSongPaths.GetWorkingSongFolder(song.SongId), fileName);

        if (!File.Exists(sourcePath))
        {
            return;
        }

        File.Copy(sourcePath, Path.Combine(songFolder, fileName), true);
    }

    private void CopyCharts(string songId, string songFolder, List<EDifficulty> difficulties)
    {
        foreach (EDifficulty difficulty in difficulties)
        {
            string sourceChartPath = _chartIO.GetChartPath(songId, difficulty);
            File.Copy(sourceChartPath, LiveSongPaths.GetPublishedChartPath(songFolder, songId, difficulty), true);
        }
    }

    /// <summary>
    /// 표시 순서와 잠금은 수록본에만 손으로 적어 온 값이라(작업본은 늘 0/false) 이어받습니다.
    /// 파일을 통째로 복사하던 때에는 다시 수록할 때마다 이 둘이 0/false로 밀렸습니다.
    /// 실제로 11_59는 수록본이 순서 2인데 작업본은 0이라, 다시 수록하면 곡 목록 맨 앞으로 올라왔습니다.
    /// </summary>
    private void WriteSongInfo(SongData song, string songFolder)
    {
        string publishedPath = LiveSongPaths.GetPublishedSongInfoPath(songFolder);

        if (File.Exists(publishedPath))
        {
            SongData publishedSong = JsonUtility.FromJson<SongData>(File.ReadAllText(publishedPath));

            if (!ReferenceEquals(publishedSong, null))
            {
                song.Order = publishedSong.Order;
                song.IsLocked = publishedSong.IsLocked;
            }
        }

        File.WriteAllText(publishedPath, JsonUtility.ToJson(song, true));
    }
}
