using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using VInspector;

/// <summary>
/// 홈 캐릭터를 누르면 웃는 모션을 한 번 틀고 Idle로 돌아갑니다(개발 기획서 2장 H06, 피그마 0. 홈화면 H06).
///
/// 웃는 모션이 도는 동안 다시 누르면 처음부터 다시 틉니다. 원문 글은 "지속 시간동안 재클릭 불가"였지만
/// 회의에서 재클릭 가능으로 바뀌었습니다(검수 2 재작업, 피그마 댓글 11/22 기획자 "재클릭 가능"과 같음).
/// 연타하면 모션이 계속 처음부터 다시 돌고, 마지막 클릭 뒤 클립 길이가 지나면 Idle로 돌아옵니다.
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

        // SetAnimation은 트랙에 쌓인 다음 항목(앞 클릭이 이어 붙인 Idle)을 비우고 새 항목을 0초부터 틉니다.
        // 그래서 재생 중에 눌러도 모션이 처음부터 다시 시작하고, Idle은 이번 클릭 기준으로 다시 이어 붙습니다.
        // 지속 시간은 애니메이션 파일 길이 그대로입니다(피그마 H06 "애니메이션 파일 시간").
        state.SetAnimation(REACTION_TRACK, entry.ReactionAnimationName, false);
        state.AddAnimation(REACTION_TRACK, UI_HomeCharacter.IDLE_ANIMATION_NAME, true, 0f);
    }
}
