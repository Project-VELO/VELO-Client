using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// StreamingAssets/Songs에 등록되지 않은 오디오 파일을 찾아, 곡 제목/BPM/작곡가/난이도를
/// UI에서 직접 입력받아 SongData(song_info.json)로 등록하는 UGUI 패널입니다.
/// </summary>
public class UI_LiveEditorSongRegistration : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown _unregisteredAudioDropdown;

    [SerializeField]
    private TMP_InputField _titleInput;

    [SerializeField]
    private TMP_InputField _bpmInput;

    [SerializeField]
    private TMP_InputField _composerInput;

    [SerializeField]
    private TMP_Dropdown _difficultyDropdown;

    [SerializeField]
    private Button _registerButton;

    [SerializeField]
    private LiveEditorController _controller;

    private List<string> _unregisteredAudioPaths;

    public void Init(LiveEditorController controller)
    {
        _controller = controller;

        PopulateDifficultyDropdown();
        RefreshUnregisteredAudioDropdown();
        _registerButton.onClick.AddListener(OnRegisterClicked);
    }

    private void PopulateDifficultyDropdown()
    {
        var options = new List<string>();
        foreach (EDifficulty difficulty in Enum.GetValues(typeof(EDifficulty)))
        {
            options.Add($"난이도: {difficulty}");
        }

        _difficultyDropdown.ClearOptions();
        _difficultyDropdown.AddOptions(options);
    }

    private void RefreshUnregisteredAudioDropdown()
    {
        _unregisteredAudioPaths = _controller.ChartIO.GetUnregisteredAudioFilePaths();

        var options = new List<string>();
        foreach (string path in _unregisteredAudioPaths)
        {
            options.Add($"오디오: {Path.GetFileNameWithoutExtension(path)}");
        }

        _unregisteredAudioDropdown.ClearOptions();
        _unregisteredAudioDropdown.AddOptions(options);
    }

    private void OnRegisterClicked()
    {
        if (_unregisteredAudioPaths == null || _unregisteredAudioPaths.Count == 0)
        {
            Debug.LogWarning("[UI_LiveEditorSongRegistration] 등록할 오디오 파일이 없습니다.");
            return;
        }

        if (_unregisteredAudioDropdown.value < 0 || _unregisteredAudioDropdown.value >= _unregisteredAudioPaths.Count)
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

        _controller.ChartIO.RegisterSong(audioPath, songId, _titleInput.text, bpm, _composerInput.text);

        var difficulty = (EDifficulty)_difficultyDropdown.value;
        _controller.LoadSongAndChart(songId, difficulty);

        RefreshUnregisteredAudioDropdown();
    }
}
