using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 편집 도중 다른 채보로 갈아타는 동작을 전담합니다.
/// 저장하지 않은 편집이 남아 있으면 먼저 저장 여부를 묻고, 확인이 끝나면 채보 선택 흐름으로 되돌립니다.
/// </summary>
public class UI_LiveEditorChartSwitcher : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorController _controller;

    [SerializeField]
    private UI_LiveEditorFlow _flow;

    [SerializeField]
    private UI_LiveEditorPopupPresenter _popupPresenter;

    [SerializeField]
    private UI_LiveEditorSaveChangesPopup _saveChangesPopup;

    [SerializeField]
    private Button _changeChartButton;

    private void Awake()
    {
        _changeChartButton.onClick.AddListener(RequestChartChange);
    }

    private void RequestChartChange()
    {
        if (ReferenceEquals(_controller.CurrentChart, null))
        {
            return;
        }

        if (!_controller.HasUnsavedChanges)
        {
            ChangeChart();
            return;
        }

        _saveChangesPopup.SetMessage($"{_controller.CurrentChart.SongId} / {_controller.CurrentDifficulty} 채보에\n저장하지 않은 편집이 있습니다.");
        _saveChangesPopup.OnSaveClicked = SaveAndChangeChart;
        _saveChangesPopup.OnDiscardClicked = ChangeChart;
        _saveChangesPopup.OnCancelClicked = _popupPresenter.CloseLatest;

        _popupPresenter.Open(_saveChangesPopup);
    }

    /// <summary>
    /// 저장에 실패하면 편집 내용이 사라지므로 채보를 바꾸지 않고 팝업을 띄운 채로 둡니다.
    /// </summary>
    private void SaveAndChangeChart()
    {
        if (!_controller.SaveCurrentChart(out List<string> errors))
        {
            string reason = errors == null ? "편집 중인 채보가 없습니다." : string.Join("\n", errors);
            Debug.LogError($"[UI_LiveEditorChartSwitcher] 채보 저장 실패로 변경을 중단했습니다:\n{reason}");
            return;
        }

        ChangeChart();
    }

    private void ChangeChart()
    {
        _popupPresenter.CloseLatest();
        _controller.CloseCurrentChart();
        _flow.ReturnToChartSelect();
    }
}
