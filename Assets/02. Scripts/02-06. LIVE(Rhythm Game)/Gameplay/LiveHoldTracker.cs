using System;
using System.Collections.Generic;

/// <summary>
/// 눌려 있는 롱노트를 레인별로 붙잡아 두고, 끝까지 유지했는지를 판정합니다.
///
/// 롱노트는 노트 하나로 셉니다. 시작 타이밍으로 판정을 미리 정해 두었다가 종료 시각까지 유지하면 그 판정을 그대로 확정하고,
/// 도중에 떼면 BAD로 강등합니다. 시작 시점에 집계하지 않는 덕분에 전체 노트 수와 정확도의 분모를 손대지 않아도 됩니다.
///
/// 한 레인에서 동시에 눌러 둘 수 있는 롱노트는 하나뿐이므로 레인 수만큼의 배열이면 충분하며, 탐색도 GC 할당도 없습니다.
/// </summary>
public class LiveHoldTracker
{
    /// <summary>
    /// 확정된 롱노트 판정입니다. 여러 개를 한 번에 걷어 갈 때 노트와 판정이 짝을 잃지 않도록 묶어 둡니다.
    /// </summary>
    public readonly struct LiveHoldResult
    {
        public readonly NoteData Note;
        public readonly EJudgement Judgement;

        public LiveHoldResult(NoteData note, EJudgement judgement)
        {
            Note = note;
            Judgement = judgement;
        }
    }

    private readonly NoteData[] _holdingNotes = new NoteData[LiveLane.COUNT];
    private readonly EJudgement[] _startJudgements = new EJudgement[LiveLane.COUNT];

    /// <summary>
    /// 길이가 있는 롱노트인지 여부입니다. 길이가 0인 롱노트는 단타와 다를 것이 없으므로 유지 판정을 걸지 않습니다.
    /// </summary>
    public static bool IsHoldNote(NoteData note)
    {
        return !ReferenceEquals(note, null) && note.NoteType == ENoteType.LONG && note.HoldDurationMs > 0;
    }

    public void Clear()
    {
        for (int i = 0; i < LiveLane.COUNT; i++)
        {
            _holdingNotes[i] = null;
        }
    }

    /// <summary>
    /// 시작 판정이 끝난 롱노트를 유지 감시 대상으로 등록합니다.
    /// </summary>
    public void BeginHold(NoteData note, EJudgement startJudgement)
    {
        if (!LiveLane.IsValid(note.Lane))
        {
            return;
        }

        int laneIndex = note.Lane - LiveLane.FIRST;
        _holdingNotes[laneIndex] = note;
        _startJudgements[laneIndex] = startJudgement;
    }

    /// <summary>
    /// 키를 뗀 순간의 판정을 확정합니다. 종료 시각을 허용치 안에서 채웠다면 시작 판정을, 그보다 일찍 뗐다면 BAD를 돌려줍니다.
    /// </summary>
    public bool TryReleaseLane(int lane, int songTimeMs, out LiveHoldResult result)
    {
        result = default;

        if (!LiveLane.IsValid(lane))
        {
            return false;
        }

        int laneIndex = lane - LiveLane.FIRST;
        NoteData note = _holdingNotes[laneIndex];

        if (ReferenceEquals(note, null))
        {
            return false;
        }

        _holdingNotes[laneIndex] = null;
        result = new LiveHoldResult(note, GetJudgementOnRelease(laneIndex, note, songTimeMs));

        return true;
    }

    /// <summary>
    /// 계속 누르고 있는 채로 종료 시각을 지난 롱노트를 확정합니다. 키를 떼지 않아도 성공으로 인정합니다.
    /// </summary>
    public void CollectCompleted(int songTimeMs, List<LiveHoldResult> results)
    {
        for (int laneIndex = 0; laneIndex < LiveLane.COUNT; laneIndex++)
        {
            NoteData note = _holdingNotes[laneIndex];

            if (ReferenceEquals(note, null) || songTimeMs < GetHoldEndTimeMs(note))
            {
                continue;
            }

            _holdingNotes[laneIndex] = null;
            results.Add(new LiveHoldResult(note, _startJudgements[laneIndex]));
        }
    }

    /// <summary>
    /// 아직 유지 중인 롱노트를 모두 BAD로 마무리합니다. 곡이 끝나거나 플레이가 중단되어 유지 여부를 더 볼 수 없을 때 사용합니다.
    /// </summary>
    public void CollectRemaining(List<LiveHoldResult> results)
    {
        for (int laneIndex = 0; laneIndex < LiveLane.COUNT; laneIndex++)
        {
            NoteData note = _holdingNotes[laneIndex];

            if (ReferenceEquals(note, null))
            {
                continue;
            }

            _holdingNotes[laneIndex] = null;
            results.Add(new LiveHoldResult(note, EJudgement.BAD));
        }
    }

    /// <summary>
    /// 허용치를 롱노트 길이로 제한합니다. 길이가 허용치보다 짧으면 인정 구간이 시작 시각보다 앞으로 가 버려,
    /// 누르자마자 떼도 끝까지 유지한 것이 됩니다. 격자가 촘촘한 채보에서는 그 길이가 쉽게 나옵니다
    /// (1/16박은 200BPM에서 75ms라 허용치 125ms보다 짧습니다).
    /// </summary>
    private EJudgement GetJudgementOnRelease(int laneIndex, NoteData note, int songTimeMs)
    {
        int toleranceMs = Math.Min(LiveJudgementRule.HOLD_RELEASE_TOLERANCE_MS, note.HoldDurationMs);
        bool isHeldToEnd = songTimeMs >= GetHoldEndTimeMs(note) - toleranceMs;

        return isHeldToEnd ? _startJudgements[laneIndex] : EJudgement.BAD;
    }

    private static int GetHoldEndTimeMs(NoteData note)
    {
        return note.TimeMs + note.HoldDurationMs;
    }
}
