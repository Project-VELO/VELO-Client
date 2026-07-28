using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 선택한 곡의 난이도를 고르는 팝업입니다.
/// 생성 흐름에서는 모든 난이도를 보여주되 이미 채보가 있는 난이도를 표시해 주고,
/// 불러오기 흐름에서는 저장된 채보가 있는 난이도만 보여줍니다.
/// </summary>
public class UI_LiveEditorDifficultySelectPopup : UI_Popup
{
    public Action<EDifficulty> OnDifficultyConfirmed;
    public Action OnBackClicked;

    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_LiveEditorListView _listView;

    [SerializeField]
    private TMP_Text _titleText;

    [SerializeField]
    private TMP_Text _emptyMessageText;

    [SerializeField]
    private Button _confirmButton;

    [SerializeField]
    private TMP_Text _confirmButtonLabel;

    [SerializeField]
    private Button _backButton;

    private readonly List<EDifficulty> _difficulties = new List<EDifficulty>();

    private int _selectedIndex = -1;

    protected override void Awake()
    {
        base.Awake();

        _confirmButton.onClick.AddListener(NotifyConfirmed);
        _backButton.onClick.AddListener(NotifyBack);
    }

    /// <summary>
    /// 팝업을 열기 직전에 호출되어 흐름에 맞는 난이도 목록으로 다시 채웁니다.
    /// </summary>
    public void RefreshDifficulties(string songId, ELiveEditorChartMode mode, List<EDifficulty> savedDifficulties)
    {
        _titleText.text = songId;
        _confirmButtonLabel.text = mode == ELiveEditorChartMode.Create ? "채보 제작 시작" : "채보 불러오기";

        _listView.ReleaseItems();
        _difficulties.Clear();
        _selectedIndex = -1;

        FillDifficulties(mode, savedDifficulties);

        _emptyMessageText.gameObject.SetActive(_difficulties.Count == 0);
        _confirmButton.interactable = false;
    }

    private void FillDifficulties(ELiveEditorChartMode mode, List<EDifficulty> savedDifficulties)
    {
        foreach (EDifficulty difficulty in Enum.GetValues(typeof(EDifficulty)))
        {
            bool isSaved = savedDifficulties != null && savedDifficulties.Contains(difficulty);

            if (mode == ELiveEditorChartMode.Load && !isSaved)
            {
                continue;
            }

            UI_LiveEditorListItem item = _listView.AcquireItem();
            if (item == null)
            {
                break;
            }

            bool shouldMarkExisting = mode == ELiveEditorChartMode.Create && isSaved;
            item.SetItem(_difficulties.Count, shouldMarkExisting ? $"{difficulty}  (채보 있음)" : difficulty.ToString(), true);
            item.OnItemClicked = SelectDifficulty;

            _difficulties.Add(difficulty);
        }
    }

    private void SelectDifficulty(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= _difficulties.Count)
        {
            return;
        }

        _selectedIndex = itemIndex;
        _listView.SetSelectedIndex(itemIndex);
        _confirmButton.interactable = true;
    }

    private void NotifyConfirmed()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _difficulties.Count)
        {
            return;
        }

        OnDifficultyConfirmed?.Invoke(_difficulties[_selectedIndex]);
    }

    private void NotifyBack()
    {
        OnBackClicked?.Invoke();
    }
}
