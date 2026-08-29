using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 곡 선택 화면의 선택 연쇄를 담당합니다. 챕터를 고르면 곡 목록이, 곡을 고르면 난이도가,
/// 난이도를 고르면 상세·기록이 따라 갱신되는 한 줄기입니다.
///
/// 화면 총괄(UI_MusicSelect)에서 떼어낸 이유는 그쪽이 진입·이탈 배선을 맡기 때문입니다.
/// 무엇을 보여줄지 고르는 일과 어디로 오갈지 정하는 일은 서로 바뀌는 이유가 다릅니다
/// (화면 총괄과 진입 시퀀스를 나누는 UI_Office / UI_OfficeDayFinishFlow와 같은 분할).
/// </summary>
public class UI_MusicSelectSelection : MonoBehaviour
{
    /// <summary>
    /// 선택이 바뀔 때마다 확정된 곡·난이도와 채보 수록 여부를 알립니다.
    /// 채보가 없으면 플레이 진입을 막아야 하므로(기획서 16-15) 총괄이 이 값에 버튼 상태를 맞춥니다.
    /// 곡이 없는 상태에서는 song이 null로 옵니다.
    /// </summary>
    public Action<SongData, EDifficulty, bool> OnSelectionChanged;

    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_MusicSelectChapterTabs _chapterTabs;

    [SerializeField]
    private UI_MusicSelectSongList _songList;

    [SerializeField]
    private UI_MusicSelectSongCover _songCover;

    [SerializeField]
    private UI_MusicSelectSongDetail _songDetail;

    [SerializeField]
    private UI_MusicSelectDifficultyTabs _difficultyTabs;

    [SerializeField]
    private UI_MusicSelectRecordPanel _recordPanel;

    private readonly LiveChartSummaryReader _chartSummaryReader = new LiveChartSummaryReader();
    private readonly SongCoverLoader _coverLoader = new SongCoverLoader();

    private int _chapterIndex = -1;
    private SongData _selectedSong;

    private void Awake()
    {
        _songList.Init(_coverLoader);
        _songCover.Init(_coverLoader);

        _chapterTabs.OnChapterSelected = chapterIndex => SetChapter(chapterIndex, 0);
        _songList.OnSongSelected = SetSong;
        _difficultyTabs.OnDifficultySelected = SetDifficulty;
    }

    // 커버는 씬에 매이지 않는 리소스라, 화면을 떠날 때 캐시가 직접 정리해 주어야 드나들 때마다 쌓이지 않습니다.
    private void OnDestroy()
    {
        _coverLoader.Clear();
    }

    public void RefreshChapters(IReadOnlyList<LiveChapterData> chapters)
    {
        _chapterTabs.RefreshChapters(chapters);
    }

    public void SetChapter(int chapterIndex, int songIndex)
    {
        _chapterIndex = chapterIndex;
        _chapterTabs.SetSelectedIndex(chapterIndex);

        IReadOnlyList<SongData> songs = LiveSongCatalog.Instance.Chapters[chapterIndex].Songs;

        // 스케줄 LIVE는 지정곡 외의 곡을 고를 수 없으므로 목록 자체를 잠급니다.
        _songList.RefreshSongs(songs, !LiveEntryContext.Instance.HasDesignatedSong);

        SetSong(Mathf.Clamp(songIndex, 0, songs.Count - 1));
    }

    /// <summary>
    /// 고를 수 있는 곡이 없을 때입니다. 화면은 그대로 두고 내용만 비웁니다(기획서 3-L "곡 선택 화면 유지").
    /// </summary>
    public void Clear()
    {
        _chapterIndex = -1;
        _selectedSong = null;

        _songList.RefreshSongs(null, false);
        _songCover.Clear();
        _difficultyTabs.RefreshDifficulties(null, _chartSummaryReader);
        _songDetail.Clear();
        _recordPanel.Clear();

        if (OnSelectionChanged != null)
        {
            OnSelectionChanged(null, EDifficulty.NORMAL, false);
        }
    }

    private void SetSong(int songIndex)
    {
        _songList.SetSelectedIndex(songIndex);
        _selectedSong = LiveSongCatalog.Instance.Chapters[_chapterIndex].Songs[songIndex];

        _songCover.RefreshSong(_selectedSong);
        _difficultyTabs.RefreshDifficulties(_selectedSong, _chartSummaryReader);

        if (!_difficultyTabs.TryGetDefaultDifficulty(out EDifficulty difficulty))
        {
            Debug.LogWarning($"[UI_MusicSelectSelection] NORMAL 채보가 수록되지 않은 곡이라 플레이할 수 없습니다: {_selectedSong.SongId}");
        }

        SetDifficulty(difficulty);
    }

    private void SetDifficulty(EDifficulty difficulty)
    {
        _difficultyTabs.SetSelectedDifficulty(difficulty);

        LiveChartSummary summary = _chartSummaryReader.GetSummary(_selectedSong, difficulty);

        _songDetail.RefreshSong(_selectedSong, summary);
        SongRecord record = PlayerDataProvider.Instance.GetSongRecord(_selectedSong.SongId, difficulty);
        _recordPanel.RefreshRecord(record, summary);

        if (OnSelectionChanged != null)
        {
            OnSelectionChanged(_selectedSong, difficulty, summary.HasChart);
        }
    }
}
