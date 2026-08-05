using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// StreamingAssets/Songs에 등록되지 않은 오디오 파일을 찾아, 곡 제목/BPM/작곡가를
/// UI에서 직접 입력받아 SongData(song_info.json)로 등록하는 UGUI 패널입니다.
/// 난이도는 채보를 만들 때 고르므로 이 패널에서는 다루지 않습니다.
/// </summary>
public class UI_LiveEditorSongRegistration : MonoBehaviour
{
    public Action<string> OnSongRegistered;
    public Action OnBackClicked;

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Dropdown _unregisteredAudioDropdown;

    [SerializeField]
    private TMP_InputField _titleInput;

    [SerializeField]
    private TMP_InputField _bpmInput;

    [SerializeField]
    private TMP_InputField _composerInput;

    [SerializeField]
    private Button _registerButton;

    [SerializeField]
    private Button _backButton;

    private LiveEditorController _controller;
    private List<string> _unregisteredAudioPaths;

    public void Init(LiveEditorController controller)
    {
        _controller = controller;

        _registerButton.onClick.AddListener(OnRegisterClicked);
        _backButton.onClick.AddListener(NotifyBack);
    }

    /// <summary>
    /// 패널이 열릴 때마다 호출되어, 그 사이 새로 추가된 음원 파일까지 목록에 반영합니다.
    /// </summary>
    public void RefreshPanel()
    {
        _unregisteredAudioPaths = _controller.SongIO.GetUnregisteredAudioFilePaths();

        var options = new List<string>();
        foreach (string path in _unregisteredAudioPaths)
        {
            options.Add($"오디오: {Path.GetFileNameWithoutExtension(path)}");
        }

        _unregisteredAudioDropdown.ClearOptions();
        _unregisteredAudioDropdown.AddOptions(options);

        _titleInput.text = string.Empty;
        _bpmInput.text = string.Empty;
        _composerInput.text = string.Empty;
    }

    private void OnRegisterClicked()
    {
        if (_unregisteredAudioPaths == null || _unregisteredAudioPaths.Count == 0)
        {
            Debug.LogWarning("[UI_LiveEditorSongRegistration] 등록할 오디오 파일이 없습니다.");
            return;
        }

        if (_unregisteredAudioDropdown.value < 0 || _unregisteredAudioPaths.Count <= _unregisteredAudioDropdown.value)
        {
            return;
        }

        if (!float.TryParse(_bpmInput.text, out float bpm) || bpm <= 0f)
        {
            Debug.LogError("[UI_LiveEditorSongRegistration] 올바른 BPM 값을 입력해주세요.");
            return;
        }

        string audioPath = _unregisteredAudioPaths[_unregisteredAudioDropdown.value];
        string songId = Path.GetFileNameWithoutExtension(audioPath);

        _controller.SongIO.RegisterSong(audioPath, songId, _titleInput.text, bpm, _composerInput.text);

        RefreshPanel();
        OnSongRegistered?.Invoke(songId);
    }

    private void NotifyBack()
    {
        OnBackClicked?.Invoke();
    }
}
