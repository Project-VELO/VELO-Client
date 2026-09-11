using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 수록 창이 쓰는, 에디터에서만 할 수 있는 뒷정리를 맡습니다.
/// 파일 복사 자체는 채보 에디터 씬과 함께 쓰는 LiveSongPublishWriter에 있습니다.
///
/// 곡 길이 측정과 에셋 데이터베이스 조작이 여기 남은 이유는 둘 다 임포터가 필요해서입니다.
/// 플레이 모드에서 도는 채보 에디터는 오디오 클립을 이미 로드해 두어 길이를 알고 있으므로 이 과정이 필요 없습니다.
/// </summary>
public class LiveSongPublisher
{
    private readonly LiveSongPublishWriter _writer = new LiveSongPublishWriter();
    private readonly LiveEditorSongIO _songIO = new LiveEditorSongIO();

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

    public bool Publish(string songId, string chapterFolder, List<EDifficulty> difficulties, out string error)
    {
        // 곡 길이를 먼저 확정해야 수록본에도 함께 반영됩니다.
        EnsureDuration(songId);

        if (!_writer.Publish(songId, chapterFolder, difficulties, out error))
        {
            return false;
        }

        AssetDatabase.Refresh();
        return true;
    }

    /// <summary>
    /// 곡 길이는 음원을 실제로 읽어야 알 수 있어 song_info.json에 비어 있는 경우가 많습니다.
    /// 채보 에디터로 곡을 한 번 열어야만 값이 생기는 상태에 기대지 않도록, 수록 시점에 직접 확정합니다.
    /// </summary>
    private void EnsureDuration(string songId)
    {
        SongData song = _songIO.LoadSong(_songIO.GetSongInfoPath(songId));
        if (ReferenceEquals(song, null))
        {
            return;
        }

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
        return _writer.GetPublishedSongIds(chapterFolder);
    }

    private string ToAssetPath(string absolutePath)
    {
        string normalized = absolutePath.Replace('\\', '/');
        string dataPath = Application.dataPath.Replace('\\', '/');

        return "Assets" + normalized.Substring(dataPath.Length);
    }
}
