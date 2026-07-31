using System;
using System.Collections.Generic;

/// <summary>
/// BaseBpm/OffsetMs/BpmChanges를 기준으로 절대 시각(ms)과 비트(beat) 좌표를 상호 변환하고,
/// 그리드 스냅 계산을 전담하는 정적 유틸리티 클래스입니다.
/// 그리드 해상도는 4/8/16/24/32분박을 모두 나눠 떨어지게 하는 비트당 24칸(LCM) 해상도로 고정합니다.
/// </summary>
public static class LiveBpmTimeConverter
{
    private const int GRID_SUBDIVISIONS_PER_BEAT = 24;

    private readonly struct Segment
    {
        public readonly int StartTimeMs;
        public readonly double Bpm;
        public readonly double StartBeat;

        public Segment(int startTimeMs, double bpm, double startBeat)
        {
            StartTimeMs = startTimeMs;
            Bpm = bpm;
            StartBeat = startBeat;
        }
    }

    public static double TimeMsToBeat(ChartData chart, int timeMs)
    {
        List<Segment> segments = BuildSegments(chart);
        Segment segment = FindSegmentByTime(segments, timeMs);
        double msPerBeat = MsPerBeat(segment.Bpm);
        return segment.StartBeat + (timeMs - segment.StartTimeMs) / msPerBeat;
    }

    public static int BeatToTimeMs(ChartData chart, double beat)
    {
        List<Segment> segments = BuildSegments(chart);
        Segment segment = FindSegmentByBeat(segments, beat);
        double msPerBeat = MsPerBeat(segment.Bpm);
        double timeMs = segment.StartTimeMs + (beat - segment.StartBeat) * msPerBeat;
        return (int)Math.Round(timeMs);
    }

    public static int SnapToGrid(ChartData chart, int timeMs, ESnapDivision division)
    {
        double beat = TimeMsToBeat(chart, timeMs);
        double gridUnit = 1.0 / (int)division;
        double snappedBeat = Math.Round(beat / gridUnit) * gridUnit;
        return BeatToTimeMs(chart, snappedBeat);
    }

    public static bool IsOnGrid(ChartData chart, int timeMs, int toleranceMs = 2)
    {
        double beat = TimeMsToBeat(chart, timeMs);
        double gridUnit = 1.0 / GRID_SUBDIVISIONS_PER_BEAT;
        double nearestGridBeat = Math.Round(beat / gridUnit) * gridUnit;
        int nearestGridTimeMs = BeatToTimeMs(chart, nearestGridBeat);
        return Math.Abs(nearestGridTimeMs - timeMs) <= toleranceMs;
    }

    private static double MsPerBeat(double bpm)
    {
        return 60000.0 / bpm;
    }

    private static List<Segment> BuildSegments(ChartData chart)
    {
        var segments = new List<Segment>
        {
            new Segment(chart.OffsetMs, chart.BaseBpm, 0.0)
        };

        if (chart.BpmChanges == null)
        {
            return segments;
        }

        var sortedChanges = new List<BpmChangeData>(chart.BpmChanges);
        sortedChanges.Sort((a, b) => a.TimeMs.CompareTo(b.TimeMs));

        foreach (BpmChangeData change in sortedChanges)
        {
            Segment previous = segments[segments.Count - 1];
            double msPerBeat = MsPerBeat(previous.Bpm);
            double elapsedBeats = (change.TimeMs - previous.StartTimeMs) / msPerBeat;
            double startBeat = previous.StartBeat + elapsedBeats;
            segments.Add(new Segment(change.TimeMs, change.Bpm, startBeat));
        }

        return segments;
    }

    private static Segment FindSegmentByTime(List<Segment> segments, int timeMs)
    {
        Segment result = segments[0];
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i].StartTimeMs <= timeMs)
            {
                result = segments[i];
            }
        }
        return result;
    }

    private static Segment FindSegmentByBeat(List<Segment> segments, double beat)
    {
        Segment result = segments[0];
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i].StartBeat <= beat)
            {
                result = segments[i];
            }
        }
        return result;
    }
}
