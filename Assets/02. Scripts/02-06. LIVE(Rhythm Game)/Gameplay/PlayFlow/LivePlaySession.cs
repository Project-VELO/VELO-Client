using UnityEngine;

/// <summary>
/// 한 판에 쓰이는 곡·채보를 불러와 트랙과 판정기에 나눠 주고, 다시 시작할 때 그 상태를 처음으로 되돌립니다.
/// 무엇을 플레이하는지는 이 클래스가 알고, 언제 플레이하는지는 LiveGameController가 정합니다.
///
/// 유니티 이벤트 메서드를 쓰지 않으므로 컴포넌트가 아니라 컨트롤러가 생성해 쓰는 일반 클래스입니다.
/// 진입 컨텍스트를 곡·채보로 해석하던 LiveSessionLoader는 여기서만 쓰여 병합했습니다.
/// </summary>
public class LivePlaySession
{
    private readonly LiveConductor _conductor;
    private readonly LiveTrackScroller _trackScroller;
    private readonly LiveJudgementProcessor _judgementProcessor;
    private readonly UI_Live _liveUI;

    public SongData Song { get; private set; }
    public ChartData Chart { get; private set; }

    public LivePlaySession(LiveConductor conductor, LiveTrackScroller trackScroller,
        LiveJudgementProcessor judgementProcessor, UI_Live liveUI)
    {
        _conductor = conductor;
        _trackScroller = trackScroller;
        _judgementProcessor = judgementProcessor;
        _liveUI = liveUI;

        // 판정기는 화면을 모르므로, 트랙을 쥔 이 클래스가 판정 통지를 받아 노트를 지웁니다.
        // 판정기와 세션은 같은 컨트롤러가 만들어 수명이 같으므로 따로 해제하지 않습니다.
        _judgementProcessor.OnNoteJudged += HideJudgedNote;
    }

    /// <summary>
    /// 진입 컨텍스트가 가리키는 곡과 채보를 열어 트랙·판정기에 실은 뒤 음원 로드를 시작합니다.
    /// 곡 길이는 음원을 받아야 알 수 있으므로 마디 테이블 구축은 InitBarLayout으로 미룹니다.
    /// </summary>
    public bool TryInitSession()
    {
        SongData song;
        ChartData chart;

        if (!TryLoad(out song, out chart))
        {
            return false;
        }

        Song = song;
        Chart = chart;

        _trackScroller.SetChart(chart);
        _judgementProcessor.InitSession(chart);
        _conductor.AudioPlayer.Init(song);

        return true;
    }

    /// <summary>
    /// 음원 로드가 끝나 곡 길이를 알게 된 시점에 호출합니다.
    /// </summary>
    public void InitBarLayout()
    {
        _trackScroller.InitBarLayout(Chart, _conductor.ClipLengthMs);
        RefreshTrackFromStart();
    }

    /// <summary>
    /// 표시와 집계를 모두 처음 상태로 되돌립니다. SetChart가 가려 둔 노트 목록까지 함께 비웁니다.
    /// </summary>
    public void ResetSession()
    {
        _conductor.Rewind();

        _trackScroller.SetChart(Chart);
        _judgementProcessor.InitSession(Chart);
        RefreshTrackFromStart();

        if (_liveUI.LaneFeedback != null)
        {
            _liveUI.LaneFeedback.ClearHighlights();
        }
    }

    /// <summary>
    /// 현재 재생 시각에 맞춰 노트를 흘려보냅니다. 카운트다운·일시정지 중에도 노트가 그대로 보이도록 매 프레임 호출합니다.
    /// </summary>
    public void RefreshTrack(int songTimeMs)
    {
        _trackScroller.RefreshScroll(songTimeMs);
    }

    private void RefreshTrackFromStart()
    {
        _trackScroller.RefreshScroll(0);
        _trackScroller.RefreshNoteVisuals();
    }

    /// <summary>
    /// 판정이 끝난 노트를 화면에서 지웁니다. note가 null이면 노트 없이 귀신 레인을 누른 오입력이라 지울 노트가 없습니다.
    /// </summary>
    private void HideJudgedNote(NoteData note, EJudgement judgement)
    {
        if (note == null)
        {
            return;
        }

        _trackScroller.NoteRenderer.HideNote(note.NoteId);
    }

    /// <summary>
    /// LiveEntryContext에 담긴 선택 정보를 실제 곡·채보 데이터로 해석합니다.
    /// 곡 선택 화면은 곡 객체가 아니라 ID만 넘기므로, 리듬게임 씬 진입 시 한 번 여기서 풀어 줍니다.
    /// </summary>
    private static bool TryLoad(out SongData song, out ChartData chart)
    {
        song = null;
        chart = null;

        LiveEntryContext context = LiveEntryContext.Instance;
        LiveSongCatalog catalog = LiveSongCatalog.Instance;

        // 곡 선택 화면을 거치지 않고 이 씬을 단독으로 열어 확인할 수도 있으므로 목록을 직접 확보합니다.
        catalog.Build();

        if (!catalog.TryGetSong(context.SelectedSongId, out song))
        {
            Debug.LogError($"[LivePlaySession] 수록 목록에서 곡을 찾지 못했습니다: {context.SelectedSongId}");
            return false;
        }

        chart = LiveChartLoader.LoadPublished(song, context.SelectedDifficulty);

        if (ReferenceEquals(chart, null))
        {
            Debug.LogError($"[LivePlaySession] 채보를 불러오지 못했습니다: {context.SelectedSongId} / {context.SelectedDifficulty}");
            song = null;
            return false;
        }

        return true;
    }
}
