using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 곡 선택 화면에서 고른 곡을 배경음 대신 들려줍니다.
///
/// 화면의 기본 배경음(ScreenBgm)을 잠시 밀어내고 그 자리에 곡을 틉니다. 준비 팝업을 열거나
/// 화면을 떠나면 원래 곡으로 되돌립니다. 고른 곡을 계속 듣고 싶은 것이 아니라,
/// 어떤 곡인지 확인하려는 것이기 때문입니다.
///
/// 음원은 수록 공간의 파일이라 임포트된 에셋이 아닙니다. 커버와 같은 방식으로 UnityWebRequest로 읽고,
/// 화면을 떠날 때 직접 파괴합니다. 씬에 매여 있지 않아 참조만 버리면 드나들 때마다 쌓입니다.
/// </summary>
public class UI_MusicSelectPreviewBgm : MonoBehaviour
{
    private AudioClip _clip;
    private string _playingSongId;

    // 음원은 비동기로 도착합니다. 도착 전에 다른 곡을 고르면 낡은 결과를 버려야 합니다.
    private int _requestGeneration;

    private void OnDisable()
    {
        Restore();
    }

    private void OnDestroy()
    {
        ReleaseClip();
    }

    /// <summary>
    /// 이 곡을 들려줍니다. 같은 곡을 다시 고른 경우에는 처음부터 되감지 않습니다.
    /// 음원이 없는 곡(아직 수록되지 않은 잠긴 곡)은 기본 배경음을 그대로 둡니다.
    /// </summary>
    public void PlaySong(SongData song)
    {
        if (ReferenceEquals(song, null) || string.IsNullOrEmpty(song.AudioFilePath))
        {
            Restore();
            return;
        }

        if (song.SongId == _playingSongId)
        {
            return;
        }

        _requestGeneration++;
        _playingSongId = song.SongId;

        string audioPath = LiveSongPaths.GetPublishedAudioPath(song.FolderPath, song.AudioFilePath);
        PlayAsync(audioPath, _requestGeneration, this.GetCancellationTokenOnDestroy()).Forget();
    }

    /// <summary>
    /// 미리듣기를 멈추고 화면의 기본 배경음으로 되돌립니다.
    /// </summary>
    public void Restore()
    {
        _requestGeneration++;
        _playingSongId = null;

        if (BgmManager.Instance != null)
        {
            BgmManager.Instance.RestoreScreenBgm();
        }
    }

    private async UniTaskVoid PlayAsync(string audioPath, int generation, CancellationToken cancellationToken)
    {
        string url = "file://" + audioPath;
        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);

        await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);

        if (generation != _requestGeneration)
        {
            return;
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[UI_MusicSelectPreviewBgm] 미리듣기 음원을 읽지 못했습니다: {request.error} ({audioPath})");
            return;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

        if (clip == null)
        {
            return;
        }

        // 앞 곡은 더 이상 쓰지 않습니다. 한 번에 한 곡만 들려주므로 캐시하지 않고 바로 버립니다.
        ReleaseClip();
        _clip = clip;

        if (BgmManager.Instance != null)
        {
            BgmManager.Instance.PlayPreview(clip);
        }
    }

    private void ReleaseClip()
    {
        if (_clip == null)
        {
            return;
        }

        Destroy(_clip);
        _clip = null;
    }
}
