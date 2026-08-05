/// <summary>
/// LIVE 진입 경로와 선택 상태를 씬 사이에서 옮기는 싱글톤입니다.
/// SceneTransitionManager는 씬 이름만 받으므로, 진입 경로에 따라 달라지는 뒤로가기 위치·지정곡 고정 여부는
/// 이 컨텍스트를 통해 전달합니다. MonoBehaviour가 아니어서 서브씬이 언로드되어도 값이 유지되며,
/// PersistentScene에 별도 오브젝트를 두지 않아도 됩니다.
/// </summary>
public class LiveEntryContext : POCOSingleton<LiveEntryContext>
{
    public EEntryType EntryType { get; private set; } = EEntryType.HOME_LIVE;
    public string ScheduleId { get; private set; }
    public string DesignatedSongId { get; private set; }
    public string SelectedSongId { get; private set; }
    public EDifficulty SelectedDifficulty { get; private set; } = EDifficulty.NORMAL;

    /// <summary>
    /// 스케줄 LIVE처럼 곡이 미리 정해진 진입 경로인지 여부입니다. 이 경우 곡 목록에서 다른 곡을 고를 수 없습니다.
    /// </summary>
    public bool HasDesignatedSong => !string.IsNullOrEmpty(DesignatedSongId);

    /// <summary>
    /// 곡 선택 화면으로 진입하기 직전에 호출합니다.
    /// </summary>
    public void SetEntry(EEntryType entryType, string designatedSongId = null, string scheduleId = null)
    {
        EntryType = entryType;
        DesignatedSongId = designatedSongId;
        ScheduleId = scheduleId;
        SelectedSongId = designatedSongId;
    }

    /// <summary>
    /// 곡 선택 화면에서 확정한 플레이 대상을 기록합니다. 리듬게임·결과 화면이 이 값을 읽습니다.
    /// </summary>
    public void SetSelection(string songId, EDifficulty difficulty)
    {
        SelectedSongId = songId;
        SelectedDifficulty = difficulty;
    }

    /// <summary>
    /// 지정곡이 수록 목록에 없을 때 호출합니다. 지정 상태만 남겨 두면 곡 목록이 잠긴 채로 열려
    /// 플레이어가 어떤 곡도 고를 수 없게 되므로, 자유 선택으로 대체하기 전에 제약을 함께 풀어 줍니다.
    /// </summary>
    public void ClearDesignatedSong()
    {
        DesignatedSongId = null;
        SelectedSongId = null;
    }

    public void Clear()
    {
        EntryType = EEntryType.HOME_LIVE;
        ScheduleId = null;
        DesignatedSongId = null;
        SelectedSongId = null;
        SelectedDifficulty = EDifficulty.NORMAL;
    }
}
