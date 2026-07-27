using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// StreamingAssets 음원 파일의 런타임 로드/재생, 배속 조절, 메트로놈 기능을 전담하는 클래스입니다.
/// 코루틴 대신 UniTask + CancellationToken 기반으로 오디오를 로드합니다.
/// </summary>
public class LiveEditorAudioPlayer : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private AudioSource _metronomeAudio;

    [SerializeField]
    private AudioClip _metronomeTickClip;

    private bool _metronomeEnabled;
    private int _nextMetronomeBeatIndex;
    private ChartData _chart;

    public AudioSource Audio => _audioSource;
    public float PlaybackSpeed { get => _audioSource.pitch; set => _audioSource.pitch = value; }
    public bool MetronomeEnabled { get => _metronomeEnabled; set => _metronomeEnabled = value; }
    public bool IsPlaying => _audioSource.isPlaying;
    public int CurrentTimeMs => _audioSource.clip == null ? 0 : Mathf.RoundToInt(_audioSource.time * 1000f);

    private void Update()
    {
        if (_metronomeEnabled && _audioSource.isPlaying)
        {
            UpdateMetronome();
        }
    }

    public void Init(SongData song)
    {
        string audioPath = Path.Combine(Application.streamingAssetsPath, "Songs", song.SongId, song.AudioFilePath);
        LoadAudioAsync(audioPath, this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void SetChart(ChartData chart)
    {
        _chart = chart;
        _nextMetronomeBeatIndex = 0;
    }

    public void Play()
    {
        _audioSource.Play();
    }

    public void Pause()
    {
        _audioSource.Pause();
    }

    public void SetSpeed(float speed)
    {
        PlaybackSpeed = speed;
    }

    public void SeekByGridStep(int direction, ESnapDivision division)
    {
        if (_chart == null || _audioSource.clip == null)
        {
            return;
        }

        double beat = LiveEditorBpmTimeConverter.TimeMsToBeat(_chart, CurrentTimeMs);
        double gridUnit = 1.0 / (int)division;
        double newBeat = Math.Max(0.0, beat + direction * gridUnit);
        int newTimeMs = LiveEditorBpmTimeConverter.BeatToTimeMs(_chart, newBeat);
        _audioSource.time = Mathf.Clamp(newTimeMs / 1000f, 0f, _audioSource.clip.length);
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
    }

    private void UpdateMetronome()
    {
        if (_chart == null)
        {
            return;
        }

        int currentTimeMs = CurrentTimeMs;
        int nextBeatTimeMs = LiveEditorBpmTimeConverter.BeatToTimeMs(_chart, _nextMetronomeBeatIndex);

        if (currentTimeMs >= nextBeatTimeMs)
        {
            _metronomeAudio.PlayOneShot(_metronomeTickClip);
            _nextMetronomeBeatIndex++;
        }
    }
}
