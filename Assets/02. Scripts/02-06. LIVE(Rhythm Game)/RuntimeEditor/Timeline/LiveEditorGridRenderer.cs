using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 마디 경계선과 분박선을 그리는 것을 전담합니다.
/// 선의 위치를 절대 시각이 아닌 마디 좌표에서 직접 산출하므로 BPM 변환 없이 매 프레임 GC 할당 없이 갱신됩니다.
/// 선 오브젝트는 Awake에서 최대 개수만큼 풀에서 미리 확보한 뒤, 이후에는 활성/비활성 토글만 수행합니다.
/// 확보한 선은 매 프레임 반환하지 않으므로 파괴될 때 한 번에 풀로 되돌려 줍니다.
/// </summary>
public class LiveEditorGridRenderer : MonoBehaviour
{
    [Header("Pool Capacity")]
    [SerializeField]
    private int _maxSubdivisionLineCount = 256;

    [SerializeField]
    private int _maxBarLineCount = 16;

    [Header("Thickness")]
    [Tooltip("멀리 있는 선의 두께를 되살리는 정도입니다. 1이면 어느 높이에서나 화면상 두께가 같고, 0이면 보정을 끕니다. 0에서는 원경의 선이 1픽셀 아래로 내려가 스크롤할 때 깜빡입니다.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _thicknessCompensation = 1f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private RectTransform _gridLineLayer;

    private readonly List<RectTransform> _subdivisionLines = new List<RectTransform>();
    private readonly List<RectTransform> _barLines = new List<RectTransform>();

    private UI_LiveTrackLanes _lanes;
    private LiveBarLayout _barLayout;
    private LiveScrollMapper _scrollMapper;

    // 매 프레임 두께에 원근 보정을 곱하므로, 배율이 누적되지 않도록 프리팹 원본 두께를 따로 기억해 둡니다.
    private float _subdivisionLineThickness;
    private float _barLineThickness;

    private void Awake()
    {
        _subdivisionLineThickness = FillLinePool(_subdivisionLines, EPoolable.EditorGridLine, _maxSubdivisionLineCount);
        _barLineThickness = FillLinePool(_barLines, EPoolable.EditorBarLine, _maxBarLineCount);
    }

    private void OnDestroy()
    {
        ReturnLinePool(_subdivisionLines, EPoolable.EditorGridLine);
        ReturnLinePool(_barLines, EPoolable.EditorBarLine);
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
            DeactivateFrom(_barLines, 0);
            DeactivateFrom(_subdivisionLines, 0);
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
                PlaceLine(_barLines[usedBarLineCount], _barLineThickness, barRatio);
                usedBarLineCount++;
            }

            usedSubdivisionLineCount = FillBarSubdivisionLines(barIndex, cellsPerBar, currentBarPosition, hitLineRatio, usedSubdivisionLineCount);
        }

        DeactivateFrom(_barLines, usedBarLineCount);
        DeactivateFrom(_subdivisionLines, usedSubdivisionLineCount);
    }

    /// <summary>
    /// 마디 하나 안쪽의 분박선을 배치하고, 다음에 사용할 선 인덱스를 반환합니다.
    /// 마디 경계(cellIndex 0)는 마디선이 담당하므로 1번 셀부터 그립니다.
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
            float ratio = _scrollMapper.ToVerticalRatio(barPosition, currentBarPosition, hitLineRatio);

            if (!_scrollMapper.IsRatioVisible(ratio))
            {
                continue;
            }

            PlaceLine(_subdivisionLines[usedLineCount], _subdivisionLineThickness, ratio);
            usedLineCount++;
        }

        return usedLineCount;
    }

    /// <summary>
    /// 선을 배치하면서 두께에 원근 보정을 곱합니다.
    /// 3D 트랙에서 누운 선은 멀어질수록 거리의 제곱으로 얇아지는데, 화면 두께가 1픽셀 아래로 내려가면
    /// 스크롤할 때 픽셀에 걸렸다 말았다 하며 깜빡이므로 트랙이 알려 주는 배율로 되돌려 줍니다.
    /// </summary>
    private void PlaceLine(RectTransform lineTransform, float baseThickness, float verticalRatio)
    {
        _lanes.GetTrackEdgesAtRatio(verticalRatio, out float leftX, out float rightX, out float y);

        lineTransform.gameObject.SetActive(true);
        lineTransform.anchoredPosition = new Vector2((leftX + rightX) * 0.5f, y);
        float compensation = Mathf.Lerp(1f, _lanes.GetFlatThicknessCompensationAtRatio(verticalRatio), _thicknessCompensation);
        lineTransform.sizeDelta = new Vector2(rightX - leftX, baseThickness * compensation);
    }

    /// <summary>
    /// 선 오브젝트를 미리 확보하고, 프리팹에 설정된 원본 두께를 돌려줍니다.
    /// </summary>
    private float FillLinePool(List<RectTransform> lines, EPoolable poolType, int capacity)
    {
        float baseThickness = 0f;

        while (lines.Count < capacity)
        {
            GameObject go = PoolManager.Instance.Pop(poolType);

            if (go == null)
            {
                break;
            }

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            baseThickness = rectTransform.sizeDelta.y;
            rectTransform.SetParent(_gridLineLayer, false);
            rectTransform.gameObject.SetActive(false);
            lines.Add(rectTransform);
        }

        return baseThickness;
    }

    /// <summary>
    /// 확보해 둔 선을 풀로 되돌립니다.
    /// 풀은 꺼내 간 오브젝트를 추적하므로 반환하지 않고 사라지면 사용량 집계가 실제와 어긋나고,
    /// 풀 정리 시점에 주인 없는 오브젝트가 남습니다.
    /// 풀이 먼저 파괴된 뒤라면 되돌릴 곳이 없으므로 목록만 비웁니다.
    /// </summary>
    private static void ReturnLinePool(List<RectTransform> lines, EPoolable poolType)
    {
        if (PoolManager.HasInstance)
        {
            foreach (RectTransform line in lines)
            {
                if (line != null)
                {
                    PoolManager.Instance.Push(poolType, line.gameObject);
                }
            }
        }

        lines.Clear();
    }

    private static void DeactivateFrom(List<RectTransform> lines, int startIndex)
    {
        for (int i = startIndex; i < lines.Count; i++)
        {
            lines[i].gameObject.SetActive(false);
        }
    }
}
