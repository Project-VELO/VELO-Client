using UnityEngine;

/// <summary>
/// 시안이 정한 노트 크기를 그대로 지키면서, 놓을 자리는 그 높이의 레인에서 구합니다.
///
/// 크기는 시안 수치를 씁니다. 판정선 높이에서 노트가 원본 픽셀 크기 그대로 보여야 하므로,
/// 그 높이의 트랙 폭에 대한 비율로 환산해 두고 깊이에 따라 함께 줄입니다.
/// 트랙 뒤쪽에서 작아지는 것은 원근이지만, 판정선까지 내려오면 다시 원본 크기가 됩니다.
///
/// 자리는 시안이 적어 둔 왼쪽 좌표를 쓰지 않습니다. 그 좌표는 노트를 레인 중앙이 아니라 안쪽으로 10px쯤
/// 밀어 두어, 1번·6번처럼 바깥 레인의 노트가 한쪽 구분선만 19.5px 넘어갔습니다. 구분선을 그리는 트랙 아트를
/// 실측하면 305.5 / 528.5 / 754.5 / 960.5 / 1166.5 / 1393.5 / 1613.5 이고 UI_LiveTrackLanes의 레인 경계와
/// 1px 안쪽으로 일치하므로, 가로 중심은 레인에서 직접 잡습니다.
///
/// 노트 그림은 투명 여백을 빼도 레인보다 넓어서(판정선에서 226 대 222), 원본 크기를 지키는 한 구분선을 조금 넘습니다.
/// 크기를 줄이면 없앨 수 있지만(전 구간을 덮으려면 0.92배) 판정선 노트가 눈에 띄게 작아지므로, 크기를 지키고
/// 넘침은 좌우로 고르게 나누는 쪽을 택했습니다. 없애려면 노트 아트를 레인 폭과 트랙 기울기에 맞춰 다시 뽑아야 합니다.
/// </summary>
public class LiveNoteDesignLayout
{
    // 판정선 높이에서의 노트 크기입니다(1920x1080 기준). 노트 스프라이트의 원본 픽셀 크기와 같습니다.
    private const float DESIGN_NOTE_HEIGHT = 56f;
    private static readonly float[] DESIGN_NOTE_WIDTHS = { 239f, 242f, 218f, 218f, 242f, 239f };

    private readonly bool _isCenteredOnBeatLine;

    public LiveNoteDesignLayout(bool isCenteredOnBeatLine)
    {
        _isCenteredOnBeatLine = isCenteredOnBeatLine;
    }

    /// <summary>
    /// 그 높이에서 노트가 놓일 자리와 크기입니다. 가로 중심은 그 높이의 레인 한가운데입니다.
    ///
    /// 세로 기준은 기본이 아랫변입니다. 판정선이 위아래 두 줄이고 시안이 그 사이를 노트로 채우므로,
    /// 중심을 맞추면 노트가 아래쪽 줄을 반쯤 넘어갑니다.
    /// 반면 박자선이 한 줄뿐인 채보 에디터에서는 아랫변을 맞추면 노트가 선 위에 통째로 올라앉아
    /// 선이 그대로 드러나므로, 한가운데를 맞춰 노트가 선을 덮게 합니다.
    /// </summary>
    public void GetNoteRect(UI_LiveTrackLanes lanes, int lane, float verticalRatio,
        out Vector2 center, out Vector2 size)
    {
        center = Vector2.zero;
        size = Vector2.zero;

        if (lanes == null || !LiveLane.IsValid(lane))
        {
            return;
        }

        lanes.GetTrackEdgesAtRatio(verticalRatio, out float trackLeftX, out float trackRightX, out float trackY);

        float trackWidth = trackRightX - trackLeftX;
        float designRatio = GetDesignRatio(lanes, DESIGN_NOTE_WIDTHS[lane - LiveLane.FIRST]);
        float height = GetNoteHeight(lanes, verticalRatio);
        float centerY = trackY + (_isCenteredOnBeatLine ? 0f : height * 0.5f);

        // 가로 중심은 노트가 놓이는 높이가 아니라 노트 세로 한가운데 높이에서 잡습니다.
        // 레인 중심은 위로 갈수록 트랙 안쪽으로 이동하는데, 아랫변 높이에서 잡으면 위로 뻗은 몸통이
        // 그 이동량만큼 통째로 바깥쪽으로 밀려 바깥 레인 노트가 옆 레인을 넘어갑니다.
        lanes.GetLaneBoundsAtRatio(lane, lanes.GetRatioAtLocalY(centerY), out float laneLeftX, out float laneRightX);

        size = new Vector2(designRatio * trackWidth, height);
        center = new Vector2((laneLeftX + laneRightX) * 0.5f, centerY);
    }

    /// <summary>
    /// 그 높이에서의 노트 두께입니다. 롱노트 꼬리와 화면 밖 판정처럼 크기만 필요한 곳에서 씁니다.
    /// </summary>
    public float GetNoteHeight(UI_LiveTrackLanes lanes, float verticalRatio)
    {
        if (lanes == null)
        {
            return 0f;
        }

        lanes.GetTrackEdgesAtRatio(verticalRatio, out float trackLeftX, out float trackRightX, out _);

        return GetDesignRatio(lanes, DESIGN_NOTE_HEIGHT) * (trackRightX - trackLeftX);
    }

    /// <summary>
    /// 시안 수치를 판정선 높이의 트랙 폭에 대한 비율로 바꿉니다.
    /// 이 비율에 그 높이의 트랙 폭을 곱하면, 멀수록 작아지고 판정선에서는 원본 크기가 되는 값이 나옵니다.
    /// </summary>
    private static float GetDesignRatio(UI_LiveTrackLanes lanes, float designPixels)
    {
        float hitLineWidth = lanes.Shape.GetWidthAtRatio(lanes.GetHitLineVerticalRatio());

        return hitLineWidth <= 0f ? 0f : designPixels / hitLineWidth;
    }
}
