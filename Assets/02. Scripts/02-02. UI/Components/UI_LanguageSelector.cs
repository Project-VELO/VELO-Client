using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 한국어·일본어·영어를 고르는 세 버튼입니다.
///
/// 설정 화면이 아직 없어 종료 확인 팝업에 얹어 둡니다. 설정 화면이 들어오면 이 컴포넌트를
/// 그쪽으로 옮기면 됩니다. 팝업이 아니라 별도 컴포넌트로 둔 이유가 이것입니다.
///
/// 누르는 순간 이 팝업과 뒤에 깔린 화면이 함께 바뀝니다(LanguageChangeRefresher).
/// 대본만은 다음에 감상 화면에 들어갈 때부터입니다. 대본은 화면에 들어가는 시점에 한 번
/// 읽고(StoryScriptLoader) 이 팝업은 감상 화면에 얹히지 않으므로, 읽는 도중 바뀌는 일은 없습니다.
///
/// 언어마다 버튼과 배경을 한 쌍씩 두는 대신 배열로 묶지 않은 것은, 프리팹에서 어느 칸이 어느 언어인지
/// 인스펙터 이름만으로 읽히게 하기 위해서입니다. 언어가 더 늘면 그때 배열로 옮깁니다.
/// </summary>
public class UI_LanguageSelector : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _koreanButton;

    [SerializeField]
    private Button _japaneseButton;

    [SerializeField]
    private Button _englishButton;

    [Header("고른 쪽을 밝게 두는 데 쓰는 배경")]
    [SerializeField]
    private Image _koreanBackground;

    [SerializeField]
    private Image _japaneseBackground;

    [SerializeField]
    private Image _englishBackground;

    [Foldout("Settings")]
    /// <summary>
    /// 고르지 않은 쪽을 흐리게 두는 색입니다. 그림을 세 벌 만들지 않고 밝기만 낮춥니다.
    /// 버튼에 언어 이름이 글자로 들어가 있어 색만으로도 어느 쪽인지 읽힙니다.
    /// </summary>
    [SerializeField]
    private Color _selectedColor = Color.white;

    [SerializeField]
    private Color _unselectedColor = new Color(1f, 1f, 1f, 0.45f);

    private void Awake()
    {
        AddListener(_koreanButton, ELanguage.KOREAN);
        AddListener(_japaneseButton, ELanguage.JAPANESE);
        AddListener(_englishButton, ELanguage.ENGLISH);
    }

    // 팝업은 닫아도 오브젝트가 남습니다. 열 때마다 지금 언어를 다시 비춰야 합니다.
    private void OnEnable()
    {
        Refresh();
    }

    private void AddListener(Button target, ELanguage language)
    {
        if (target == null)
        {
            return;
        }

        target.onClick.AddListener(() => Select(language));
    }

    private void Select(ELanguage language)
    {
        LanguageSetting.Set(language);
        Refresh();
    }

    private void Refresh()
    {
        ELanguage current = LanguageSetting.Current;

        RefreshEntry(_koreanButton, _koreanBackground, current == ELanguage.KOREAN);
        RefreshEntry(_japaneseButton, _japaneseBackground, current == ELanguage.JAPANESE);
        RefreshEntry(_englishButton, _englishBackground, current == ELanguage.ENGLISH);
    }

    /// <summary>
    /// 보고 있는 언어를 다시 누르는 것은 아무 일도 하지 않으므로 눌리지 않게 둡니다.
    /// </summary>
    private void RefreshEntry(Button button, Image background, bool isSelected)
    {
        if (background != null)
        {
            background.color = isSelected ? _selectedColor : _unselectedColor;
        }

        if (button != null)
        {
            button.interactable = !isSelected;
        }
    }
}
