using System.Collections.Generic;

/// <summary>
/// 커맨드 패턴 기반으로 채보 편집 작업의 실행 취소(Undo)/다시 실행(Redo)을 관리하는 클래스입니다.
/// 실시간 레코딩 중 입력된 노트는 각각 개별 Undo 단위로 처리합니다.
/// 채보가 마지막 저장 이후 바뀌었는지도 이 클래스가 알 수 있으므로 함께 추적합니다.
///
/// 유니티 생명주기를 쓰지 않는 순수 로직이므로 컴포넌트가 아니며, 채보 상태를 쥔 LiveEditorController가 생성해 소유합니다.
/// </summary>
public class LiveEditorUndoRedoManager
{
    private readonly LiveEditorTimeline _timeline;

    private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
    private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

    private bool _hasUnsavedChanges;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// Undo로 되돌려도 저장본과 같아졌다고 단정할 수 없으므로, 편집이 일어나면 저장 전까지 계속 true로 둡니다.
    /// </summary>
    public bool HasUnsavedChanges => _hasUnsavedChanges;

    public LiveEditorUndoRedoManager(LiveEditorTimeline timeline)
    {
        _timeline = timeline;
    }

    public void PushCommand(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
        _hasUnsavedChanges = true;
        _timeline.RefreshNoteVisuals();
    }

    public void Undo()
    {
        if (_undoStack.Count == 0)
        {
            return;
        }

        ICommand command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
        _hasUnsavedChanges = true;
        _timeline.RefreshNoteVisuals();
    }

    public void Redo()
    {
        if (_redoStack.Count == 0)
        {
            return;
        }

        ICommand command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
        _hasUnsavedChanges = true;
        _timeline.RefreshNoteVisuals();
    }

    public void MarkSaved()
    {
        _hasUnsavedChanges = false;
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        _hasUnsavedChanges = false;
    }
}
