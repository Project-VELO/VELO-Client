using Spine.Unity;
using UnityEngine;

/// <summary>
/// 미리보기 칸(P07) 안에 인물 전신이 들어가도록 SkeletonGraphic의 크기와 위치를 맞춥니다.
///
/// 인물마다 Spine 내보내기 배율과 몸 크기가 달라(리아 1058x2804, 지우 762x2527 등) 고정 배율로는
/// 어떤 인물은 칸을 넘치고 어떤 인물은 작게 보입니다. 그래서 인물을 바꿀 때마다 메시 경계로 다시 잽니다.
/// 미리보기 칸이 SkeletonGraphic의 부모이고, SkeletonGraphic의 기준점은 부모 가운데라고 가정합니다.
/// </summary>
public static class UI_SkeletonPreviewFitter
{
    private static readonly Vector2 CENTER = new Vector2(0.5f, 0.5f);

    public static void Fit(SkeletonGraphic graphic)
    {
        RectTransform box = graphic.rectTransform.parent as RectTransform;

        if (box == null || !graphic.MatchRectTransformWithBounds())
        {
            return;
        }

        RectTransform rect = graphic.rectTransform;
        Vector2 size = rect.rect.size;

        if (size.x <= 0f || size.y <= 0f)
        {
            return;
        }

        float scale = Mathf.Min(box.rect.width / size.x, box.rect.height / size.y);

        rect.localScale = Vector3.one * scale;
        rect.anchoredPosition = (rect.pivot - CENTER) * size * scale;
    }
}
