using System.Collections.Generic;

/// <summary>
/// 수록 공간의 챕터 폴더 하나를 런타임에서 다루기 위한 클래스입니다.
/// 곡 선택 화면의 챕터 탭 하나가 이 데이터 하나에 대응합니다.
/// </summary>
public class LiveChapterData
{
    private readonly List<SongData> _songs = new List<SongData>();

    public int Order { get; set; }
    public string ChapterId { get; set; }
    public string DisplayName { get; set; }
    public string UnlockStoryId { get; set; }
    public string FolderPath { get; set; }

    public List<SongData> Songs => _songs;

    public bool HasUnlockCondition => !string.IsNullOrEmpty(UnlockStoryId);
}
