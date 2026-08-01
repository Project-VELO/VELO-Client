using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 레인 입력과 만료된 노트를 판정으로 바꾸고, 그 결과를 집계와 화면 표시로 잇는 조정자입니다.
/// 판정 규칙 자체는 LiveJudgementRule이, 노트 선택은 LiveNoteQueue가, 롱노트 유지 감시는 LiveHoldTracker가,
/// 누적은 LiveScoreTracker가 맡습니다.
/// </summary>
public class LiveJudgementProcessor : MonoBehaviour
{
    public Action OnGhostFailed;

    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_Live _liveUI;

    [SerializeField]
    private LiveTrackScroller _trackScroller;

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

    public void InitSession(ChartData chart)
    {
        _noteQueue.InitNotes(chart);
        _scoreTracker.Clear();
        _holdTracker.Clear();
        _expiredNotes.Clear();
        _holdResults.Clear();
        _hasGhostFailed = false;

        RefreshHud();
        _liveUI.JudgementPanel.ClearJudgement();
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
        RefreshHud();
        _liveUI.JudgementPanel.RefreshJudgement(EJudgement.BAD);
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
        _trackScroller.NoteRenderer.HideNote(note.NoteId);

        RefreshHud();
        _liveUI.JudgementPanel.RefreshJudgement(judgement);

        if (note.NoteType == ENoteType.GHOST && judgement == EJudgement.BAD)
        {
            FailByGhostNote();
        }
    }

    private void FailByGhostNote()
    {
        _hasGhostFailed = true;
        OnGhostFailed?.Invoke();
    }

    private void RefreshHud()
    {
        _liveUI.ScorePanel.SetScore(_scoreTracker.Score);
        _liveUI.ComboPanel.SetCombo(_scoreTracker.Combo);

        // 진행도 막대는 현재까지의 정확도를 나타내므로, 전체 노트가 아니라 이미 판정된 노트를 분모로 씁니다.
        float accuracy = LiveRankEvaluator.GetAccuracy(_scoreTracker.Score, _scoreTracker.JudgedNoteCount);
        _liveUI.ScorePanel.SetRankProgress(accuracy * 0.01f);
    }
}
