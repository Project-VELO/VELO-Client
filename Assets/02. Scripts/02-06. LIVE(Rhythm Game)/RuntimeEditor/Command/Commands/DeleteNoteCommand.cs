using System.Collections.Generic;

/// <summary>
/// 채보에서 하나 이상의 노트를 삭제하는 커맨드입니다. (다중 삭제 지원)
/// </summary>
public class DeleteNoteCommand : ICommand
{
    private readonly List<NoteData> _notes;
    private readonly List<NoteData> _targets;

    public DeleteNoteCommand(List<NoteData> notes, List<NoteData> targets)
    {
        _notes = notes;
        _targets = new List<NoteData>(targets);
    }

    public void Execute()
    {
        foreach (NoteData note in _targets)
        {
            _notes.Remove(note);
        }
    }

    public void Undo()
    {
        foreach (NoteData note in _targets)
        {
            _notes.Add(note);
        }
    }
}
