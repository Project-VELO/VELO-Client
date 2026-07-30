using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 선택한 곡·난이도의 최고 기록(점수, 랭크)을 표시합니다.
/// </summary>
public class UI_MusicSelectRecordPanel : MonoBehaviour
{
    private const string UNPLAYED_RANK_TEXT = "-";
    private const string UNPLAYED_LABEL_TEXT = "UNPLAYED";

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _bestScoreText;

    [SerializeField]
    private TMP_Text _bestRankText;

    // 다음 랭크까지 필요한 점수는 점수 체계(만점·랭크별 임계 점수)가 확정되어야 계산할 수 있습니다.
    // 판정 엔진과 결과 화면이 붙기 전까지는 비워 두고, 그때 이 텍스트를 채웁니다.
    [SerializeField]
    private TMP_Text _nextRankText;

    public void RefreshRecord(SongRecord record)
    {
        if (ReferenceEquals(record, null))
        {
            Clear();
            return;
        }

        _bestScoreText.text = record.BestScore.ToString("N0");
        _bestRankText.text = record.BestRank.ToString();
        _nextRankText.text = string.Empty;
    }

    public void Clear()
    {
        _bestScoreText.text = UNPLAYED_LABEL_TEXT;
        _bestRankText.text = UNPLAYED_RANK_TEXT;
        _nextRankText.text = string.Empty;
    }
}
