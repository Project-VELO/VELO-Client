/// <summary>
/// ETextSpeed를 타이핑 루프가 쓸 수 있는 값으로 바꿉니다.
///
/// 배수를 대본이 아니라 코드에 두는 이유는, 기획이 시트에 적는 것은 "느리게/빠르게"라는 상대적 지시이지
/// 초 단위 수치가 아니기 때문입니다. 기준 속도는 화면 인스펙터(_secondsPerCharacter)에 있고,
/// 여기서는 그 기준에 대한 비율만 정합니다. 체감을 바꿀 때 대본 전체를 고칠 필요가 없습니다.
/// </summary>
public static class StoryTypingSpeed
{
    private const float SLOW_MULTIPLIER = 2f;
    private const float VERY_SLOW_MULTIPLIER = 4f;
    private const float FAST_MULTIPLIER = 0.5f;

    /// <summary>
    /// 타이핑 없이 즉시 전부 출력하는 줄인지 여부입니다.
    /// </summary>
    public static bool IsInstant(ETextSpeed speed)
    {
        return speed == ETextSpeed.INSTANT;
    }

    /// <summary>
    /// 글자가 아니라 단어 단위로 끊어 출력하는 줄인지 여부입니다.
    /// </summary>
    public static bool IsWordByWord(ETextSpeed speed)
    {
        return speed == ETextSpeed.WORD_BY_WORD;
    }

    /// <summary>
    /// 글자 하나가 출력되는 간격입니다. WORD_BY_WORD는 간격 자체는 기준과 같고,
    /// 도달한 지점을 단어 끝까지 밀어 내는 방식으로 끊어 출력합니다.
    /// </summary>
    public static float GetSecondsPerCharacter(ETextSpeed speed, float baseSeconds)
    {
        switch (speed)
        {
            case ETextSpeed.SLOW:
                return baseSeconds * SLOW_MULTIPLIER;

            case ETextSpeed.VERY_SLOW:
                return baseSeconds * VERY_SLOW_MULTIPLIER;

            case ETextSpeed.FAST:
                return baseSeconds * FAST_MULTIPLIER;

            default:
                return baseSeconds;
        }
    }
}
