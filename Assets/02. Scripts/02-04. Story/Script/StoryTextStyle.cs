using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 대사 문구 한 벌의 글꼴·크기·색입니다. 대본의 TextStyleId 하나에 대응합니다(연출표의 [디자인] 열).
///
/// 대본이 "명조체 24pt 밝은 회색" 같은 값을 직접 들고 있지 않은 이유는, 같은 조합이 수십 줄에 반복되기 때문입니다.
/// 색 하나를 바꿀 때 대본 전체가 아니라 이 표만 고치면 됩니다.
/// </summary>
[Serializable]
public class StoryTextStyle
{
    /// <summary>
    /// 비워 두면 대사 상자의 기본 글꼴로 돌아갑니다. 명조·고딕처럼 글꼴이 바뀌는 줄에만 채웁니다.
    /// "앞 줄 글꼴 유지"가 아니라 "기본값 복귀"인 이유는, 유지로 두면 명조 한 줄 뒤의
    /// 모든 기본 줄이 명조로 나오기 때문입니다.
    /// </summary>
    [SerializeField]
    private TMP_FontAsset _font;

    [SerializeField]
    private float _fontSize = 24f;

    [SerializeField]
    private Color _color = Color.white;

    /// <summary>
    /// 굵게·기울임 같은 강조입니다. 연출표의 "굵은 고딕"이 여기에 해당합니다.
    /// </summary>
    [SerializeField]
    private FontStyles _fontStyle = FontStyles.Normal;

    /// <summary>
    /// 줄이 바뀔 때마다 호출됩니다. 네 값을 모두 덮어쓰는 이유는, 앞 줄의 스타일이 남지 않게 하기 위해서입니다.
    /// 일부만 대입하면 붉은 40pt 다음의 기본 줄이 붉은색을 물려받습니다.
    /// </summary>
    public void ApplyTo(TMP_Text target, TMP_FontAsset fallbackFont)
    {
        target.font = _font != null ? _font : fallbackFont;
        target.fontSize = _fontSize;
        target.color = _color;
        target.fontStyle = _fontStyle;
    }
}
