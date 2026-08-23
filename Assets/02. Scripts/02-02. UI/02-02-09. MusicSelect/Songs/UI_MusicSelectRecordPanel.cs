using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 선택한 곡·난이도의 내 최고 기록(점수, 랭크)을 표시합니다.
/// 랭크는 글자가 아니라 등급 아트로 보여 줍니다.
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
    private Image _bestRankImage;

    /// <summary>
    /// 한 단계 위 등급의 기준 점수입니다.
    /// </summary>
    [SerializeField]
    private TMP_Text _nextGoalText;

    [Foldout("Project")]
    [Header("Rank Icons")]
    [SerializeField]
    private Sprite _perfectSRankIcon;

    [SerializeField]
    private Sprite _sRankIcon;

    [SerializeField]
    private Sprite _aRankIcon;

    [SerializeField]
    private Sprite _bRankIcon;

    [SerializeField]
    private Sprite _cRankIcon;

    /// <summary>
    /// 목표 점수는 채보의 노트 수에서 나오므로 기록과 함께 요약을 받습니다.
    /// 아직 한 번도 클리어하지 않은 곡도 목표는 있으므로, 기록이 없다고 목표까지 비우지는 않습니다.
    /// </summary>
    public void RefreshRecord(SongRecord record, LiveChartSummary summary)
    {
        bool hasRecord = !ReferenceEquals(record, null);

        _bestScoreText.text = hasRecord ? record.BestScore.ToString("N0") : UNPLAYED_LABEL_TEXT;
        SetRankIcon(hasRecord ? GetRankIcon(record.BestRank) : null);
        SetNextGoal(hasRecord ? record.BestRank : ELiveRank.FAILED, summary);
    }

    public void Clear()
    {
        _bestScoreText.text = UNPLAYED_LABEL_TEXT;
        SetRankIcon(null);
        _nextGoalText.text = NO_GOAL_TEXT;
    }

    private void SetNextGoal(ELiveRank currentRank, LiveChartSummary summary)
    {
        int noteCount = ReferenceEquals(summary, null) ? 0 : summary.NoteCount;

        _nextGoalText.text = LiveRankGoal.TryGetGoalScore(currentRank, noteCount, out int goalScore)
            ? goalScore.ToString("N0")
            : NO_GOAL_TEXT;
    }

    /// <summary>
    /// 기록이 없거나 FAILED로 끝난 곡은 보여 줄 등급 아트가 없으므로 아이콘을 감춥니다.
    /// 빈 스프라이트를 남기면 흰 사각형이 그대로 노출됩니다.
    /// </summary>
    private void SetRankIcon(Sprite icon)
    {
        _bestRankImage.sprite = icon;
        _bestRankImage.enabled = icon != null;
    }

    private Sprite GetRankIcon(ELiveRank rank)
    {
        switch (rank)
        {
            case ELiveRank.PERFECT_S:
                return _perfectSRankIcon;

            case ELiveRank.S:
                return _sRankIcon;

            case ELiveRank.A:
                return _aRankIcon;

            case ELiveRank.B:
                return _bRankIcon;

            case ELiveRank.C:
                return _cRankIcon;

            default:
                return null;
        }
    }
}
