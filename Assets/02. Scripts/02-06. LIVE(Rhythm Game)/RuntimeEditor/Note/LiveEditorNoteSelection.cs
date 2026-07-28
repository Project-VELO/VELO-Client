using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 편집 대상으로 선택된 노트 목록과, 클릭 지점 근처에 이미 배치된 노트를 찾는 판정을 전담합니다.
/// </summary>
public class LiveEditorNoteSelection
{
    // 클릭 지점과 이 시간차 이내에 있는 같은 레인의 노트는 새로 배치하지 않고 선택 대상으로 간주합니다.
    private const int SELECTION_TOLERANCE_MS = 60;

    private readonly List<NoteData> _notes = new List<NoteData>();

    public IReadOnlyList<NoteData> Notes => _notes;

    public void Select(NoteData note, bool isAdditive)
    {
        if (!isAdditive)
        {
            _notes.Clear();
        }

        _notes.Add(note);
    }

    public void Clear()
    {
        _notes.Clear();
    }

    public NoteData FindNoteNear(ChartData chart, int lane, int timeMs)
    {
        NoteData closest = null;
        int closestDeltaMs = int.MaxValue;

        foreach (NoteData note in chart.Notes)
        {
            if (note.Lane != lane)
            {
                continue;
            }

            int deltaMs = Mathf.Abs(note.TimeMs - timeMs);

            if (deltaMs <= SELECTION_TOLERANCE_MS && deltaMs < closestDeltaMs)
            {
                closest = note;
                closestDeltaMs = deltaMs;
            }
        }

        return closest;
    }
}
