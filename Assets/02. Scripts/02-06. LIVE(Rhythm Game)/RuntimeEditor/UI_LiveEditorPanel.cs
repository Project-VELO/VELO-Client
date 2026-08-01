using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VInspector;

/// <summary>
/// 편집 중인 채보의 저장과 테스트 플레이 버튼을 담당하는 UGUI 패널입니다.
/// 어떤 곡/난이도를 편집할지는 진입 시 팝업 흐름(UI_LiveEditorFlow)에서 정해지므로 여기서는 다루지 않습니다.
/// 테스트 플레이의 실제 진입·종료 절차는 LiveEditorTestPlayController가 맡고, 이 패널은 버튼과 문구만 다룹니다.
/// </summary>
public class UI_LiveEditorPanel : MonoBehaviour
{
    private const string PLAYTEST_START_LABEL = "테스트 플레이";
    private const string PLAYTEST_STOP_LABEL = "테스트 중지";

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _saveButton;

    [SerializeField]
    private Button _playtestButton;

    [Tooltip("테스트 플레이 버튼의 문구입니다. 비워 두면 문구를 바꾸지 않습니다.")]
    [SerializeField]
    private TMP_Text _playtestButtonText;

    [SerializeField]
    private LiveEditorTestPlayController _testPlayController;

    [SerializeField]
    private UI_LiveEditorToast _toast;

    private LiveEditorController _controller;

    public void Init(LiveEditorController controller)
    {
        _controller = controller;

        _saveButton.onClick.AddListener(OnSaveClicked);
        _playtestButton.onClick.AddListener(OnPlaytestClicked);

        _testPlayController.OnTestPlayStateChanged += RefreshPlaytestLabel;
        RefreshPlaytestLabel();
    }

    private void OnDestroy()
    {
        if (_testPlayController != null)
        {
            _testPlayController.OnTestPlayStateChanged -= RefreshPlaytestLabel;
        }
    }

    private void OnSaveClicked()
    {
        bool isSaved = _controller.SaveCurrentChart(out List<string> errors);
        if (isSaved)
        {
            if (_toast != null)
            {
                _toast.Show("채보 저장을 완료했습니다.");
            }
            return;
        }

        string reason = errors == null ? "편집 중인 채보가 없습니다." : string.Join("\n", errors);
        Debug.LogError($"[UI_LiveEditorPanel] 채보 저장 실패:\n{reason}");
    }

    /// <summary>
    /// 시작에 실패하는 사유(채보 없음, 음원 로드 전)는 컨트롤러가 이미 경고로 남기므로,
    /// 여기서는 화면에서도 알 수 있도록 상태가 실제로 바뀌었는지만 확인해 안내합니다.
    /// </summary>
    private void OnPlaytestClicked()
    {
        bool wasTestPlaying = _testPlayController.IsTestPlaying;
        _testPlayController.ToggleTestPlay();

        if (wasTestPlaying || _testPlayController.IsTestPlaying || _toast == null)
        {
            return;
        }

        _toast.Show("테스트 플레이를 시작할 수 없습니다. 채보와 음원을 먼저 불러와 주세요.");
    }

    private void RefreshPlaytestLabel()
    {
        if (_playtestButtonText == null)
        {
            return;
        }

        _playtestButtonText.text = _testPlayController.IsTestPlaying ? PLAYTEST_STOP_LABEL : PLAYTEST_START_LABEL;
    }
}
