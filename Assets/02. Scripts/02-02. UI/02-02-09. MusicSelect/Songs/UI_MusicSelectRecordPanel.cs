using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 선택한 곡·난이도의 내 최고 기록과, 한 단계 위 등급의 기준 점수를 표시합니다.
/// 등급은 글자가 아니라 아트로 보여 줍니다.
/// </summary>
public class UI_MusicSelectRecordPanel : MonoBehaviour
{
    private const string UNPLAYED_LABEL_TEXT = "UNPLAYED";

    /// <summary>
    /// 더 오를 등급이 없거나(PERFECT_S) 곡을 고르지 않아 목표를 셀 수 없을 때 쓰는 표기입니다.
    /// </summary>
    private const string NO_GOAL_TEXT = "-";

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _bestScoreText;

    [SerializeField]
    private UI_RankIcon _bestRankIcon;

    /// <summary>
    /// 한 단계 위 등급의 기준 점수와 그 등급의 아트입니다.
    /// </summary>
    [SerializeField]
    private TMP_Text _nextGoalText;

    [SerializeField]
    private UI_RankIcon _nextGoalRankIcon;

    /// <summary>
    /// 목표 점수는 채보의 노트 수에서 나오므로 기록과 함께 요약을 받습니다.
    /// 아직 클리어하지 않은 곡도 목표는 있으므로, 기록이 없다고 목표까지 비우지는 않습니다.
    /// </summary>
    public void RefreshRecord(SongRecord record, LiveChartSummary summary)
    {
        bool hasRecord = !ReferenceEquals(record, null);

        _bestScoreText.text = hasRecord ? record.BestScore.ToString("N0") : UNPLAYED_LABEL_TEXT;
        SetBestRank(hasRecord ? record.BestRank : ELiveRank.FAILED, hasRecord);
        SetNextGoal(hasRecord ? record.BestRank : ELiveRank.FAILED, summary);
    }

    public void Clear()
    {
        _bestScoreText.text = UNPLAYED_LABEL_TEXT;
        _bestRankIcon.Clear();
        _nextGoalText.text = NO_GOAL_TEXT;
        _nextGoalRankIcon.Clear();
    }

    /// <summary>
    /// 기록이 없는 곡은 보여 줄 등급 자체가 없으므로 자리를 비웁니다.
    /// FAILED로 끝난 기록은 아트가 없어 UI_RankIcon이 알아서 대체 표기로 넘깁니다.
    /// </summary>
    private void SetBestRank(ELiveRank rank, bool hasRecord)
    {
        if (!hasRecord)
        {
            _bestRankIcon.Clear();
            return;
        }

        _bestRankIcon.RefreshRank(rank);
    }

    private void SetNextGoal(ELiveRank currentRank, LiveChartSummary summary)
    {
        int noteCount = ReferenceEquals(summary, null) ? 0 : summary.NoteCount;

        if (!LiveRankGoal.TryGetGoal(currentRank, noteCount, out ELiveRank goalRank, out int goalScore))
        {
            _nextGoalText.text = NO_GOAL_TEXT;
            _nextGoalRankIcon.Clear();
            return;
        }

        _nextGoalText.text = goalScore.ToString("N0");
        _nextGoalRankIcon.RefreshRank(goalRank);
    }
}
