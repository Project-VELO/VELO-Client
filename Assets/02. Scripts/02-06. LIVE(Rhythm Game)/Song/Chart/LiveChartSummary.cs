/// <summary>
/// 곡 선택 화면의 난이도별 표시에 필요한 채보 요약값입니다.
/// </summary>
public class LiveChartSummary
{
    public bool HasChart { get; set; }
    public int NoteCount { get; set; }
    public int Level { get; set; }

    public bool HasLevel => 0 < Level;
}
