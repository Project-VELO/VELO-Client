using System;
using UnityEngine;

/// <summary>
/// 각 곡 및 난이도 조합별 플레이 최고 기록을 관리하는 DTO 클래스입니다.
/// </summary>
[Serializable]
public class SongRecord
{
    [SerializeField]
    private int _bestScore = 0;

    [SerializeField]
    private ERhythmRank _bestRank = ERhythmRank.FAILED;

    [SerializeField]
    private bool _isFullCombo = false;

    public int BestScore
    {
        get { return _bestScore; }
        set { _bestScore = value; }
    }

    public ERhythmRank BestRank
    {
        get { return _bestRank; }
        set { _bestRank = value; }
    }

    public bool IsFullCombo
    {
        get { return _isFullCombo; }
        set { _isFullCombo = value; }
    }

    /// <summary>
    /// 새로운 점수로 기록 갱신을 시도합니다.
    /// </summary>
    /// <param name="score">갱신할 점수</param>
    /// <param name="rank">갱신할 랭크</param>
    /// <param name="isFullCombo">풀콤보 여부</param>
    /// <returns>기록이 갱신되었으면 true, 그렇지 않으면 false</returns>
    public bool TryUpdateRecord(int score, ERhythmRank rank, bool isFullCombo)
    {
        if (score > _bestScore || (score == _bestScore && isFullCombo && !_isFullCombo))
        {
            _bestScore = score;
            _bestRank = rank;
            _isFullCombo = isFullCombo;
            return true;
        }

        return false;
    }
}
