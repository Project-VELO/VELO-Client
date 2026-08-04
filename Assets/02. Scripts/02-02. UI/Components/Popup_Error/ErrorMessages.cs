/// <summary>
/// 오류 안내 팝업의 문구를 한곳에 모읍니다(기획서 3-L).
///
/// 문구는 기획 확정 전 잠정값입니다. 호출부에 흩어 두면 확정 시 세 화면을 따로 고쳐야 하고,
/// 같은 상황에 다른 문구가 남을 여지가 생겨 여기로 모았습니다.
/// </summary>
public static class ErrorMessages
{
    /// <summary>
    /// 수록된 곡을 읽지 못했거나 해금된 챕터가 없을 때입니다. 곡 선택 화면은 유지합니다(3-L:1095).
    /// </summary>
    public const string SONG_DATA_MISSING = "곡 데이터를 불러오지 못했습니다.";

    /// <summary>
    /// 대본을 읽지 못했을 때입니다. 안내를 닫으면 스토리 목록으로 돌아갑니다(3-L:1099).
    /// </summary>
    public const string STORY_SCRIPT_MISSING = "스토리 대사 데이터를 불러오지 못했습니다.";

    /// <summary>
    /// 재시도까지 실패했을 때입니다(3-L:1101). 진행은 막지 않고 알리기만 합니다.
    /// </summary>
    public const string SAVE_FAILED = "저장에 실패했습니다. 진행 상황이 저장되지 않을 수 있습니다.";
}
