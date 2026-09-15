using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 글자가 구워진 그림을 언어에 따라 통째로 갈아 끼웁니다.
///
/// 스토리 선택 화면의 배경처럼 머리글이 배경 그림에 박혀 있어 글자만 떼어낼 수 없는 곳에 씁니다.
/// 언어마다 따로 그린 판을 두고 바꿉니다. 글자를 지운 판 위에 글자를 얹는 UI_LocalizedNavButton과
/// 달리 글자를 얹지 않으므로, 판마다 글자 위치와 자간을 디자인 그대로 지킬 수 있습니다.
///
/// 일본어가 아니면 한국어 판을 씁니다. 번역이 없는 글자가 한국어 원문으로 남는 UiText와 같은 규칙입니다.
/// </summary>
public class UI_LocalizedImage : MonoBehaviour, ILanguageRefreshable
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _image;

    [Foldout("Project")]
    [SerializeField]
    private Sprite _koreanSprite;

    [SerializeField]
    private Sprite _japaneseSprite;

    /// <summary>
    /// 켤 때마다 맞춥니다. 다른 화면에서 언어를 바꾸고 이 화면에 들어오는 경우입니다.
    /// </summary>
    private void OnEnable()
    {
        Refresh();
    }

    /// <summary>
    /// 이 화면이 떠 있는 채로 설정 팝업에서 언어를 바꾸면 여기로 옵니다.
    /// 바꿀 것이 글자가 아니라 그림이라 UiTextLocalizer의 훑기로는 바뀌지 않습니다.
    /// </summary>
    public void RefreshLanguage()
    {
        Refresh();
    }

    private void Refresh()
    {
        _image.sprite = LanguageSetting.Current == ELanguage.JAPANESE ? _japaneseSprite : _koreanSprite;
    }
}
