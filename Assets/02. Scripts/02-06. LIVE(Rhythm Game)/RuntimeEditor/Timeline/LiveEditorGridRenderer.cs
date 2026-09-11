using UnityEngine;
using VInspector;

/// <summary>
/// 마디 경계선과 분박선을 그리는 것을 전담합니다.
/// 선의 위치를 절대 시각이 아닌 마디 좌표에서 직접 산출하므로 BPM 변환 없이 매 프레임 GC 할당 없이 갱신됩니다.
/// 선 오브젝트를 빌리고 돌려주는 일은 LiveEditorGridLinePool이 맡습니다.
/// </summary>
public class LiveEditorGridRenderer : MonoBehaviour
{
    // 화면 두께가 1픽셀 아래로 내려가면 스크롤할 때 픽셀에 걸렸다 말았다 하며 선이 깜빡이므로 하한을 둡니다.
    private const float MIN_LINE_THICKNESS = 1f;

    [Header("Pool Capacity")]
    [SerializeField]
    private int _maxSubdivisionLineCount = 256;

    [SerializeField]
    private int _maxBarLineCount = 16;

    [Header("Thickness")]
    [Tooltip("멀리 있는 선을 트랙과 같은 비율로 얇게 만드는 정도입니다. 1이면 트랙과 함께 줄어들어 원근이 살고, 0이면 어느 높이에서나 화면상 두께가 같습니다.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _depthShrink = 1f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private RectTransform _gridLineLayer;

    private readonly LiveEditorGridLinePool _subdivisionLines = new LiveEditorGridLinePool(EPoolable.EditorGridLine);
    private readonly LiveEditorGridLinePool _barLines = new LiveEditorGridLinePool(EPoolable.EditorBarLine);

    private UI_LiveTrackLanes _lanes;
    private LiveBarLayout _barLayout;
    private LiveScrollMapper _scrollMapper;

    private void Awake()
    {
        _subdivisionLines.Fill(_gridLineLayer, _maxSubdivisionLineCount);
        _barLines.Fill(_gridLineLayer, _maxBarLineCount);
    }

    private void OnDestroy()
    {
        _subdivisionLines.ReturnAll();
        _barLines.ReturnAll();
    }

    public void Init(UI_LiveTrackLanes lanes, LiveBarLayout barLayout, LiveScrollMapper scrollMapper)
    {
        _lanes = lanes;
        _barLayout = barLayout;
        _scrollMapper = scrollMapper;
    }

    public void RefreshGrid(double currentBarPosition, ESnapDivision division)
    {
        if (ReferenceEquals(_barLayout, null) || !_barLayout.IsBuilt || _lanes == null)
        {
            _barLines.DeactivateFrom(0);
            _subdivisionLines.DeactivateFrom(0);
            return;
        }

        float hitLineRatio = _lanes.GetHitLineVerticalRatio();
        _scrollMapper.GetVisibleBarRange(currentBarPosition, _barLayout.BarCount, out int startBarIndex, out int endBarIndex);
        int cellsPerBar = _barLayout.GetCellsPerBar(division);

        int usedBarLineCount = 0;
        int usedSubdivisionLineCount = 0;

        for (int barIndex = startBarIndex; barIndex <= endBarIndex; barIndex++)
        {
            float barRatio = _scrollMapper.ToVerticalRatio(barIndex, currentBarPosition, hitLineRatio);
            if (_scrollMapper.IsRatioVisible(barRatio) && usedBarLineCount < _barLines.Count)
            {
                PlaceLine(_barLines.GetLine(usedBarLineCount), _barLines.BaseThickness, barRatio);
                usedBarLineCount++;
            }

            usedSubdivisionLineCount = FillBarSubdivisionLines(barIndex, cellsPerBar, currentBarPosition, hitLineRatio, usedSubdivisionLineCount);
        }

        _barLines.DeactivateFrom(usedBarLineCount);
        _subdivisionLines.DeactivateFrom(usedSubdivisionLineCount);
    }

    /// <summary>
    /// 마디 하나 안쪽의 분박선을 배치하고, 다음에 사용할 선 인덱스를 반환합니다.
    /// 마디 경계(cellIndex 0)는 마디선이 담당하므로 1번 셀부터 그립니다.
    /// 음원이 끝난 뒤의 칸에는 노트를 놓을 수 없으므로(LiveBarLayout.TryGetCellAtBarPosition) 선도 그리지 않습니다.
    /// </summary>
    private int FillBarSubdivisionLines(int barIndex, int cellsPerBar, double currentBarPosition, float hitLineRatio, int usedLineCount)
    {
        for (int cellIndex = 1; cellIndex < cellsPerBar; cellIndex++)
        {
            if (_subdivisionLines.Count <= usedLineCount)
            {
                break;
            }

            double barPosition = barIndex + (double)cellIndex / cellsPerBar;

            if (_barLayout.SongEndBarPosition < barPosition)
            {
                break;
            }

            float ratio = _scrollMapper.ToVerticalRatio(barPosition, currentBarPosition, hitLineRatio);

            if (!_scrollMapper.IsRatioVisible(ratio))
            {
                continue;
            }

            PlaceLine(_subdivisionLines.GetLine(usedLineCount), _subdivisionLines.BaseThickness, ratio);
            usedLineCount++;
        }

        return usedLineCount;
    }

    /// <summary>
    /// 선을 배치하면서 두께에 그 높이의 트랙 축소 배율을 곱합니다.
    /// 선을 트랙과 같이 좁혀야 멀리 있는 마디가 원근감 있게 보이지만, 그대로 두면 원경에서 서브픽셀까지 얇아져
    /// 스크롤할 때 깜빡이므로 하한을 둡니다.
    /// </summary>
    private void PlaceLine(RectTransform lineTransform, float baseThickness, float verticalRatio)
    {
        _lanes.GetTrackEdgesAtRatio(verticalRatio, out float leftX, out float rightX, out float y);

        float depthScale = Mathf.Lerp(1f, _lanes.GetApparentScaleAtRatio(verticalRatio), _depthShrink);
        float thickness = Mathf.Max(MIN_LINE_THICKNESS, baseThickness * depthScale);

        lineTransform.gameObject.SetActive(true);
        lineTransform.anchoredPosition = new Vector2((leftX + rightX) * 0.5f, y);
        lineTransform.sizeDelta = new Vector2(rightX - leftX, thickness);
    }
}
