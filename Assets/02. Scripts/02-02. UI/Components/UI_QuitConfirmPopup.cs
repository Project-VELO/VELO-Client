using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 게임을 끝낼지 묻는 팝업입니다(임시).
///
/// 아직 설정 화면이 없어 게임을 끝낼 방법이 창 닫기밖에 없습니다. 설정 화면이 들어오면
/// 그 안의 종료 항목이 이 자리를 대신합니다.
///
/// 닫기는 UI_Popup 이 이미 처리하므로 여기서는 종료 버튼만 맡습니다.
/// 두 버튼의 뒤처리가 다르지 않아(닫으면 그만) 스토리 종료 팝업처럼 경로를 구분하지 않습니다.
/// </summary>
public class UI_QuitConfirmPopup : UI_Popup
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _quitButton;

    protected override void Awake()
    {
        base.Awake();

        if (_quitButton != null)
        {
            _quitButton.onClick.AddListener(Quit);
        }
    }

    /// <summary>
    /// 에디터에서는 Application.Quit 이 아무 일도 하지 않아, 눌러도 반응이 없는 것처럼 보입니다.
    /// 확인할 수 있도록 재생 모드를 대신 멈춥니다.
    /// </summary>
    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
