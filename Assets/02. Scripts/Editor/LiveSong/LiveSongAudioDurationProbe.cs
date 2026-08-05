using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 음원 파일의 재생 길이를 재는 에디터 전용 도구입니다.
/// StreamingAssets의 파일은 임포트 대상이 아니라 AudioClip으로 바로 읽을 수 없으므로,
/// Assets 아래로 잠시 복사해 Unity 임포터에 길이 계산을 맡깁니다.
/// mp3의 VBR 헤더까지 직접 해석하는 것보다 안전하고, 수록 시 한 번만 수행하므로 비용도 문제되지 않습니다.
/// </summary>
public static class LiveSongAudioDurationProbe
{
    private const string TEMP_ASSET_PATH_WITHOUT_EXTENSION = "Assets/__LiveSongDurationProbe";

    public static bool TryMeasureSeconds(string audioFilePath, out float seconds)
    {
        seconds = 0f;

        if (!File.Exists(audioFilePath))
        {
            return false;
        }

        // 경로를 고정하면 같은 이름의 기존 에셋을 덮어쓴 뒤 finally에서 지워 버릴 수 있어, 비어 있는 경로를 새로 받습니다.
        string tempAssetPath = AssetDatabase.GenerateUniqueAssetPath(TEMP_ASSET_PATH_WITHOUT_EXTENSION + Path.GetExtension(audioFilePath));

        try
        {
            File.Copy(audioFilePath, tempAssetPath, true);
            AssetDatabase.ImportAsset(tempAssetPath, ImportAssetOptions.ForceSynchronousImport);

            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(tempAssetPath);
            if (clip == null)
            {
                Debug.LogWarning($"[LiveSongAudioDurationProbe] 음원을 AudioClip으로 읽지 못했습니다: {audioFilePath}");
                return false;
            }

            seconds = clip.length;
            return 0f < seconds;
        }
        finally
        {
            // 측정용 임시 에셋은 프로젝트에 남으면 안 되므로 실패 여부와 무관하게 지웁니다.
            AssetDatabase.DeleteAsset(tempAssetPath);
        }
    }
}
