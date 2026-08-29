using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 감상 화면의 배경과 캐릭터를 갈아 끼웁니다(기획서 6.2).
///
/// 전환 방식과 시간은 기획서가 정하지 않았습니다(16-5는 "대사 데이터에 맞게 변경된다"까지만 요구).
/// 지금은 즉시 교체지만, 페이드를 넣게 되어도 이 클래스 안에서만 바뀌도록 교체 지점을 모아 두었습니다.
/// </summary>
public class UI_StoryStage : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _background;

    [SerializeField]
    private Image _leftCharacter;

    [SerializeField]
    private Image _centerCharacter;

    [SerializeField]
    private Image _rightCharacter;

    [SerializeField]
    private Image _upperCharacter;

    [Foldout("Project")]
    [SerializeField]
    private StoryVisualBinder _visualBinder;

    /// <summary>
    /// 인물을 어느 크기로 어디에 세울지 정합니다. 인물마다 그림이 달라 자리를 프리팹에 굳혀 둘 수 없습니다.
    /// </summary>
    [SerializeField]
    private StoryCharacterLayoutTable _layoutTable;

    public void SetBackground(string backgroundId)
    {
        Sprite sprite = _visualBinder.GetBackground(backgroundId);

        _background.sprite = sprite;

        // 스프라이트가 없으면 단색으로 떨어집니다. 흰색으로 두면 화면이 하얗게 날아갑니다.
        _background.color = sprite == null ? _visualBinder.BackgroundPlaceholderColor : Color.white;

        ApplyBackgroundCover(sprite);
    }

    /// <summary>
    /// 배경을 원본 비율 그대로 화면에 채웁니다.
    ///
    /// 배경이 전부 16:9는 아닙니다. 세로로 긴 것도 있고 4:3도 있는데, 화면 틀에 그대로 늘리면
    /// 인물이 옆으로 퍼집니다. 그렇다고 틀 안에 넣으면 좌우에 검은 자리가 크게 남습니다.
    ///
    /// 그래서 짧은 쪽을 화면에 맞추고 긴 쪽이 넘치게 둡니다. 넘치는 부분은 시점 이동이 훑고
    /// 지나갈 여지가 되기도 합니다. 세로로 긴 그림을 위에서 아래로 내려다보는 컷이 그렇습니다.
    /// </summary>
    private void ApplyBackgroundCover(Sprite sprite)
    {
        RectTransform rect = _background.rectTransform;

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

    /// <summary>
    /// 이번 줄에 세울 인물들을 좌·중·우·상단 슬롯에 배치합니다.
    /// 비어 있는 쪽은 아무도 세우지 않습니다(지문이거나 1인 장면).
    ///
    /// 네 슬롯을 한 번에 받는 이유는, 한쪽만 갱신하는 경로를 두면 다인 장면에서
    /// 한 자리만 바뀌고 나머지가 앞 줄 상태로 남는 실수가 생기기 때문입니다.
    /// </summary>
    public void SetSpeakers(StoryLineData line)
    {
        SetCharacter(EStoryCharacterSlot.LEFT, line.CharacterId, line.ExpressionId);
        SetCharacter(EStoryCharacterSlot.CENTER, line.CenterCharacterId, line.CenterExpressionId);
        SetCharacter(EStoryCharacterSlot.RIGHT, line.RightCharacterId, line.RightExpressionId);
        SetCharacter(EStoryCharacterSlot.UPPER, line.UpperCharacterId, line.UpperExpressionId);
    }

    private Image ResolveSlot(EStoryCharacterSlot slot)
    {
        switch (slot)
        {
            case EStoryCharacterSlot.LEFT: return _leftCharacter;
            case EStoryCharacterSlot.CENTER: return _centerCharacter;
            case EStoryCharacterSlot.UPPER: return _upperCharacter;
            default: return _rightCharacter;
        }
    }

    private void SetCharacter(EStoryCharacterSlot slot, string characterId, string expressionId)
    {
        Image target = ResolveSlot(slot);

        if (string.IsNullOrEmpty(characterId))
        {
            target.gameObject.SetActive(false);
            return;
        }

        target.gameObject.SetActive(true);

        Sprite sprite = _visualBinder.GetCharacter(characterId, expressionId);
        target.sprite = sprite;

        // 아직 초상 자산이 없어 대부분 여기로 옵니다. 캐릭터마다 색을 달리해 화자 교체가 눈에 보이게 합니다.
        target.color = sprite == null ? _visualBinder.GetCharacterPlaceholderColor(characterId) : Color.white;

        if (_layoutTable != null)
        {
            _layoutTable.Apply(target, slot, characterId);
        }
    }
}
