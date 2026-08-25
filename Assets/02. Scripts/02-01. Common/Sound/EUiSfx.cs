/// <summary>
/// 버튼을 만졌을 때 나는 소리입니다.
///
/// 스토리 효과음(StoryAudioBinder)과 나눈 이유는 규칙이 다르기 때문입니다.
/// 저쪽은 대본이 줄마다 지정하는 연출용 소리이고, 이쪽은 화면 어디서나 같은 조작에
/// 같은 소리가 나야 하는 조작 피드백입니다.
///
/// 세이브에 남는 값이 아니라 인스펙터 표의 키로만 쓰이므로 순서를 바꿔도 됩니다.
/// </summary>
public enum EUiSfx
{
    /// <summary>
    /// 소리를 내지 않습니다. 조용해야 하는 버튼이 이 값을 씁니다.
    /// </summary>
    NONE,

    /// <summary>
    /// 버튼 위에 마우스가 올라갔을 때입니다.
    /// </summary>
    BUTTON_HOVER,

    /// <summary>
    /// 버튼을 눌렀을 때의 기본 소리입니다.
    /// </summary>
    BUTTON_CLICK,

    /// <summary>
    /// 뒤로가기처럼 화면을 물러나는 버튼입니다. 나아가는 조작과 소리로 구분합니다.
    /// </summary>
    BUTTON_BACK
}
