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
    [Tooltip("판정선 높이에서의 노트 두께입니다. 더 위쪽은 원근에 따라 같은 비율로 얇아집니다.")]
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
            float halfHeightRatio = GetHalfHeightRatio(headRatio, trackHeight);
            bool isVisible = _scrollMapper.IsSpanVisible(headRatio, tailRatio + halfHeightRatio)
                && !_hiddenNoteIds.Contains(note.NoteId);
            handle.RectTransform.gameObject.SetActive(isVisible);

            if (!isVisible)
            {
                continue;
            }

            // 롱노트는 머리를 판정선에 세워 두고 몸통이 먹혀 들어가게 합니다. 판정선을 넘긴 만큼이 곧 지나간 길이입니다.
            float drawRatio = isHold ? Mathf.Max(headRatio, hitLineRatio) : headRatio;
            RefreshMarker(handle, note.Lane, drawRatio);

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
    /// 노트 머리를 그 높이의 레인 폭에 맞춰 놓습니다.
    /// 레인 폭을 꽉 채우면 인접한 레인의 같은 박자 노트와 맞닿아 한 덩어리로 보이므로 양옆을 조금 덜어냅니다.
    /// </summary>
    private void RefreshMarker(LiveNoteVisualHandle handle, int lane, float verticalRatio)
    {
        handle.RectTransform.anchoredPosition = _lanes.GetLaneCenterPosition(lane, verticalRatio);

        float leftX, rightX;
        _lanes.GetLaneBoundsAtRatio(lane, verticalRatio, out leftX, out rightX);
        float noteWidth = (rightX - leftX) * (1f - _settings.HorizontalPaddingRatio * 2f);

        handle.RectTransform.sizeDelta = new Vector2(noteWidth, _settings.GetNoteHeightAtRatio(_lanes, verticalRatio));
    }
}
