using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 커맨드 패턴 기반으로 채보 편집 작업의 실행 취소(Undo)/다시 실행(Redo)을 관리하는 클래스입니다.
/// 실시간 레코딩 중 입력된 노트는 각각 개별 Undo 단위로 처리합니다.
/// </summary>
public class LiveEditorUndoRedoManager : MonoBehaviour
{
    [SerializeField]
    private LiveEditorTimeline _timeline;

    private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
    private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public void PushCommand(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
        _timeline.SyncNoteVisuals();
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
        _timeline.SyncNoteVisuals();
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
        _timeline.SyncNoteVisuals();
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}
