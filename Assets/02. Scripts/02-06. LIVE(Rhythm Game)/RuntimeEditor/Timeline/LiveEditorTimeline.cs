using UnityEngine;
using VInspector;

/// <summary>
/// 채보 에디터 트랙의 스크롤 상태를 관리하는 조율 클래스입니다.
/// 노트를 흘려보내는 공통 동작은 LiveTrackScroller가 담당하고, 이 클래스는 편집에만 필요한
/// 격자·마디번호 표시와 격자 셀 좌표 변환을 덧붙입니다.
/// </summary>
public class LiveEditorTimeline : LiveTrackScroller
{
    [Header("Grid")]
    [SerializeField]
    private ESnapDivision _snapDivision = ESnapDivision.Sixteenth;

    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorGridRenderer _gridRenderer;

    [SerializeField]
    private LiveEditorBarLabelRenderer _barLabelRenderer;

    public ESnapDivision SnapDivision { get => _snapDivision; set => _snapDivision = value; }

    protected override void Awake()
    {
        base.Awake();

        _gridRenderer.Init(Lanes, BarLayout, ScrollMapper);
        _barLabelRenderer.Init(Lanes, BarLayout, ScrollMapper);
    }

    public override void InitBarLayout(ChartData chart, int songLengthMs)
    {
        base.InitBarLayout(chart, songLengthMs);
        _barLabelRenderer.InitBarNumberTexts();
    }

    public override void RefreshScroll(int currentTimeMs)
    {
        base.RefreshScroll(currentTimeMs);

        _gridRenderer.RefreshGrid(CurrentBarPosition, _snapDivision);
        _barLabelRenderer.RefreshBarLabels(CurrentBarPosition);
    }

    /// <summary>
    /// 트랙 위 세로 비율을 화면에 그려진 격자와 동일한 셀(마디/셀 인덱스)로 변환합니다.
    /// </summary>
    public bool TryGetCellAtVerticalRatio(float verticalRatio, out int barIndex, out int cellIndex)
    {
        barIndex = 0;
        cellIndex = 0;

        if (Lanes == null || !BarLayout.IsBuilt)
        {
            return false;
        }

        float hitLineRatio = Lanes.GetHitLineVerticalRatio();
        double barPosition = ScrollMapper.ToBarPosition(verticalRatio, CurrentBarPosition, hitLineRatio);

        return BarLayout.TryGetCellAtBarPosition(barPosition, _snapDivision, out barIndex, out cellIndex);
    }

    public int GetCellTimeMs(int barIndex, int cellIndex)
    {
        return BarLayout.GetCellTimeMs(barIndex, cellIndex, _snapDivision);
    }

    public int GetCurrentBarIndex()
    {
        int lastBarIndex = Mathf.Max(0, BarLayout.BarCount - 1);
        return Mathf.Clamp(Mathf.FloorToInt((float)CurrentBarPosition), 0, lastBarIndex);
    }
}
