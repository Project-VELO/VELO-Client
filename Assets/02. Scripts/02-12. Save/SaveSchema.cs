/// <summary>
/// 세이브 파일의 스키마 버전입니다.
///
/// 마스터 데이터(MasterDataSchema)와 버전을 따로 두는 이유는 둘이 서로 다른 이유로 바뀌기 때문입니다.
/// 기획 데이터의 컬럼이 늘어나는 것과 플레이어 진행 데이터의 구조가 바뀌는 것은 별개입니다.
///
/// 필드를 추가할 때는 이 값을 올리고, 기존 필드의 이름은 바꾸지 않습니다.
/// JsonUtility에는 필드 별칭 기능이 없어 이름을 바꾸면 기존 세이브의 값이 조용히 사라집니다.
/// </summary>
public static class SaveSchema
{
    public const int CURRENT_VERSION = 1;

    /// <summary>
    /// 아직 저장을 거치지 않은 데이터의 버전입니다.
    /// PlayerData의 기본값이며, 세이브 파일에서 이 값이 나오면 내용이 없는 파일로 판단합니다.
    /// </summary>
    public const int UNSAVED_VERSION = 0;
}
