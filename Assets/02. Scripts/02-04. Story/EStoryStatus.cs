/// <summary>
/// 스토리의 진행 및 해금 상태입니다.
///
/// 세이브 데이터에 저장됩니다. JsonUtility가 enum을 선언 순서에 따른 정수로 직렬화하므로
/// 새 값은 반드시 맨 뒤에만 추가합니다. 순서를 바꾸면 기존 세이브의 COMPLETED가 조용히 뒤바뀝니다.
/// </summary>
public enum EStoryStatus
{
    LOCKED,
    UNLOCKED,
    COMPLETED
}
