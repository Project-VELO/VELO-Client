using System;
using System.Collections.Generic;

/// <summary>
/// 복사한 노트 목록을 특정 시간 오프셋만큼 이동하여 채보에 붙여넣는 커맨드입니다.
/// </summary>
public class PasteCommand : ICommand
{
    private readonly List<NoteData> _notes;
    private readonly List<NoteData> _pastedNotes;

    public PasteCommand(List<NoteData> notes, List<NoteData> sourceNotes, int timeOffsetMs)
    {
        _notes = notes;
        _pastedNotes = new List<NoteData>(sourceNotes.Count);

        foreach (NoteData source in sourceNotes)
        {
            var copy = new NoteData
            {
                NoteId = Guid.NewGuid().ToString(),
                TimeMs = source.TimeMs + timeOffsetMs,
                Lane = source.Lane,
                NoteType = source.NoteType,
                HoldDurationMs = source.HoldDurationMs,
            };
            _pastedNotes.Add(copy);
        }
    }

    public void Execute()
    {
        _notes.AddRange(_pastedNotes);
    }

    public void Undo()
    {
        foreach (NoteData note in _pastedNotes)
        {
            _notes.Remove(note);
        }
    }
}
