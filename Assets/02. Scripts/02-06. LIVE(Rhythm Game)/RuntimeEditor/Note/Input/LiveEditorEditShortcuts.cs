using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

/// <summary>
/// Undo/Redo, 복사/붙여넣기, 대칭, 삭제 등 편집 단축키를 처리하는 클래스입니다.
/// 마우스 편집과 공유하는 선택(LiveEditorNoteSelection)을 대상으로 커맨드를 생성합니다.
/// 재생 및 탐색 단축키는 LiveEditorNavigationShortcuts가 담당합니다.
/// </summary>
public class LiveEditorEditShortcuts : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorController _controller;

    [SerializeField]
    private LiveEditorNoteEditing _noteEditing;

    [SerializeField]
    private LiveAudioPlayer _audioPlayer;

    private readonly List<InputAction> _actions = new List<InputAction>();
    private readonly List<NoteData> _clipboardNotes = new List<NoteData>();

    private void Awake()
    {
        BindAction("Undo", "<Keyboard>/ctrl+z", _ => _controller.UndoRedo.Undo());
        BindAction("Redo", "<Keyboard>/ctrl+y", _ => _controller.UndoRedo.Redo());
        BindAction("Copy", "<Keyboard>/ctrl+c", _ => CopySelection());
        BindAction("Paste", "<Keyboard>/ctrl+v", _ => PasteClipboard());
        BindAction("Mirror", "<Keyboard>/ctrl+m", _ => MirrorSelection());
        BindAction("Delete", "<Keyboard>/delete", _ => DeleteSelection());
    }

    private void OnEnable()
    {
        foreach (InputAction action in _actions)
        {
            action.Enable();
        }
    }

    private void OnDisable()
    {
        foreach (InputAction action in _actions)
        {
            action.Disable();
        }
    }

    private void OnDestroy()
    {
        foreach (InputAction action in _actions)
        {
            action.Dispose();
        }

        _actions.Clear();
    }

    /// <summary>
    /// 팝업이 떠 있는 동안에는 InputHandler가 입력을 차단하므로, 모든 단축키를 한곳에서 함께 막습니다.
    /// </summary>
    private void BindAction(string actionName, string binding, Action<InputAction.CallbackContext> callback)
    {
        var action = new InputAction(actionName, binding: binding);
        action.performed += context =>
        {
            if (InputHandler.IsInputBlocked)
            {
                return;
            }

            callback(context);
        };
        _actions.Add(action);
    }

    private void CopySelection()
    {
        _clipboardNotes.Clear();
        _clipboardNotes.AddRange(_noteEditing.Selection.Notes);
    }

    private void PasteClipboard()
    {
        if (_clipboardNotes.Count == 0)
        {
            return;
        }

        int timeOffsetMs = _audioPlayer.CurrentTimeMs - _clipboardNotes[0].TimeMs;
        _controller.UndoRedo.PushCommand(new PasteCommand(_controller.CurrentChart.Notes, _clipboardNotes, timeOffsetMs));
    }

    private void MirrorSelection()
    {
        if (_noteEditing.Selection.Count == 0)
        {
            return;
        }

        _controller.UndoRedo.PushCommand(new MirrorCommand(new List<NoteData>(_noteEditing.Selection.Notes)));
    }

    private void DeleteSelection()
    {
        if (_noteEditing.Selection.Count == 0)
        {
            return;
        }

        _noteEditing.NoteWriter.DeleteNotes(new List<NoteData>(_noteEditing.Selection.Notes));
        _noteEditing.Selection.Clear();
    }
}
