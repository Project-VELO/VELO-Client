/// <summary>
/// 리듬게임 한 판의 진행 상태입니다.
/// </summary>
public enum ELiveGameState
{
    Loading,

    /// <summary>
    /// 시작 또는 재개 직전의 카운트다운 중입니다. 이 동안에는 노트 입력을 받지 않습니다.
    /// </summary>
    Countdown,

    Playing,

    /// <summary>
    /// 일시정지 팝업이 열려 음악·노트·타이머가 모두 멈춘 상태입니다.
    /// </summary>
    Paused,

    /// <summary>
    /// 곡을 완주해 판정이 끝나고 결과 화면으로 넘어가기를 기다리는 중입니다.
    /// </summary>
    Finishing,
}
