using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 설정 팝업의 데이터 초기화 버튼입니다. 누르면 한 번 더 묻는 팝업을 엽니다.
///
/// 설정 화면이 아직 없어 언어 선택처럼 종료 확인 팝업에 얹어 둡니다. 설정 화면이 들어오면
/// 이 컴포넌트를 그쪽으로 옮기면 됩니다. 팝업이 아니라 별도 컴포넌트로 둔 이유가 이것입니다(UI_LanguageSelector와 같음).
///
/// 확인 팝업은 설정 팝업의 자식으로 둡니다. 설정 팝업은 여러 화면에 얹히는 우상단 메뉴가 들고 다니므로
/// 화면마다 따로 둘 수 없고, UIManager에 화면 전용 팝업을 쌓지 않기 위해서입니다.
/// </summary>
public class UI_ResetDataButton : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _resetButton;

    [SerializeField]
    private UI_ResetDataConfirmPopup _confirmPopup;

    private void Awake()
    {
        _resetButton.onClick.AddListener(OpenConfirmPopup);
    }

    /// <summary>
    /// 팝업 스택은 PersistentScene의 UIManager가 들고 있으므로, 화면만 단독으로 열어 확인할 때는 없을 수 있습니다.
    /// </summary>
    private void OpenConfirmPopup()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.OpenPopup(_confirmPopup);
    }
}
