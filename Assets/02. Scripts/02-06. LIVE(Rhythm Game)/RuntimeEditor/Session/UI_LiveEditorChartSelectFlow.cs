using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 곡 선택 → 난이도 선택 → (기존 채보 처리 확인) → 채보 열기까지의 선택 구간을 전담합니다.
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
    private UI_LiveEditorSongSelectPopup _songSelectPopup;

    [SerializeField]
    private UI_LiveEditorDifficultySelectPopup _difficultySelectPopup;

    [SerializeField]
    private UI_LiveEditorConfirmPopup _confirmPopup;

    private ELiveEditorChartMode _mode = ELiveEditorChartMode.Create;
    private string _selectedSongId;
    private EDifficulty _selectedDifficulty;

    // 자체 상태가 없어 누가 만들어도 결과가 같으므로, 씬에서 참조를 배선하지 않고 직접 만들어 씁니다.
    private readonly UI_LiveEditorPopupPresenter _popupPresenter = new UI_LiveEditorPopupPresenter();

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

        bool isCreateMode = _mode == ELiveEditorChartMode.Create;

        // 불러오기 흐름은 저장된 난이도만 목록에 올리므로 확인 없이 바로 엽니다.
        bool hasExistingChart = isCreateMode && _controller.ChartIO.HasChart(_selectedSongId, difficulty);
        if (!hasExistingChart)
        {
            OpenEditor(isCreateMode);
            return;
        }

        // 이미 채보가 있는 난이도를 만들기로 골랐으므로, 지우고 새로 시작할지 아니면 그대로 이어서 편집할지 묻습니다.
        // 두 선택지 모두 편집으로 이어지므로 취소 쪽 문구도 무엇을 하는지 드러나게 적습니다.
        _confirmPopup.SetMessage(
            $"{_selectedSongId} / {difficulty} 채보가 이미 있습니다.\n기존 채보를 삭제하고 새로 만들까요?",
            "삭제하고 새로 만들기",
            "기존 채보 불러오기");
        _confirmPopup.OnConfirmed = ReplaceExistingChart;
        _confirmPopup.OnCanceled = LoadExistingChart;
        _popupPresenter.Open(_confirmPopup);
    }

    /// <summary>
    /// 기존 채보 파일을 지우고 빈 채보로 새로 시작합니다.
    /// 빈 채보는 OpenChart가 곧바로 파일로 남기므로, 여기서는 지우는 것까지만 책임집니다.
    /// </summary>
    private void ReplaceExistingChart()
    {
        if (!_controller.ChartIO.DeleteChart(_selectedSongId, _selectedDifficulty))
        {
            Debug.LogError($"[UI_LiveEditorChartSelectFlow] 삭제할 채보 파일을 찾을 수 없습니다: {_selectedSongId} / {_selectedDifficulty}");
        }

        _popupPresenter.CloseLatest();
        OpenEditor(true);
    }

    /// <summary>
    /// 새로 만들지 않기로 했으므로 기존 채보를 그대로 열어 이어서 편집합니다.
    /// </summary>
    private void LoadExistingChart()
    {
        _popupPresenter.CloseLatest();
        OpenEditor(false);
    }

    private void OpenEditor(bool isNew)
    {
        if (!_controller.OpenChart(_selectedSongId, _selectedDifficulty, isNew))
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
