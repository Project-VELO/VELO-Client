/// <summary>
/// 대사 출력 속도 기준표입니다. 일반적인 비주얼 노벨의 체감 속도를 기준으로 잡았습니다.
///
/// | 출력 기준        | 글자당 출력 속도 | 1초당 출력량 | 활용처                                              |
/// |------------------|------------------|--------------|-----------------------------------------------------|
/// | 즉시 INSTANT     | 0.00초           | 딜레이 없음  | 시스템 메시지, 당황해서 말을 쏟아내는 대사          |
/// | 보통 NORMAL      | 0.03 ~ 0.05초    | 약 20~30자   | 일상 대화, 평범한 상황 설명 (기본값)                |
/// | 느리게 SLOW      | 0.08 ~ 0.10초    | 약 10~12자   | 무거운 독백, 슬프거나 심각한 결정                   |
/// | 단어 WORD_BY_WORD| 0.20 ~ 0.30초    | 약 3~5단어   | 또박또박 화를 낼 때, 초월적 존재의 경고             |
///
/// 모든 속도에 공통으로, 마침표·물음표·느낌표·쉼표가 출력되면 0.5초 멈춥니다.
///
/// 표에서 조절할 수 있는 값은 보통 속도 하나뿐입니다. 화면 인스펙터(_secondsPerCharacter)에 있고,
/// 느리게는 그 값에 대한 비율로 두어 기준을 바꿔도 표의 관계가 유지됩니다. 단어 단위와 정적은
/// 글자 수와 무관한 연출이라 절대값으로 둡니다.
/// </summary>
public static class StoryTypingSpeed
{
    /// <summary>
    /// 느리게는 보통의 2.25배입니다. 기본값 0.04초 기준으로 0.09초가 되어 표의 0.08~0.10초 구간 가운데에 옵니다.
    /// </summary>
    private const float SLOW_MULTIPLIER = 2.25f;

    /// <summary>
    /// 단어 하나가 통째로 찍히는 간격입니다. 글자 수와 무관하므로 배수가 아닌 절대값입니다.
    /// </summary>
    private const float WORD_SECONDS = 0.25f;

    /// <summary>
    /// 문장 부호에서 멈추는 시간입니다.
    /// </summary>
    public const float PAUSE_SECONDS = 0.5f;

    /// <summary>
    /// 정적을 넣을 문장 부호입니다. "..."이나 "?!"처럼 이어진 부호는 한 번만 멈춥니다(IsPauseBoundary).
    /// </summary>
    private const string PAUSE_CHARACTERS = ".?!,";

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
    /// 한 번 출력할 때마다 기다리는 시간입니다. 단어 단위는 이 값이 글자가 아니라 단어 하나의 간격입니다.
    /// </summary>
    public static float GetStepSeconds(ETextSpeed speed, float baseSeconds)
    {
        switch (speed)
        {
            case ETextSpeed.SLOW:
                return baseSeconds * SLOW_MULTIPLIER;

            case ETextSpeed.WORD_BY_WORD:
                return WORD_SECONDS;

            default:
                return baseSeconds;
        }
    }

    /// <summary>
    /// 방금 출력한 글자에서 멈춰야 하는지 봅니다.
    ///
    /// 이어진 부호마다 멈추면 "..." 하나에 1.5초가 걸리므로, 부호 묶음의 마지막 글자에서만 멈춥니다.
    /// 대본에는 "...", "!!", "?!" 같은 묶음이 373번 나옵니다.
    /// </summary>
    public static bool IsPauseBoundary(char current, char next)
    {
        return IsPauseCharacter(current) && !IsPauseCharacter(next);
    }

    private static bool IsPauseCharacter(char value)
    {
        return PAUSE_CHARACTERS.IndexOf(value) >= 0;
    }
}
