/// <summary>
/// 리듬게임의 난이도입니다.
///
/// 채보 파일명과 세이브의 곡 기록 키에 쓰이며, JsonUtility가 선언 순서에 따른 정수로 직렬화합니다.
/// 새 난이도는 반드시 HARD 뒤에만 추가합니다. 앞이나 중간에 끼워 넣으면 기존 세이브의 NORMAL 기록이 EASY로 뒤바뀝니다.
/// </summary>
public enum EDifficulty
{
    EASY,

    /// <summary>
    /// 보통 난이도입니다. 현재 활성화된 유일한 난이도입니다.
    /// </summary>
    NORMAL,

    HARD
}
