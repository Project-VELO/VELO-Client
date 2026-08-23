using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 채보 작업 공간(Songs/, Charts/)의 결과물을 곡 선택 화면이 읽는 수록 공간(LiveSongs/)으로 옮기는 것을 전담합니다.
/// 수록 폴더는 음원까지 포함한 자기 완결적 묶음으로 만들어, 작업 중인 채보가 곡 선택 화면에 새어 나오지 않게 합니다.
/// </summary>
public class LiveSongPublisher
{
    private readonly LiveEditorSongIO _songIO = new LiveEditorSongIO();
    private readonly LiveEditorChartIO _chartIO = new LiveEditorChartIO();

    public string CreateChapterFolder(int order, string chapterId)
    {
        string chapterFolder = LiveSongPaths.GetPublishedChapterFolder(order, chapterId);
        Directory.CreateDirectory(chapterFolder);

        // 표시명을 손볼 자리를 미리 만들어 둡니다. 파일이 없어도 폴더명으로 동작하므로 덮어쓰지는 않습니다.
        string chapterInfoPath = LiveSongPaths.GetPublishedChapterInfoPath(chapterFolder);
        if (!File.Exists(chapterInfoPath))
        {
            var info = new ChapterInfo { DisplayName = chapterId, UnlockStoryId = string.Empty };
            File.WriteAllText(chapterInfoPath, JsonUtility.ToJson(info, true));
        }

        AssetDatabase.Refresh();
        return chapterFolder;
    }

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

        // 곡 정보를 복사하기 전에 확정해야 수록본에도 함께 반영됩니다.
        EnsureDuration(song);
        File.Copy(_songIO.GetSongInfoPath(songId), LiveSongPaths.GetPublishedSongInfoPath(songFolder), true);

        AssetDatabase.Refresh();
        return true;
    }

    /// <summary>
    /// 곡 길이는 음원을 실제로 읽어야 알 수 있어 song_info.json에 비어 있는 경우가 많습니다.
    /// 채보 에디터로 곡을 한 번 열어야만 값이 생기는 상태에 기대지 않도록, 수록 시점에 직접 확정합니다.
    /// </summary>
    private void EnsureDuration(SongData song)
    {
        string audioPath = LiveSongPaths.GetWorkingAudioPath(song.SongId, song.AudioFilePath);
        if (!LiveSongAudioDurationProbe.TryMeasureSeconds(audioPath, out float seconds))
        {
            return;
        }

        if (Mathf.Approximately(song.Duration, seconds))
        {
            return;
        }

        song.Duration = seconds;
        _songIO.SaveSong(_songIO.GetSongInfoPath(song.SongId), song);
    }

    /// <summary>
    /// 수록을 취소합니다. 폴더만 지우면 Unity가 만들어 둔 메타 파일이 남아 경고가 뜨므로 AssetDatabase로 지웁니다.
    /// </summary>
    public bool Unpublish(string chapterFolder, string songId)
    {
        string songFolder = LiveSongPaths.GetPublishedSongFolder(chapterFolder, songId);
        if (!Directory.Exists(songFolder))
        {
            return false;
        }

        return AssetDatabase.DeleteAsset(ToAssetPath(songFolder));
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

    private string ToAssetPath(string absolutePath)
    {
        string normalized = absolutePath.Replace('\\', '/');
        string dataPath = Application.dataPath.Replace('\\', '/');

        return "Assets" + normalized.Substring(dataPath.Length);
    }
}
