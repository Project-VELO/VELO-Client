using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 곡 선택 → 난이도 선택 → (덮어쓰기 확인) → 채보 열기까지의 선택 구간을 전담합니다.
/// 씬 진입부터의 전체 흐름은 UI_LiveEditorFlow가 관리하며, 이 클래스는 결과만 대리자로 통지합니다.
/// </summary>
public class UI_LiveEditorChartSelectFlow : MonoBehaviour
{
    public Action OnBackToStartRequested;
    public Action OnChartOpened;

    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorController _controller;

    [SerializeField]
    private UI_LiveEditorPopupPresenter _popupPresenter;

    [SerializeField]
    private UI_LiveEditorSongSelectPopup _songSelectPopup;

    [SerializeField]
    private UI_LiveEditorDifficultySelectPopup _difficultySelectPopup;

    [SerializeField]
    private UI_LiveEditorConfirmPopup _confirmPopup;

    private ELiveEditorChartMode _mode = ELiveEditorChartMode.Create;
    private string _selectedSongId;
    private EDifficulty _selectedDifficulty;

    private void Awake()
    {
        _songSelectPopup.OnSongSelected = OnSongSelected;
        _songSelectPopup.OnBackClicked = NotifyBackToStart;

        _difficultySelectPopup.OnDifficultyConfirmed = OnDifficultyConfirmed;
        _difficultySelectPopup.OnBackClicked = ShowSongSelect;
    }

    public void ShowSongSelect(ELiveEditorChartMode mode)
    {
        _mode = mode;
        ShowSongSelect();
    }

    private void ShowSongSelect()
    {
        _popupPresenter.CloseLatest();

        bool isCreateMode = _mode == ELiveEditorChartMode.Create;
        string title = isCreateMode ? "채보를 만들 곡 선택" : "불러올 채보의 곡 선택";

        // 불러오기 흐름에서는 저장된 채보가 하나도 없는 곡을 목록에서 제외합니다.
        List<string> allSongIds = _controller.SongIO.GetAllSongIds();
        List<string> songIds = isCreateMode ? allSongIds : _controller.ChartIO.GetSongIdsWithCharts(allSongIds);
        _songSelectPopup.RefreshSongs(title, songIds);

        _popupPresenter.Open(_songSelectPopup);
    }

    private void OnSongSelected(string songId)
    {
        _selectedSongId = songId;

        _popupPresenter.CloseLatest();
        _difficultySelectPopup.RefreshDifficulties(songId, _mode, _controller.ChartIO.GetSavedDifficulties(songId));

        _popupPresenter.Open(_difficultySelectPopup);
    }

    private void OnDifficultyConfirmed(EDifficulty difficulty)
    {
        _selectedDifficulty = difficulty;

        bool isOverwriting = _mode == ELiveEditorChartMode.Create && _controller.ChartIO.HasChart(_selectedSongId, difficulty);
        if (!isOverwriting)
        {
            OpenEditor();
            return;
        }

        // 난이도 팝업 위에 그대로 겹쳐 띄워, 취소하면 난이도 선택으로 자연스럽게 돌아가게 합니다.
        _confirmPopup.SetMessage($"{_selectedSongId} / {difficulty} 채보가 이미 있습니다.\n빈 채보로 덮어쓸까요?", "덮어쓰기");
        _confirmPopup.OnConfirmed = OnOverwriteConfirmed;
        _confirmPopup.OnCanceled = _popupPresenter.CloseLatest;
        _popupPresenter.Open(_confirmPopup);
    }

    private void OnOverwriteConfirmed()
    {
        _popupPresenter.CloseLatest();
        OpenEditor();
    }

    private void OpenEditor()
    {
        if (!_controller.OpenChart(_selectedSongId, _selectedDifficulty, _mode == ELiveEditorChartMode.Create))
        {
            return;
        }

        // 편집으로 넘어갈 때는 남아 있는 팝업을 한 번에 정리해야 에디터 입력 차단이 함께 풀립니다.
        _popupPresenter.CloseAll();
        OnChartOpened?.Invoke();
    }

    private void NotifyBackToStart()
    {
        OnBackToStartRequested?.Invoke();
    }
}
