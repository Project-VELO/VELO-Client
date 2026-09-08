using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 배경 그림을 화면 틀에 채워 넣는 크기를 잽니다.
///
/// 무대(UI_StoryStage)에서 떼어 낸 이유는 바뀌는 이유가 다르기 때문입니다. 여기는 비율이 다른
/// 배경이 새로 들어올 때만 손대는 계산이고, 무대는 인물과 슬롯이 늘 때마다 바뀝니다.
/// </summary>
public static class StoryBackgroundFit
{
    /// <summary>
    /// 원본 비율 그대로 화면을 덮습니다. 짧은 쪽을 화면에 맞추고 긴 쪽은 넘겨 잘라 냅니다.
    ///
    /// 배경이 전부 16:9는 아닙니다. 4화의 4-A는 4:3이고 10화의 10-C는 세로로 긴 그림입니다.
    /// 화면 틀에 그대로 늘리면 인물이 옆으로 퍼지므로 비율은 반드시 지켜야 합니다.
    ///
    /// 그림 전체가 보이도록 틀 안에 담아 본 적이 있는데, 그러면 비율이 다른 배경에서
    /// 테두리 바깥의 빈 자리가 드러났습니다. 특히 시점 이동과 흔들림이 그 자리를 화면 안으로
    /// 끌고 들어옵니다. 잘리더라도 빈 자리를 보이지 않는 쪽을 택했습니다.
    ///
    /// 잘리는 것이 아까우면 배경을 16:9로 다시 뽑는 것이 답입니다. 40장 중 36장은 이미 16:9라
    /// 이 계산에 걸리지 않습니다.
    /// </summary>
    public static void Cover(Image background, Sprite sprite)
    {
        RectTransform rect = background.rectTransform;

        if (sprite == null || !(rect.parent is RectTransform frame))
        {
            return;
        }

        Vector2 frameSize = frame.rect.size;

        if (frameSize.x <= 0f || frameSize.y <= 0f || sprite.rect.height <= 0f)
        {
            return;
        }

        float spriteAspect = sprite.rect.width / sprite.rect.height;
        float frameAspect = frameSize.x / frameSize.y;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        rect.sizeDelta = spriteAspect < frameAspect
            ? new Vector2(frameSize.x, frameSize.x / spriteAspect)
            : new Vector2(frameSize.y * spriteAspect, frameSize.y);
    }
}
