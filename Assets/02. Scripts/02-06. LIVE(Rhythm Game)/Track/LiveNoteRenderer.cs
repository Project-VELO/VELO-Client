using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 매 프레임 노트 마커의 위치와 크기를 갱신합니다. 오브젝트를 빌리고 돌려주는 일은 LiveNoteVisualPool이 맡습니다.
/// 노트 위치는 절대 시각이 아닌 마디 좌표를 거쳐 계산하므로 화면에 그려진 마디 격자와 항상 정렬됩니다.
/// </summary>
public class LiveNoteRenderer : MonoBehaviour
{
    [Header("Note Marker")]
    [Tooltip("판정선 높이에서의 노트 두께입니다. 더 위쪽은 원근에 따라 같은 비율로 얇아집니다.")]
    [SerializeField]
    private float _noteHeight = 16f;

    [Tooltip("멀리 있는 노트가 보이지 않을 만큼 얇아지지 않도록 보장하는 최소 두께입니다.")]
    [SerializeField]
    private float _minNoteHeight = 4f;

    [Tooltip("멀어질수록 노트가 얇아지는 정도입니다. 1이면 원근에 완전히 비례해 얇아지고, 0이면 어디서나 두께가 같습니다. 값이 클수록 두께 변화가 눈에 띕니다.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _thicknessFalloff = 0.5f;

    [Tooltip("노트 양옆에 남기는 여백을 레인 폭에 대한 비율로 지정합니다. 인접한 레인에 같은 박자로 놓인 노트가 한 덩어리로 보이지 않게 합니다. 픽셀이 아닌 비율이므로 원근에 따라 여백도 함께 좁아집니다.")]
    [Range(0f, 0.25f)]
    [SerializeField]
    private float _horizontalPaddingRatio = 0.04f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private RectTransform _noteLayer;

    // 리듬게임에서 판정이 끝난 노트를 가리는 목록입니다. 채보 데이터에서 노트를 지우면 결과 집계와
    // 다시하기가 망가지므로, 표시 여부만 따로 관리합니다. 채보 에디터는 이 목록을 채우지 않습니다.
    private readonly HashSet<string> _hiddenNoteIds = new HashSet<string>();

    private LiveNoteVisualPool _visualPool;
    private UI_LiveTrackLanes _lanes;
    private LiveBarLayout _barLayout;
    private LiveScrollMapper _scrollMapper;
    private ChartData _chart;

    private LiveNoteVisualPool VisualPool => _visualPool ?? (_visualPool = new LiveNoteVisualPool(_noteLayer));

    public void Init(UI_LiveTrackLanes lanes, LiveBarLayout barLayout, LiveScrollMapper scrollMapper)
    {
        _lanes = lanes;
        _barLayout = barLayout;
        _scrollMapper = scrollMapper;
    }

    public void SetChart(ChartData chart)
    {
        VisualPool.ReleaseAll();
        _hiddenNoteIds.Clear();
        _chart = chart;
        RefreshNoteVisuals();
    }

    /// <summary>
    /// 판정이 끝난 노트를 화면에서 지웁니다. 채보의 노트 목록은 그대로 두므로 결과 집계에는 영향이 없습니다.
    /// </summary>
    public void HideNote(string noteId)
    {
        _hiddenNoteIds.Add(noteId);
    }

    public void ClearHiddenNotes()
    {
        _hiddenNoteIds.Clear();
    }

    public void RefreshNoteVisuals()
    {
        if (ReferenceEquals(_chart, null))
        {
            return;
        }

        VisualPool.RefreshVisuals(_chart.Notes);
    }

    public void RefreshNotePositions(double currentBarPosition)
    {
        if (ReferenceEquals(_chart, null) || _lanes == null)
        {
            return;
        }

        float hitLineRatio = _lanes.GetHitLineVerticalRatio();

        foreach (NoteData note in _chart.Notes)
        {
            LiveNoteVisualHandle handle;
            if (!VisualPool.TryGetHandle(note.NoteId, out handle))
            {
                continue;
            }

            double barPosition = _barLayout.GetBarPosition(note.TimeMs);
            float ratio = _scrollMapper.ToVerticalRatio(barPosition, currentBarPosition, hitLineRatio);
            bool isVisible = _scrollMapper.IsRatioVisible(ratio) && !_hiddenNoteIds.Contains(note.NoteId);

            handle.RectTransform.gameObject.SetActive(isVisible);

            if (!isVisible)
            {
                continue;
            }

            handle.RectTransform.anchoredPosition = _lanes.GetLaneCenterPosition(note.Lane, ratio);

            // 사다리꼴 트랙은 높이에 따라 레인 폭이 달라지므로, 노트 가로 폭을 그 높이의 레인 폭에 맞춰 늘립니다.
            // 레인 폭을 꽉 채우면 인접한 레인의 같은 박자 노트와 맞닿아 한 덩어리로 보이므로 양옆을 조금 덜어냅니다.
            float leftX, rightX;
            _lanes.GetLaneBoundsAtRatio(note.Lane, ratio, out leftX, out rightX);
            float noteWidth = (rightX - leftX) * (1f - _horizontalPaddingRatio * 2f);
            handle.RectTransform.sizeDelta = new Vector2(noteWidth, GetNoteHeightAtRatio(ratio));
        }
    }

    /// <summary>
    /// 노트 두께를 그 높이의 원근 배율만큼 조정합니다.
    /// 폭만 커지고 두께가 고정이면 다가오는 것이 아니라 옆으로 늘어나는 것처럼 보이므로 두 값을 같은 비율로 묶되,
    /// 그대로 두면 두께가 다섯 배 넘게 변해 눈에 거슬리므로 감소량을 조절할 수 있게 했습니다.
    ///
    /// 화면상 두께는 넘긴 배율에 정비례하므로, "두께 고정" 배율과 "원근 완전 비례" 배율을 그대로 섞으면
    /// 화면에서도 그 비율만큼 정확히 섞입니다. 트랙이 2D든 3D든 같은 결과가 나옵니다.
    /// </summary>
    private float GetNoteHeightAtRatio(float verticalRatio)
    {
        float constantScale = _lanes.GetFlatThicknessCompensationAtRatio(verticalRatio);
        float perspectiveScale = _lanes.GetPerspectiveThicknessScaleAtRatio(verticalRatio);

        return Mathf.Max(_minNoteHeight, _noteHeight * Mathf.Lerp(constantScale, perspectiveScale, _thicknessFalloff));
    }
}
