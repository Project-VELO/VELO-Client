using System;
using System.Collections.Generic;

/// <summary>
/// 레인 입력과 만료된 노트를 판정으로 바꾸고, 그 결과를 통지하는 조정자입니다.
/// 판정 규칙 자체는 LiveJudgementRule이, 노트 선택은 LiveNoteQueue가, 롱노트 유지 감시는 LiveHoldTracker가,
/// 누적은 LiveScoreTracker가 맡습니다.
///
/// 화면 표시를 직접 밀어 넣지 않고 아래 이벤트로만 알립니다. 판정 코어가 UI를 참조하면
/// 리듬게임 씬과 채보 에디터가 같은 판정기를 공유하는 지금 구조에서 UI 구성 차이가 곧 판정기 수정이 됩니다.
///
/// 유니티 이벤트 메서드를 쓰지 않으므로 컴포넌트가 아니라 각 씬의 컨트롤러가 생성해 쓰는 일반 클래스입니다.
/// </summary>
public class LiveJudgementProcessor
{
    /// <summary>
    /// 판정 하나가 확정될 때마다 알립니다.
    /// 표시(판정 문구)와 노트 숨김이 함께 구독하므로, 대입으로 서로를 지우지 못하게 event로 선언합니다.
    /// </summary>
    public event Action<NoteData, EJudgement> OnNoteJudged;
    public event Action OnScoreChanged;
    public event Action OnSessionReset;

    private readonly LiveNoteQueue _noteQueue = new LiveNoteQueue();
    private readonly LiveScoreTracker _scoreTracker = new LiveScoreTracker();
    private readonly LiveHoldTracker _holdTracker = new LiveHoldTracker();
    private readonly List<NoteData> _expiredNotes = new List<NoteData>();
    private readonly List<LiveHoldTracker.LiveHoldResult> _holdResults = new List<LiveHoldTracker.LiveHoldResult>();

    public LiveScoreTracker ScoreTracker => _scoreTracker;
    public int TotalNoteCount => _noteQueue.TotalNoteCount;

    /// <summary>
    /// 한 판의 집계를 처음 상태로 되돌립니다.
    /// startTimeMs는 곡 중간부터 시작할 때만 지정하며, 그보다 앞선 노트는 이번 판의 집계 대상에서 빠집니다.
    /// </summary>
    public void InitSession(ChartData chart, int startTimeMs = 0)
    {
        _noteQueue.InitNotes(chart, startTimeMs);
        _scoreTracker.Clear();
        _holdTracker.Clear();
        _expiredNotes.Clear();
        _holdResults.Clear();

        OnSessionReset?.Invoke();
    }

    /// <summary>
    /// 해당 레인의 키가 눌린 순간을 판정합니다.
    /// 입력이 닿는 노트가 없으면 아무 일도 하지 않습니다. 귀신 레인도 예외가 아니어서, 노트가 없는 레인을 누르는 것은
    /// 어느 레인이든 대가가 없습니다. 이르게 누른 입력에 대한 처벌은 LiveNoteQueue의 선입력 소비 구간이 맡습니다.
    /// </summary>
    public void PressLane(int lane, int songTimeMs)
    {
        if (!_noteQueue.TryTake(lane, songTimeMs, out NoteData note, out int errorMs))
        {
            return;
        }

        EJudgement judgement = LiveJudgementRule.Judge(errorMs);

        // 롱노트는 끝까지 유지해야 판정이 확정되므로, 시작 판정만 맡겨 두고 지금은 집계하지 않습니다.
        if (LiveHoldTracker.IsHoldNote(note))
        {
            _holdTracker.BeginHold(note, judgement);
            return;
        }

        ApplyJudgement(note, judgement);
    }

    /// <summary>
    /// 눌러 둔 롱노트를 뗀 순간을 판정합니다. 단타 노트를 뗀 것이라면 붙잡아 둔 롱노트가 없으므로 아무 일도 하지 않습니다.
    /// </summary>
    public void ReleaseLane(int lane, int songTimeMs)
    {
        if (_holdTracker.TryReleaseLane(lane, songTimeMs, out LiveHoldTracker.LiveHoldResult result))
        {
            ApplyJudgement(result.Note, result.Judgement);
        }
    }

    /// <summary>
    /// 유효 입력 시간이 지난 노트를 모두 BAD로 처리하고, 끝까지 눌러 유지한 롱노트를 확정합니다.
    /// </summary>
    public void RefreshExpiredNotes(int songTimeMs)
    {
        _expiredNotes.Clear();
        _noteQueue.CollectExpired(songTimeMs, _expiredNotes);
        ApplyBadToCollectedNotes();

        _holdResults.Clear();
        _holdTracker.CollectCompleted(songTimeMs, _holdResults);
        ApplyCollectedHoldResults();
    }

    /// <summary>
    /// 완주 시점에 아직 판정되지 않은 노트를 모두 BAD로 마무리합니다.
    /// 마지막 노트가 곡 끝에 붙어 있으면 유효 입력 시간이 끝나기 전에 곡이 먼저 끝날 수 있는데,
    /// 그대로 두면 정확도의 분모(전체 노트 수)에는 들어가면서 판정 개수에는 빠져 결과가 어긋납니다.
    /// 유지 중이던 롱노트도 더 이상 끝까지 눌렀는지 확인할 수 없으므로 같은 이유로 BAD로 닫습니다.
    /// </summary>
    public void FlushRemainingNotes()
    {
        _holdResults.Clear();
        _holdTracker.CollectRemaining(_holdResults);
        ApplyCollectedHoldResults();

        _expiredNotes.Clear();
        _noteQueue.CollectRemaining(_expiredNotes);
        ApplyBadToCollectedNotes();
    }

    private void ApplyCollectedHoldResults()
    {
        for (int i = 0; i < _holdResults.Count; i++)
        {
            ApplyJudgement(_holdResults[i].Note, _holdResults[i].Judgement);
        }
    }

    private void ApplyBadToCollectedNotes()
    {
        for (int i = 0; i < _expiredNotes.Count; i++)
        {
            ApplyJudgement(_expiredNotes[i], EJudgement.BAD);
        }
    }

    private void ApplyJudgement(NoteData note, EJudgement judgement)
    {
        _scoreTracker.Apply(judgement);

        // 감점을 통지보다 앞에 두어야 HUD가 이미 깎인 점수로 한 번만 갱신됩니다(3-I-6).
        if (note.NoteType == ENoteType.GHOST && judgement == EJudgement.BAD)
        {
            _scoreTracker.ApplyGhostMissPenalty();
        }

        OnScoreChanged?.Invoke();
        OnNoteJudged?.Invoke(note, judgement);
    }
}
