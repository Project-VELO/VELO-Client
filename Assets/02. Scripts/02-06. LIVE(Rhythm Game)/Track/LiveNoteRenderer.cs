using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 노트 마커의 두께·여백 튜닝값입니다. 렌더러가 POCO라 인스펙터에 직접 노출될 수 없으므로,
/// 렌더러를 소유한 LiveTrackScroller가 이 묶음을 직렬화해 생성 시 넘깁니다.
/// </summary>
[Serializable]
public class LiveNoteRenderSettings
{
    [Tooltip("판정선 높이에서의 노트 두께입니다. 더 위쪽은 원근에 따라 같은 비율로 얇아집니다.\n" +
             "트랙은 눕혀 놓은 띠라 이 값이 화면에서는 원근으로 짧아집니다. 노트 스프라이트가 원본 비율(가로 239 : 세로 56)로 보이려면 " +
             "판정선에서의 화면상 높이가 56px이어야 하므로, 카메라 리그를 바꿨다면 이 값도 다시 맞춰야 합니다.")]
    public float NoteHeight = 16f;

    [Tooltip("멀리 있는 노트가 보이지 않을 만큼 얇아지지 않도록 보장하는 최소 두께입니다.")]
    public float MinNoteHeight = 4f;

    [Tooltip("멀어질수록 노트가 얇아지는 정도입니다. 1이면 원근에 완전히 비례해 얇아지고, 0이면 어디서나 두께가 같습니다. 값이 클수록 두께 변화가 눈에 띕니다.")]
    [Range(0f, 1f)]
    public float ThicknessFalloff = 0.5f;

    [Tooltip("노트 양옆에 남기는 여백을 레인 폭에 대한 비율로 지정합니다. 인접한 레인에 같은 박자로 놓인 노트가 한 덩어리로 보이지 않게 합니다. 픽셀이 아닌 비율이므로 원근에 따라 여백도 함께 좁아집니다.")]
    [Range(0f, 0.25f)]
    public float HorizontalPaddingRatio = 0.04f;

    [Tooltip("롱노트 몸통 텍스처 한 장이 차지하는 트랙 위 세로 길이입니다. 트랙 기준이라 하이스피드를 바꿔도 밀도가 일정합니다.")]
    public float BodyTileLength = 64f;

    /// <summary>
    /// 노트 두께를 그 높이의 원근 배율만큼 조정합니다. 폭만 커지고 두께가 고정이면 다가오는 대신 옆으로 늘어나 보이므로
    /// 두 값을 같은 비율로 묶되, 그대로 두면 두께가 다섯 배 넘게 변해 눈에 거슬리므로 감소량을 조절할 수 있게 했습니다.
    /// </summary>
    public float GetNoteHeightAtRatio(UI_LiveTrackLanes lanes, float verticalRatio)
    {
        float constantScale = lanes.GetFlatThicknessCompensationAtRatio(verticalRatio);
        float perspectiveScale = lanes.GetPerspectiveThicknessScaleAtRatio(verticalRatio);

        return Mathf.Max(MinNoteHeight, NoteHeight * Mathf.Lerp(constantScale, perspectiveScale, ThicknessFalloff));
    }
}

/// <summary>
/// 매 프레임 노트 마커의 위치와 크기를 갱신합니다. 오브젝트를 빌리고 돌려주는 일은 LiveNoteVisualPool이,
/// 롱노트 몸통과 꼬리는 LiveHoldNoteRenderer가 맡습니다.
/// 노트 위치는 절대 시각이 아닌 마디 좌표를 거쳐 계산하므로 화면에 그려진 마디 격자와 항상 정렬됩니다.
///
/// 유니티 이벤트 메서드를 쓰지 않으므로 컴포넌트가 아니라 LiveTrackScroller가 생성해 쓰는 일반 클래스입니다.
/// </summary>
public class LiveNoteRenderer
{
    private readonly LiveNoteRenderSettings _settings;
    private readonly LiveNoteVisualPool _visualPool;
    private readonly LiveHoldNoteRenderer _holdRenderer;
    private readonly LiveNoteLaneFit _laneFit = new LiveNoteLaneFit();
    private readonly RectTransform _noteLayer;
    private readonly LiveNoteSpriteTable _spriteTable;

    // 리듬게임에서 판정이 끝난 노트를 가리는 목록입니다. 채보 데이터에서 노트를 지우면 결과 집계와
    // 다시하기가 망가지므로, 표시 여부만 따로 관리합니다. 채보 에디터는 이 목록을 채우지 않습니다.
    private readonly HashSet<string> _hiddenNoteIds = new HashSet<string>();

    private UI_LiveTrackLanes _lanes;
    private LiveBarLayout _barLayout;
    private LiveScrollMapper _scrollMapper;
    private ChartData _chart;

    public LiveNoteRenderer(LiveNoteRenderSettings settings, RectTransform noteLayer, LiveNoteSpriteTable spriteTable)
    {
        _settings = settings;
        _noteLayer = noteLayer;
        _spriteTable = spriteTable;
        _visualPool = new LiveNoteVisualPool(noteLayer, spriteTable);
        _holdRenderer = new LiveHoldNoteRenderer(settings);
    }

    public void Init(UI_LiveTrackLanes lanes, LiveBarLayout barLayout, LiveScrollMapper scrollMapper)
    {
        _lanes = lanes;
        _barLayout = barLayout;
        _scrollMapper = scrollMapper;
        _holdRenderer.Init(lanes, scrollMapper);
    }

    public void SetChart(ChartData chart)
    {
        _visualPool.ReleaseAll();
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

        _visualPool.RefreshVisuals(_chart.Notes);
    }

    public void RefreshNotePositions(double currentBarPosition)
    {
        if (ReferenceEquals(_chart, null) || _lanes == null)
        {
            return;
        }

        _laneFit.RefreshIfChanged(_lanes, _noteLayer, _spriteTable);

        float hitLineRatio = _lanes.GetHitLineVerticalRatio();
        float trackHeight = GetTrackHeight();

        foreach (NoteData note in _chart.Notes)
        {
            LiveNoteVisualHandle handle;
            if (!_visualPool.TryGetHandle(note.NoteId, out handle))
            {
                continue;
            }

            bool isHold = LiveHoldTracker.IsHoldNote(note) && handle.HoldVisual != null;
            float headRatio = GetVerticalRatio(note.TimeMs, currentBarPosition, hitLineRatio);
            float tailRatio = isHold
                ? GetVerticalRatio(note.TimeMs + note.HoldDurationMs, currentBarPosition, hitLineRatio)
                : headRatio;

            // 가시 판정은 노트의 한 점만 보므로, 반높이만큼 아래로 넓혀 두어야 트랙 끝에서 노트가 잘린 채 사라지지 않습니다.
            // 트랙 끝에 마지막까지 걸리는 것은 꼬리이고 두께는 위로 갈수록 두꺼워지므로, 여유도 꼬리 높이에서 잽니다.
            float halfHeightRatio = GetHalfHeightRatio(tailRatio, trackHeight);
            bool isVisible = _scrollMapper.IsSpanVisible(headRatio, tailRatio + halfHeightRatio)
                && !_hiddenNoteIds.Contains(note.NoteId);
            handle.RectTransform.gameObject.SetActive(isVisible);

            if (!isVisible)
            {
                continue;
            }

            // 롱노트는 머리를 판정선에 세워 두고 몸통이 먹혀 들어가게 합니다. 판정선을 넘긴 만큼이 곧 지나간 길이입니다.
            float drawRatio = isHold ? Mathf.Max(headRatio, hitLineRatio) : headRatio;
            RefreshMarker(handle, note.Lane, drawRatio, isHold);

            // 길이가 0이어도 넘깁니다. 편집 중 길이가 줄어든 롱노트의 몸통이 그대로 남지 않게 하려면 매번 다시 재야 합니다.
            if (handle.HoldVisual != null)
            {
                _holdRenderer.RefreshHold(handle, headRatio, tailRatio, drawRatio);
            }
        }
    }

    private float GetVerticalRatio(int timeMs, double currentBarPosition, float hitLineRatio)
    {
        return _scrollMapper.ToVerticalRatio(_barLayout.GetBarPosition(timeMs), currentBarPosition, hitLineRatio);
    }

    /// <summary>
    /// 노트 두께의 절반을 트랙 세로 비율로 환산합니다. 세로 비율은 트랙 길이에 정비례하므로 나눗셈 한 번이면 됩니다.
    /// </summary>
    private float GetHalfHeightRatio(float verticalRatio, float trackHeight)
    {
        if (trackHeight <= 0f)
        {
            return 0f;
        }

        return _settings.GetNoteHeightAtRatio(_lanes, verticalRatio) * 0.5f / trackHeight;
    }

    private float GetTrackHeight()
    {
        _lanes.GetTrackEdgesAtRatio(0f, out _, out _, out float nearY);
        _lanes.GetTrackEdgesAtRatio(1f, out _, out _, out float farY);

        return farY - nearY;
    }

    /// <summary>
    /// 노트 머리를 제 레인의 그 높이에 놓습니다.
    ///
    /// 마커는 트랙에 눕히지 않고 카메라를 향해 세웁니다. 아트에 이미 원근 기울기가 그려져 있어
    /// 눕히면 카메라가 한 번 더 기울이고 세로로 눌러, 시안과 모양이 달라지고 레인 구분선에서
    /// 떨어집니다(LiveNoteLaneFit 참고). 세운 면은 거리에 따라 크기만 줄어듭니다.
    ///
    /// 세로 기준은 아랫변입니다. 판정선이 위아래 두 줄이고 시안이 그 사이를 노트로 채우므로,
    /// 중심을 맞추면 노트가 아래쪽 줄을 반쯤 넘어갑니다(3차 빌드 피드백).
    /// </summary>
    private void RefreshMarker(LiveNoteVisualHandle handle, int lane, float verticalRatio, bool isHold)
    {
        Vector2 laneCenter = _lanes.GetLaneCenterPosition(lane, verticalRatio);

        _lanes.GetLaneBoundsAtRatio(lane, verticalRatio, out float leftX, out float rightX);
        float laneWidth = (rightX - leftX) * (1f - _settings.HorizontalPaddingRatio * 2f);
        float laneHeight = _settings.GetNoteHeightAtRatio(_lanes, verticalRatio);

        // 롱노트는 몸통이 이 사각형에 붙어 트랙을 따라 자라므로 눕힌 채로 둡니다.
        if (isHold || !_laneFit.IsSolved || _laneFit.Camera == null)
        {
            handle.RectTransform.localRotation = Quaternion.identity;
            handle.RectTransform.anchoredPosition = new Vector2(laneCenter.x, laneCenter.y + laneHeight * 0.5f);
            handle.RectTransform.sizeDelta = new Vector2(laneWidth, laneHeight);
            return;
        }

        Vector2 noteSize = _laneFit.GetNoteSize(lane, new Vector2(laneWidth, laneHeight));
        handle.RectTransform.sizeDelta = noteSize;
        handle.RectTransform.rotation = _laneFit.Camera.transform.rotation;

        _lanes.GetTrackEdgesAtRatio(verticalRatio, out float trackLeftX, out float trackRightX, out _);
        float centerX = _laneFit.GetNoteCenterX(lane, trackRightX - trackLeftX, laneCenter.x);

        // 세운 면이라 아랫변을 트랙 위 제 시각 자리에 두려면 카메라 기준 위쪽으로 반높이만큼 올립니다.
        Vector3 trackPoint = _noteLayer.TransformPoint(new Vector3(centerX, laneCenter.y, 0f));
        handle.RectTransform.position = trackPoint
            + _laneFit.Camera.transform.up * (noteSize.y * 0.5f * _noteLayer.lossyScale.y);
    }
}
