using System.Collections.Generic;

/// <summary>
/// 저장 직전 채보 데이터의 유효성을 검사하는 전담 클래스입니다.
/// </summary>
public class LiveEditorChartValidator
{
    public List<string> Validate(ChartData chart, SongData song)
    {
        var errors = new List<string>();

        ValidateOffset(chart, errors);
        ValidateGhostLaneConstraint(chart, errors);
        ValidateHoldDurations(chart, song, errors);
        ValidateOverlaps(chart, errors);
        ValidateGridAlignment(chart, errors);

        return errors;
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

    private void ValidateHoldDurations(ChartData chart, SongData song, List<string> errors)
    {
        int songDurationMs = (int)(song.Duration * 1000f);

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
            List<NoteData> laneNotes = pair.Value;
            laneNotes.Sort((a, b) => a.TimeMs.CompareTo(b.TimeMs));

            for (int i = 1; i < laneNotes.Count; i++)
            {
                NoteData previous = laneNotes[i - 1];
                NoteData current = laneNotes[i];
                int previousEndTimeMs = previous.TimeMs + previous.HoldDurationMs;

                if (current.TimeMs < previousEndTimeMs)
                {
                    errors.Add($"[{previous.NoteId}]-[{current.NoteId}] 같은 레인({pair.Key})에서 노트 구간이 겹칩니다.");
                }
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
