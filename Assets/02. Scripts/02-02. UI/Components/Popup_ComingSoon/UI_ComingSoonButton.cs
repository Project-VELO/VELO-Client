using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 아직 기능이 없는 버튼에 붙입니다. 누르면 준비 중 안내를 띄웁니다.
///
/// 버튼을 비활성화하지 않고 눌리게 두는 이유는, 회색으로 꺼진 버튼은 "고장"으로 보이고
/// 아무 반응 없는 버튼은 "눌리지 않았나?"로 보이기 때문입니다. 둘 다 아직 없는 기능이라는
/// 사실을 전하지 못합니다.
///
/// 기능이 붙는 시점에 이 컴포넌트만 떼면 됩니다. 버튼 쪽에는 아무 흔적도 남기지 않습니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class UI_ComingSoonButton : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _button;

    private void Reset()
    {
        _button = GetComponent<Button>();
    }

    private void Awake()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        _button.onClick.AddListener(UI_ComingSoonPopup.Open);
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(UI_ComingSoonPopup.Open);
        }
    }
}
