using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 롱노트 몸통의 배치를 전담합니다. 머리 마커는 LiveNoteRenderer가 일반 노트와 같은 방식으로 처리합니다.
///
/// 트랙이 사다리꼴이라 몸통은 위로 갈수록 좁아지면서 레인을 따라 안쪽으로 기웁니다. 직사각형 하나로는 그 모양이 나오지 않으므로
/// 깊이를 잘게 나눠 각 높이의 노트 자리를 그대로 재고, 그 줄들을 이어 띠로 만듭니다.
/// </summary>
public class LiveHoldNoteRenderer
{
    // 몸통을 나눌 깊이 간격입니다. 겉보기 폭이 깊이에 반비례해 휘므로, 양 끝만 이으면 몸통 경계가 레인에서 떠 보입니다.
    private const float SEGMENT_DEPTH_STEP = 1f / 64f;
    private const int MIN_SEGMENT_COUNT = 1;

    // 줄은 구간 수보다 하나 많으므로, 버퍼 크기에서 거꾸로 구해 두 값이 어긋나지 않게 합니다.
    private const int MAX_SEGMENT_COUNT = LiveHoldBodySample.MAX_COUNT - 1;

    private readonly LiveNoteRenderSettings _settings;
    private readonly LiveNoteDesignLayout _designLayout;
    private readonly List<LiveHoldBodySample> _bodySamples = new List<LiveHoldBodySample>(LiveHoldBodySample.MAX_COUNT);

    private UI_LiveTrackLanes _lanes;
    private LiveScrollMapper _scrollMapper;

    public LiveHoldNoteRenderer(LiveNoteRenderSettings settings, LiveNoteDesignLayout designLayout)
    {
        _settings = settings;
        _designLayout = designLayout;
    }

    public void Init(UI_LiveTrackLanes lanes, LiveScrollMapper scrollMapper)
    {
        _lanes = lanes;
        _scrollMapper = scrollMapper;
    }

    /// <summary>
    /// 몸통을 drawStartRatio부터 꼬리까지 채웁니다. drawStartRatio는 판정선에 먹히고 남은 시작점입니다.
    /// </summary>
    public void RefreshHold(LiveNoteVisualHandle handle, int lane, float headRatio, float tailRatio, float drawStartRatio)
    {
        float endRatio = Mathf.Min(tailRatio, _scrollMapper.SpawnRatio);

        _designLayout.GetNoteRect(_lanes, lane, drawStartRatio, out Vector2 origin, out _);
        RefreshBodySamples(lane, headRatio, drawStartRatio, endRatio, origin);
        handle.HoldVisual.RefreshBody(_bodySamples);
    }

    /// <summary>
    /// 몸통을 이룰 가로 줄들을 쌓습니다. 줄이 타는 선은 그 깊이의 기준선, 곧 머리와 꼬리가 올라타는 시각의 선입니다.
    /// 길이가 0이면 단타와 다를 것이 없으므로 빈 목록을 넘겨 몸통을 감춥니다. 편집 중 길이를 0으로 줄인 롱노트도 여기로 걸립니다.
    /// </summary>
    private void RefreshBodySamples(int lane, float headRatio, float startRatio, float endRatio, Vector2 origin)
    {
        _bodySamples.Clear();

        if (endRatio <= startRatio)
        {
            return;
        }

        // 타일 길이를 트랙 좌표로 재므로 하이스피드를 바꿔도 텍스처 밀도가 일정합니다.
        float tileLength = Mathf.Max(1f, _settings.BodyTileLength);
        float headY = _lanes.GetLocalY(headRatio);
        int segmentCount = Mathf.Clamp(
            Mathf.CeilToInt((endRatio - startRatio) / SEGMENT_DEPTH_STEP), MIN_SEGMENT_COUNT, MAX_SEGMENT_COUNT);

        for (int i = 0; i <= segmentCount; i++)
        {
            float ratio = Mathf.Lerp(startRatio, endRatio, (float)i / segmentCount);
            _designLayout.GetNoteRect(_lanes, lane, ratio, out Vector2 center, out Vector2 size);
            _lanes.GetLaneBoundsAtRatio(lane, ratio, out float laneLeftX, out float laneRightX);

            // 머리 마커가 이미 레인 폭이지만, 여백 설정이 들어와도 몸통이 옆 레인으로 새지 않도록 레인 안으로 잘라 둡니다.
            float leftX = Mathf.Max(center.x - size.x * 0.5f, laneLeftX);
            float rightX = Mathf.Min(center.x + size.x * 0.5f, laneRightX);

            // 줄은 노트 사각형이 아니라 기준선을 따라갑니다. 사각형의 아랫변으로 잡으면 기준선을 노트 한가운데에
            // 맞추는 채보 에디터에서 몸통만 반높이 아래로 처지고 꼬리도 그만큼 일찍 끝납니다.
            float lineY = _lanes.GetLocalY(ratio);

            _bodySamples.Add(new LiveHoldBodySample(
                leftX - origin.x,
                rightX - origin.x,
                lineY - origin.y,
                (lineY - headY) / tileLength));
        }
    }
}
