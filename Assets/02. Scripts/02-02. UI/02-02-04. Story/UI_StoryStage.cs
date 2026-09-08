using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 감상 화면의 배경과 캐릭터를 갈아 끼웁니다(기획서 6.2).
///
/// 전환 방식과 시간은 기획서가 정하지 않았습니다(16-5는 "대사 데이터에 맞게 변경된다"까지만 요구).
/// 인물의 등장·퇴장만 페이드로 두고 배경은 즉시 교체입니다. 배경은 컷이 바뀌는 자리라
/// 서서히 겹치면 두 장소가 한동안 포개어져 보입니다.
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

    /// <summary>
    /// 인물이 나타나고 사라지는 데 걸리는 시간입니다. 대사를 읽는 흐름을 끊지 않도록 짧게 둡니다.
    /// 0으로 두면 예전처럼 즉시 켜지고 꺼집니다.
    /// </summary>
    [Foldout("Settings")]
    [SerializeField]
    [Min(0f)]
    private float _characterFadeSeconds = 0.25f;

    /// <summary>
    /// 자리마다 등장·퇴장 상태를 따로 들고 있어야 해서 슬롯별로 하나씩 둡니다.
    /// </summary>
    private readonly Dictionary<EStoryCharacterSlot, StoryStandingSlot> _standingSlots =
        new Dictionary<EStoryCharacterSlot, StoryStandingSlot>();

    private void Awake()
    {
        _standingSlots[EStoryCharacterSlot.LEFT] = new StoryStandingSlot(_leftCharacter);
        _standingSlots[EStoryCharacterSlot.CENTER] = new StoryStandingSlot(_centerCharacter);
        _standingSlots[EStoryCharacterSlot.RIGHT] = new StoryStandingSlot(_rightCharacter);
        _standingSlots[EStoryCharacterSlot.UPPER] = new StoryStandingSlot(_upperCharacter);
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<EStoryCharacterSlot, StoryStandingSlot> pair in _standingSlots)
        {
            pair.Value.Dispose();
        }
    }

    public void SetBackground(string backgroundId)
    {
        Sprite sprite = _visualBinder.GetBackground(backgroundId);

        _background.sprite = sprite;

        // 스프라이트가 없으면 단색으로 떨어집니다. 흰색으로 두면 화면이 하얗게 날아갑니다.
        _background.color = sprite == null ? _visualBinder.BackgroundPlaceholderColor : Color.white;

        StoryBackgroundFit.Cover(_background, sprite);
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
        SetCharacter(EStoryCharacterSlot.LEFT, line.CharacterId, line.ExpressionId, line.Transition);
        SetCharacter(EStoryCharacterSlot.CENTER, line.CenterCharacterId, line.CenterExpressionId, line.CenterTransition);
        SetCharacter(EStoryCharacterSlot.RIGHT, line.RightCharacterId, line.RightExpressionId, line.RightTransition);
        SetCharacter(EStoryCharacterSlot.UPPER, line.UpperCharacterId, line.UpperExpressionId, line.UpperTransition);
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

    private void SetCharacter(EStoryCharacterSlot slot, string characterId, string expressionId,
        EStoryCharacterTransition transition)
    {
        Image target = ResolveSlot(slot);
        StoryStandingSlot standing = _standingSlots[slot];
        CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();

        if (string.IsNullOrEmpty(characterId))
        {
            standing.Exit(transition, _characterFadeSeconds, cancellationToken);
            return;
        }

        // 그림과 자리를 먼저 잡습니다. 등장 연출이 제자리를 기준으로 움직이므로,
        // 자리가 정해지기 전에 시작하면 앞 인물의 자리에서 들어옵니다.
        Sprite sprite = _visualBinder.GetCharacter(characterId, expressionId);

        target.sprite = sprite;

        // 아직 초상 자산이 없어 대부분 여기로 옵니다. 캐릭터마다 색을 달리해 화자 교체가 눈에 보이게 합니다.
        // 알파는 등장·퇴장이 정합니다. 여기서 통째로 덮으면 페이드 중인 인물이 갑자기 다 보입니다.
        Color tint = sprite == null ? _visualBinder.GetCharacterPlaceholderColor(characterId) : Color.white;
        tint.a = target.color.a;
        target.color = tint;

        if (_layoutTable != null)
        {
            _layoutTable.Apply(target, slot, characterId);
        }

        // 같은 인물이 이어서 서 있는 줄입니다. 다시 등장 연출을 걸면 줄을 넘길 때마다 깜빡입니다.
        if (standing.Holds(characterId))
        {
            return;
        }

        if (target.gameObject.activeSelf)
        {
            standing.Replace(characterId);
            return;
        }

        standing.Enter(characterId, transition, _characterFadeSeconds, cancellationToken);
    }
}
