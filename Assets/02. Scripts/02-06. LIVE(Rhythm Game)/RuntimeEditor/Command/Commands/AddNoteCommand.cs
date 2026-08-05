using System.Collections.Generic;

/// <summary>
/// 채보에 노트 하나를 추가하는 커맨드입니다.
/// </summary>
public class AddNoteCommand : ICommand
{
    private readonly List<NoteData> _notes;
    private readonly NoteData _note;

    public AddNoteCommand(List<NoteData> notes, NoteData note)
    {
        _notes = notes;
        _note = note;
    }

    public void Execute()
    {
        _notes.Add(_note);
    }

    public void Undo()
    {
        _notes.Remove(_note);
    }
}
