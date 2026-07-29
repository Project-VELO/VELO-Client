using UnityEngine;
using VInspector;

/// <summary>
/// 채보 에디터 트랙의 스크롤 상태를 관리하는 조율 클래스입니다.
/// 재생 시각을 마디 좌표로 환산해 노트/격자/마디번호 렌더러에 전달하며, 실제 그리기 책임은 각 렌더러가 나눠 가집니다.
/// 마디 테이블과 스크롤 매퍼는 이 클래스가 소유하고 렌더러들에게 참조로 주입하므로, 하이스피드나 마디 재구축이
/// 즉시 모든 렌더러에 반영됩니다.
/// </summary>
public class LiveEditorTimeline : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField]
    private ESnapDivision _snapDivision = ESnapDivision.Sixteenth;

    [Header("Scroll")]
    [Tooltip("노트가 나타나기 시작하는 트랙 깊이입니다. 1이면 트랙 맨 뒤이며, 낮출수록 노트가 판정선까지 오면서 겪는 확대 배율이 줄어듭니다. 트랙 아트웍은 바뀌지 않습니다.")]
    [Range(LiveEditorScrollMapper.MIN_SPAWN_RATIO, LiveEditorScrollMapper.MAX_SPAWN_RATIO)]
    [SerializeField]
    private float _noteSpawnRatio = LiveEditorScrollMapper.MAX_SPAWN_RATIO;

    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_LiveTrackLanes _lanes;

    [SerializeField]
    private LiveEditorNoteRenderer _noteRenderer;

    [SerializeField]
    private LiveEditorGridRenderer _gridRenderer;

    [SerializeField]
    private LiveEditorBarLabelRenderer _barLabelRenderer;

    private readonly LiveEditorBarLayout _barLayout = new LiveEditorBarLayout();
    private readonly LiveEditorScrollMapper _scrollMapper = new LiveEditorScrollMapper();

    private double _currentBarPosition;

    public ESnapDivision SnapDivision { get => _snapDivision; set => _snapDivision = value; }
    public UI_LiveTrackLanes Lanes => _lanes;
    public LiveEditorBarLayout BarLayout => _barLayout;
    public double CurrentBarPosition => _currentBarPosition;
    public float HiSpeed { get => _scrollMapper.HiSpeed; set => _scrollMapper.HiSpeed = value; }

    private void Awake()
    {
        RefreshScrollShape();

        _noteRenderer.Init(_lanes, _barLayout, _scrollMapper);
        _gridRenderer.Init(_lanes, _barLayout, _scrollMapper);
        _barLabelRenderer.Init(_lanes, _barLayout, _scrollMapper);
    }

#if UNITY_EDITOR
    /// <summary>
    /// 플레이 도중 인스펙터에서 등장 지점을 바꿔 가며 눈으로 확인할 수 있도록, 값이 바뀔 때마다 즉시 반영합니다.
    /// </summary>
    private void OnValidate()
    {
        RefreshScrollShape();
    }
#endif

    /// <summary>
    /// 노트 등장 지점을 스크롤 매퍼에 반영합니다.
    /// </summary>
    private void RefreshScrollShape()
    {
        _scrollMapper.SpawnRatio = _noteSpawnRatio;
    }

    public void SetChart(ChartData chart)
    {
        _noteRenderer.SetChart(chart);
    }

    /// <summary>
    /// 곡 길이를 알게 된 시점(오디오 로드 완료)에 호출되어 곡 전체를 덮는 마디 테이블을 구축합니다.
    /// </summary>
    public void InitBarLayout(ChartData chart, int songLengthMs)
    {
        _barLayout.InitBars(chart, songLengthMs);
        _barLabelRenderer.InitBarNumberTexts();
    }

    public void RefreshNoteVisuals()
    {
        _noteRenderer.RefreshNoteVisuals();
        _noteRenderer.RefreshNotePositions(_currentBarPosition);
    }

    public void RefreshScroll(int currentTimeMs)
    {
        _currentBarPosition = _barLayout.GetBarPosition(currentTimeMs);

        _gridRenderer.RefreshGrid(_currentBarPosition, _snapDivision);
        _barLabelRenderer.RefreshBarLabels(_currentBarPosition);
        _noteRenderer.RefreshNotePositions(_currentBarPosition);
    }

    /// <summary>
    /// 트랙 위 세로 비율을 화면에 그려진 격자와 동일한 셀(마디/셀 인덱스)로 변환합니다.
    /// </summary>
    public bool TryGetCellAtVerticalRatio(float verticalRatio, out int barIndex, out int cellIndex)
    {
        barIndex = 0;
        cellIndex = 0;

        if (_lanes == null || !_barLayout.IsBuilt)
        {
            return false;
        }

        float hitLineRatio = _lanes.GetHitLineVerticalRatio();
        double barPosition = _scrollMapper.ToBarPosition(verticalRatio, _currentBarPosition, hitLineRatio);

        return _barLayout.TryGetCellAtBarPosition(barPosition, _snapDivision, out barIndex, out cellIndex);
    }

    public int GetCellTimeMs(int barIndex, int cellIndex)
    {
        return _barLayout.GetCellTimeMs(barIndex, cellIndex, _snapDivision);
    }

    public int GetCurrentBarIndex()
    {
        int lastBarIndex = Mathf.Max(0, _barLayout.BarCount - 1);
        return Mathf.Clamp(Mathf.FloorToInt((float)_currentBarPosition), 0, lastBarIndex);
    }
}
