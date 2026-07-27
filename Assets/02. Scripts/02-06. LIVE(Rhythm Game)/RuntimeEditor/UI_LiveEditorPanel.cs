using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 곡 선택, 배속/메트로놈/스냅 컨트롤, 저장/로드, 플레이테스트(잠금) 버튼을 담당하는 UGUI 패널입니다.
/// </summary>
public class UI_LiveEditorPanel : MonoBehaviour
{
    private static readonly ESnapDivision[] SnapDivisions =
    {
        ESnapDivision.Quarter,
        ESnapDivision.Eighth,
        ESnapDivision.Sixteenth,
        ESnapDivision.TripletEighth,
        ESnapDivision.ThirtySecond,
    };

    private static readonly float[] PlaybackSpeeds = { 0.5f, 0.75f, 1.0f, 1.5f };

    [SerializeField]
    private TMP_Dropdown _songDropdown;

    [SerializeField]
    private TMP_Dropdown _difficultyDropdown;

    [SerializeField]
    private TMP_Dropdown _snapDropdown;

    [SerializeField]
    private TMP_Dropdown _speedDropdown;

    [SerializeField]
    private Toggle _metronomeToggle;

    [SerializeField]
    private Button _saveButton;

    [SerializeField]
    private Button _loadButton;

    [SerializeField]
    private Button _playtestButton;

    [SerializeField]
    private LiveEditorTimeline _timeline;

    [SerializeField]
    private LiveEditorAudioPlayer _audioPlayer;

    private LiveEditorController _controller;
    private List<string> _songIds;

    public void Init(LiveEditorController controller)
    {
        _controller = controller;

        PopulateSongDropdown();
        PopulateDifficultyDropdown();
        PopulateSnapDropdown();
        PopulateSpeedDropdown();
        BindButtonEvents();
        LockPlaytestButton();
    }

    private void PopulateSongDropdown()
    {
        _songIds = _controller.ChartIO.GetAllSongIds();
        var displayOptions = new List<string>();
        foreach (string songId in _songIds)
        {
            displayOptions.Add($"곡: {songId}");
        }

        _songDropdown.ClearOptions();
        _songDropdown.AddOptions(displayOptions);
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

    private void PopulateSnapDropdown()
    {
        var options = new List<string> { "스냅: 1/4", "스냅: 1/8", "스냅: 1/16", "스냅: 1/24 (3연음)", "스냅: 1/32" };
        _snapDropdown.ClearOptions();
        _snapDropdown.AddOptions(options);
        _snapDropdown.onValueChanged.AddListener(OnSnapChanged);
    }

    private void PopulateSpeedDropdown()
    {
        var options = new List<string> { "배속: 0.5x", "배속: 0.75x", "배속: 1.0x", "배속: 1.5x" };
        _speedDropdown.ClearOptions();
        _speedDropdown.AddOptions(options);
        _speedDropdown.onValueChanged.AddListener(OnSpeedChanged);
    }

    private void BindButtonEvents()
    {
        _saveButton.onClick.AddListener(OnSaveClicked);
        _loadButton.onClick.AddListener(OnLoadClicked);
        _playtestButton.onClick.AddListener(OnPlaytestClicked);
        _metronomeToggle.onValueChanged.AddListener(OnMetronomeToggled);
    }

    private void LockPlaytestButton()
    {
        _playtestButton.interactable = false;
    }

    private void OnSnapChanged(int index)
    {
        if (index < 0 || index >= SnapDivisions.Length)
        {
            return;
        }

        _timeline.SnapDivision = SnapDivisions[index];
    }

    private void OnSpeedChanged(int index)
    {
        if (index < 0 || index >= PlaybackSpeeds.Length)
        {
            return;
        }

        _audioPlayer.SetSpeed(PlaybackSpeeds[index]);
    }

    private void OnMetronomeToggled(bool isOn)
    {
        _audioPlayer.MetronomeEnabled = isOn;
    }

    private void OnLoadClicked()
    {
        if (_songIds == null || _songDropdown.value >= _songIds.Count)
        {
            return;
        }

        string songId = _songIds[_songDropdown.value];
        var difficulty = (EDifficulty)_difficultyDropdown.value;
        _controller.LoadSongAndChart(songId, difficulty);
    }

    private void OnSaveClicked()
    {
        var difficulty = (EDifficulty)_difficultyDropdown.value;
        bool success = _controller.SaveCurrentChart(difficulty, out List<string> errors);

        if (!success)
        {
            Debug.LogError($"[UI_LiveEditorPanel] 채보 저장 실패:\n{string.Join("\n", errors)}");
        }
    }

    private void OnPlaytestClicked()
    {
        Debug.LogWarning("[UI_LiveEditorPanel] 플레이 테스트 기능은 아직 지원되지 않습니다. (판정 엔진 미구현)");
    }
}
