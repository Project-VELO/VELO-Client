using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using VInspector;

/// <summary>
/// 홈 캐릭터를 누르면 웃는 모션을 한 번 틀고 Idle로 돌아갑니다(개발 기획서 2장 H06, 피그마 0. 홈화면 H06).
///
/// 웃는 모션이 도는 동안의 재클릭은 막습니다("해당 애니메이션 지속 시간동안 재클릭 불가", 검수 1 답).
/// 잠금 플래그를 따로 두지 않고 트랙 0에 지금 도는 애니메이션으로 판단합니다. 인물을 바꾸면 Spine이
/// 상태를 새로 만들어 Idle부터 시작하므로, 플래그를 두면 교체 순간에 잠긴 채 남을 수 있기 때문입니다.
///
/// 클릭 범위는 SkeletonGraphic의 RectTransform 사각형입니다(검수 1 답). 대사 넘김은 H07이 따로 맡습니다.
/// </summary>
public class UI_HomeCharacterReaction : MonoBehaviour, IPointerClickHandler
{
    private const int REACTION_TRACK = 0;

    [Foldout("Hierarchy")]
    [SerializeField]
    private SkeletonGraphic _skeletonGraphic;

    [SerializeField]
    private UI_HomeCharacter _homeCharacter;

    public void OnPointerClick(PointerEventData eventData)
    {
        // 팝업이 열려 있는 동안 뒤 화면 입력은 막습니다(개발 기획서 3장 "팝업 열린 상태: 뒤 메뉴 입력 차단").
        if (UIManager.Instance != null && UIManager.Instance.HasPopups)
        {
            return;
        }

        PlayReaction();
    }

    private void PlayReaction()
    {
        HomeCharacterEntry entry = _homeCharacter.CurrentEntry;

        if (ReferenceEquals(entry, null) || string.IsNullOrEmpty(entry.ReactionAnimationName))
        {
            return;
        }

        Spine.AnimationState state = _skeletonGraphic.AnimationState;

        if (ReferenceEquals(state, null))
        {
            return;
        }

        if (ReferenceEquals(_skeletonGraphic.SkeletonData.FindAnimation(entry.ReactionAnimationName), null))
        {
            Debug.LogWarning($"[UI_HomeCharacterReaction] {entry.CharacterId}에 웃는 모션 {entry.ReactionAnimationName}이 없습니다.");
            return;
        }

        if (IsReactionPlaying(state.GetCurrent(REACTION_TRACK), entry.ReactionAnimationName))
        {
            return;
        }

        // 지속 시간은 애니메이션 파일 길이 그대로입니다(피그마 H06 "애니메이션 파일 시간").
        // 지연 0으로 이어 붙이면 Spine이 앞 모션 길이만큼 기다렸다가 Idle을 반복합니다.
        state.SetAnimation(REACTION_TRACK, entry.ReactionAnimationName, false);
        state.AddAnimation(REACTION_TRACK, UI_HomeCharacter.IDLE_ANIMATION_NAME, true, 0f);
    }

    /// <summary>
    /// Spine은 Idle을 기본 믹스 시간(0.2초)만큼 앞당겨 시작하고 그동안 웃는 모션을 섞어 뺍니다.
    /// 현재 항목만 보면 그 0.2초 동안 잠금이 풀리므로, 섞여 빠지는 쪽(MixingFrom)까지 봅니다.
    /// </summary>
    private static bool IsReactionPlaying(TrackEntry current, string reactionName)
    {
        if (ReferenceEquals(current, null))
        {
            return false;
        }

        if (current.Animation.Name == reactionName)
        {
            return true;
        }

        TrackEntry mixingFrom = current.MixingFrom;
        return !ReferenceEquals(mixingFrom, null) && mixingFrom.Animation.Name == reactionName;
    }
}
