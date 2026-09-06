using UnityEngine;

/// <summary>
/// 시안이 정한 노트 자리와 크기를 트랙 좌표로 옮깁니다.
///
/// 외주 시안(Figma Section 4 - 리듬게임 진행 화면)은 판정선 높이에 6개 노트를 원본 픽셀 크기 그대로 올려 두고,
/// 그 상태에서 노트가 레인 좌우 구분선과 맞물립니다. 노트가 기울어진 평행사변형이라 그림이 레인을 채우는 자리와
/// 사각형의 자리가 다르므로, 레인 중심에서 계산하지 않고 시안이 적어 둔 왼쪽 좌표를 그대로 씁니다.
/// 바깥 레인일수록 차이가 커서 11px까지 벌어집니다.
///
/// 좌표는 트랙 폭에 대한 비율로 환산해 두므로 깊이가 달라져도, 화면 해상도가 달라져도 같은 그림이 나옵니다.
/// 시안의 맨 위 노트가 43x10, 판정선 노트가 239x56인 것이 곧 "노트는 트랙 폭에 정비례해 작아진다"는 뜻입니다.
/// </summary>
public class LiveNoteDesignLayout
{
    private const float DESIGN_SCREEN_WIDTH = 1920f;
    private const float DESIGN_NOTE_HEIGHT = 56f;

    // 판정선 높이에서 각 노트의 화면 왼쪽 좌표입니다(1920x1080 기준).
    private static readonly float[] DESIGN_NOTE_LEFTS = { 308f, 526f, 751f, 954f, 1154f, 1373f };

    // 같은 높이에서의 노트 폭이며, 프로젝트에 들어와 있는 노트 스프라이트의 원본 가로 크기와 같습니다.
    private static readonly float[] DESIGN_NOTE_WIDTHS = { 239f, 242f, 218f, 218f, 243f, 239f };

    /// <summary>
    /// 그 높이에서 노트가 놓일 자리와 크기입니다. 세로는 아랫변을 기준으로 잡습니다.
    /// 판정선이 위아래 두 줄이고 시안이 그 사이를 노트로 채우므로, 중심을 맞추면 아래쪽 줄을 반쯤 넘어갑니다.
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

        GetDesignRatios(lanes, lane, out float leftRatio, out float widthRatio, out float heightRatio);

        size = new Vector2(widthRatio * trackWidth, heightRatio * trackWidth);
        center = new Vector2(trackLeftX + (leftRatio + widthRatio * 0.5f) * trackWidth, trackY + size.y * 0.5f);
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
        GetDesignRatios(lanes, LiveLane.FIRST, out _, out _, out float heightRatio);

        return heightRatio * (trackRightX - trackLeftX);
    }

    /// <summary>
    /// 시안 수치를 판정선 높이의 트랙 폭에 대한 비율로 바꿉니다.
    /// 트랙은 화면 가운데를 중심으로 놓이므로 그 폭만 알면 왼쪽 끝이 정해집니다.
    /// </summary>
    private static void GetDesignRatios(UI_LiveTrackLanes lanes, int lane,
        out float leftRatio, out float widthRatio, out float heightRatio)
    {
        float hitLineWidth = lanes.Shape.GetWidthAtRatio(lanes.GetHitLineVerticalRatio());

        if (hitLineWidth <= 0f)
        {
            leftRatio = 0f;
            widthRatio = 0f;
            heightRatio = 0f;
            return;
        }

        int index = lane - LiveLane.FIRST;
        float trackLeftAtHitLine = DESIGN_SCREEN_WIDTH * 0.5f - hitLineWidth * 0.5f;

        leftRatio = (DESIGN_NOTE_LEFTS[index] - trackLeftAtHitLine) / hitLineWidth;
        widthRatio = DESIGN_NOTE_WIDTHS[index] / hitLineWidth;
        heightRatio = DESIGN_NOTE_HEIGHT / hitLineWidth;
    }
}
