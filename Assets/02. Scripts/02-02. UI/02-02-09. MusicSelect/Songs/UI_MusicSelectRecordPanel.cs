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

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _bestScoreText;

    [SerializeField]
    private Image _bestRankImage;

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

    public void RefreshRecord(SongRecord record)
    {
        if (ReferenceEquals(record, null))
        {
            Clear();
            return;
        }

        _bestScoreText.text = record.BestScore.ToString("N0");
        SetRankIcon(GetRankIcon(record.BestRank));
    }

    public void Clear()
    {
        _bestScoreText.text = UNPLAYED_LABEL_TEXT;
        SetRankIcon(null);
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
