/// <summary>
/// NEXT 입력 하나가 지금 상태에서 무엇이 되는지입니다(기획서 6.3의 NEXT 3단계).
/// </summary>
public enum EStoryNextAction
{
    /// <summary>
    /// 아무것도 하지 않습니다. 팝업이 열려 있거나, 대사가 아직 뜨지 않았거나, 다 나온 직후의 잠금 중입니다.
    /// </summary>
    NONE,

    /// <summary>
    /// 출력 중인 남은 글자를 즉시 채웁니다.
    /// </summary>
    FILL_TEXT,

    /// <summary>
    /// 다음 줄로 넘깁니다.
    /// </summary>
    MOVE_NEXT
}
