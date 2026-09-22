using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 리듬게임 씬에서 확정된 판정을 채보의 노트별로 누적해 로컬에 저장합니다.
/// 여러 번 플레이테스트한 뒤 GOOD/BAD가 반복되는 노트를 찾아 채보 타이밍을 고치기 위한 개발용 기록이라,
/// 개발 빌드와 에디터에서만 동작합니다. 채보 에디터 테스트 플레이는 기록하지 않으므로 LivePlaySession만 이 클래스를 만듭니다.
///
/// 판정마다 파일을 쓰지 않고 메모리에 쌓아 두었다가, LivePlaySession이 정한 시점(재시작, 씬 이탈)에 한 번 씁니다.
/// 판정 통지는 매 프레임 올 수 있으므로 통지 처리는 사전 조회와 정수 증가만 합니다.
/// </summary>
public class LiveNoteHitStatRecorder
{
    private readonly Dictionary<string, LiveNoteHitStat> _statsByNoteId = new Dictionary<string, LiveNoteHitStat>();
    private readonly LiveJudgementProcessor _judgementProcessor;

    // 정식 빌드에서는 구독도 파일 입출력도 하지 않습니다. 빌드 종류는 실행 중 바뀌지 않으므로 생성할 때 한 번만 읽습니다.
    private readonly bool _isEnabled = Debug.isDebugBuild;

    private LiveChartHitStats _chartStats;
    private bool _hasJudgedInSession;
    private bool _hasUnsavedChanges;

    public LiveNoteHitStatRecorder(LiveJudgementProcessor judgementProcessor)
    {
        _judgementProcessor = judgementProcessor;

        if (!_isEnabled)
        {
            return;
        }

        _judgementProcessor.OnNoteJudged += RecordJudgement;
        _judgementProcessor.OnSessionReset += ResetSessionState;
    }

    /// <summary>
    /// 플레이할 채보의 통계를 불러와 채보에 맞춥니다. 저장된 통계가 없으면 새로 만듭니다.
    /// 곡과 난이도를 통계 객체에 담아 두므로, 저장할 때는 싱글톤을 다시 읽지 않습니다.
    /// </summary>
    public void InitChart(string songId, EDifficulty difficulty, ChartData chart)
    {
        if (!_isEnabled)
        {
            return;
        }

        LiveChartHitStats stats = LiveChartHitStatsStorage.Load(songId, difficulty);

        if (ReferenceEquals(stats, null))
        {
            stats = new LiveChartHitStats();
            stats.InitChart(songId, difficulty);
        }

        _hasUnsavedChanges = LiveChartHitStatsSync.Sync(stats, chart);
        _chartStats = stats;
        RebuildLookup();
    }

    public void Save()
    {
        if (!_isEnabled || ReferenceEquals(_chartStats, null) || !_hasUnsavedChanges)
        {
            return;
        }

        if (LiveChartHitStatsStorage.Save(_chartStats))
        {
            _hasUnsavedChanges = false;
        }
    }

    public void Release()
    {
        if (!_isEnabled)
        {
            return;
        }

        _judgementProcessor.OnNoteJudged -= RecordJudgement;
        _judgementProcessor.OnSessionReset -= ResetSessionState;
    }

    private void RecordJudgement(LiveNoteJudgement result)
    {
        if (ReferenceEquals(_chartStats, null) || string.IsNullOrEmpty(result.Note.NoteId))
        {
            return;
        }

        // 사전에 없는 노트는 ID가 비었거나 겹쳐 동기화에서 뺀 노트입니다.
        if (!_statsByNoteId.TryGetValue(result.Note.NoteId, out LiveNoteHitStat stat))
        {
            return;
        }

        if (!_hasJudgedInSession)
        {
            _hasJudgedInSession = true;
            _chartStats.IncreasePlayCount();
        }

        stat.Apply(result);
        _hasUnsavedChanges = true;
    }

    private void ResetSessionState()
    {
        _hasJudgedInSession = false;
    }

    private void RebuildLookup()
    {
        _statsByNoteId.Clear();

        foreach (LiveNoteHitStat stat in _chartStats.Notes)
        {
            _statsByNoteId[stat.NoteId] = stat;
        }
    }
}
