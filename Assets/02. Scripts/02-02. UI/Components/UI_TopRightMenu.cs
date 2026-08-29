using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 화면 우상단의 공지·이벤트·설정 버튼입니다.
///
/// 지금은 설정 버튼만 배선되어 있고, 그것도 설정 화면이 아직 없어 임시로 게임 종료를 묻습니다.
/// 설정 화면이 들어오면 이 배선이 그쪽으로 바뀝니다.
///
/// 팝업은 이 메뉴 프리팹이 들고 있습니다. 메뉴가 여러 화면에 얹히므로 화면마다 팝업을
/// 따로 두면 같은 것을 여러 벌 관리하게 되고, UIManager 에 화면별 메서드를 쌓지 않기 위해서입니다.
/// </summary>
public class UI_TopRightMenu : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _settingButton;

    [SerializeField]
    private UI_QuitConfirmPopup _quitConfirmPopup;

    private void Awake()
    {
        if (_settingButton != null)
        {
            _settingButton.onClick.AddListener(OpenQuitConfirm);
        }
    }

    /// <summary>
    /// 팝업 스택은 PersistentScene 의 UIManager 가 들고 있으므로, 이 화면만 단독으로 열어
    /// 확인할 때는 없을 수 있습니다.
    /// </summary>
    private void OpenQuitConfirm()
    {
        if (_quitConfirmPopup == null || UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.OpenPopup(_quitConfirmPopup);
    }
}
