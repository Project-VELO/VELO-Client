using UnityEngine;
using VInspector;

/// <summary>
/// 노트가 흐르는 트랙의 스크롤 상태를 관리하는 공용 조율 클래스입니다.
/// 재생 시각을 마디 좌표로 환산해 노트 렌더러에 전달하며, 마디 테이블과 스크롤 매퍼를 소유합니다.
///
/// 채보 에디터와 리듬게임 씬은 "시각을 받아 노트를 흘려보낸다"는 부분이 완전히 같으므로 이 클래스를 공유합니다.
/// 격자·마디번호처럼 편집에만 필요한 표시는 LiveEditorTimeline이 상속으로 덧붙입니다.
/// </summary>
public class LiveTrackScroller : MonoBehaviour
{
    [Header("Scroll")]
    [Tooltip("노트가 나타나기 시작하는 트랙 깊이입니다. 1이면 트랙 맨 뒤이며, 낮출수록 노트가 판정선까지 오면서 겪는 확대 배율이 줄어듭니다. 트랙 아트웍은 바뀌지 않습니다.")]
    [Range(LiveScrollMapper.MIN_SPAWN_RATIO, LiveScrollMapper.MAX_SPAWN_RATIO)]
    [SerializeField]
    private float _noteSpawnRatio = LiveScrollMapper.MAX_SPAWN_RATIO;

    [Header("Note Marker")]
    [SerializeField]
    private LiveNoteRenderSettings _noteRenderSettings = new LiveNoteRenderSettings();

    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_LiveTrackLanes _lanes;

    [SerializeField]
    private RectTransform _noteLayer;

    private readonly LiveBarLayout _barLayout = new LiveBarLayout();
    private readonly LiveScrollMapper _scrollMapper = new LiveScrollMapper();

    private LiveNoteRenderer _noteRenderer;
    private double _currentBarPosition;

    public UI_LiveTrackLanes Lanes => _lanes;
    public LiveBarLayout BarLayout => _barLayout;
    public LiveNoteRenderer NoteRenderer => _noteRenderer;
    public double CurrentBarPosition => _currentBarPosition;
    public float HiSpeed { get => _scrollMapper.HiSpeed; set => _scrollMapper.HiSpeed = value; }

    protected LiveScrollMapper ScrollMapper => _scrollMapper;

    protected virtual void Awake()
    {
        RefreshScrollShape();

        _noteRenderer = new LiveNoteRenderer(_noteRenderSettings, _noteLayer);
        _noteRenderer.Init(_lanes, _barLayout, _scrollMapper);
    }

#if UNITY_EDITOR
    /// <summary>
    /// 플레이 도중 인스펙터에서 등장 지점을 바꿔 가며 눈으로 확인할 수 있도록, 값이 바뀔 때마다 즉시 반영합니다.
    /// </summary>
    protected virtual void OnValidate()
    {
        RefreshScrollShape();
    }
#endif

    public virtual void SetChart(ChartData chart)
    {
        _noteRenderer.SetChart(chart);
    }

    /// <summary>
    /// 곡 길이를 알게 된 시점(오디오 로드 완료)에 호출되어 곡 전체를 덮는 마디 테이블을 구축합니다.
    /// </summary>
    public virtual void InitBarLayout(ChartData chart, int songLengthMs)
    {
        _barLayout.InitBars(chart, songLengthMs);
    }

    public void RefreshNoteVisuals()
    {
        _noteRenderer.RefreshNoteVisuals();
        _noteRenderer.RefreshNotePositions(_currentBarPosition);
    }

    public virtual void RefreshScroll(int currentTimeMs)
    {
        _currentBarPosition = _barLayout.GetBarPosition(currentTimeMs);
        _noteRenderer.RefreshNotePositions(_currentBarPosition);
    }

    /// <summary>
    /// 노트 등장 지점을 스크롤 매퍼에 반영합니다.
    /// </summary>
    private void RefreshScrollShape()
    {
        _scrollMapper.SpawnRatio = _noteSpawnRatio;
    }
}
