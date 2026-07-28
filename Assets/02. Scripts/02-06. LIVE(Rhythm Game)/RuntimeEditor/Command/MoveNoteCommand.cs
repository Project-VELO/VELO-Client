/// <summary>
/// 노트의 시간(TimeMs)과 레인(Lane)을 이동하는 커맨드입니다.
/// </summary>
public class MoveNoteCommand : ICommand
{
    private readonly NoteData _note;
    private readonly int _previousTimeMs;
    private readonly int _previousLane;
    private readonly int _newTimeMs;
    private readonly int _newLane;

    public MoveNoteCommand(NoteData note, int previousTimeMs, int previousLane, int newTimeMs, int newLane)
    {
        _note = note;
        _previousTimeMs = previousTimeMs;
        _previousLane = previousLane;
        _newTimeMs = newTimeMs;
        _newLane = newLane;
    }

    public void Execute()
    {
        _note.TimeMs = _newTimeMs;
        _note.Lane = _newLane;
    }

    public void Undo()
    {
        _note.TimeMs = _previousTimeMs;
        _note.Lane = _previousLane;
    }
}
