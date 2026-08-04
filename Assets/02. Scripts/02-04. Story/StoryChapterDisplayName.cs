/// <summary>
/// 챕터 ID를 화면에 출력할 문자열로 바꿉니다.
///
/// stories.json에 표시명 필드가 없어 코드에서 변환합니다. 챕터가 둘뿐인 1차 MVP에
/// chapters.json 같은 마스터 테이블을 새로 만들면 MasterDataProvider·Paths·Loader 세 파일을
/// 함께 고쳐야 해서, 얻는 것에 비해 건드리는 범위가 큽니다.
///
/// 표시 문구는 기획 소관이므로 아래 표 한 곳만 고치면 전 화면에 반영되도록 몰아 두었습니다.
/// 챕터가 늘거나 한글 부제가 붙으면 이 파일만 손보면 됩니다.
/// </summary>
public static class StoryChapterDisplayName
{
    private const string UNKNOWN_CHAPTER = "CHAPTER ?";

    /// <summary>
    /// 표시명을 돌려줍니다. 표에 없는 챕터는 ID를 그대로 보여 줍니다.
    /// 빈 칸으로 두면 데이터가 잘못됐다는 사실이 화면에서 드러나지 않습니다.
    /// </summary>
    public static string Get(string chapterId)
    {
        if (string.IsNullOrEmpty(chapterId))
        {
            return UNKNOWN_CHAPTER;
        }

        switch (chapterId)
        {
            // PROLOGUE는 newgame_config.json의 StartChapterId와 LiveSongs/00_PROLOGUE가 함께 쓰는
            // ID라 데이터 쪽 이름을 바꾸지 않고 표시만 기획서 표기에 맞춥니다(기획서 3-F-3-1).
            case "PROLOGUE": return "CHAPTER 0";
            case "CHAPTER_01": return "CHAPTER 1";
            default: return chapterId;
        }
    }
}
