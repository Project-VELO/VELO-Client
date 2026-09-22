using System;
using UnityEngine;

/// <summary>
/// 채보에 배치된 노트 하나가 플레이테스트에서 받은 판정을 누적합니다. LiveChartHitStats의 한 줄입니다.
///
/// 기록 당시의 시각·레인·종류·롱노트 길이를 함께 남깁니다. NoteId는 노트를 옮기거나 반전해도 유지되므로,
/// 채보를 고친 뒤 같은 ID가 다른 자리를 가리키면 이 값으로 알아보고 0부터 다시 쌓습니다(LiveChartHitStatsSync).
///
/// 평균·표준편차 대신 합과 제곱합을 쌓습니다. 판이 늘어도 더하기만 하면 되고,
/// 조회할 때 평균 = 합 / 표본 수, 분산 = 제곱합 / 표본 수 − 평균²으로 되살릴 수 있습니다.
/// </summary>
[Serializable]
public class LiveNoteHitStat
{
    [SerializeField]
    private string _noteId;

    [SerializeField]
    private int _timeMs;

    [SerializeField]
    private int _lane;

    [SerializeField]
    private ENoteType _noteType;

    [SerializeField]
    private int _holdDurationMs;

    [SerializeField]
    private int _perfectCount;

    [SerializeField]
    private int _greatCount;

    [SerializeField]
    private int _goodCount;

    [SerializeField]
    private int _badCount;

    [SerializeField]
    private int _earlyBadCount;

    [SerializeField]
    private int _missCount;

    [SerializeField]
    private int _holdBreakCount;

    [SerializeField]
    private int _timingSampleCount;

    [SerializeField]
    private long _timingErrorSumMs;

    [SerializeField]
    private long _timingErrorSquareSumMs;

    public string NoteId => _noteId;
    public int TimeMs => _timeMs;
    public int Lane => _lane;
    public ENoteType NoteType => _noteType;
    public int HoldDurationMs => _holdDurationMs;
    public int PerfectCount => _perfectCount;
    public int GreatCount => _greatCount;
    public int GoodCount => _goodCount;
    public int BadCount => _badCount;

    /// <summary>
    /// 이르게 눌러 노트를 소비했지만 GOOD 창 밖이었던 BAD입니다. 많으면 노트가 늦게 찍혔을 가능성이 큽니다.
    /// </summary>
    public int EarlyBadCount => _earlyBadCount;

    /// <summary>
    /// 입력 없이 지나간 BAD입니다. 늦은 쪽은 GOOD 창을 넘기면 입력 자체가 들어오지 않으므로,
    /// 치지 않은 경우와 너무 늦게 친 경우가 함께 섞입니다.
    /// </summary>
    public int MissCount => _missCount;

    public int HoldBreakCount => _holdBreakCount;
    public int TimingSampleCount => _timingSampleCount;
    public long TimingErrorSumMs => _timingErrorSumMs;
    public long TimingErrorSquareSumMs => _timingErrorSquareSumMs;
    public int JudgedCount => _perfectCount + _greatCount + _goodCount + _badCount;

    /// <summary>
    /// 현재 노트의 자리를 스냅샷으로 담고 누적값을 모두 비웁니다. 새 노트와 자리가 바뀐 노트에 씁니다.
    /// </summary>
    public void InitFromNote(NoteData note)
    {
        _noteId = note.NoteId;
        _timeMs = note.TimeMs;
        _lane = note.Lane;
        _noteType = note.NoteType;
        _holdDurationMs = note.HoldDurationMs;

        _perfectCount = 0;
        _greatCount = 0;
        _goodCount = 0;
        _badCount = 0;
        _earlyBadCount = 0;
        _missCount = 0;
        _holdBreakCount = 0;
        _timingSampleCount = 0;
        _timingErrorSumMs = 0;
        _timingErrorSquareSumMs = 0;
    }

    public bool IsSameSnapshot(NoteData note)
    {
        return _timeMs == note.TimeMs && _lane == note.Lane && _noteType == note.NoteType && _holdDurationMs == note.HoldDurationMs;
    }

    public void Apply(in LiveNoteJudgement result)
    {
        IncreaseJudgementCount(result.Judgement);

        if (result.Judgement == EJudgement.BAD)
        {
            IncreaseBadCauseCount(result.Cause);
        }

        if (IsTimingSample(result))
        {
            long errorMs = result.ErrorMs;

            _timingSampleCount++;
            _timingErrorSumMs += errorMs;
            _timingErrorSquareSumMs += errorMs * errorMs;
        }
    }

    private void IncreaseJudgementCount(EJudgement judgement)
    {
        switch (judgement)
        {
            case EJudgement.PERFECT:
                _perfectCount++;
                break;
            case EJudgement.GREAT:
                _greatCount++;
                break;
            case EJudgement.GOOD:
                _goodCount++;
                break;
            default:
                _badCount++;
                break;
        }
    }

    /// <summary>
    /// 입력으로 난 BAD는 이른 쪽에서만 나옵니다. 늦은 쪽은 GOOD 창을 넘기는 순간 입력을 받지 않고 만료되기 때문입니다.
    /// </summary>
    private void IncreaseBadCauseCount(ELiveJudgementCause cause)
    {
        switch (cause)
        {
            case ELiveJudgementCause.MISS:
                _missCount++;
                break;
            case ELiveJudgementCause.HOLD_BREAK:
                _holdBreakCount++;
                break;
            default:
                _earlyBadCount++;
                break;
        }
    }

    /// <summary>
    /// 오차 표본은 GOOD 창(±125ms) 안의 입력만 씁니다. 판정 창은 이른 쪽이 250ms, 늦은 쪽이 125ms로 비대칭이라,
    /// 이른 쪽 BAD까지 넣으면 늦은 쪽에는 대응하는 표본이 없어 평균이 이른 쪽으로 치우칩니다. 이른 쪽 BAD는 개수로만 셉니다.
    /// </summary>
    private static bool IsTimingSample(in LiveNoteJudgement result)
    {
        if (!result.HasTimingError)
        {
            return false;
        }

        int absErrorMs = result.ErrorMs < 0 ? -result.ErrorMs : result.ErrorMs;

        return absErrorMs <= LiveJudgementRule.GOOD_WINDOW_MS;
    }
}
