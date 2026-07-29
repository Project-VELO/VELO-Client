using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 마디 경계선과 분박선을 그리는 것을 전담합니다.
/// 선의 위치를 절대 시각이 아닌 마디 좌표에서 직접 산출하므로 BPM 변환 없이 매 프레임 GC 할당 없이 갱신됩니다.
/// 선 오브젝트는 Awake에서 최대 개수만큼 풀에서 미리 확보한 뒤, 이후에는 활성/비활성 토글만 수행합니다.
/// </summary>
public class LiveEditorGridRenderer : MonoBehaviour
{
    [Header("Pool Capacity")]
    [SerializeField]
    private int _maxSubdivisionLineCount = 256;

    [SerializeField]
    private int _maxBarLineCount = 16;

    [Foldout("Hierarchy")]
    [SerializeField]
    private RectTransform _gridLineLayer;

    private readonly List<RectTransform> _subdivisionLines = new List<RectTransform>();
    private readonly List<RectTransform> _barLines = new List<RectTransform>();

    private UI_LiveTrackLanes _lanes;
    private LiveEditorBarLayout _barLayout;
    private LiveEditorScrollMapper _scrollMapper;

    private void Awake()
    {
        FillLinePool(_subdivisionLines, EPoolable.EditorGridLine, _maxSubdivisionLineCount);
        FillLinePool(_barLines, EPoolable.EditorBarLine, _maxBarLineCount);
    }

    public void Init(UI_LiveTrackLanes lanes, LiveEditorBarLayout barLayout, LiveEditorScrollMapper scrollMapper)
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
                PlaceLine(_barLines[usedBarLineCount], barRatio);
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
            if (usedLineCount >= _subdivisionLines.Count)
            {
                break;
            }

            double barPosition = barIndex + (double)cellIndex / cellsPerBar;
            float ratio = _scrollMapper.ToVerticalRatio(barPosition, currentBarPosition, hitLineRatio);

            if (!_scrollMapper.IsRatioVisible(ratio))
            {
                continue;
            }

            PlaceLine(_subdivisionLines[usedLineCount], ratio);
            usedLineCount++;
        }

        return usedLineCount;
    }

    private void PlaceLine(RectTransform lineTransform, float verticalRatio)
    {
        _lanes.GetTrackEdgesAtRatio(verticalRatio, out float leftX, out float rightX, out float y);

        lineTransform.gameObject.SetActive(true);
        lineTransform.anchoredPosition = new Vector2((leftX + rightX) * 0.5f, y);
        lineTransform.sizeDelta = new Vector2(rightX - leftX, lineTransform.sizeDelta.y);
    }

    private void FillLinePool(List<RectTransform> lines, EPoolable poolType, int capacity)
    {
        while (lines.Count < capacity)
        {
            GameObject go = PoolManager.Instance.Pop(poolType);

            if (go == null)
            {
                return;
            }

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.SetParent(_gridLineLayer, false);
            rectTransform.gameObject.SetActive(false);
            lines.Add(rectTransform);
        }
    }

    private static void DeactivateFrom(List<RectTransform> lines, int startIndex)
    {
        for (int i = startIndex; i < lines.Count; i++)
        {
            lines[i].gameObject.SetActive(false);
        }
    }
}
