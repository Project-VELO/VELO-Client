using System.Collections.Generic;

/// <summary>
/// 수록 공간에서 읽어온 챕터·곡 목록을 보관하고 조회를 제공하는 싱글톤입니다.
/// 씬을 넘나들며 곡 정보를 다시 읽지 않아도 되도록 POCO 싱글톤으로 두었습니다.
/// </summary>
public class LiveSongCatalog : POCOSingleton<LiveSongCatalog>
{
    private readonly LiveSongCatalogLoader _loader = new LiveSongCatalogLoader();
    private readonly List<LiveChapterData> _chapters = new List<LiveChapterData>();
    private readonly Dictionary<string, SongData> _songsBySongId = new Dictionary<string, SongData>();

    public List<LiveChapterData> Chapters => _chapters;
    public bool IsBuilt { get; private set; }

    /// <summary>
    /// 수록 공간을 훑어 목록을 만듭니다. 이미 만들어 두었으면 다시 읽지 않습니다.
    /// </summary>
    public void Build()
    {
        if (IsBuilt)
        {
            return;
        }

        Rebuild();
    }

    /// <summary>
    /// 수록 내용이 바뀐 경우(에디터에서 곡을 새로 수록한 경우 등) 강제로 다시 읽습니다.
    /// </summary>
    public void Rebuild()
    {
        _chapters.Clear();
        _songsBySongId.Clear();

        _chapters.AddRange(_loader.LoadChapters());

        foreach (LiveChapterData chapter in _chapters)
        {
            foreach (SongData song in chapter.Songs)
            {
                _songsBySongId[song.SongId] = song;
            }
        }

        IsBuilt = true;
    }

    public bool TryGetSong(string songId, out SongData song)
    {
        song = null;

        if (string.IsNullOrEmpty(songId))
        {
            return false;
        }

        return _songsBySongId.TryGetValue(songId, out song);
    }

    /// <summary>
    /// 지정곡이 속한 챕터의 인덱스를 찾습니다. 스케줄 LIVE처럼 곡이 먼저 정해진 진입 경로에서
    /// 해당 곡이 보이는 챕터 탭을 열어 주기 위해 사용합니다.
    /// </summary>
    public int FindChapterIndex(string songId)
    {
        if (!TryGetSong(songId, out SongData song))
        {
            return -1;
        }

        for (int i = 0; i < _chapters.Count; i++)
        {
            if (_chapters[i].ChapterId == song.ChapterId)
            {
                return i;
            }
        }

        return -1;
    }

    public int FindSongIndex(int chapterIndex, string songId)
    {
        if (chapterIndex < 0 || chapterIndex >= _chapters.Count || string.IsNullOrEmpty(songId))
        {
            return -1;
        }

        List<SongData> songs = _chapters[chapterIndex].Songs;
        for (int i = 0; i < songs.Count; i++)
        {
            if (songs[i].SongId == songId)
            {
                return i;
            }
        }

        return -1;
    }
}
