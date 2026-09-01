using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// EASY / NORMAL / HARD 난이도 버튼을 관리합니다.
/// NORMAL만 사용합니다(기획서 10.4, 16-8) — 세 버튼을 항상 표시하되
/// EASY와 HARD는 채보가 있어도 클릭할 수 없고, NORMAL도 채보가 수록된 곡에서만 선택됩니다.
/// </summary>
public class UI_MusicSelectDifficultyTabs : MonoBehaviour
{
    public Action<EDifficulty> OnDifficultySelected;

    private const string EMPTY_LEVEL_TEXT = "Lv.-";

    [Foldout("Hierarchy")]
    [Header("Easy")]
    [SerializeField]
    private Button _easyButton;

    [SerializeField]
    private TMP_Text _easyLevelText;

    [Foldout("Hierarchy")]
    [Header("Normal")]
    [SerializeField]
    private Button _normalButton;

    [SerializeField]
    private TMP_Text _normalLevelText;

    [Foldout("Hierarchy")]
    [Header("Hard")]
    [SerializeField]
    private Button _hardButton;

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

    /// <summary>
    /// 모든 LIVE에 NORMAL이 자동 적용됩니다(기획서 16-8). 다른 난이도로의 폴백은 두지 않으므로
    /// NORMAL 채보가 없는 곡은 false를 돌려주어 호출부가 플레이를 막게 합니다.
    /// </summary>
    public bool TryGetDefaultDifficulty(out EDifficulty difficulty)
    {
        difficulty = EDifficulty.NORMAL;
        return _availableDifficulties.Contains(EDifficulty.NORMAL);
    }

    public bool IsAvailable(EDifficulty difficulty)
    {
        return _availableDifficulties.Contains(difficulty);
    }

    private void RefreshTab(EDifficulty difficulty, Button button, TMP_Text levelText, SongData song, LiveChartSummaryReader summaryReader)
    {
        bool isAvailable = _availableDifficulties.Contains(difficulty);

        // EASY와 HARD는 채보가 수록돼 있어도 클릭할 수 없습니다(기획서 10.4 "표시되어도 클릭되지 않는다").
        // 레벨 표기는 정보 제공이므로 그대로 둡니다.
        button.interactable = isAvailable && difficulty == EDifficulty.NORMAL;

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
