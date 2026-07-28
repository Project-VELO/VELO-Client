using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 채보 에디터 진입부터 편집 시작까지의 화면 전환을 총괄합니다.
/// 시작 팝업 → (곡 등록 | 곡 선택 → 난이도 선택) → 편집 순서로 진행하며, 편집에 들어가기 전에는
/// 팝업을 닫을 수 없게 하여 곡/채보가 없는 상태로 에디터에 남는 경우를 막습니다.
/// </summary>
public class UI_LiveEditorFlow : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorController _controller;

    [SerializeField]
    private UI_LiveEditorPopupPresenter _popupPresenter;

    [SerializeField]
    private UI_LiveEditorStartPopup _startPopup;

    [SerializeField]
    private UI_LiveEditorSongSelectPopup _songSelectPopup;

    [SerializeField]
    private UI_LiveEditorDifficultySelectPopup _difficultySelectPopup;

    [SerializeField]
    private UI_LiveEditorConfirmPopup _confirmPopup;

    [SerializeField]
    private UI_LiveEditorSongRegistration _songRegistrationPanel;

    [SerializeField]
    private GameObject _editorControlPanel;

    private ELiveEditorChartMode _mode = ELiveEditorChartMode.Create;
    private string _selectedSongId;
    private EDifficulty _selectedDifficulty;
    private bool _isEditing;

    private void Awake()
    {
        _songRegistrationPanel.Init(_controller);

        _startPopup.OnRegisterSongClicked = ShowSongRegistration;
        _startPopup.OnCreateChartClicked = ShowSongSelectForCreate;
        _startPopup.OnLoadChartClicked = ShowSongSelectForLoad;

        _songRegistrationPanel.OnSongRegistered = OnSongRegistered;
        _songRegistrationPanel.OnBackClicked = ShowStartPopup;

        _songSelectPopup.OnSongSelected = OnSongSelected;
        _songSelectPopup.OnBackClicked = ShowStartPopup;

        _difficultySelectPopup.OnDifficultyConfirmed = OnDifficultyConfirmed;
        _difficultySelectPopup.OnBackClicked = ShowSongSelect;
    }

    private void Start()
    {
        ShowStartPopup();
    }

    /// <summary>
    /// UIManager는 ESC 입력을 최상단 팝업 닫기로 처리하므로, 편집을 시작하기 전에 팝업이 모두 사라지면
    /// 곡도 채보도 없는 상태로 남게 됩니다. 그런 경우 시작 팝업을 다시 띄워 흐름을 유지합니다.
    /// </summary>
    private void Update()
    {
        if (_isEditing || _songRegistrationPanel.gameObject.activeSelf || _popupPresenter.HasPopups)
        {
            return;
        }

        ShowStartPopup();
    }

    /// <summary>
    /// 채보 삭제처럼 편집을 이어갈 수 없게 된 경우, 외부에서 흐름을 처음으로 되돌릴 때 호출합니다.
    /// </summary>
    public void ReturnToStart()
    {
        _isEditing = false;
        ShowStartPopup();
    }

    private void ShowStartPopup()
    {
        _popupPresenter.CloseLatest();
        _songRegistrationPanel.gameObject.SetActive(false);
        _editorControlPanel.SetActive(false);

        _popupPresenter.Open(_startPopup);
    }

    private void ShowSongRegistration()
    {
        _popupPresenter.CloseLatest();
        _editorControlPanel.SetActive(false);

        _songRegistrationPanel.gameObject.SetActive(true);
        _songRegistrationPanel.RefreshPanel();
    }

    private void OnSongRegistered(string songId)
    {
        _songRegistrationPanel.gameObject.SetActive(false);
        ShowSongSelectForCreate();
    }

    private void ShowSongSelectForCreate()
    {
        _mode = ELiveEditorChartMode.Create;
        ShowSongSelect();
    }

    private void ShowSongSelectForLoad()
    {
        _mode = ELiveEditorChartMode.Load;
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
        bool isOpened = _controller.OpenChart(_selectedSongId, _selectedDifficulty, _mode == ELiveEditorChartMode.Create);
        if (!isOpened)
        {
            return;
        }

        // 편집으로 넘어갈 때는 남아 있는 팝업을 한 번에 정리해야 에디터 입력 차단이 함께 풀립니다.
        _popupPresenter.CloseAll();

        _isEditing = true;
        _songRegistrationPanel.gameObject.SetActive(false);
        _editorControlPanel.SetActive(true);
    }
}
