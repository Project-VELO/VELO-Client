using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레인 입력과 만료된 노트를 판정으로 바꾸고, 그 결과를 통지하는 조정자입니다.
/// 판정 규칙 자체는 LiveJudgementRule이, 노트 선택은 LiveNoteQueue가, 롱노트 유지 감시는 LiveHoldTracker가,
/// 누적은 LiveScoreTracker가 맡습니다.
///
/// 화면 표시를 직접 밀어 넣지 않고 아래 이벤트로만 알립니다. 판정 코어가 UI를 참조하면
/// 리듬게임 씬과 채보 에디터가 같은 판정기를 공유하는 지금 구조에서 UI 구성 차이가 곧 판정기 수정이 됩니다.
/// </summary>
public class LiveJudgementProcessor : MonoBehaviour
{
    public Action OnGhostFailed;

    /// <summary>
    /// 판정 하나가 확정될 때마다 알립니다. note가 null이면 노트 없이 귀신 레인을 누른 오입력입니다(3-I-6).
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

    // 귀신 실패가 확정되면 남은 노트와 입력을 더 이상 집계하지 않습니다. 결과는 실패 시점까지만 저장합니다(3-I-6).
    private bool _hasGhostFailed;

    public LiveScoreTracker ScoreTracker => _scoreTracker;
    public int TotalNoteCount => _noteQueue.TotalNoteCount;
    public bool HasGhostFailed => _hasGhostFailed;

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
        _hasGhostFailed = false;

        OnSessionReset?.Invoke();
    }

    /// <summary>
    /// 해당 레인의 키가 눌린 순간을 판정합니다.
    /// 판정 범위에 귀신 노트가 없는데 귀신 레인이 눌렸다면 오입력이므로 즉시 실패로 확정합니다(3-I-6).
    /// </summary>
    public void PressLane(int lane, int songTimeMs)
    {
        if (_hasGhostFailed)
        {
            return;
        }

        if (_noteQueue.TryTake(lane, songTimeMs, out NoteData note, out int errorMs))
        {
            EJudgement judgement = LiveJudgementRule.Judge(errorMs);

            // 롱노트는 끝까지 유지해야 판정이 확정되므로, 시작 판정만 맡겨 두고 지금은 집계하지 않습니다.
            if (LiveHoldTracker.IsHoldNote(note))
            {
                _holdTracker.BeginHold(note, judgement);
                return;
            }

            ApplyJudgement(note, judgement);
            return;
        }

        if (lane != LiveLane.GHOST)
        {
            return;
        }

        _scoreTracker.AddGhostMisinput();
        OnScoreChanged?.Invoke();
        OnNoteJudged?.Invoke(null, EJudgement.BAD);
        FailByGhostNote();
    }

    /// <summary>
    /// 눌러 둔 롱노트를 뗀 순간을 판정합니다. 단타 노트를 뗀 것이라면 붙잡아 둔 롱노트가 없으므로 아무 일도 하지 않습니다.
    /// </summary>
    public void ReleaseLane(int lane, int songTimeMs)
    {
        if (_hasGhostFailed)
        {
            return;
        }

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
        if (_hasGhostFailed)
        {
            return;
        }

        _expiredNotes.Clear();
        _noteQueue.CollectExpired(songTimeMs, _expiredNotes);
        ApplyBadToCollectedNotes();

        // 만료 처리 도중 귀신 노트가 실패했다면 그 시점에서 플레이가 끝나므로, 롱노트는 더 이상 확정하지 않습니다(3-I-6).
        if (_hasGhostFailed)
        {
            return;
        }

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
        if (_hasGhostFailed)
        {
            return;
        }

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

            // 귀신 노트가 하나라도 BAD면 그 시점에서 플레이가 끝나므로, 뒤의 노트는 집계하지 않습니다(3-I-6).
            if (_hasGhostFailed)
            {
                return;
            }
        }
    }

    private void ApplyJudgement(NoteData note, EJudgement judgement)
    {
        _scoreTracker.Apply(judgement);

        OnScoreChanged?.Invoke();
        OnNoteJudged?.Invoke(note, judgement);

        if (note.NoteType == ENoteType.GHOST && judgement == EJudgement.BAD)
        {
            FailByGhostNote();
        }
    }

    private void FailByGhostNote()
    {
        _hasGhostFailed = true;

        // 실패 시점 이후는 집계하지 않으므로, 눌러 둔 롱노트도 판정되지 않은 채로 놓아 줍니다.
        _holdTracker.Clear();

        OnGhostFailed?.Invoke();
    }
}
