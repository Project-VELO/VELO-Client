using System;
using System.Collections.Generic;

/// <summary>
/// 눌려 있는 롱노트를 레인별로 붙잡아 두고, 끝까지 유지했는지를 판정합니다.
///
/// 롱노트는 노트 하나로 셉니다. 시작 타이밍으로 판정을 미리 정해 두었다가 종료 시각까지 유지하면 그 판정을 그대로 확정하고,
/// 도중에 떼면 BAD로 강등합니다. 시작 시점에 집계하지 않는 덕분에 전체 노트 수와 정확도의 분모를 손대지 않아도 됩니다.
/// 누른 순간의 입력 오차도 함께 보관했다가, 어느 경로로 확정되든 판정에 실어 보냅니다.
///
/// 한 레인에서 동시에 눌러 둘 수 있는 롱노트는 하나뿐이므로 레인 수만큼의 배열이면 충분하며, 탐색도 GC 할당도 없습니다.
/// </summary>
public class LiveHoldTracker
{
    private readonly NoteData[] _holdingNotes = new NoteData[LiveLane.COUNT];
    private readonly EJudgement[] _startJudgements = new EJudgement[LiveLane.COUNT];
    private readonly int[] _startErrorsMs = new int[LiveLane.COUNT];

    /// <summary>
    /// 길이가 있는 롱노트인지 여부입니다. 길이가 0인 롱노트는 단타와 다를 것이 없으므로 유지 판정을 걸지 않습니다.
    /// </summary>
    public static bool IsHoldNote(NoteData note)
    {
        return !ReferenceEquals(note, null) && note.NoteType == ENoteType.LONG && 0 < note.HoldDurationMs;
    }

    public void Clear()
    {
        for (int i = 0; i < LiveLane.COUNT; i++)
        {
            _holdingNotes[i] = null;
            _startErrorsMs[i] = 0;
        }
    }

    /// <summary>
    /// 시작 판정이 끝난 롱노트를 유지 감시 대상으로 등록합니다.
    /// </summary>
    public void BeginHold(NoteData note, EJudgement startJudgement, int startErrorMs)
    {
        if (!LiveLane.IsValid(note.Lane))
        {
            return;
        }

        int laneIndex = note.Lane - LiveLane.FIRST;
        _holdingNotes[laneIndex] = note;
        _startJudgements[laneIndex] = startJudgement;
        _startErrorsMs[laneIndex] = startErrorMs;
    }

    /// <summary>
    /// 키를 뗀 순간의 판정을 확정합니다. 종료 시각을 허용치 안에서 채웠다면 시작 판정을, 그보다 일찍 뗐다면 BAD를 돌려줍니다.
    /// </summary>
    public bool TryReleaseLane(int lane, int songTimeMs, out LiveNoteJudgement result)
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
        result = GetJudgementOnRelease(laneIndex, note, songTimeMs);

        return true;
    }

    /// <summary>
    /// 계속 누르고 있는 채로 종료 시각을 지난 롱노트를 확정합니다. 키를 떼지 않아도 성공으로 인정합니다.
    /// </summary>
    public void CollectCompleted(int songTimeMs, List<LiveNoteJudgement> results)
    {
        for (int laneIndex = 0; laneIndex < LiveLane.COUNT; laneIndex++)
        {
            NoteData note = _holdingNotes[laneIndex];

            if (ReferenceEquals(note, null) || songTimeMs < GetHoldEndTimeMs(note))
            {
                continue;
            }

            _holdingNotes[laneIndex] = null;
            results.Add(CreateHeldToEndJudgement(laneIndex, note));
        }
    }

    /// <summary>
    /// 아직 유지 중인 롱노트를 모두 BAD로 마무리합니다. 곡이 끝나거나 플레이가 중단되어 유지 여부를 더 볼 수 없을 때 사용합니다.
    /// 완주 시점까지 유지 중이었다면 꼬리가 곡 끝을 넘은 채보이므로, 끝까지 유지하지 못한 것(HOLD_BREAK)으로 알립니다.
    /// </summary>
    public void CollectRemaining(List<LiveNoteJudgement> results)
    {
        for (int laneIndex = 0; laneIndex < LiveLane.COUNT; laneIndex++)
        {
            NoteData note = _holdingNotes[laneIndex];

            if (ReferenceEquals(note, null))
            {
                continue;
            }

            _holdingNotes[laneIndex] = null;
            results.Add(CreateHoldBreakJudgement(laneIndex, note));
        }
    }

    /// <summary>
    /// 허용치를 롱노트 길이로 제한합니다. 길이가 허용치보다 짧으면 인정 구간이 시작 시각보다 앞으로 가 버려,
    /// 누르자마자 떼도 끝까지 유지한 것이 됩니다. 격자가 촘촘한 채보에서는 그 길이가 쉽게 나옵니다
    /// (1/16박은 200BPM에서 75ms라 허용치 125ms보다 짧습니다).
    /// </summary>
    private LiveNoteJudgement GetJudgementOnRelease(int laneIndex, NoteData note, int songTimeMs)
    {
        int toleranceMs = Math.Min(LiveJudgementRule.HOLD_RELEASE_TOLERANCE_MS, note.HoldDurationMs);
        bool isHeldToEnd = GetHoldEndTimeMs(note) - toleranceMs <= songTimeMs;

        return isHeldToEnd ? CreateHeldToEndJudgement(laneIndex, note) : CreateHoldBreakJudgement(laneIndex, note);
    }

    private LiveNoteJudgement CreateHeldToEndJudgement(int laneIndex, NoteData note)
    {
        return new LiveNoteJudgement(note, _startJudgements[laneIndex], ELiveJudgementCause.INPUT, _startErrorsMs[laneIndex]);
    }

    private LiveNoteJudgement CreateHoldBreakJudgement(int laneIndex, NoteData note)
    {
        return new LiveNoteJudgement(note, EJudgement.BAD, ELiveJudgementCause.HOLD_BREAK, _startErrorsMs[laneIndex]);
    }

    private static int GetHoldEndTimeMs(NoteData note)
    {
        return note.TimeMs + note.HoldDurationMs;
    }
}
