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

    // 목록을 그대로 노출하면 화면이나 에디터 툴에서 실수로 비울 수 있어, 읽기 전용으로만 내보냅니다.
    public IReadOnlyList<SongData> Songs => _songs;

    public bool HasUnlockCondition => !string.IsNullOrEmpty(UnlockStoryId);

    public void AddSong(SongData song)
    {
        _songs.Add(song);
    }
}
