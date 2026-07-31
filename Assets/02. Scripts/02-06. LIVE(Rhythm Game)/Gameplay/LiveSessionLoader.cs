using UnityEngine;

/// <summary>
/// LiveEntryContext에 담긴 선택 정보를 실제 곡·채보 데이터로 해석합니다.
/// 곡 선택 화면은 곡 객체가 아니라 ID만 넘기므로, 리듬게임 씬 진입 시 한 번 여기서 풀어 줍니다.
/// </summary>
public static class LiveSessionLoader
{
    public static bool TryLoad(out SongData song, out ChartData chart)
    {
        song = null;
        chart = null;

        LiveEntryContext context = LiveEntryContext.Instance;
        LiveSongCatalog catalog = LiveSongCatalog.Instance;

        // 곡 선택 화면을 거치지 않고 이 씬을 단독으로 열어 확인할 수도 있으므로 목록을 직접 확보합니다.
        catalog.Build();

        if (!catalog.TryGetSong(context.SelectedSongId, out song))
        {
            Debug.LogError($"[LiveSessionLoader] 수록 목록에서 곡을 찾지 못했습니다: {context.SelectedSongId}");
            return false;
        }

        chart = LiveChartLoader.LoadPublished(song, context.SelectedDifficulty);

        if (ReferenceEquals(chart, null))
        {
            Debug.LogError($"[LiveSessionLoader] 채보를 불러오지 못했습니다: {context.SelectedSongId} / {context.SelectedDifficulty}");
            song = null;
            return false;
        }

        return true;
    }
}
