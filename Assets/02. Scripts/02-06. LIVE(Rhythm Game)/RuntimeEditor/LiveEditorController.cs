using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 채보 에디터의 상태를 관리하고 현재 편집 중인 채보/곡 데이터를 보유하는 총괄 클래스입니다.
/// </summary>
public class LiveEditorController : MonoBehaviour
{
    public enum EEditorState
    {
        Editing,
        Paused,
        TestPlay,
    }

    [SerializeField]
    private LiveEditorAudioPlayer _audioPlayer;

    [SerializeField]
    private LiveEditorTimeline _timeline;

    [SerializeField]
    private LiveEditorUndoRedoManager _undoRedoManager;

    [SerializeField]
    private UI_LiveEditorPanel _uiPanel;

    [SerializeField]
    private UI_LiveEditorSongRegistration _songRegistrationPanel;

    private readonly LiveEditorChartIO _chartIO = new LiveEditorChartIO();

    private EEditorState _state = EEditorState.Editing;
    private ChartData _currentChart;
    private SongData _currentSong;

    public EEditorState State => _state;
    public ChartData CurrentChart => _currentChart;
    public SongData CurrentSong => _currentSong;
    public LiveEditorChartIO ChartIO => _chartIO;

    private void Awake()
    {
        _uiPanel.Init(this);
        _songRegistrationPanel.Init(this);
    }

    private void Update()
    {
        if (_state != EEditorState.Editing)
        {
            return;
        }

        _timeline.SyncScroll(_audioPlayer.CurrentTimeMs);
    }


    public void Init(SongData song, ChartData chart)
    {
        _currentSong = song;
        _currentChart = chart;

        _audioPlayer.SetChart(chart);
        _audioPlayer.Init(song);
        _timeline.SetChart(chart);
        _undoRedoManager.Clear();
        SetState(EEditorState.Editing);
    }

    public void SetState(EEditorState newState)
    {
        if (newState == EEditorState.TestPlay)
        {
            Debug.LogWarning("[LiveEditorController] TestPlay 상태는 아직 잠금 처리되어 있습니다. (판정 엔진 미구현)");
            return;
        }

        _state = newState;

        if (_state == EEditorState.Paused)
        {
            _audioPlayer.Pause();
        }
    }

    public void LoadSongAndChart(string songId, EDifficulty difficulty)
    {
        SongData song = _chartIO.LoadSong(_chartIO.GetSongInfoPath(songId));
        if (song == null)
        {
            Debug.LogError($"[LiveEditorController] SongData를 찾을 수 없습니다: {songId}");
            return;
        }

        ChartData chart = _chartIO.LoadChart(_chartIO.GetChartPath(songId, difficulty));
        if (chart == null)
        {
            chart = new ChartData();
            chart.SongId = songId;
            chart.BaseBpm = song.Bpm;
        }

        Init(song, chart);
    }

    public bool SaveCurrentChart(EDifficulty difficulty, out List<string> errors)
    {
        string path = _chartIO.GetChartPath(_currentChart.SongId, difficulty);
        return _chartIO.SaveChart(path, _currentChart, _currentSong, out errors);
    }
}
