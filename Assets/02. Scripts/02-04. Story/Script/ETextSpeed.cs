/// <summary>
/// 대사 한 줄의 출력 속도이자 방식입니다(연출표의 [속도] 열).
///
/// 속도 배수만으로는 "단어 단위 툭툭"을 표현할 수 없어 방식까지 함께 담습니다.
/// 실제 초 단위 값은 StoryTypingSpeed가 화면의 기준 속도에 곱해 정합니다.
///
/// 대사 JSON과의 호환을 위해 새 값은 맨 뒤에만 추가합니다.
/// </summary>
public enum ETextSpeed
{
    /// <summary>
    /// 기준 속도입니다. 값을 적지 않은 줄이 여기로 떨어집니다.
    /// </summary>
    NORMAL,

    SLOW,

    FAST,

    /// <summary>
    /// SLOW보다 더 느립니다. 씬을 닫는 마지막 줄처럼 여운을 남겨야 하는 곳에 씁니다.
    /// </summary>
    VERY_SLOW,

    /// <summary>
    /// 타이핑 없이 한 번에 전부 출력합니다. 비명이나 효과음 지문처럼 순간적으로 튀어나와야 하는 줄에 씁니다.
    /// </summary>
    INSTANT,

    /// <summary>
    /// 글자가 아니라 단어 단위로 끊어서 출력합니다.
    /// </summary>
    WORD_BY_WORD
}
