using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 롱노트 몸통의 배치를 전담합니다. 머리 마커는 LiveNoteRenderer가 일반 노트와 같은 방식으로 처리합니다.
///
/// 트랙이 사다리꼴이라 몸통은 위로 갈수록 좁아지면서 레인을 따라 안쪽으로 기웁니다. 직사각형 하나로는 그 모양이 나오지 않으므로
/// 깊이를 잘게 나눠 각 높이의 노트 자리를 그대로 재고, 그 줄들을 이어 띠로 만듭니다.
///
/// 몸통 그림은 반복 무늬가 아니라 레인 띠를 화면 높이 1080 전체에 보이는 모양 그대로 1:1로 그린 것입니다.
/// 멀수록 작아지는 무늬와 트랙 끝의 흐려짐이 그림에 이미 들어 있어 노트를 따라 흘려보내면 원근이 어긋나므로,
/// 그림은 화면에 고정해 두고 몸통이 덮은 구간만 드러냅니다.
/// </summary>
public class LiveHoldNoteRenderer
{
    // 몸통을 나눌 깊이 간격입니다. 겉보기 폭이 깊이에 반비례해 휘므로, 양 끝만 이으면 몸통 경계가 레인에서 떠 보입니다.
    private const float SEGMENT_DEPTH_STEP = 1f / 64f;
    private const int MIN_SEGMENT_COUNT = 1;

    // 줄은 구간 수보다 하나 많으므로, 버퍼 크기에서 거꾸로 구해 두 값이 어긋나지 않게 합니다.
    private const int MAX_SEGMENT_COUNT = LiveHoldBodySample.MAX_COUNT - 1;

    // 몸통 그림(Image_Live_Note_Long_Lane1~6)의 왼쪽 끝이 놓이는 트랙 가로 좌표입니다(트랙 한가운데가 0, 시안 px).
    // 그림마다 잘라 낸 여백이 달라(2·5번은 띠 밖으로 삐져나온 무늬 때문에 26px쯤 넓습니다) 그림 크기로는 자리를 정할 수 없어,
    // 높이마다 띠 가장자리(알파 50%)를 재고 레인 경계와의 좌우 여유가 같아지는 자리를 구했습니다. 모든 높이에서 1px 안으로 맞습니다.
    // 그림을 다시 뽑으면 이 값도 다시 재야 합니다.
    private static readonly List<float> BODY_ART_LEFT_XS = new List<float> { -706f, -493f, -219f, 1f, 42f, 86f };

    private readonly LiveNoteDesignLayout _designLayout;
    private readonly List<LiveHoldBodySample> _bodySamples = new List<LiveHoldBodySample>(LiveHoldBodySample.MAX_COUNT);

    private UI_LiveTrackLanes _lanes;
    private LiveScrollMapper _scrollMapper;

    public LiveHoldNoteRenderer(LiveNoteDesignLayout designLayout)
    {
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
    public void RefreshHold(LiveNoteVisualHandle handle, int lane, float tailRatio, float drawStartRatio)
    {
        float endRatio = Mathf.Min(tailRatio, _scrollMapper.SpawnRatio);

        _designLayout.GetNoteRect(_lanes, lane, drawStartRatio, out Vector2 origin, out _);
        RefreshBodySamples(lane, drawStartRatio, endRatio, origin);
        handle.HoldVisual.RefreshBody(_bodySamples);
    }

    /// <summary>
    /// 몸통을 이룰 가로 줄들을 쌓습니다. 줄이 타는 선은 그 깊이의 기준선, 곧 머리와 꼬리가 올라타는 시각의 선입니다.
    /// 길이가 0이면 단타와 다를 것이 없으므로 빈 목록을 넘겨 몸통을 감춥니다. 편집 중 길이를 0으로 줄인 롱노트도 여기로 걸립니다.
    /// </summary>
    private void RefreshBodySamples(int lane, float startRatio, float endRatio, Vector2 origin)
    {
        _bodySamples.Clear();

        if (endRatio <= startRatio)
        {
            return;
        }

        float artLeftX = BODY_ART_LEFT_XS[lane - LiveLane.FIRST];
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
                _lanes.GetDesignX(leftX) - artLeftX,
                _lanes.GetDesignX(rightX) - artLeftX,
                _lanes.Shape.GetScreenYAtRatio(ratio)));
        }
    }
}
