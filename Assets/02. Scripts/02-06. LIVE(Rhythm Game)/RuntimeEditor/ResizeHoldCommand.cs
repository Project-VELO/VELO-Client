/// <summary>
/// 롱노트의 HoldDurationMs를 조절하는 커맨드입니다.
/// </summary>
public class ResizeHoldCommand : ICommand
{
    private readonly NoteData _note;
    private readonly int _previousHoldDurationMs;
    private readonly int _newHoldDurationMs;

    public ResizeHoldCommand(NoteData note, int previousHoldDurationMs, int newHoldDurationMs)
    {
        _note = note;
        _previousHoldDurationMs = previousHoldDurationMs;
        _newHoldDurationMs = newHoldDurationMs;
    }

    public void Execute()
    {
        _note.HoldDurationMs = _newHoldDurationMs;
    }

    public void Undo()
    {
        _note.HoldDurationMs = _previousHoldDurationMs;
    }
}
