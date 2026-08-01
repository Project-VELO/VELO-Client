using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 채보 에디터의 상태를 관리하고 현재 편집 중인 채보/곡 데이터를 보유하는 총괄 클래스입니다.
/// 곡 길이는 오디오 클립을 실제로 로드해야 알 수 있으므로, 로드 완료 통지를 받은 시점에 마디 테이블을 구축합니다.
/// 어떤 곡/난이도를 열지 결정하는 화면 흐름은 UI_LiveEditorFlow가 담당합니다.
/// </summary>
public class LiveEditorController : MonoBehaviour
{
    public enum EEditorState
    {
        Editing,
        Paused,
        TestPlay,
    }

    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveAudioPlayer _audioPlayer;

    [SerializeField]
    private LiveEditorTimeline _timeline;

    [SerializeField]
    private UI_LiveEditorPanel _uiPanel;

    [SerializeField]
    private UI_LiveEditorTrackControls _trackControls;

    private readonly LiveEditorChartIO _chartIO = new LiveEditorChartIO();
    private readonly LiveEditorSongIO _songIO = new LiveEditorSongIO();

    private LiveEditorSongMetadataWriter _songMetadataWriter;
    private LiveEditorUndoRedoManager _undoRedoManager;

    private EEditorState _state = EEditorState.Editing;
    private ChartData _currentChart;
    private SongData _currentSong;
    private EDifficulty _currentDifficulty;

    public EEditorState State => _state;
    public ChartData CurrentChart => _currentChart;
    public SongData CurrentSong => _currentSong;
    public EDifficulty CurrentDifficulty => _currentDifficulty;
    public bool HasUnsavedChanges => UndoRedo.HasUnsavedChanges;
    public LiveEditorChartIO ChartIO => _chartIO;
    public LiveEditorSongIO SongIO => _songIO;

    /// <summary>
    /// 편집 이력은 채보 상태와 한 몸이므로 이 클래스가 소유하고, 커맨드를 쌓는 쪽에서 여기로 접근합니다.
    /// 다른 컴포넌트의 Awake에서 먼저 요청할 수 있어 실행 순서에 기대지 않도록 첫 접근 시점에 만듭니다.
    /// </summary>
    public LiveEditorUndoRedoManager UndoRedo => _undoRedoManager ??= new LiveEditorUndoRedoManager(_timeline);

    private LiveEditorSongMetadataWriter SongMetadataWriter => _songMetadataWriter ??= new LiveEditorSongMetadataWriter(_songIO);

    private void Awake()
    {
        _uiPanel.Init(this);
        _trackControls.Init();

        _audioPlayer.OnClipLoaded += OnAudioClipLoaded;
    }

    private void Update()
    {
        // 정지 상태에서도 마디 탐색으로 스크롤 위치가 바뀌므로, 테스트 플레이가 아닌 한 계속 갱신합니다.
        if (_state == EEditorState.TestPlay)
        {
            return;
        }

        _timeline.RefreshScroll(_audioPlayer.CurrentTimeMs);
    }

    private void OnDestroy()
    {
        _audioPlayer.OnClipLoaded -= OnAudioClipLoaded;
    }

    /// <summary>
    /// 지정한 곡/난이도의 채보를 편집 대상으로 엽니다.
    /// isNew가 true면 빈 채보를 만들어 즉시 파일로 저장한 뒤 엽니다.
    /// </summary>
    public bool OpenChart(string songId, EDifficulty difficulty, bool isNew)
    {
        SongData song = _songIO.LoadSong(_songIO.GetSongInfoPath(songId));
        if (song == null)
        {
            Debug.LogError($"[LiveEditorController] SongData를 찾을 수 없습니다: {songId}");
            return false;
        }

        ChartData chart;

        if (isNew)
        {
            // 실패 사유는 CreateEmptyChart가 이미 남기므로 여기서 덧붙이지 않습니다.
            chart = CreateEmptyChart(song, difficulty);
            if (chart == null)
            {
                return false;
            }
        }
        else
        {
            chart = _chartIO.LoadChart(_chartIO.GetChartPath(songId, difficulty));
            if (chart == null)
            {
                Debug.LogError($"[LiveEditorController] 채보를 찾을 수 없습니다: {songId} / {difficulty}");
                return false;
            }
        }

        _currentDifficulty = difficulty;
        Init(song, chart);
        return true;
    }

    public void Init(SongData song, ChartData chart)
    {
        _currentSong = song;
        _currentChart = chart;

        _audioPlayer.SetChart(chart);
        _audioPlayer.Init(song);
        _timeline.SetChart(chart);
        UndoRedo.Clear();
        SetState(EEditorState.Editing);
    }

    /// <summary>
    /// 편집 상태를 바꿉니다. 테스트 플레이의 진입·종료 절차는 LiveEditorTestPlayController가 맡으므로,
    /// 이 메서드는 상태 값과 그에 딸린 재생 정지만 처리합니다.
    /// </summary>
    public void SetState(EEditorState newState)
    {
        _state = newState;

        if (_state == EEditorState.Paused)
        {
            _audioPlayer.Pause();
        }
    }

    /// <summary>
    /// 편집 대상을 비우고 트랙 표시를 초기화합니다. 채보를 삭제한 직후처럼 편집을 이어갈 수 없을 때 호출합니다.
    /// </summary>
    public void CloseCurrentChart()
    {
        _audioPlayer.Pause();

        _currentChart = null;
        _currentSong = null;

        _timeline.SetChart(null);
        _timeline.InitBarLayout(null, 0);
        UndoRedo.Clear();
    }

    public bool SaveCurrentChart(out List<string> errors)
    {
        errors = null;

        if (ReferenceEquals(_currentChart, null))
        {
            return false;
        }

        string path = _chartIO.GetChartPath(_currentChart.SongId, _currentDifficulty);
        bool isSaved = _chartIO.SaveChart(path, _currentChart, _currentSong, out errors);

        if (isSaved)
        {
            SongMetadataWriter.WriteChartMetadata(_currentSong, _currentDifficulty, _currentChart);
            UndoRedo.MarkSaved();
        }

        return isSaved;
    }

    /// <summary>
    /// 빈 채보를 만들어 곧바로 파일로 남깁니다. 이후 목록에서 "불러오기" 대상으로 잡히게 하기 위함입니다.
    /// 파일로 남기지 못했다면 저장된 채보를 편집하는 것처럼 열려서는 안 되므로 실패로 처리합니다.
    /// </summary>
    private ChartData CreateEmptyChart(SongData song, EDifficulty difficulty)
    {
        var chart = new ChartData();
        chart.SongId = song.SongId;
        chart.BaseBpm = song.Bpm;

        if (!_chartIO.SaveChart(_chartIO.GetChartPath(song.SongId, difficulty), chart, song, out List<string> errors))
        {
            Debug.LogError($"[LiveEditorController] 빈 채보 저장 실패:\n{string.Join("\n", errors)}");
            return null;
        }

        return chart;
    }

    private void OnAudioClipLoaded()
    {
        // 곡 길이를 알 수 있는 최초 시점이므로, 저장 시 유효성 검사에 쓰이는 SongData.Duration을 채우고
        // 곡 선택 화면이 읽을 수 있도록 파일에도 반영합니다.
        SongMetadataWriter.WriteDuration(_currentSong, _audioPlayer.ClipLengthMs / 1000f);

        _timeline.InitBarLayout(_currentChart, _audioPlayer.ClipLengthMs);
        _timeline.RefreshScroll(_audioPlayer.CurrentTimeMs);
        _timeline.RefreshNoteVisuals();
    }
}
