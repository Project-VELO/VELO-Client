using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 저장된 판정 통계를 지금 플레이하는 채보에 맞춥니다. 채보를 불러올 때 한 번만 도는 작업이라 할당을 아끼지 않습니다.
///
/// 채보에 새로 생긴 노트는 기록을 만들고, 채보에서 사라진 노트는 기록을 지웁니다.
/// NoteId는 노트를 옮기거나 반전해도 유지되므로, 같은 ID라도 기록 당시와 자리가 다르면 옛 자리에서 쌓인 판정은
/// 새 자리를 설명하지 못합니다. 그런 노트는 0부터 다시 쌓습니다.
/// </summary>
public static class LiveChartHitStatsSync
{
    /// <summary>
    /// 기록을 하나라도 추가·초기화·제거했으면 true를 돌려줍니다. 호출하는 쪽은 이 값으로 저장할 것이 있는지 판단합니다.
    /// </summary>
    public static bool Sync(LiveChartHitStats stats, ChartData chart)
    {
        Dictionary<string, NoteData> chartNotesById = CollectChartNotes(chart, out int excludedCount);
        Dictionary<string, LiveNoteHitStat> statsById = new Dictionary<string, LiveNoteHitStat>();
        int removedCount = RemoveStaleStats(stats.Notes, chartNotesById, statsById);
        int addedCount = 0;
        int resetCount = 0;

        foreach (NoteData note in chartNotesById.Values)
        {
            if (!statsById.TryGetValue(note.NoteId, out LiveNoteHitStat stat))
            {
                stat = new LiveNoteHitStat();
                stat.InitFromNote(note);
                stats.Notes.Add(stat);
                addedCount++;
                continue;
            }

            if (!stat.IsSameSnapshot(note))
            {
                stat.InitFromNote(note);
                resetCount++;
            }
        }

        stats.Notes.Sort(CompareByTimeThenLane);
        LogSummary(stats, addedCount, resetCount, removedCount, excludedCount);

        return 0 < addedCount + resetCount + removedCount;
    }

    /// <summary>
    /// ID가 비었거나 겹친 노트는 어느 기록에 판정을 붙여야 할지 알 수 없으므로, 겹친 쪽 모두를 통계에서 뺍니다.
    /// 저장 검사(LiveEditorChartValidator)를 통과한 채보라면 생기지 않지만, 손으로 고친 파일을 대비합니다.
    /// </summary>
    private static Dictionary<string, NoteData> CollectChartNotes(ChartData chart, out int excludedCount)
    {
        Dictionary<string, NoteData> notesById = new Dictionary<string, NoteData>();
        HashSet<string> duplicatedIds = new HashSet<string>();
        excludedCount = 0;

        foreach (NoteData note in chart.Notes)
        {
            if (string.IsNullOrEmpty(note.NoteId))
            {
                excludedCount++;
                continue;
            }

            if (!notesById.ContainsKey(note.NoteId))
            {
                notesById.Add(note.NoteId, note);
                continue;
            }

            duplicatedIds.Add(note.NoteId);
            excludedCount++;
        }

        foreach (string duplicatedId in duplicatedIds)
        {
            notesById.Remove(duplicatedId);
            excludedCount++;
        }

        return notesById;
    }

    /// <summary>
    /// 채보에 없는 노트의 기록과, 파일 안에서 ID가 겹친 기록을 지웁니다. 남은 기록은 statsById에 채워 돌려줍니다.
    /// </summary>
    private static int RemoveStaleStats(List<LiveNoteHitStat> noteStats, Dictionary<string, NoteData> chartNotesById,
        Dictionary<string, LiveNoteHitStat> statsById)
    {
        int removedCount = 0;

        for (int i = noteStats.Count - 1; 0 <= i; i--)
        {
            LiveNoteHitStat stat = noteStats[i];
            bool isInChart = !string.IsNullOrEmpty(stat.NoteId) && chartNotesById.ContainsKey(stat.NoteId);

            if (isInChart && !statsById.ContainsKey(stat.NoteId))
            {
                statsById.Add(stat.NoteId, stat);
                continue;
            }

            noteStats.RemoveAt(i);
            removedCount++;
        }

        return removedCount;
    }

    private static int CompareByTimeThenLane(LiveNoteHitStat left, LiveNoteHitStat right)
    {
        int timeOrder = left.TimeMs.CompareTo(right.TimeMs);

        return timeOrder != 0 ? timeOrder : left.Lane.CompareTo(right.Lane);
    }

    private static void LogSummary(LiveChartHitStats stats, int addedCount, int resetCount, int removedCount, int excludedCount)
    {
        if (0 < excludedCount)
        {
            Debug.LogWarning($"[LiveChartHitStatsSync] {stats.SongId}/{stats.Difficulty}: NoteId가 비었거나 겹친 노트 {excludedCount}개는 판정 통계에서 뺍니다.");
        }

        if (addedCount + resetCount + removedCount == 0)
        {
            return;
        }

        Debug.Log($"[LiveChartHitStatsSync] {stats.SongId}/{stats.Difficulty} 판정 통계를 채보에 맞췄습니다. 추가 {addedCount}, 자리가 바뀌어 초기화 {resetCount}, 제거 {removedCount}");
    }
}
