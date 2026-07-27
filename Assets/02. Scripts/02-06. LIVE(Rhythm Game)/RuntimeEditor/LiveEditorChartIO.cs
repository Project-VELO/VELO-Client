using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// StreamingAssets 경로의 채보(ChartData) 및 곡 메타데이터(SongData) JSON 입출력을 전담하는 클래스입니다.
/// 저장 직전 LiveEditorChartValidator를 통해 유효성 검사를 수행하며, 검사에 실패하면 저장하지 않습니다.
/// </summary>
public class LiveEditorChartIO
{
    private readonly LiveEditorChartValidator _validator = new LiveEditorChartValidator();

    public bool SaveChart(string path, ChartData chart, SongData song, out List<string> errors)
    {
        errors = _validator.Validate(chart, song);
        if (errors.Count > 0)
        {
            return false;
        }

        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonUtility.ToJson(chart, true);
        File.WriteAllText(path, json);
        return true;
    }

    public ChartData LoadChart(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<ChartData>(json);
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

    public string GetChartPath(string songId, EDifficulty difficulty)
    {
        return Path.Combine(Application.streamingAssetsPath, "Charts", $"{songId}_{difficulty}.json");
    }

    public string GetSongInfoPath(string songId)
    {
        return Path.Combine(Application.streamingAssetsPath, "Songs", songId, "song_info.json");
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


    public List<string> GetUnregisteredAudioFilePaths()
    {
        var result = new List<string>();
        string songsRoot = Path.Combine(Application.streamingAssetsPath, "Songs");

        if (!Directory.Exists(songsRoot))
        {
            return result;
        }

        foreach (string filePath in Directory.GetFiles(songsRoot))
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (extension == ".mp3" || extension == ".wav")
            {
                result.Add(filePath);
            }
        }

        return result;
    }

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
