using System;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 채보 에디터 씬에 진입하면 가장 먼저 열리는 시작 팝업입니다.
/// 셋 중 하나를 반드시 고르도록 닫기 버튼을 두지 않으며, 하위 팝업에서 뒤로가기로 이 팝업에 돌아옵니다.
/// </summary>
public class UI_LiveEditorStartPopup : UI_Popup
{
    public Action OnRegisterSongClicked;
    public Action OnCreateChartClicked;
    public Action OnLoadChartClicked;

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _registerSongButton;

    [SerializeField]
    private Button _createChartButton;

    [SerializeField]
    private Button _loadChartButton;

    protected override void Awake()
    {
        base.Awake();

        _registerSongButton.onClick.AddListener(NotifyRegisterSong);
        _createChartButton.onClick.AddListener(NotifyCreateChart);
        _loadChartButton.onClick.AddListener(NotifyLoadChart);
    }

    private void NotifyRegisterSong()
    {
        OnRegisterSongClicked?.Invoke();
    }

    private void NotifyCreateChart()
    {
        OnCreateChartClicked?.Invoke();
    }

    private void NotifyLoadChart()
    {
        OnLoadChartClicked?.Invoke();
    }
}
