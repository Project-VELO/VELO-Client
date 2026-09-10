using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// 수록된 곡과 채보가 실제로 플레이 가능한 형태인지 검사합니다.
///
/// 한 곡에 난이도별 채보가 여러 개 붙는 구조라, 채보를 새로 뽑을 때마다 곡을 하나 더 만들어 버리기 쉽습니다.
/// 실제로 'Till The End 136'과 'Till The End 136 1-16'이 같은 곡인데 별개의 곡으로 갈라져 있었고,
/// 스케줄이 수록되지 않은 쪽을 가리켜 완료 판정이 통째로 막혔습니다.
///
/// 곡이 갈라지면 곡 목록에 같은 곡이 두 번 뜨고, 최고 기록도 따로 쌓이며, 지정곡을 적을 때
/// 어느 쪽이 맞는지 알 수 없습니다. 그래서 조용히 넘기지 않고 여기서 알립니다.
/// </summary>
public class LiveSongValidator
{
    public void Validate(MasterDataValidationReport report)
    {
        List<SongData> songs = LiveSongCatalog.Instance.Chapters.SelectMany(chapter => chapter.Songs).ToList();

        report.AddSummary($"수록 곡 {songs.Count} / 채보 {songs.Sum(song => song.Charts.Count)}");

        foreach (SongData song in songs)
        {
            ValidateCharts(song, report);
        }

        ValidateDuplicateSongs(songs, report);
    }

    /// <summary>
    /// 채보가 하나도 없는 곡과 파일이 사라진 채보를 찾습니다.
    ///
    /// 채보가 없어도 곡 목록에는 그대로 뜹니다. 고른 뒤에야 진입하지 못한다는 것을 알게 되므로
    /// 목록에 올리기 전에 걸러야 합니다.
    /// </summary>
    private void ValidateCharts(SongData song, MasterDataValidationReport report)
    {
        if (song.Charts.Count == 0)
        {
            report.AddWarning($"[곡] '{song.SongId}'에 채보가 하나도 없습니다. 목록에는 뜨지만 플레이할 수 없습니다.");
            return;
        }

        foreach (KeyValuePair<EDifficulty, ChartMetadata> pair in song.Charts)
        {
            string path = LiveSongPaths.GetPublishedChartPath(song.FolderPath, song.SongId, pair.Key);

            if (!File.Exists(path))
            {
                report.AddError($"[곡] '{song.SongId}'의 {pair.Key} 채보 파일이 없습니다: {path}");
            }
        }
    }

    /// <summary>
    /// 같은 곡이 여러 ID로 갈라져 있는지 찾습니다.
    ///
    /// 음원 파일명·BPM·재생 시간이 모두 같으면 같은 곡으로 봅니다. 채보만 다른 것을 곡으로 나눈 경우가
    /// 여기 걸립니다. 제목으로 비교하지 않는 것은, 갈라진 쪽이 대개 작업용 이름을 그대로 달고 있어
    /// 정작 제목이 서로 다르기 때문입니다.
    ///
    /// 오류가 아니라 경고인 것은, 같은 음원을 쓰는 별개의 곡(리믹스 등)이 나중에 생길 수 있어서입니다.
    /// </summary>
    private void ValidateDuplicateSongs(List<SongData> songs, MasterDataValidationReport report)
    {
        IEnumerable<IGrouping<string, SongData>> groups = songs
            .Where(song => !string.IsNullOrEmpty(song.AudioFilePath))
            .GroupBy(song => $"{song.AudioFilePath}|{song.Bpm}|{song.Duration}");

        foreach (IGrouping<string, SongData> group in groups)
        {
            if (group.Count() < 2)
            {
                continue;
            }

            string ids = string.Join(", ", group.Select(song => $"'{song.SongId}'"));

            report.AddWarning($"[곡] 음원과 길이가 같은 곡이 {group.Count()}개로 갈라져 있습니다: {ids}. "
                + "채보만 다른 것이라면 한 곡의 난이도별 채보로 합쳐야 합니다.");
        }
    }
}
