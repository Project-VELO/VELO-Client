/// <summary>
/// 스토리 대사 한 줄의 표시 방식입니다. 셋의 화자명 박스 처리가 서로 달라 구분이 필요합니다.
///
/// 세이브에는 저장되지 않지만, 대사 JSON과의 호환을 위해 새 값은 맨 뒤에만 추가합니다.
/// </summary>
public enum ELineType
{
    /// <summary>
    /// 화자가 없는 지문입니다. 화자명 박스를 표시하지 않습니다.
    /// </summary>
    NARRATION,

    /// <summary>
    /// 화자가 있는 일반 대사입니다. 화자명 박스에 해당 캐릭터의 표시명을 출력합니다.
    /// </summary>
    DIALOGUE,

    /// <summary>
    /// 화자의 속마음입니다. 화자명은 표시하되 대사와 구분되는 연출을 적용합니다.
    /// </summary>
    MONOLOGUE
}
