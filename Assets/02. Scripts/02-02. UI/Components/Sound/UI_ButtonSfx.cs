using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 버튼 하나의 조작음입니다. 올려놓았을 때와 눌렀을 때를 각각 정합니다.
///
/// 대부분의 버튼은 같은 소리를 쓰므로 화면마다 UI_ScreenButtonSfx가 이 컴포넌트를 붙여 줍니다.
/// 뒤로가기처럼 다른 소리를 내야 하는 버튼만 프리팹에 미리 붙여 값을 바꿔 둡니다.
/// 이미 붙어 있으면 화면이 덮어쓰지 않습니다.
/// </summary>
public class UI_ButtonSfx : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Foldout("Settings")]
    [SerializeField]
    private EUiSfx _hover = EUiSfx.BUTTON_HOVER;

    [SerializeField]
    private EUiSfx _click = EUiSfx.BUTTON_CLICK;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Play(_hover);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Play(_click);
    }

    /// <summary>
    /// 누를 수 없는 버튼은 소리를 내지 않습니다. 잠긴 버튼이 눌린 것처럼 들리면
    /// 반응이 없는 이유를 화면에서 찾게 됩니다.
    /// </summary>
    private void Play(EUiSfx sfx)
    {
        if (_button != null && !_button.interactable)
        {
            return;
        }

        if (UiSfxManager.Instance != null)
        {
            UiSfxManager.Instance.Play(sfx);
        }
    }
}
