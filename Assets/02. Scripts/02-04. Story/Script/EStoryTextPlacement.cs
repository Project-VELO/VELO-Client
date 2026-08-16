/// <summary>
/// 대사 한 줄이 화면 어디에 나오는지입니다(연출표의 [텍스트 디자인] 열).
///
/// 대부분은 하단 대화창이지만, 바람결에 섞인 속삭임처럼 인물도 대화창도 없이
/// 문장만 남겨야 하는 줄이 있어 화면 중앙 배치를 따로 둡니다.
///
/// 대사 JSON과의 호환을 위해 새 값은 맨 뒤에만 추가합니다.
/// </summary>
public enum EStoryTextPlacement
{
    /// <summary>
    /// 하단 대화창입니다. 값을 적지 않은 줄이 여기로 떨어집니다.
    /// </summary>
    DIALOG_BOX,

    /// <summary>
    /// 대화창 없이 화면 한가운데에 문장만 띄웁니다.
    /// </summary>
    SCREEN_CENTER
}
