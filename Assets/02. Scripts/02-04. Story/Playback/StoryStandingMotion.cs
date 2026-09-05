using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 인물이 자리에 드나드는 움직임을 그립니다(EStoryCharacterTransition).
///
/// 자리를 지키는 일(StoryStandingSlot)과 나눈 이유는 바뀌는 이유가 다르기 때문입니다.
/// 방식이 하나 늘 때마다 자리 관리 코드를 건드리면 캐리오버 처리가 함께 흔들립니다.
/// </summary>
public static class StoryStandingMotion
{
    /// <summary>
    /// 미끄러져 들어올 때 자기 자리에서 얼마나 바깥에서 출발하는지입니다.
    /// 화면 밖까지 빼지 않는 것은, 좌우 자리가 이미 화면 끝에 붙어 있어 조금만 밀어도 들어오는 맛이 나기 때문입니다.
    /// </summary>
    private const float SLIDE_DISTANCE = 240f;

    /// <summary>
    /// 위에서 내려올 때의 높이입니다. 인물 키의 3분의 1쯤이라 천장에서 내려오는 것처럼 보입니다.
    /// </summary>
    private const float DROP_HEIGHT = 320f;

    /// <summary>
    /// 튀어나올 때 잠깐 커지는 배율입니다. 1을 넘겨야 튕기는 인상이 납니다.
    /// </summary>
    private const float POPUP_OVERSHOOT = 1.06f;

    /// <summary>
    /// 이 방식이 시간을 들여 그리는 움직임인지입니다. 즉시 나타나는 방식은 기다릴 것이 없습니다.
    /// </summary>
    public static bool IsInstant(EStoryCharacterTransition transition)
    {
        return transition == EStoryCharacterTransition.CUT;
    }

    /// <summary>
    /// 등장을 그립니다. 시작 상태를 먼저 잡고 제자리로 돌아옵니다.
    ///
    /// 시작 상태를 여기서 잡는 것은, 첫 프레임에 제자리 그림이 한 번 스쳐 보이지 않게 하려는 것입니다.
    /// </summary>
    public static UniTask EnterAsync(RectTransform rect, EStoryCharacterTransition transition,
        Vector2 home, float seconds, CancellationToken cancellationToken)
    {
        Vector2 from = home + GetOffset(rect, transition);

        return StoryEffectTween.LerpAsync(seconds, progress =>
        {
            rect.anchoredPosition = Vector2.Lerp(from, home, progress);
            rect.localScale = Vector3.one * GetEnterScale(transition, progress);
        }, cancellationToken);
    }

    /// <summary>
    /// 퇴장을 그립니다. 들어온 방향으로 되돌아 나갑니다.
    /// </summary>
    public static UniTask ExitAsync(RectTransform rect, EStoryCharacterTransition transition,
        Vector2 home, float seconds, CancellationToken cancellationToken)
    {
        Vector2 to = home + GetOffset(rect, transition);

        return StoryEffectTween.LerpAsync(seconds, progress =>
        {
            rect.anchoredPosition = Vector2.Lerp(home, to, progress);
            rect.localScale = Vector3.one * Mathf.Lerp(1f, GetExitScale(transition), progress);
        }, cancellationToken);
    }

    /// <summary>
    /// 등장이 시작하는 자리입니다. 제자리를 기준으로 얼마나 떨어져 있는지를 돌려줍니다.
    ///
    /// 미끄러짐은 자기가 선 쪽 바깥에서 옵니다. 왼쪽 인물이 오른쪽에서 들어오면
    /// 화면을 가로질러 지나가는 그림이 되어 자리의 뜻이 사라집니다.
    /// </summary>
    private static Vector2 GetOffset(RectTransform rect, EStoryCharacterTransition transition)
    {
        switch (transition)
        {
            case EStoryCharacterTransition.SLIDE:
                return new Vector2(rect.pivot.x < 0.5f ? -SLIDE_DISTANCE : SLIDE_DISTANCE, 0f);

            case EStoryCharacterTransition.DROP:
                return new Vector2(0f, DROP_HEIGHT);

            default:
                return Vector2.zero;
        }
    }

    /// <summary>
    /// 등장 중의 배율입니다. 튀어나오는 방식만 작게 시작해 살짝 넘겼다가 제자리로 옵니다.
    /// </summary>
    private static float GetEnterScale(EStoryCharacterTransition transition, float progress)
    {
        if (transition != EStoryCharacterTransition.POPUP)
        {
            return 1f;
        }

        // 앞 절반은 커지고 뒤 절반은 제자리로 돌아옵니다. 한 번에 1로 가면 튕기는 맛이 없습니다.
        return progress < 0.5f
            ? Mathf.Lerp(0.88f, POPUP_OVERSHOOT, progress * 2f)
            : Mathf.Lerp(POPUP_OVERSHOOT, 1f, (progress - 0.5f) * 2f);
    }

    private static float GetExitScale(EStoryCharacterTransition transition)
    {
        return transition == EStoryCharacterTransition.POPUP ? 0.88f : 1f;
    }
}
