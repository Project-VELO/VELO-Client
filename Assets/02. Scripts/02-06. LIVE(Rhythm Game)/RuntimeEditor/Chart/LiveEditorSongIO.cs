using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// StreamingAssets/Songs 아래의 곡 메타데이터(song_info.json) 입출력과 신규 음원 등록을 전담하는 클래스입니다.
/// 채보 파일 자체는 LiveEditorChartIO가 다룹니다.
/// </summary>
public class LiveEditorSongIO
{
    public string GetSongInfoPath(string songId)
    {
        return Path.Combine(Application.streamingAssetsPath, "Songs", songId, "song_info.json");
    }

    public void SaveSong(string path, SongData song)
    {
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonUtility.ToJson(song, true);
        File.WriteAllText(path, json);
    }

    public SongData LoadSong(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SongData>(json);
    }

    public List<string> GetAllSongIds()
    {
        var songIds = new List<string>();
        string songsRoot = Path.Combine(Application.streamingAssetsPath, "Songs");

        if (!Directory.Exists(songsRoot))
        {
            return songIds;
        }

        foreach (string directory in Directory.GetDirectories(songsRoot))
        {
            songIds.Add(Path.GetFileName(directory));
        }

        return songIds;
    }

    /// <summary>
    /// Songs 폴더 바로 아래에 놓인, 아직 곡으로 등록되지 않은 음원 파일 경로를 찾습니다.
    /// </summary>
    public List<string> GetUnregisteredAudioFilePaths()
    {
        var audioFilePaths = new List<string>();
        string songsRoot = Path.Combine(Application.streamingAssetsPath, "Songs");

        if (!Directory.Exists(songsRoot))
        {
            return audioFilePaths;
        }

        foreach (string filePath in Directory.GetFiles(songsRoot))
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (extension == ".mp3" || extension == ".wav")
            {
                audioFilePaths.Add(filePath);
            }
        }

        return audioFilePaths;
    }

    /// <summary>
    /// 음원 파일을 곡 전용 폴더로 옮기고 곡 메타데이터를 생성합니다.
    /// </summary>
    public void RegisterSong(string audioFilePath, string songId, string title, float bpm, string composer)
    {
        string songFolder = Path.Combine(Application.streamingAssetsPath, "Songs", songId);
        Directory.CreateDirectory(songFolder);

        string destinationAudioFileName = "audio" + Path.GetExtension(audioFilePath);
        string destinationAudioPath = Path.Combine(songFolder, destinationAudioFileName);
        File.Move(audioFilePath, destinationAudioPath);

        string orphanedMetaPath = audioFilePath + ".meta";
        if (File.Exists(orphanedMetaPath))
        {
            File.Delete(orphanedMetaPath);
        }

        var song = new SongData
        {
            SongId = songId,
            Title = title,
            Bpm = bpm,
            AudioFilePath = destinationAudioFileName,
            Composer = composer,
        };

        SaveSong(GetSongInfoPath(songId), song);
    }
}
