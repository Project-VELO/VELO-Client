using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VInspector;

/// <summary>
/// StreamingAssets 음원 파일의 런타임 로드/재생, 탐색, 배속 조절을 전담하는 클래스입니다.
/// 코루틴 대신 UniTask + CancellationToken 기반으로 오디오를 로드하며,
/// 로드가 끝나야 곡 길이를 알 수 있으므로 완료 시점을 OnClipLoaded로 통지합니다.
/// </summary>
public class LiveEditorAudioPlayer : MonoBehaviour
{
    public Action OnClipLoaded;

    // AudioSource.time에 클립 길이를 그대로 대입하면 재생 위치가 범위를 벗어나므로 끝에서 살짝 앞을 최대값으로 삼습니다.
    private const float CLIP_END_MARGIN_SECONDS = 0.01f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private AudioSource _audioSource;

    private int _playbackTimeMs;
    private bool _isSourcePlaying;
    private bool _isPlaybackFinished;
    private ChartData _chart;

    public AudioSource Audio => _audioSource;
    public float PlaybackSpeed { get => _audioSource.pitch; set => _audioSource.pitch = value; }
    public bool IsPlaying => _audioSource.isPlaying;
    public bool IsClipLoaded => _audioSource.clip != null;
    public int ClipLengthMs => _audioSource.clip == null ? 0 : Mathf.RoundToInt(_audioSource.clip.length * 1000f);

    // AudioSource.time은 클립이 없거나 재생이 멈춘 상태에서 유효하지 않으므로, 그 밖의 구간은 별도로 보관한 값을 사용합니다.
    public int CurrentTimeMs => IsPlayingClip ? Mathf.RoundToInt(_audioSource.time * 1000f) : _playbackTimeMs;

    private bool IsPlayingClip => _audioSource.clip != null && _audioSource.isPlaying;

    private void Update()
    {
        bool isPlayingNow = IsPlayingClip;

        if (isPlayingNow)
        {
            _playbackTimeMs = Mathf.RoundToInt(_audioSource.time * 1000f);
        }
        else if (_isSourcePlaying)
        {
            // 사용자가 멈춘 것이 아니라 소스가 스스로 멈춘 경우이므로, 곡이 끝까지 재생된 것으로 봅니다.
            _isPlaybackFinished = true;
        }

        _isSourcePlaying = isPlayingNow;
    }

    /// <summary>
    /// 편집 중 다른 곡으로 바꿀 수 있으므로, 이전 곡의 재생 상태와 위치를 먼저 완전히 비웁니다.
    /// </summary>
    public void Init(SongData song)
    {
        _audioSource.Stop();
        _audioSource.clip = null;
        _playbackTimeMs = 0;
        _isSourcePlaying = false;
        _isPlaybackFinished = false;

        string audioPath = Path.Combine(Application.streamingAssetsPath, "Songs", song.SongId, song.AudioFilePath);
        LoadAudioAsync(audioPath, this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void SetChart(ChartData chart)
    {
        _chart = chart;
    }

    public void Play()
    {
        if (_audioSource.clip == null)
        {
            return;
        }

        // 곡이 끝까지 재생된 뒤 다시 누르면 끝에 붙어 멈춰 있으므로 처음으로 되감아 줍니다.
        if (_isPlaybackFinished)
        {
            _playbackTimeMs = 0;
            _isPlaybackFinished = false;
        }

        // 정지 중 마디 탐색으로 옮겨둔 위치에서 이어서 재생합니다.
        _audioSource.time = ToClampedSeconds(_playbackTimeMs);
        _audioSource.Play();
    }

    /// <summary>
    /// 멈추는 순간의 재생 위치를 직접 붙잡아 둡니다. Update의 갱신에만 기대면 프레임이 밀렸을 때 위치가 어긋납니다.
    /// </summary>
    public void Pause()
    {
        if (IsPlayingClip)
        {
            _playbackTimeMs = Mathf.RoundToInt(_audioSource.time * 1000f);
        }

        // 사용자가 멈춘 것이므로, Update가 이를 곡 종료로 오해하지 않도록 상태를 먼저 내려 둡니다.
        _isSourcePlaying = false;
        _audioSource.Pause();
    }

    public void SetSpeed(float speed)
    {
        PlaybackSpeed = speed;
    }

    public void SetPlaybackTime(int timeMs)
    {
        if (_audioSource.clip == null)
        {
            return;
        }

        float seconds = ToClampedSeconds(timeMs);
        _audioSource.time = seconds;
        _playbackTimeMs = Mathf.RoundToInt(seconds * 1000f);

        // 직접 위치를 옮겼다면 더 이상 "곡이 끝난 상태"가 아니므로 되감기 대상에서 제외합니다.
        _isPlaybackFinished = false;
    }

    private float ToClampedSeconds(int timeMs)
    {
        float maxSeconds = Mathf.Max(0f, _audioSource.clip.length - CLIP_END_MARGIN_SECONDS);
        return Mathf.Clamp(timeMs / 1000f, 0f, maxSeconds);
    }

    public void SeekByGridStep(int direction, ESnapDivision division)
    {
        if (ReferenceEquals(_chart, null) || _audioSource.clip == null)
        {
            return;
        }

        double beat = LiveEditorBpmTimeConverter.TimeMsToBeat(_chart, CurrentTimeMs);
        double gridUnit = 1.0 / (int)division;
        double newBeat = Math.Max(0.0, beat + direction * gridUnit);
        SetPlaybackTime(LiveEditorBpmTimeConverter.BeatToTimeMs(_chart, newBeat));
    }

    private async UniTaskVoid LoadAudioAsync(string audioPath, CancellationToken cancellationToken)
    {
        string url = "file://" + audioPath;
        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);

        await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[LiveEditorAudioPlayer] 오디오 로드 실패: {request.error} ({audioPath})");
            return;
        }

        _audioSource.clip = DownloadHandlerAudioClip.GetContent(request);
        OnClipLoaded?.Invoke();
    }
}
