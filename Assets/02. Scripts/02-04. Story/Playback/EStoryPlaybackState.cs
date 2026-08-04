/// <summary>
/// 스토리 감상 화면의 재생 단계입니다.
///
/// 기획서 6.3의 NEXT 2단계 동작과 6.4의 팝업 중 입력 차단을 상태로 구분하기 위한 값입니다.
/// 같은 NEXT 클릭이라도 출력 중이면 "즉시 전체 출력", 출력이 끝났으면 "다음 대사"로 갈라집니다.
/// </summary>
public enum EStoryPlaybackState
{
    /// <summary>
    /// 대본을 읽는 중입니다. 아직 어떤 입력도 받지 않습니다.
    /// </summary>
    LOADING,

    /// <summary>
    /// 현재 줄을 한 글자씩 출력하는 중입니다. NEXT를 누르면 남은 글자를 즉시 채웁니다.
    /// </summary>
    TYPING,

    /// <summary>
    /// 현재 줄이 다 출력되어 다음 입력을 기다립니다.
    /// </summary>
    WAITING_NEXT,

    /// <summary>
    /// 팝업이 열려 진행이 멈춘 상태입니다(기획서 6.4). 커서는 움직이지 않습니다.
    /// </summary>
    PAUSED,

    /// <summary>
    /// 마지막 NEXT를 받아 완료 처리와 화면 이동을 진행 중입니다.
    /// 이 상태에서 들어오는 입력은 전부 무시합니다(기획서 3-L 연속 클릭).
    /// </summary>
    FINISHING
}
