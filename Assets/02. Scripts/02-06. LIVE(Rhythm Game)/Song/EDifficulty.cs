/// <summary>
/// 리듬게임의 난이도를 정의하는 열거형입니다.
///
/// 이 값은 채보 파일명(LiveSongPaths.GetChartFileName)과 세이브의 곡 기록 키(SongRecordKey)에 쓰이며,
/// JsonUtility는 enum을 선언 순서에 따른 정수로 직렬화합니다.
/// 새 난이도는 반드시 HARD 뒤에만 추가합니다. 앞이나 중간에 끼워 넣으면 기존 세이브의 NORMAL 기록이 EASY로 뒤바뀝니다.
/// </summary>
public enum EDifficulty
{
    /// <summary>
    /// 쉬움 난이도입니다.
    /// </summary>
    EASY,

    /// <summary>
    /// 보통 난이도입니다. 1차 MVP 버전에서는 이 난이도만 활성화됩니다.
    /// </summary>
    NORMAL,

    /// <summary>
    /// 어려움 난이도입니다.
    /// </summary>
    HARD
}
