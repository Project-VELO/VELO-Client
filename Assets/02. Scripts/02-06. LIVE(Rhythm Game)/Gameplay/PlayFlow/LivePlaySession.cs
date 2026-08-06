using UnityEngine;
using VInspector;

/// <summary>
/// 한 판에 쓰이는 곡·채보를 불러와 트랙과 판정기에 나눠 주고, 다시 시작할 때 그 상태를 처음으로 되돌립니다.
/// 무엇을 플레이하는지는 이 클래스가 알고, 언제 플레이하는지는 LiveGameController가 정합니다.
/// </summary>
public class LivePlaySession : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveConductor _conductor;

    [SerializeField]
    private LiveTrackScroller _trackScroller;

    [SerializeField]
    private LiveJudgementProcessor _judgementProcessor;

    [SerializeField]
    private UI_Live _liveUI;

    public SongData Song { get; private set; }
    public ChartData Chart { get; private set; }

    private void Awake()
    {
        _judgementProcessor.OnNoteJudged += HideJudgedNote;
    }

    private void OnDestroy()
    {
        _judgementProcessor.OnNoteJudged -= HideJudgedNote;
    }

    /// <summary>
    /// 진입 컨텍스트가 가리키는 곡과 채보를 열어 트랙·판정기에 실은 뒤 음원 로드를 시작합니다.
    /// 곡 길이는 음원을 받아야 알 수 있으므로 마디 테이블 구축은 InitBarLayout으로 미룹니다.
    /// </summary>
    public bool TryInitSession()
    {
        SongData song;
        ChartData chart;

        if (!LiveSessionLoader.TryLoad(out song, out chart))
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
    /// 판정이 끝난 노트를 화면에서 지웁니다. 판정기는 화면을 모르므로, 트랙을 쥔 이 클래스가 통지를 받아 처리합니다.
    /// note가 null이면 노트 없이 귀신 레인을 누른 오입력이라 지울 노트가 없습니다.
    /// </summary>
    private void HideJudgedNote(NoteData note, EJudgement judgement)
    {
        if (note == null)
        {
            return;
        }

        _trackScroller.NoteRenderer.HideNote(note.NoteId);
    }
}
