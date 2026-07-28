using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 곡 전체 길이를 마디 단위로 분할한 테이블을 구축하고, 마디/셀 좌표와 절대 시각(ms) 사이의 변환을 전담합니다.
/// 마디 경계는 시간이 아닌 비트 좌표계에서 계산한 뒤 환산하므로, 곡 중간에 BPM이 바뀌어도 마디가 밀리지 않습니다.
/// 매 프레임 호출되는 조회 API(GetBarPosition 등)는 미리 구축된 마디 테이블만 이진 탐색하여 GC 할당이 없으며,
/// 정확한 시각이 필요한 편집 시점 API(GetCellTimeMs)에서만 BPM 변환기를 사용합니다.
/// </summary>
public class LiveEditorBarLayout
{
    private const int DEFAULT_BEATS_PER_BAR = 4;

    // 잘못된 BPM이나 곡 길이가 입력됐을 때 마디 생성이 무한히 반복되는 것을 막는 상한입니다.
    private const int MAX_BAR_COUNT = 4096;

    private readonly List<LiveEditorBarInfo> _bars = new List<LiveEditorBarInfo>();

    private ChartData _chart;
    private int _beatsPerBar = DEFAULT_BEATS_PER_BAR;
    private int _songLengthMs;

    public IReadOnlyList<LiveEditorBarInfo> Bars => _bars;
    public int BarCount => _bars.Count;
    public int BeatsPerBar => _beatsPerBar;
    public int SongLengthMs => _songLengthMs;
    public bool IsBuilt => _bars.Count > 0;

    /// <summary>
    /// 곡 길이를 모두 덮을 때까지 마디를 생성합니다. 곡을 새로 불러올 때마다 한 번씩만 호출됩니다.
    /// </summary>
    public void InitBars(ChartData chart, int songLengthMs)
    {
        _bars.Clear();
        _chart = chart;
        _songLengthMs = songLengthMs;

        if (ReferenceEquals(chart, null))
        {
            return;
        }

        _beatsPerBar = chart.BeatsPerBar;

        for (int barIndex = 0; barIndex < MAX_BAR_COUNT; barIndex++)
        {
            double startBeat = (double)barIndex * _beatsPerBar;
            int startTimeMs = LiveEditorBpmTimeConverter.BeatToTimeMs(chart, startBeat);
            int endTimeMs = LiveEditorBpmTimeConverter.BeatToTimeMs(chart, startBeat + _beatsPerBar);

            _bars.Add(new LiveEditorBarInfo(barIndex, startTimeMs, endTimeMs, startBeat));

            if (endTimeMs >= songLengthMs)
            {
                break;
            }
        }
    }

    public int GetCellsPerBar(ESnapDivision division)
    {
        return _beatsPerBar * (int)division;
    }

    public bool TryGetBar(int barIndex, out LiveEditorBarInfo bar)
    {
        if (barIndex < 0 || barIndex >= _bars.Count)
        {
            bar = default;
            return false;
        }

        bar = _bars[barIndex];
        return true;
    }

    public int GetBarStartTimeMs(int barIndex)
    {
        if (_bars.Count == 0)
        {
            return 0;
        }

        return _bars[Mathf.Clamp(barIndex, 0, _bars.Count - 1)].StartTimeMs;
    }

    /// <summary>
    /// 절대 시각을 "마디 인덱스 + 마디 내 진행 비율" 형태의 연속 좌표로 변환합니다.
    /// 스크롤 위치 계산에 매 프레임 사용되므로 마디 내부는 선형 보간으로 처리합니다.
    /// </summary>
    public double GetBarPosition(int timeMs)
    {
        if (_bars.Count == 0)
        {
            return 0.0;
        }

        int barIndex = FindBarIndexByTime(timeMs);
        LiveEditorBarInfo bar = _bars[barIndex];
        int durationMs = bar.DurationMs;

        if (durationMs <= 0)
        {
            return barIndex;
        }

        return barIndex + (double)(timeMs - bar.StartTimeMs) / durationMs;
    }

    /// <summary>
    /// 마디/셀 좌표를 정확한 절대 시각으로 변환합니다. 노트 배치 시점에만 호출되므로 BPM 변환기를 그대로 사용합니다.
    /// </summary>
    public int GetCellTimeMs(int barIndex, int cellIndex, ESnapDivision division)
    {
        if (ReferenceEquals(_chart, null))
        {
            return 0;
        }

        double beat = (double)barIndex * _beatsPerBar + (double)cellIndex / (int)division;
        return LiveEditorBpmTimeConverter.BeatToTimeMs(_chart, beat);
    }

    /// <summary>
    /// 연속 마디 좌표를 가장 가까운 격자 셀로 스냅합니다. 화면에 그려진 격자와 실제 배치 위치를 일치시킵니다.
    /// </summary>
    public bool TryGetCellAtBarPosition(double barPosition, ESnapDivision division, out int barIndex, out int cellIndex)
    {
        barIndex = 0;
        cellIndex = 0;

        if (_bars.Count == 0)
        {
            return false;
        }

        int cellsPerBar = GetCellsPerBar(division);
        barIndex = Mathf.FloorToInt((float)barPosition);
        cellIndex = Mathf.RoundToInt((float)((barPosition - barIndex) * cellsPerBar));

        // 마디 끝으로 반올림된 셀은 다음 마디의 첫 셀과 같은 위치이므로 다음 마디로 넘깁니다.
        if (cellIndex >= cellsPerBar)
        {
            barIndex++;
            cellIndex = 0;
        }

        if (barIndex < 0 || barIndex >= _bars.Count)
        {
            return false;
        }

        return true;
    }

    private int FindBarIndexByTime(int timeMs)
    {
        int low = 0;
        int high = _bars.Count - 1;

        while (low < high)
        {
            int mid = (low + high + 1) / 2;
            if (_bars[mid].StartTimeMs <= timeMs)
            {
                low = mid;
            }
            else
            {
                high = mid - 1;
            }
        }

        return low;
    }
}
