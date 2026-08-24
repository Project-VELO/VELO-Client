/// <summary>
/// 리듬게임 레인 구성을 한곳에 모아 둔 상수 모음입니다.
/// 레인 번호는 1부터 시작하며(NoteData.Lane과 동일), 마지막 레인은 귀신 노트 전용입니다.
/// 트랙 렌더링·채보 편집·판정이 각자 6을 적어 두면 레인 수를 바꿀 때 한 곳만 놓쳐도 어긋나므로 여기로 모았습니다.
/// </summary>
public static class LiveLane
{
    public const int FIRST = 1;
    public const int COUNT = 6;
    public const int LAST = COUNT;

    /// <summary>
    /// 귀신 노트 전용 레인입니다. 이 레인의 노트를 놓치면 점수가 깎이고(LiveJudgementRule.GHOST_MISS_PENALTY_SCORE), 그 손실이 정확도를 통해 랭크에 반영됩니다.
    /// </summary>
    public const int GHOST = COUNT;

    public static bool IsValid(int lane)
    {
        return FIRST <= lane && lane <= LAST;
    }
}
