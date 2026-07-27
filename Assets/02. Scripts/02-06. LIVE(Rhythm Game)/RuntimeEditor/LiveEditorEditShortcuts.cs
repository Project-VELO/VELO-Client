using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Undo/Redo, 복사/붙여넣기, 대칭, 삭제 등 편집 단축키를 처리하는 클래스입니다.
/// LiveEditorInputHandler가 관리하는 선택(Selection)을 대상으로 커맨드를 생성합니다.
/// </summary>
public class LiveEditorEditShortcuts : MonoBehaviour
{
    [SerializeField]
    private LiveEditorController _controller;

    [SerializeField]
    private LiveEditorInputHandler _inputHandler;

    [SerializeField]
    private LiveEditorAudioPlayer _audioPlayer;

    [SerializeField]
    private LiveEditorUndoRedoManager _undoRedoManager;

    [SerializeField]
    private LiveEditorTimeline _timeline;

    private InputAction _undoAction;
    private InputAction _redoAction;
    private InputAction _copyAction;
    private InputAction _pasteAction;
    private InputAction _mirrorAction;
    private InputAction _deleteAction;
    private InputAction _playPauseAction;
    private InputAction _seekBackAction;
    private InputAction _seekForwardAction;

    private readonly List<NoteData> _clipboard = new List<NoteData>();

    private void Awake()
    {
        _undoAction = new InputAction("Undo", binding: "<Keyboard>/ctrl+z");
        _redoAction = new InputAction("Redo", binding: "<Keyboard>/ctrl+y");
        _copyAction = new InputAction("Copy", binding: "<Keyboard>/ctrl+c");
        _pasteAction = new InputAction("Paste", binding: "<Keyboard>/ctrl+v");
        _mirrorAction = new InputAction("Mirror", binding: "<Keyboard>/ctrl+m");
        _deleteAction = new InputAction("Delete", binding: "<Keyboard>/delete");
        _playPauseAction = new InputAction("PlayPause", binding: "<Keyboard>/space");
        _seekBackAction = new InputAction("SeekBack", binding: "<Keyboard>/leftArrow");
        _seekForwardAction = new InputAction("SeekForward", binding: "<Keyboard>/rightArrow");

        _undoAction.performed += _ => _undoRedoManager.Undo();
        _redoAction.performed += _ => _undoRedoManager.Redo();
        _copyAction.performed += _ => CopySelection();
        _pasteAction.performed += _ => PasteClipboard();
        _mirrorAction.performed += _ => MirrorSelection();
        _deleteAction.performed += _ => DeleteSelection();
        _playPauseAction.performed += _ => TogglePlayPause();
        _seekBackAction.performed += _ => Seek(-1);
        _seekForwardAction.performed += _ => Seek(1);
    }

    private void OnEnable()
    {
        _undoAction.Enable();
        _redoAction.Enable();
        _copyAction.Enable();
        _pasteAction.Enable();
        _mirrorAction.Enable();
        _deleteAction.Enable();
        _playPauseAction.Enable();
        _seekBackAction.Enable();
        _seekForwardAction.Enable();
    }

    private void OnDisable()
    {
        _undoAction.Disable();
        _redoAction.Disable();
        _copyAction.Disable();
        _pasteAction.Disable();
        _mirrorAction.Disable();
        _deleteAction.Disable();
        _playPauseAction.Disable();
        _seekBackAction.Disable();
        _seekForwardAction.Disable();
    }

    private void TogglePlayPause()
    {
        if (_controller.CurrentChart == null)
        {
            return;
        }

        if (_audioPlayer.IsPlaying)
        {
            _controller.SetState(LiveEditorController.EEditorState.Paused);
        }
        else
        {
            _audioPlayer.Play();
            _controller.SetState(LiveEditorController.EEditorState.Editing);
        }
    }

    private void Seek(int direction)
    {
        if (_controller.CurrentChart == null || _audioPlayer.IsPlaying)
        {
            return;
        }

        _audioPlayer.SeekByGridStep(direction, _timeline.SnapDivision);
    }

    private void CopySelection()
    {
        _clipboard.Clear();
        _clipboard.AddRange(_inputHandler.Selection);
    }

    private void PasteClipboard()
    {
        if (_clipboard.Count == 0)
        {
            return;
        }

        int timeOffsetMs = _audioPlayer.CurrentTimeMs - _clipboard[0].TimeMs;
        _undoRedoManager.PushCommand(new PasteCommand(_controller.CurrentChart.Notes, _clipboard, timeOffsetMs));
    }

    private void MirrorSelection()
    {
        if (_inputHandler.Selection.Count == 0)
        {
            return;
        }

        _undoRedoManager.PushCommand(new MirrorCommand(new List<NoteData>(_inputHandler.Selection)));
    }

    private void DeleteSelection()
    {
        if (_inputHandler.Selection.Count == 0)
        {
            return;
        }

        _undoRedoManager.PushCommand(new DeleteNoteCommand(_controller.CurrentChart.Notes, new List<NoteData>(_inputHandler.Selection)));
        _inputHandler.ClearSelection();
    }
}
