using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 편집 중인 채보 파일을 삭제하는 동작을 전담합니다.
/// 되돌릴 수 없는 작업이므로 반드시 확인 팝업을 거치며, 삭제 후에는 편집을 이어갈 수 없으므로
/// 흐름을 시작 팝업으로 되돌립니다.
/// </summary>
public class UI_LiveEditorChartDeleter : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorController _controller;

    [SerializeField]
    private UI_LiveEditorFlow _flow;

    [SerializeField]
    private UI_LiveEditorConfirmPopup _confirmPopup;

    [SerializeField]
    private Button _deleteButton;

    // 자체 상태가 없어 누가 만들어도 결과가 같으므로, 씬에서 참조를 배선하지 않고 직접 만들어 씁니다.
    private readonly UI_LiveEditorPopupPresenter _popupPresenter = new UI_LiveEditorPopupPresenter();

    private void Awake()
    {
        _deleteButton.onClick.AddListener(RequestDelete);
    }

    private void RequestDelete()
    {
        if (ReferenceEquals(_controller.CurrentChart, null))
        {
            Debug.LogWarning("[UI_LiveEditorChartDeleter] 편집 중인 채보가 없습니다.");
            return;
        }

        string songId = _controller.CurrentChart.SongId;
        EDifficulty difficulty = _controller.CurrentDifficulty;

        _confirmPopup.SetMessage($"{songId} / {difficulty} 채보를 삭제할까요?\n삭제한 채보는 되돌릴 수 없습니다.", "삭제");
        _confirmPopup.OnConfirmed = DeleteChart;
        _confirmPopup.OnCanceled = _popupPresenter.CloseLatest;

        _popupPresenter.Open(_confirmPopup);
    }

    private void DeleteChart()
    {
        string songId = _controller.CurrentChart.SongId;
        EDifficulty difficulty = _controller.CurrentDifficulty;

        if (!_controller.ChartIO.DeleteChart(songId, difficulty))
        {
            Debug.LogError($"[UI_LiveEditorChartDeleter] 채보 파일을 찾을 수 없습니다: {songId} / {difficulty}");
        }

        _popupPresenter.CloseLatest();
        _controller.CloseCurrentChart();
        _flow.ReturnToStart();
    }
}
