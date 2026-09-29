using UnityEngine;

/// <summary>
/// NEXT 입력이 지금 무엇이 되어야 하는지를 정합니다. 정하기만 하고 실행은 StoryProgressFlow가 합니다.
///
/// 흐름에서 떼어낸 이유는 이 판단이 상태·글자 시작 여부·마지막 글자 시각·현재 줄 네 가지를 함께 보는
/// 순수한 규칙이라, 상태를 바꾸는 코드와 섞여 있으면 어느 줄이 판단이고 어느 줄이 전이인지
/// 읽기 어렵기 때문입니다.
/// </summary>
public static class StoryNextDecider
{
    /// <summary>
    /// 대사가 다 나온 뒤 다음으로 넘길 수 있게 되기까지의 시간입니다.
    ///
    /// 마지막 글자가 찍히는 순간에 이미 눌러 둔 손가락이 그대로 다음 줄로 넘겨 버리면,
    /// 방금 나온 문장을 읽지 못한 채 화면이 바뀝니다. 읽을 틈을 두려고 잠깐 잠급니다.
    /// </summary>
    private const float NEXT_LOCK_SECONDS = 0.5f;

    /// <summary>
    /// 출력 중 → 즉시 전체 출력 / 출력 완료 → 다음 대사. PAUSED와 FINISHING에서는 아무 반응도 하지 않습니다(기획서 6.4, 3-L).
    /// </summary>
    public static EStoryNextAction Decide(EStoryPlaybackState state, bool hasTextStarted, float completedAt,
        StoryLineData current)
    {
        if (state != EStoryPlaybackState.TYPING && state != EStoryPlaybackState.WAITING_NEXT)
        {
            return EStoryNextAction.NONE;
        }

        // 대사가 아직 뜨지 않았습니다. 컷씬은 그림이 자리를 잡을 동안 글자를 늦춰 내는데,
        // 이때 누른 건너뛰기는 채울 글자가 없어 빈 화면만 남깁니다.
        if (state == EStoryPlaybackState.TYPING && !hasTextStarted)
        {
            return EStoryNextAction.NONE;
        }

        // 다 나온 지 얼마 되지 않았습니다. 마지막 글자와 같이 눌린 손가락이 그대로 넘기지 않게 잠급니다.
        if (state == EStoryPlaybackState.WAITING_NEXT && Time.unscaledTime - completedAt < NEXT_LOCK_SECONDS)
        {
            return EStoryNextAction.NONE;
        }

        // 글자가 없는 컷은 채울 것이 없어 누르는 즉시 넘어갑니다. 컷은 그림과 소리가 함께 흐르는
        // 한 덩어리라, 읽을 문장이 없으면 1단계를 두어 봐야 헛누름이 됩니다.
        //
        // 글자가 있는 컷은 보통 줄과 같이 두 번에 나눕니다. 한 번에 넘기면 읽던 문장이 잘려 나가고,
        // 컷 길이는 읽는 속도를 모르는 값이라 사람이 다 읽었는지를 대신 판단할 수 없습니다.
        if (StoryCutRunner.IsCut(current) && string.IsNullOrEmpty(current.Text))
        {
            return EStoryNextAction.MOVE_NEXT;
        }

        return state == EStoryPlaybackState.TYPING ? EStoryNextAction.FILL_TEXT : EStoryNextAction.MOVE_NEXT;
    }
}
