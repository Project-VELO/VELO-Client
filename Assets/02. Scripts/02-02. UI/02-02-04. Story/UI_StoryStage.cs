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
    private Image _rightCharacter;

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
    /// 화자를 세웁니다. 대본의 CharacterId가 한 줄에 하나뿐이라 왼쪽만 씁니다.
    /// characterId가 비어 있으면(지문) 아무도 세우지 않습니다.
    /// </summary>
    public void SetSpeaker(string characterId, string expressionId)
    {
        SetCharacter(EStoryCharacterSlot.LEFT, characterId, expressionId);
        SetCharacter(EStoryCharacterSlot.RIGHT, null, null);
    }

    private void SetCharacter(EStoryCharacterSlot slot, string characterId, string expressionId)
    {
        Image target = slot == EStoryCharacterSlot.LEFT ? _leftCharacter : _rightCharacter;

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
