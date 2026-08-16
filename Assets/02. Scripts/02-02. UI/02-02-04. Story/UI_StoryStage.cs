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

    public void SetBackground(string backgroundId)
    {
        Sprite sprite = _visualBinder.GetBackground(backgroundId);

        _background.sprite = sprite;

        // 스프라이트가 없으면 단색으로 떨어집니다. 흰색으로 두면 화면이 하얗게 날아갑니다.
        _background.color = sprite == null ? _visualBinder.BackgroundPlaceholderColor : Color.white;
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
    }
}
