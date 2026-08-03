using UnityEngine;
using VInspector;

/// <summary>
/// 리듬게임의 재생 시각을 판정에 쓸 수 있는 정밀도로 제공합니다.
///
/// AudioSource.time은 오디오 버퍼 단위로만 갱신되어 수십 ms씩 계단식으로 튀는데, 판정 창이 ±45ms이므로
/// 그대로 쓰면 같은 타이밍에 친 입력이 프레임에 따라 PERFECT와 GREAT를 오갑니다.
/// 그래서 곡의 0ms에 해당하는 dspTime을 기준점으로 잡아 두고, 버퍼와 버퍼 사이는 프레임 시간으로 메웁니다.
/// 되감기나 프레임 급락으로 기준점이 어긋날 수 있으므로 실제 재생 위치와의 차이도 계속 감시합니다.
/// </summary>
public class LiveConductor : MonoBehaviour
{
    private const double MS_PER_SECOND = 1000.0;

    // 한 프레임에 메울 수 있는 최대 시간입니다. 히치가 나도 시각이 크게 앞서 나가지 않도록 막습니다.
    private const double MAX_INTERPOLATION_SECONDS = 0.1;

    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveAudioPlayer _audioPlayer;

    [Header("Sync")]
    [Tooltip("실제 재생 위치와 계산된 시각의 차이가 이 값을 넘으면 기준점을 다시 맞춥니다. AudioSource.time 자체가 버퍼 단위로 튀므로 너무 작게 두면 매번 재동기화됩니다.")]
    [SerializeField]
    private int _resyncThresholdMs = 60;

    private double _dspSongStartTime;
    private double _lastDspTime;
    private double _interpolatedSeconds;
    private bool _isRunning;
    private bool _isSyncPending;

    public LiveAudioPlayer AudioPlayer => _audioPlayer;
    public int SongTimeMs { get; private set; }
    public int ClipLengthMs => _audioPlayer.ClipLengthMs;
    public bool IsRunning => _isRunning;
    public bool IsSongFinished => _audioPlayer.IsClipLoaded && SongTimeMs >= _audioPlayer.ClipLengthMs;

    private void Update()
    {
        if (!_isRunning)
        {
            return;
        }

        if (_isSyncPending)
        {
            RefreshPendingSync();
            return;
        }

        RefreshSongTime();
        RefreshDriftCorrection();
    }

    /// <summary>
    /// 현재 재생 위치에서 곡을 시작하거나 이어서 재생합니다.
    /// </summary>
    public void Play()
    {
        _audioPlayer.Play();
        _isRunning = true;

        // Play 직후에는 아직 첫 오디오 버퍼가 나가지 않아 재생 위치가 0에 머물러 있으므로,
        // 실제로 소리가 흐르기 시작한 뒤에 기준점을 잡습니다.
        _isSyncPending = true;
        SongTimeMs = _audioPlayer.CurrentTimeMs;
    }

    public void Pause()
    {
        _audioPlayer.Pause();
        _isRunning = false;
        _isSyncPending = false;
    }

    public void Stop()
    {
        Pause();
        SongTimeMs = _audioPlayer.CurrentTimeMs;
    }

    /// <summary>
    /// 곡을 처음으로 되감습니다. 재생은 시작하지 않으므로 카운트다운을 거쳐 Play를 호출해야 합니다.
    /// </summary>
    public void Rewind()
    {
        Pause();
        _audioPlayer.SetPlaybackTime(0);
        SongTimeMs = 0;
    }

    private void RefreshPendingSync()
    {
        if (!_audioPlayer.IsPlaying)
        {
            return;
        }

        float clipSeconds = _audioPlayer.Audio.time;
        if (clipSeconds <= 0f)
        {
            return;
        }

        AnchorTo(clipSeconds);
        _isSyncPending = false;
    }

    private void RefreshSongTime()
    {
        double dspTime = AudioSettings.dspTime;

        if (dspTime > _lastDspTime)
        {
            _lastDspTime = dspTime;
            _interpolatedSeconds = 0.0;
        }
        else
        {
            _interpolatedSeconds = System.Math.Min(_interpolatedSeconds + Time.unscaledDeltaTime, MAX_INTERPOLATION_SECONDS);
        }

        double songSeconds = _lastDspTime - _dspSongStartTime + _interpolatedSeconds;
        int songTimeMs = (int)(songSeconds * MS_PER_SECOND);

        // 메워 둔 시간은 dspTime이 갱신되는 순간 0으로 돌아가므로, 그 프레임의 계산값이 직전보다 뒤로 밀릴 수 있습니다.
        // 판정 시각이 되감기면 이미 지나간 노트가 유효 입력 구간에 다시 들어오므로 앞으로만 흐르게 고정합니다.
        // 실제 재생 위치와 어긋난 경우는 RefreshDriftCorrection이 SongTimeMs를 직접 덮어써 되돌립니다.
        SongTimeMs = Mathf.Max(SongTimeMs, songTimeMs);
    }

    /// <summary>
    /// 계산된 시각이 실제 재생 위치에서 크게 벗어나면 기준점을 다시 잡습니다.
    /// 프레임 히치나 오디오 장치 지연이 누적되어 노트와 소리가 어긋나는 것을 막습니다.
    /// </summary>
    private void RefreshDriftCorrection()
    {
        if (!_audioPlayer.IsPlaying)
        {
            return;
        }

        float clipSeconds = _audioPlayer.Audio.time;
        int clipTimeMs = Mathf.RoundToInt(clipSeconds * (float)MS_PER_SECOND);

        if (Mathf.Abs(SongTimeMs - clipTimeMs) <= _resyncThresholdMs)
        {
            return;
        }

        AnchorTo(clipSeconds);
        SongTimeMs = clipTimeMs;
    }

    private void AnchorTo(float clipSeconds)
    {
        _dspSongStartTime = AudioSettings.dspTime - clipSeconds;
        _lastDspTime = AudioSettings.dspTime;
        _interpolatedSeconds = 0.0;
    }
}
