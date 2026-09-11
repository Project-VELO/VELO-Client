using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 저장 직전 채보 데이터의 유효성을 검사하는 전담 클래스입니다.
/// </summary>
public class LiveEditorChartValidator
{
    public List<string> Validate(ChartData chart, SongData song)
    {
        var errors = new List<string>();

        // 곡 길이는 초 단위 실수로 보관되므로 반올림해야 저장할 때 쓴 ms 값으로 정확히 되돌아옵니다.
        // 버림으로 깎으면 음원 끝에 딱 맞춘 노트가 곡 밖으로 밀려나 거부됩니다.
        int songDurationMs = Mathf.RoundToInt(song.Duration * 1000f);

        ValidateOffset(chart, errors);
        ValidateSongEnd(chart, songDurationMs, errors);
        ValidateGhostLaneConstraint(chart, errors);
        ValidateHoldDurations(chart, songDurationMs, errors);
        ValidateOverlaps(chart, errors);
        ValidateGridAlignment(chart, errors);

        return errors;
    }

    /// <summary>
    /// 음원이 끝난 뒤의 노트를 거릅니다. 곡이 끝나는 순간 남은 노트는 한꺼번에 닫히므로 반드시 BAD가 되고,
    /// 귀신 노트라면 감점까지 따라옵니다.
    /// 곡 길이를 아직 모르는 상태(빈 채보를 만들 때)에서는 검사할 기준이 없으므로 건너뜁니다.
    /// </summary>
    private void ValidateSongEnd(ChartData chart, int songDurationMs, List<string> errors)
    {
        if (songDurationMs <= 0)
        {
            return;
        }

        foreach (NoteData note in chart.Notes)
        {
            if (songDurationMs < note.TimeMs)
            {
                errors.Add($"[{note.NoteId}] TimeMs({note.TimeMs})가 곡 길이({songDurationMs}ms)를 넘습니다.");
            }
        }
    }

    private void ValidateOffset(ChartData chart, List<string> errors)
    {
        foreach (NoteData note in chart.Notes)
        {
            if (note.TimeMs < chart.OffsetMs)
            {
                errors.Add($"[{note.NoteId}] TimeMs({note.TimeMs})가 OffsetMs({chart.OffsetMs})보다 이전입니다.");
            }
        }
    }

    private void ValidateGhostLaneConstraint(ChartData chart, List<string> errors)
    {
        foreach (NoteData note in chart.Notes)
        {
            bool isLaneSix = note.Lane == 6;
            bool isGhost = note.NoteType == ENoteType.GHOST;

            if (isGhost && !isLaneSix)
            {
                errors.Add($"[{note.NoteId}] GHOST 노트는 6번 레인에만 배치할 수 있습니다. (현재 레인: {note.Lane})");
            }
            else if (isLaneSix && !isGhost)
            {
                errors.Add($"[{note.NoteId}] 6번 레인에는 GHOST 노트만 배치할 수 있습니다. (현재 타입: {note.NoteType})");
            }
        }
    }

    private void ValidateHoldDurations(ChartData chart, int songDurationMs, List<string> errors)
    {
        foreach (NoteData note in chart.Notes)
        {
            if (note.NoteType != ENoteType.LONG)
            {
                continue;
            }

            if (note.HoldDurationMs <= 0)
            {
                errors.Add($"[{note.NoteId}] 롱노트의 HoldDurationMs는 0보다 커야 합니다.");
                continue;
            }

            int endTimeMs = note.TimeMs + note.HoldDurationMs;
            if (0 < songDurationMs && songDurationMs < endTimeMs)
            {
                errors.Add($"[{note.NoteId}] 롱노트 종료 시각({endTimeMs}ms)이 곡 길이({songDurationMs}ms)를 초과합니다.");
            }
        }
    }

    private void ValidateOverlaps(ChartData chart, List<string> errors)
    {
        var notesByLane = new Dictionary<int, List<NoteData>>();
        foreach (NoteData note in chart.Notes)
        {
            if (!notesByLane.TryGetValue(note.Lane, out List<NoteData> laneNotes))
            {
                laneNotes = new List<NoteData>();
                notesByLane[note.Lane] = laneNotes;
            }
            laneNotes.Add(note);
        }

        foreach (KeyValuePair<int, List<NoteData>> pair in notesByLane)
        {
            ValidateLaneOverlaps(pair.Key, pair.Value, errors);
        }
    }

    /// <summary>
    /// 한 레인 안에서 구간이 겹치는 노트를 찾습니다.
    ///
    /// 바로 앞 노트 하나만 보면 안 됩니다. 긴 롱노트 뒤에 짧은 노트가 끼면 그 짧은 노트가 기준이 되어,
    /// 롱노트 몸통 한가운데에 묻힌 다음 노트를 놓칩니다. 지금까지 가장 멀리 뻗은 끝 시각을 이어 가며 비교합니다.
    ///
    /// 시각이 완전히 같은 중복 노트는 여기서 걸리지 않습니다(앞 노트의 끝이 곧 자기 시작 시각이라 부등호가 성립하지 않습니다).
    /// 이미 수록된 채보에 그런 노트가 남아 있어, 오류로 올리면 손대지 않은 채보까지 저장이 막히므로 배치 단계에서만 막습니다.
    /// </summary>
    private void ValidateLaneOverlaps(int lane, List<NoteData> laneNotes, List<string> errors)
    {
        laneNotes.Sort((a, b) => a.TimeMs.CompareTo(b.TimeMs));

        NoteData coveringNote = laneNotes[0];
        int coveredUntilMs = coveringNote.TimeMs + coveringNote.HoldDurationMs;

        for (int i = 1; i < laneNotes.Count; i++)
        {
            NoteData current = laneNotes[i];

            if (current.TimeMs < coveredUntilMs)
            {
                errors.Add($"[{coveringNote.NoteId}]-[{current.NoteId}] 같은 레인({lane})에서 노트 구간이 겹칩니다.");
            }

            int currentEndTimeMs = current.TimeMs + current.HoldDurationMs;

            if (coveredUntilMs < currentEndTimeMs)
            {
                coveringNote = current;
                coveredUntilMs = currentEndTimeMs;
            }
        }
    }

    private void ValidateGridAlignment(ChartData chart, List<string> errors)
    {
        foreach (NoteData note in chart.Notes)
        {
            if (!LiveBpmTimeConverter.IsOnGrid(chart, note.TimeMs))
            {
                errors.Add($"[{note.NoteId}] TimeMs({note.TimeMs})가 그리드 스냅 위치에 정렬되어 있지 않습니다.");
            }
        }
    }
}
