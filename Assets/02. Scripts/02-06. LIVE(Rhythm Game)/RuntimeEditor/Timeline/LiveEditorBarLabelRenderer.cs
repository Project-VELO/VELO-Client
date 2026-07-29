using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 스크롤되는 트랙 왼쪽에 마디 번호를 표시하는 것을 전담합니다.
/// 매 프레임 문자열을 조합하면 GC 할당이 발생하므로, 마디 번호 문자열은 마디 테이블 생성 시 한 번에 만들어 두고
/// 라벨 슬롯에 배정된 마디가 바뀔 때만 텍스트를 교체합니다.
/// </summary>
public class LiveEditorBarLabelRenderer : MonoBehaviour
{
    private const int UNBOUND_BAR_INDEX = -1;

    [Header("Pool Capacity")]
    [SerializeField]
    private int _maxBarLabelCount = 16;

    [Header("Layout")]
    [SerializeField]
    private float _horizontalOffset = -60f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private RectTransform _barLabelLayer;

    private readonly List<TMP_Text> _barLabels = new List<TMP_Text>();
    private readonly List<int> _boundBarIndices = new List<int>();
    private readonly List<string> _barNumberTexts = new List<string>();

    private UI_LiveTrackLanes _lanes;
    private LiveEditorBarLayout _barLayout;
    private LiveEditorScrollMapper _scrollMapper;

    private void Awake()
    {
        FillLabelPool();
    }

    public void Init(UI_LiveTrackLanes lanes, LiveEditorBarLayout barLayout, LiveEditorScrollMapper scrollMapper)
    {
        _lanes = lanes;
        _barLayout = barLayout;
        _scrollMapper = scrollMapper;
    }

    /// <summary>
    /// 마디 테이블이 새로 만들어진 뒤 호출되어, 표시할 마디 번호 문자열을 미리 생성해 둡니다.
    /// </summary>
    public void InitBarNumberTexts()
    {
        _barNumberTexts.Clear();

        if (ReferenceEquals(_barLayout, null))
        {
            return;
        }

        for (int barIndex = 0; barIndex < _barLayout.BarCount; barIndex++)
        {
            _barNumberTexts.Add($"bar {barIndex + 1}");
        }

        for (int i = 0; i < _boundBarIndices.Count; i++)
        {
            _boundBarIndices[i] = UNBOUND_BAR_INDEX;
        }
    }

    public void RefreshBarLabels(double currentBarPosition)
    {
        if (ReferenceEquals(_barLayout, null) || !_barLayout.IsBuilt || _lanes == null)
        {
            DeactivateFrom(0);
            return;
        }

        float hitLineRatio = _lanes.GetHitLineVerticalRatio();
        _scrollMapper.GetVisibleBarRange(currentBarPosition, _barLayout.BarCount, out int startBarIndex, out int endBarIndex);

        int usedLabelCount = 0;

        for (int barIndex = startBarIndex; barIndex <= endBarIndex; barIndex++)
        {
            if (usedLabelCount >= _barLabels.Count || barIndex >= _barNumberTexts.Count)
            {
                break;
            }

            float ratio = _scrollMapper.ToVerticalRatio(barIndex, currentBarPosition, hitLineRatio);

            if (!_scrollMapper.IsRatioVisible(ratio))
            {
                continue;
            }

            PlaceLabel(usedLabelCount, barIndex, ratio);
            usedLabelCount++;
        }

        DeactivateFrom(usedLabelCount);
    }

    private void PlaceLabel(int labelIndex, int barIndex, float verticalRatio)
    {
        TMP_Text label = _barLabels[labelIndex];

        if (_boundBarIndices[labelIndex] != barIndex)
        {
            label.text = _barNumberTexts[barIndex];
            _boundBarIndices[labelIndex] = barIndex;
        }

        _lanes.GetTrackEdgesAtRatio(verticalRatio, out float leftX, out float _, out float y);

        label.gameObject.SetActive(true);
        label.rectTransform.anchoredPosition = new Vector2(leftX + _horizontalOffset, y);

        // 3D 트랙에서 라벨은 바닥에 누워 있어 세로만 거리의 제곱으로 찌그러집니다.
        // 가로는 이미 거리에 반비례해 줄어드므로, 세로도 같은 비율이 되도록 되돌려 글자 비율을 지킵니다.
        float verticalScale = _lanes.GetPerspectiveThicknessScaleAtRatio(verticalRatio);
        label.rectTransform.localScale = new Vector3(1f, verticalScale, 1f);
    }

    private void FillLabelPool()
    {
        while (_barLabels.Count < _maxBarLabelCount)
        {
            GameObject go = PoolManager.Instance.Pop(EPoolable.EditorBarLabel);

            if (go == null)
            {
                return;
            }

            TMP_Text label = go.GetComponent<TMP_Text>();
            label.rectTransform.SetParent(_barLabelLayer, false);
            label.gameObject.SetActive(false);

            _barLabels.Add(label);
            _boundBarIndices.Add(UNBOUND_BAR_INDEX);
        }
    }

    private void DeactivateFrom(int startIndex)
    {
        for (int i = startIndex; i < _barLabels.Count; i++)
        {
            _barLabels[i].gameObject.SetActive(false);
        }
    }
}
