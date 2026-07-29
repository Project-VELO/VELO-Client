using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 채보 에디터 진입부터 편집 시작까지의 화면 전환을 총괄합니다.
/// 시작 팝업 → (곡 등록 | 곡·난이도 선택) → 편집 순서로 진행하며, 편집에 들어가기 전에는
/// 팝업을 닫을 수 없게 하여 곡/채보가 없는 상태로 에디터에 남는 경우를 막습니다.
/// 곡·난이도 선택 구간 자체는 UI_LiveEditorChartSelectFlow가 담당합니다.
/// </summary>
public class UI_LiveEditorFlow : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorController _controller;

    [SerializeField]
    private UI_LiveEditorChartSelectFlow _chartSelectFlow;

    [SerializeField]
    private UI_LiveEditorStartPopup _startPopup;

    [SerializeField]
    private UI_LiveEditorSongRegistration _songRegistrationPanel;

    [Header("Editor Only UI")]
    [SerializeField]
    private List<GameObject> _editorOnlyPanels = new List<GameObject>();

    // 자체 상태가 없어 누가 만들어도 결과가 같으므로, 씬에서 참조를 배선하지 않고 직접 만들어 씁니다.
    private readonly UI_LiveEditorPopupPresenter _popupPresenter = new UI_LiveEditorPopupPresenter();

    private bool _isEditing;

    private void Awake()
    {
        _songRegistrationPanel.Init(_controller);

        _startPopup.OnRegisterSongClicked = ShowSongRegistration;
        _startPopup.OnCreateChartClicked = ShowSongSelectForCreate;
        _startPopup.OnLoadChartClicked = ShowSongSelectForLoad;

        _songRegistrationPanel.OnSongRegistered = OnSongRegistered;
        _songRegistrationPanel.OnBackClicked = ShowStartPopup;

        _chartSelectFlow.OnBackToStartRequested = ShowStartPopup;
        _chartSelectFlow.OnChartOpened = EnterEditing;
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

    /// <summary>
    /// 편집 도중 다른 채보로 갈아탈 때 호출합니다. 곡 선택부터 다시 진행합니다.
    /// </summary>
    public void ReturnToChartSelect()
    {
        _isEditing = false;
        ShowSongSelectForLoad();
    }

    private void ShowStartPopup()
    {
        _popupPresenter.CloseLatest();
        _songRegistrationPanel.gameObject.SetActive(false);
        SetEditorUiActive(false);

        _popupPresenter.Open(_startPopup);
    }

    /// <summary>
    /// 편집용 UI는 채보를 열기 전에는 쓸 일이 없으므로 함께 켜고 끕니다.
    /// 씬에는 꺼진 상태로 저장해 두어 진입 시점의 SetActive 호출에 기대지 않습니다.
    /// </summary>
    private void SetEditorUiActive(bool isActive)
    {
        foreach (GameObject panel in _editorOnlyPanels)
        {
            panel.SetActive(isActive);
        }
    }

    private void ShowSongRegistration()
    {
        _popupPresenter.CloseLatest();
        SetEditorUiActive(false);

        _songRegistrationPanel.gameObject.SetActive(true);
        _songRegistrationPanel.RefreshPanel();
    }

    private void OnSongRegistered(string songId)
    {
        ShowSongSelectForCreate();
    }

    private void ShowSongSelectForCreate()
    {
        ShowSongSelect(ELiveEditorChartMode.Create);
    }

    private void ShowSongSelectForLoad()
    {
        ShowSongSelect(ELiveEditorChartMode.Load);
    }

    private void ShowSongSelect(ELiveEditorChartMode mode)
    {
        _songRegistrationPanel.gameObject.SetActive(false);
        SetEditorUiActive(false);

        _chartSelectFlow.ShowSongSelect(mode);
    }

    private void EnterEditing()
    {
        _isEditing = true;
        _songRegistrationPanel.gameObject.SetActive(false);
        SetEditorUiActive(true);
    }
}
