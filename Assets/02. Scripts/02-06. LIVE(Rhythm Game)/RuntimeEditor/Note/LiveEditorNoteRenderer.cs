using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 노트 마커 오브젝트의 풀 획득/반환과 매 프레임 위치 갱신을 전담합니다.
/// 노트 위치는 절대 시각이 아닌 마디 좌표를 거쳐 계산하므로 화면에 그려진 마디 격자와 항상 정렬됩니다.
/// 갱신 과정에서 GC 할당이 발생하지 않도록 임시 컬렉션은 모두 필드로 재사용합니다.
/// </summary>
public class LiveEditorNoteRenderer : MonoBehaviour
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

    [Foldout("Hierarchy")]
    [SerializeField]
    private RectTransform _noteLayer;

    private readonly Dictionary<string, LiveEditorNoteVisualHandle> _noteVisuals = new Dictionary<string, LiveEditorNoteVisualHandle>();
    private readonly HashSet<string> _livingNoteIds = new HashSet<string>();
    private readonly List<string> _staleNoteIds = new List<string>();

    private UI_LiveTrackLanes _lanes;
    private LiveEditorBarLayout _barLayout;
    private LiveEditorScrollMapper _scrollMapper;
    private ChartData _chart;

    public void Init(UI_LiveTrackLanes lanes, LiveEditorBarLayout barLayout, LiveEditorScrollMapper scrollMapper)
    {
        _lanes = lanes;
        _barLayout = barLayout;
        _scrollMapper = scrollMapper;
    }

    public void SetChart(ChartData chart)
    {
        ReleaseAllNoteVisuals();
        _chart = chart;
        RefreshNoteVisuals();
    }

    /// <summary>
    /// 현재 채보의 노트 목록과 시각 오브젝트 풀을 다시 일치시킵니다. 커맨드 실행 및 Undo/Redo 직후에 호출됩니다.
    /// </summary>
    public void RefreshNoteVisuals()
    {
        if (ReferenceEquals(_chart, null))
        {
            return;
        }

        _livingNoteIds.Clear();

        foreach (NoteData note in _chart.Notes)
        {
            _livingNoteIds.Add(note.NoteId);

            if (_noteVisuals.ContainsKey(note.NoteId))
            {
                continue;
            }

            AcquireNoteVisual(note);
        }

        ReleaseStaleNoteVisuals();
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
            if (!_noteVisuals.TryGetValue(note.NoteId, out LiveEditorNoteVisualHandle handle))
            {
                continue;
            }

            double barPosition = _barLayout.GetBarPosition(note.TimeMs);
            float ratio = _scrollMapper.ToVerticalRatio(barPosition, currentBarPosition, hitLineRatio);
            bool isVisible = _scrollMapper.IsRatioVisible(ratio);

            handle.RectTransform.gameObject.SetActive(isVisible);

            if (!isVisible)
            {
                continue;
            }

            handle.RectTransform.anchoredPosition = _lanes.GetLaneCenterPosition(note.Lane, ratio);

            // 사다리꼴 트랙은 높이에 따라 레인 폭이 달라지므로, 노트 가로 폭을 그 높이의 레인 폭에 맞춰 늘립니다.
            _lanes.GetLaneBoundsAtRatio(note.Lane, ratio, out float leftX, out float rightX);
            handle.RectTransform.sizeDelta = new Vector2(rightX - leftX, GetNoteHeightAtRatio(ratio));
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

    private void AcquireNoteVisual(NoteData note)
    {
        EPoolable poolType = GetPoolTypeForNoteType(note.NoteType);
        GameObject go = PoolManager.Instance.Pop(poolType);

        if (go == null)
        {
            return;
        }

        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.SetParent(_noteLayer, false);
        _noteVisuals[note.NoteId] = new LiveEditorNoteVisualHandle(rectTransform, poolType);
    }

    private void ReleaseStaleNoteVisuals()
    {
        _staleNoteIds.Clear();

        foreach (string noteId in _noteVisuals.Keys)
        {
            if (!_livingNoteIds.Contains(noteId))
            {
                _staleNoteIds.Add(noteId);
            }
        }

        foreach (string noteId in _staleNoteIds)
        {
            LiveEditorNoteVisualHandle handle = _noteVisuals[noteId];
            PoolManager.Instance.Push(handle.PoolType, handle.RectTransform.gameObject);
            _noteVisuals.Remove(noteId);
        }
    }

    private void ReleaseAllNoteVisuals()
    {
        foreach (LiveEditorNoteVisualHandle handle in _noteVisuals.Values)
        {
            PoolManager.Instance.Push(handle.PoolType, handle.RectTransform.gameObject);
        }

        _noteVisuals.Clear();
    }

    private static EPoolable GetPoolTypeForNoteType(ENoteType noteType)
    {
        switch (noteType)
        {
            case ENoteType.GHOST:
                return EPoolable.EditorNoteGhost;
            case ENoteType.LONG:
                return EPoolable.EditorNoteLong;
            default:
                return EPoolable.EditorNoteNormal;
        }
    }
}
