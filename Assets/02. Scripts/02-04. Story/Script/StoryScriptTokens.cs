/// <summary>
/// 대본에서 "값 없음"을 명시적으로 지시하는 예약어입니다.
///
/// 배경·캐릭터·표정은 빈 칸이 "직전 상태 유지"를 뜻하기 때문에(StoryScriptLoader의 캐리오버),
/// 빈 칸으로는 "이제 아무도 세우지 마라"를 말할 수 없습니다. 등장한 캐릭터가 회차 끝까지 남습니다.
/// 그래서 해제에는 별도의 값이 필요합니다.
/// </summary>
public static class StoryScriptTokens
{
    /// <summary>
    /// 캐릭터를 내리라는 지시입니다. 표정도 함께 지워집니다.
    /// </summary>
    public const string NONE = "NONE";

    /// <summary>
    /// BGM을 멈추라는 지시입니다. BgmId는 캐리오버 대상이 아니지만,
    /// 빈 칸이 "직전 곡 유지"를 뜻하는 것은 같으므로 중지에는 이 값을 씁니다.
    /// </summary>
    public const string BGM_NONE = "BGM_NONE";
}
