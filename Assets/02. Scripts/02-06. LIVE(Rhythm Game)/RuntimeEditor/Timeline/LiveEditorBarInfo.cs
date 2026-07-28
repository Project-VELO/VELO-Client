/// <summary>
/// 마디 하나의 절대 시각 범위와 시작 비트를 담는 불변 DTO 구조체입니다.
/// </summary>
public readonly struct LiveEditorBarInfo
{
    public readonly int Index;
    public readonly int StartTimeMs;
    public readonly int EndTimeMs;
    public readonly double StartBeat;

    public LiveEditorBarInfo(int index, int startTimeMs, int endTimeMs, double startBeat)
    {
        Index = index;
        StartTimeMs = startTimeMs;
        EndTimeMs = endTimeMs;
        StartBeat = startBeat;
    }

    public int DurationMs => EndTimeMs - StartTimeMs;
}
