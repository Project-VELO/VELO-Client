using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 한국어와 일본어를 고르는 두 버튼입니다.
///
/// 설정 화면이 아직 없어 종료 확인 팝업에 얹어 둡니다. 설정 화면이 들어오면 이 컴포넌트를
/// 그쪽으로 옮기면 됩니다. 팝업이 아니라 별도 컴포넌트로 둔 이유가 이것입니다.
///
/// 고른 언어는 다음에 감상 화면에 들어갈 때부터 반영됩니다. 대본은 화면에 들어가는 시점에
/// 한 번 읽고(StoryScriptLoader) 이 팝업은 감상 화면에 얹히지 않으므로, 읽는 도중 바뀌는 일은 없습니다.
/// </summary>
public class UI_LanguageSelector : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _koreanButton;

    [SerializeField]
    private Button _japaneseButton;

    [Header("고른 쪽을 밝게 두는 데 쓰는 배경")]
    [SerializeField]
    private Image _koreanBackground;

    [SerializeField]
    private Image _japaneseBackground;

    [Foldout("Settings")]
    /// <summary>
    /// 고르지 않은 쪽을 흐리게 두는 색입니다. 그림을 두 벌 만들지 않고 밝기만 낮춥니다.
    /// 버튼에 언어 이름이 글자로 들어가 있어 색만으로도 어느 쪽인지 읽힙니다.
    /// </summary>
    [SerializeField]
    private Color _selectedColor = Color.white;

    [SerializeField]
    private Color _unselectedColor = new Color(1f, 1f, 1f, 0.45f);

    private void Awake()
    {
        if (_koreanButton != null)
        {
            _koreanButton.onClick.AddListener(() => Select(ELanguage.KOREAN));
        }

        if (_japaneseButton != null)
        {
            _japaneseButton.onClick.AddListener(() => Select(ELanguage.JAPANESE));
        }
    }

    // 팝업은 닫아도 오브젝트가 남습니다. 열 때마다 지금 언어를 다시 비춰야 합니다.
    private void OnEnable()
    {
        Refresh();
    }

    private void Select(ELanguage language)
    {
        LanguageSetting.Set(language);
        Refresh();
    }

    private void Refresh()
    {
        bool isKorean = LanguageSetting.Current == ELanguage.KOREAN;

        SetColor(_koreanBackground, isKorean);
        SetColor(_japaneseBackground, !isKorean);

        // 보고 있는 언어를 다시 누르는 것은 아무 일도 하지 않으므로 눌리지 않게 둡니다.
        SetInteractable(_koreanButton, !isKorean);
        SetInteractable(_japaneseButton, isKorean);
    }

    private void SetColor(Image target, bool isSelected)
    {
        if (target == null)
        {
            return;
        }

        target.color = isSelected ? _selectedColor : _unselectedColor;
    }

    private static void SetInteractable(Button target, bool interactable)
    {
        if (target == null)
        {
            return;
        }

        target.interactable = interactable;
    }
}
