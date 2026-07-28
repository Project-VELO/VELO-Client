using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 편집 중인 채보의 저장과 플레이테스트(잠금) 버튼을 담당하는 UGUI 패널입니다.
/// 어떤 곡/난이도를 편집할지는 진입 시 팝업 흐름(UI_LiveEditorFlow)에서 정해지므로 여기서는 다루지 않습니다.
/// </summary>
public class UI_LiveEditorPanel : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _saveButton;

    [SerializeField]
    private Button _playtestButton;

    private LiveEditorController _controller;

    public void Init(LiveEditorController controller)
    {
        _controller = controller;

        _saveButton.onClick.AddListener(OnSaveClicked);
        _playtestButton.onClick.AddListener(OnPlaytestClicked);
        LockPlaytestButton();
    }

    private void LockPlaytestButton()
    {
        _playtestButton.interactable = false;
    }

    private void OnSaveClicked()
    {
        bool isSaved = _controller.SaveCurrentChart(out List<string> errors);
        if (isSaved)
        {
            return;
        }

        string reason = errors == null ? "편집 중인 채보가 없습니다." : string.Join("\n", errors);
        Debug.LogError($"[UI_LiveEditorPanel] 채보 저장 실패:\n{reason}");
    }

    private void OnPlaytestClicked()
    {
        Debug.LogWarning("[UI_LiveEditorPanel] 플레이 테스트 기능은 아직 지원되지 않습니다. (판정 엔진 미구현)");
    }
}
