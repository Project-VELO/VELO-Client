/// <summary>
/// 감상 화면에서 캐릭터가 서는 자리입니다.
///
/// 좌·우 두 자리로 시작했으나 임원 회의처럼 세 인물이 한 화면에 서는 장면이 생겨 가운데를 더했습니다.
/// 대본은 자리마다 별도 컬럼을 가지며, 빈 칸은 직전 유지, NONE은 내리라는 지시입니다.
/// </summary>
public enum EStoryCharacterSlot
{
    LEFT,
    CENTER,
    RIGHT
}
