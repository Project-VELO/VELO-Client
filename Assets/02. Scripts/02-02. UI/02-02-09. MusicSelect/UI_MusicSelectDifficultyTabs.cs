using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// EASY / NORMAL / HARD 난이도 버튼을 관리합니다.
/// 세 버튼을 항상 표시하되, 수록된 채보 파일이 있는 난이도만 선택할 수 있게 합니다.
/// </summary>
public class UI_MusicSelectDifficultyTabs : MonoBehaviour
{
    public Action<EDifficulty> OnDifficultySelected;

    private const string EMPTY_LEVEL_TEXT = "Lv.-";

    [Header("Tab Colors")]
    [SerializeField]
    private Color _normalColor = new Color(0.16f, 0.15f, 0.24f, 1f);

    [SerializeField]
    private Color _selectedColor = new Color(0.55f, 0.38f, 0.94f, 1f);

    [Foldout("Hierarchy")]
    [Header("Easy")]
    [SerializeField]
    private Button _easyButton;

    [SerializeField]
    private Image _easyBackground;

    [SerializeField]
    private TMP_Text _easyLevelText;

    [Foldout("Hierarchy")]
    [Header("Normal")]
    [SerializeField]
    private Button _normalButton;

    [SerializeField]
    private Image _normalBackground;

    [SerializeField]
    private TMP_Text _normalLevelText;

    [Foldout("Hierarchy")]
    [Header("Hard")]
    [SerializeField]
    private Button _hardButton;

    [SerializeField]
    private Image _hardBackground;

    [SerializeField]
    private TMP_Text _hardLevelText;

    private readonly List<EDifficulty> _availableDifficulties = new List<EDifficulty>();

    private void Awake()
    {
        _easyButton.onClick.AddListener(() => NotifyDifficultySelected(EDifficulty.EASY));
        _normalButton.onClick.AddListener(() => NotifyDifficultySelected(EDifficulty.NORMAL));
        _hardButton.onClick.AddListener(() => NotifyDifficultySelected(EDifficulty.HARD));
    }

    public void RefreshDifficulties(SongData song, LiveChartSummaryReader summaryReader)
    {
        _availableDifficulties.Clear();

        if (!ReferenceEquals(song, null))
        {
            _availableDifficulties.AddRange(summaryReader.GetAvailableDifficulties(song));
        }

        RefreshTab(EDifficulty.EASY, _easyButton, _easyLevelText, song, summaryReader);
        RefreshTab(EDifficulty.NORMAL, _normalButton, _normalLevelText, song, summaryReader);
        RefreshTab(EDifficulty.HARD, _hardButton, _hardLevelText, song, summaryReader);
    }

    public void SetSelectedDifficulty(EDifficulty difficulty)
    {
        _easyBackground.color = difficulty == EDifficulty.EASY ? _selectedColor : _normalColor;
        _normalBackground.color = difficulty == EDifficulty.NORMAL ? _selectedColor : _normalColor;
        _hardBackground.color = difficulty == EDifficulty.HARD ? _selectedColor : _normalColor;
    }

    /// <summary>
    /// 기본 난이도는 기획서상 NORMAL이지만, NORMAL 채보가 아직 없는 곡은 존재하는 첫 난이도로 대신합니다.
    /// 선택 가능한 난이도가 하나도 없으면 false를 돌려주어 호출부가 플레이를 막게 합니다.
    /// </summary>
    public bool TryGetDefaultDifficulty(out EDifficulty difficulty)
    {
        difficulty = EDifficulty.NORMAL;

        if (_availableDifficulties.Count == 0)
        {
            return false;
        }

        if (_availableDifficulties.Contains(EDifficulty.NORMAL))
        {
            return true;
        }

        difficulty = _availableDifficulties[0];
        return true;
    }

    public bool IsAvailable(EDifficulty difficulty)
    {
        return _availableDifficulties.Contains(difficulty);
    }

    private void RefreshTab(EDifficulty difficulty, Button button, TMP_Text levelText, SongData song, LiveChartSummaryReader summaryReader)
    {
        bool isAvailable = _availableDifficulties.Contains(difficulty);
        button.interactable = isAvailable;

        if (!isAvailable || ReferenceEquals(song, null))
        {
            levelText.text = EMPTY_LEVEL_TEXT;
            return;
        }

        LiveChartSummary summary = summaryReader.GetSummary(song, difficulty);
        levelText.text = summary.HasLevel ? $"Lv.{summary.Level}" : EMPTY_LEVEL_TEXT;
    }

    private void NotifyDifficultySelected(EDifficulty difficulty)
    {
        if (!IsAvailable(difficulty))
        {
            return;
        }

        OnDifficultySelected?.Invoke(difficulty);
    }
}
