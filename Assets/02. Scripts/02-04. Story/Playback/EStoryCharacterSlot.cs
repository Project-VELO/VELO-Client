/// <summary>
/// 감상 화면에서 캐릭터가 서는 자리입니다.
///
/// 프리팹에 좌·우 두 자리가 있지만 대본의 CharacterId는 한 줄에 하나뿐이라,
/// 화자는 왼쪽에만 세우고 오른쪽은 비웁니다.
/// 2인 동시 표시가 필요해지면 대본에 두 번째 캐릭터 컬럼이 생겨야 합니다.
/// </summary>
public enum EStoryCharacterSlot
{
    LEFT,
    RIGHT
}
