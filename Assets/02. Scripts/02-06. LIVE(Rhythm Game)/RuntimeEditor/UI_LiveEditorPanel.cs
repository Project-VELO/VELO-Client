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

    private const string SAVE_COMPLETED_TOAST = "채보 저장을 완료했습니다.";
    private const string SAVE_AND_PUBLISH_TOAST = "채보를 저장하고 수록본까지 갱신했습니다.";
    private const string PUBLISH_FAILED_TOAST = "채보는 저장했지만 수록본 갱신에 실패했습니다.";

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

    /// <summary>
    /// 컨트롤러가 Init(this)로 자신을 넘겨 주면 컨트롤러→패널→테스트 플레이→컨트롤러의
    /// 참조 순환이 생기므로, 다른 에디터 UI들처럼 패널이 인스펙터에서 컨트롤러를 직접 참조합니다.
    /// </summary>
    [SerializeField]
    private LiveEditorController _controller;

    private void Awake()
    {
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
        bool isSaved = _controller.SaveCurrentChart(out List<string> errors, out ELivePublishSyncResult syncResult);
        if (isSaved)
        {
            ShowSaveToast(syncResult);
            return;
        }

        string reason = errors == null ? "편집 중인 채보가 없습니다." : string.Join("\n", errors);
        Debug.LogError($"[UI_LiveEditorPanel] 채보 저장 실패:\n{reason}");
    }

    /// <summary>
    /// 수록된 채보는 이 저장으로 게임에도 반영되지만 수록되지 않은 채보는 그렇지 않습니다.
    /// 저장했으니 반영됐겠거니 하는 오해가 이 화면에서 시작되므로, 결과에 따라 문구를 나눕니다.
    /// </summary>
    private void ShowSaveToast(ELivePublishSyncResult syncResult)
    {
        if (syncResult == ELivePublishSyncResult.Failed)
        {
            Debug.LogError($"[UI_LiveEditorPanel] {PUBLISH_FAILED_TOAST}");
        }

        if (_toast == null)
        {
            return;
        }

        switch (syncResult)
        {
            case ELivePublishSyncResult.Updated:
                _toast.Show(SAVE_AND_PUBLISH_TOAST);
                break;

            case ELivePublishSyncResult.Failed:
                _toast.Show(PUBLISH_FAILED_TOAST);
                break;

            default:
                _toast.Show(SAVE_COMPLETED_TOAST);
                break;
        }
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
