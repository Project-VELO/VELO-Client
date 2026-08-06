using UnityEngine;
using VInspector;

/// <summary>
/// 일시정지 팝업의 표시와 버튼 연결을 담당합니다.
/// 실제로 음악·노트·타이머를 멈추고 되돌리는 것은 LiveGameController의 상태 전환이며,
/// 이 클래스는 "언제 팝업을 띄우고 어떤 선택을 전달할지"만 알고 있습니다.
/// </summary>
public class LivePauseController : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveGameController _gameController;

    [SerializeField]
    private UI_Live _liveUI;

    [SerializeField]
    private UI_LivePausePopup _pausePopup;

    private void Awake()
    {
        _liveUI.ScorePanel.PauseButton.onClick.AddListener(OpenPausePopup);

        _pausePopup.OnResumeRequested += _gameController.ResumePlay;
        _pausePopup.OnRestartRequested += _gameController.RestartPlay;
        _pausePopup.OnQuitRequested += _gameController.QuitPlay;
    }

    private void OnDestroy()
    {
        if (_liveUI != null && _liveUI.ScorePanel != null && _liveUI.ScorePanel.PauseButton != null)
        {
            _liveUI.ScorePanel.PauseButton.onClick.RemoveListener(OpenPausePopup);
        }

        if (_pausePopup == null || _gameController == null)
        {
            return;
        }

        _pausePopup.OnResumeRequested -= _gameController.ResumePlay;
        _pausePopup.OnRestartRequested -= _gameController.RestartPlay;
        _pausePopup.OnQuitRequested -= _gameController.QuitPlay;
    }

    private void OpenPausePopup()
    {
        // 팝업 스택은 PersistentScene의 UIManager가 들고 있습니다. 팝업을 띄우지 못하는데 게임을 먼저 멈추면
        // 다시 재개할 방법이 없으므로, 띄울 수 있는지부터 확인하고 멈춥니다.
        if (UIManager.Instance == null)
        {
            Debug.LogWarning("[LivePauseController] UIManager가 없어 일시정지 팝업을 열지 못했습니다. PersistentScene이 로드되어 있는지 확인해 주세요.");
            return;
        }

        if (!_gameController.TryPause())
        {
            return;
        }

        UIManager.Instance.OpenPopup(_pausePopup);
    }
}
