/// <summary>
/// 난이도 선택 팝업이 어떤 흐름에서 열렸는지 구분하는 열거형입니다.
/// </summary>
public enum ELiveEditorChartMode
{
    /// <summary>
    /// 빈 채보를 새로 만드는 흐름입니다. 모든 난이도를 고를 수 있습니다.
    /// </summary>
    Create,

    /// <summary>
    /// 저장된 채보를 이어서 편집하는 흐름입니다. 이미 파일이 있는 난이도만 고를 수 있습니다.
    /// </summary>
    Load,
}
