using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 글자가 그림에 구워진 버튼을 언어에 따라 갈아 끼웁니다.
///
/// 홈 내비게이션 버튼은 배경 사진과 아이콘과 화살표와 글자가 한 장에 들어 있습니다.
/// 통째로 글자로 바꾸면 그림이 사라지므로, 한글만 지운 판을 따로 두고 그 위에 글자를 얹습니다.
///
/// 한국어에서는 원본을 그대로 씁니다. 원본이 디자인의 기준이고, 글자 위치와 자간이
/// 그림에 맞춰 다듬어져 있어 글자로 다시 그리면 미묘하게 달라지기 때문입니다.
/// 바꿔야 하는 쪽만 바꿉니다.
/// </summary>
public class UI_LocalizedNavButton : MonoBehaviour, ILanguageRefreshable
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _background;

    /// <summary>한글을 지운 판 위에 얹을 글자입니다. 한국어에서는 꺼 둡니다.</summary>
    [SerializeField]
    private TMP_Text _label;

    [Foldout("Project")]
    [SerializeField]
    private Sprite _koreanSprite;

    [SerializeField]
    private Sprite _localizedSprite;

    [Foldout("Settings")]
    /// <summary>한국어 원문입니다. UiText가 지금 언어로 바꿔 줍니다.</summary>
    [SerializeField]
    private string _korean = string.Empty;

    /// <summary>
    /// 켤 때마다 맞춥니다. 홈 화면은 씬을 다시 읽지 않고 켜고 끄기만 하므로,
    /// 팝업에서 언어를 바꾸고 돌아왔을 때 여기서 따라잡습니다.
    /// </summary>
    private void OnEnable()
    {
        Refresh();
    }

    /// <summary>
    /// 언어를 고른 자리에서 부릅니다. 바꿔야 할 것이 글자가 아니라 판이라 훑기로는 바뀌지 않고,
    /// 홈 화면은 팝업 뒤에 켜진 채로 남아 있어 OnEnable도 오지 않습니다.
    /// </summary>
    public void RefreshLanguage()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (_background == null)
        {
            return;
        }

        bool isKorean = LanguageSetting.Current == ELanguage.KOREAN;
        Sprite sprite = isKorean ? _koreanSprite : _localizedSprite;

        if (sprite != null)
        {
            _background.sprite = sprite;
        }

        if (_label == null)
        {
            return;
        }

        _label.gameObject.SetActive(!isKorean);

        if (!isKorean)
        {
            _label.text = UiText.Localize(_korean);
        }
    }
}
