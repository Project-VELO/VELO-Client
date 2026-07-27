using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BPM/분박 기준 그리드 라인과 노트 시각화를 담당하며, 오디오 재생 시간에 맞춰 스크롤 위치를 동기화합니다.
/// 그리드 라인/노트 마커의 생성·해제는 모두 PoolManager를 통해 처리하고, 매 프레임에는 위치 갱신 및
/// 활성/비활성 토글만 수행하여 GC 할당이 발생하지 않도록 합니다.
/// </summary>
public class LiveEditorTimeline : MonoBehaviour
{
    private readonly struct NoteVisualHandle
    {
        public readonly RectTransform RectTransform;
        public readonly EPoolable PoolType;

        public NoteVisualHandle(RectTransform rectTransform, EPoolable poolType)
        {
            RectTransform = rectTransform;
            PoolType = poolType;
        }
    }

    [SerializeField]
    private UI_TrapezoidLanes _lanes;

    [SerializeField]
    private RectTransform _noteLayer;

    [SerializeField]
    private RectTransform _gridLineLayer;

    [SerializeField]
    private ESnapDivision _snapDivision = ESnapDivision.Sixteenth;

    [SerializeField]
    private float _leadTimeMs = 2000f;

    [SerializeField]
    private int _maxGridLineCount = 128;

    private ChartData _chart;
    private float _currentTimeMs;
    private readonly List<RectTransform> _gridLinePool = new List<RectTransform>();
    private readonly List<int> _gridTimeBuffer = new List<int>();
    private readonly Dictionary<string, NoteVisualHandle> _noteVisuals = new Dictionary<string, NoteVisualHandle>();

    public ESnapDivision SnapDivision { get => _snapDivision; set => _snapDivision = value; }
    public UI_TrapezoidLanes Lanes => _lanes;

    private void Awake()
    {
        EnsureGridLinePool();
    }

    public void SetChart(ChartData chart)
    {
        ClearNoteVisuals();
        _chart = chart;
        SpawnNoteVisuals();
    }

    /// <summary>
    /// Undo/Redo 및 커맨드 실행 이후 현재 채보의 Notes 리스트와 시각 오브젝트 풀을 다시 일치시킵니다.
    /// </summary>
    public void SyncNoteVisuals()
    {
        if (_chart == null)
        {
            return;
        }

        var currentIds = new HashSet<string>();
        foreach (NoteData note in _chart.Notes)
        {
            currentIds.Add(note.NoteId);
            if (!_noteVisuals.ContainsKey(note.NoteId))
            {
                EPoolable poolType = GetPoolTypeForNoteType(note.NoteType);
                GameObject go = PoolManager.Instance.Pop(poolType);
                RectTransform rectTransform = go.GetComponent<RectTransform>();
                rectTransform.SetParent(_noteLayer, false);
                _noteVisuals[note.NoteId] = new NoteVisualHandle(rectTransform, poolType);
            }
        }

        List<string> staleIds = null;
        foreach (string noteId in _noteVisuals.Keys)
        {
            if (!currentIds.Contains(noteId))
            {
                staleIds ??= new List<string>();
                staleIds.Add(noteId);
            }
        }

        if (staleIds != null)
        {
            foreach (string noteId in staleIds)
            {
                NoteVisualHandle handle = _noteVisuals[noteId];
                PoolManager.Instance.Push(handle.PoolType, handle.RectTransform.gameObject);
                _noteVisuals.Remove(noteId);
            }
        }

        UpdateNoteVisualPositions();
    }

    public void SyncScroll(float currentTimeMs)
    {
        _currentTimeMs = currentTimeMs;
        RenderGridLines();
        UpdateNoteVisualPositions();
    }

    public void RenderGridLines()
    {
        if (_chart == null || _lanes == null)
        {
            return;
        }

        int startTimeMs = Mathf.RoundToInt(_currentTimeMs);
        int endTimeMs = Mathf.RoundToInt(_currentTimeMs + _leadTimeMs);
        LiveEditorBpmTimeConverter.FillGridTimesInRange(_chart, startTimeMs, endTimeMs, _snapDivision, _gridTimeBuffer);

        float hitLineRatio = _lanes.GetHitLineVerticalRatio();

        for (int i = 0; i < _gridLinePool.Count; i++)
        {
            bool isActive = i < _gridTimeBuffer.Count;
            _gridLinePool[i].gameObject.SetActive(isActive);

            if (!isActive)
            {
                continue;
            }

            float ratio = TimeToVerticalRatio(_gridTimeBuffer[i], hitLineRatio);
            _lanes.GetTrackEdgesAtRatio(ratio, out float leftX, out float rightX, out float y);

            RectTransform lineTransform = _gridLinePool[i];
            lineTransform.anchoredPosition = new Vector2((leftX + rightX) * 0.5f, y);
            lineTransform.sizeDelta = new Vector2(rightX - leftX, lineTransform.sizeDelta.y);
        }
    }

    private void UpdateNoteVisualPositions()
    {
        if (_chart == null || _lanes == null)
        {
            return;
        }

        float hitLineRatio = _lanes.GetHitLineVerticalRatio();

        foreach (NoteData note in _chart.Notes)
        {
            if (!_noteVisuals.TryGetValue(note.NoteId, out NoteVisualHandle handle))
            {
                continue;
            }

            float timeUntilHitMs = note.TimeMs - _currentTimeMs;
            bool isVisible = timeUntilHitMs <= _leadTimeMs && timeUntilHitMs >= -_leadTimeMs * 0.25f;
            handle.RectTransform.gameObject.SetActive(isVisible);

            if (!isVisible)
            {
                continue;
            }

            float ratio = TimeToVerticalRatio(note.TimeMs, hitLineRatio);
            handle.RectTransform.anchoredPosition = _lanes.GetLaneCenterPosition(note.Lane, ratio);
        }
    }

    private float TimeToVerticalRatio(int timeMs, float hitLineRatio)
    {
        float timeUntilHitMs = timeMs - _currentTimeMs;
        return hitLineRatio + (timeUntilHitMs / _leadTimeMs) * (1f - hitLineRatio);
    }

    private void EnsureGridLinePool()
    {
        while (_gridLinePool.Count < _maxGridLineCount)
        {
            GameObject go = PoolManager.Instance.Pop(EPoolable.EditorGridLine);
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.SetParent(_gridLineLayer, false);
            _gridLinePool.Add(rectTransform);
        }
    }

    private void SpawnNoteVisuals()
    {
        if (_chart == null)
        {
            return;
        }

        foreach (NoteData note in _chart.Notes)
        {
            EPoolable poolType = GetPoolTypeForNoteType(note.NoteType);
            GameObject go = PoolManager.Instance.Pop(poolType);
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.SetParent(_noteLayer, false);
            _noteVisuals[note.NoteId] = new NoteVisualHandle(rectTransform, poolType);
        }
    }

    private void ClearNoteVisuals()
    {
        foreach (NoteVisualHandle handle in _noteVisuals.Values)
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


    public int RatioToTimeMs(float verticalRatio)
    {
        if (_lanes == null)
        {
            return Mathf.RoundToInt(_currentTimeMs);
        }

        float hitLineRatio = _lanes.GetHitLineVerticalRatio();
        float denominator = 1f - hitLineRatio;
        float timeUntilHitMs = denominator > 0f ? (verticalRatio - hitLineRatio) / denominator * _leadTimeMs : 0f;
        return Mathf.RoundToInt(_currentTimeMs + timeUntilHitMs);
    }
}
