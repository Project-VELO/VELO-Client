/// <summary>
/// 리듬게임 한 판의 진행 상태입니다.
/// </summary>
public enum ELiveGameState
{
    /// <summary>
    /// 곡과 채보, 음원을 불러오는 중입니다.
    /// </summary>
    Loading,

    /// <summary>
    /// 시작 또는 재개 직전의 카운트다운 중입니다. 이 동안에는 노트 입력을 받지 않습니다.
    /// </summary>
    Countdown,

    /// <summary>
    /// 곡이 흐르며 판정이 이루어지는 중입니다.
    /// </summary>
    Playing,

    /// <summary>
    /// 일시정지 팝업이 열려 음악·노트·타이머가 모두 멈춘 상태입니다.
    /// </summary>
    Paused,

    /// <summary>
    /// 완주 또는 귀신 노트 실패로 판정이 끝나고 결과 화면으로 넘어가기를 기다리는 중입니다.
    /// </summary>
    Finishing,
}
