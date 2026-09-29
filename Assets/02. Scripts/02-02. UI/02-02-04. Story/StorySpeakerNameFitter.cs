using TMPro;
using UnityEngine;

/// <summary>
/// 화자명이 박스보다 길 때만 글자를 줄입니다. 짧은 이름은 기본 크기 그대로입니다.
///
/// 영어 화자명(Station Director, Spirit Audience)이 한국어보다 길어 박스를 넘칩니다.
/// TMP의 자동 크기는 줄바꿈을 끈 상태에서 가로로 넘치는 것을 보지 않으므로 폭을 직접 재서 비율로 줄입니다.
/// 줄바꿈으로 두 줄을 만드는 대신 줄이는 것은, 이름표 높이가 한 줄 기준이기 때문입니다.
///
/// 대사 상자(UI_StoryDialogBox)에서 떼어낸 것은 그쪽이 한 줄분의 표시를 갈아 끼우는 일을 맡고,
/// 글자 폭을 재는 계산은 그 흐름과 따로 바뀌기 때문입니다.
/// </summary>
public static class StorySpeakerNameFitter
{
    /// <summary>
    /// 글자가 이미 들어간 뒤에 부릅니다. preferredWidth는 지금 든 글자로 잽니다.
    /// </summary>
    public static void Fit(TMP_Text target, float defaultFontSize, float minFontSize, float padding)
    {
        target.fontSize = defaultFontSize;

        float available = target.rectTransform.rect.width - padding * 2f;
        float preferred = target.preferredWidth;

        if (available <= 0f || preferred <= available)
        {
            return;
        }

        target.fontSize = Mathf.Max(minFontSize, defaultFontSize * available / preferred);
    }
}
