using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 감상 화면의 인물을 자기 자리에 세웁니다.
///
/// 인물마다 그림의 크기와 구도가 달라 자리를 프리팹에 한 벌로 굳혀 둘 수 없습니다.
/// 어느 인물이 오느냐에 따라 폭과 화면 끝에서 띄우는 거리가 달라지므로 표로 두고 갈아 끼웁니다.
///
/// 세우는 규칙 자체는 여기에만 있습니다. UI_StoryStage는 무엇을 세울지만 정하고
/// 어디에 어떻게 세울지는 넘기는 편이, 그림이 바뀔 때 고칠 자리를 한 곳으로 모읍니다.
/// </summary>
public class StoryCharacterLayoutTable : MonoBehaviour
{
    [Foldout("Settings")]
    [SerializeField]
    private SerializableDictionary<string, StoryCharacterLayout> _layouts =
        new SerializableDictionary<string, StoryCharacterLayout>();

    /// <summary>
    /// 표에 없는 인물이 떨어지는 값입니다. 화면 높이에 맞춰 화면 끝에 붙습니다.
    /// </summary>
    [SerializeField]
    private StoryCharacterLayout _defaultLayout = new StoryCharacterLayout();

    /// <summary>
    /// 인물을 자기 자리에 세웁니다.
    ///
    /// 그림이 없으면(임시 색으로 떨어진 경우) 크기를 건드리지 않습니다.
    /// 원본 비율을 알 수 없어 계산하면 납작하거나 길쭉한 사각형이 나옵니다.
    /// </summary>
    public void Apply(Image target, EStoryCharacterSlot slot, string characterId)
    {
        if (target == null || target.sprite == null)
        {
            return;
        }

        StoryCharacterLayout layout = GetLayout(characterId);
        Rect spriteRect = target.sprite.rect;

        if (spriteRect.width <= 0f || spriteRect.height <= 0f)
        {
            return;
        }

        RectTransform rect = target.rectTransform;
        float height = ResolveHeight(layout, rect, spriteRect);
        float width = height * spriteRect.width / spriteRect.height;

        rect.sizeDelta = new Vector2(width, height);

        ApplyAnchor(rect, slot, layout.EdgeOffset);
    }

    /// <summary>
    /// 세울 높이를 정합니다. 표에 값이 없으면 감상 화면의 높이를 그대로 씁니다.
    ///
    /// 화면 높이를 기본값으로 두면 새 그림이 어떤 크기로 들어와도 일단 화면 안에 담깁니다.
    /// 원본 크기를 기본값으로 두면 2000x3000 같은 그림이 그대로 들어가 다리만 보입니다.
    /// </summary>
    private static float ResolveHeight(StoryCharacterLayout layout, RectTransform rect, Rect spriteRect)
    {
        if (0 < layout.Height)
        {
            return layout.Height;
        }

        if (rect.parent is RectTransform frame && 0f < frame.rect.height)
        {
            return frame.rect.height;
        }

        return spriteRect.height;
    }

    private StoryCharacterLayout GetLayout(string characterId)
    {
        if (!string.IsNullOrEmpty(characterId)
            && _layouts.TryGetValue(characterId, out StoryCharacterLayout layout))
        {
            return layout;
        }

        return _defaultLayout;
    }

    /// <summary>
    /// 바닥을 딛는 세 자리는 화면 아래에 딱 붙이고, 좌우 자리는 자기 쪽 끝에서 띄웁니다.
    ///
    /// UPPER는 건드리지 않습니다. 천장에 걸터앉은 인물을 위한 자리라 바닥에 붙이면
    /// 그 자리를 둔 이유가 사라집니다(EStoryCharacterSlot.UPPER).
    /// </summary>
    private void ApplyAnchor(RectTransform rect, EStoryCharacterSlot slot, int edgeOffset)
    {
        if (slot == EStoryCharacterSlot.UPPER)
        {
            return;
        }

        Vector2 anchor = GetBottomAnchor(slot);

        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = new Vector2(GetOffsetX(slot, edgeOffset), 0f);
    }

    /// <summary>
    /// 좌·우 자리는 기준점을 자기 쪽 끝에 둡니다. 그래야 폭이 바뀌어도 붙은 쪽이 움직이지 않습니다.
    /// </summary>
    private Vector2 GetBottomAnchor(EStoryCharacterSlot slot)
    {
        switch (slot)
        {
            case EStoryCharacterSlot.LEFT:
                return new Vector2(0f, 0f);

            case EStoryCharacterSlot.RIGHT:
                return new Vector2(1f, 0f);

            default:
                return new Vector2(0.5f, 0f);
        }
    }

    private float GetOffsetX(EStoryCharacterSlot slot, int edgeOffset)
    {
        switch (slot)
        {
            case EStoryCharacterSlot.LEFT:
                return edgeOffset;

            case EStoryCharacterSlot.RIGHT:
                return -edgeOffset;

            default:
                return 0f;
        }
    }
}
