using UnityEngine;

/// <summary>
/// 판정선에서 노트가 화면에 맺히는 폭이 스프라이트 원본 폭과 같아지도록, 레인별 노트 폭(트랙 좌표)을 구합니다.
///
/// 외주 시안(Figma Section 4 - 리듬게임 진행 화면)은 6개 노트를 판정선 위에 원본 픽셀 그대로 올려 두었고
/// (239/242/218/218/243/239 x 56, 1920x1080 기준), 그 상태에서 노트가 레인 좌우 구분선과 정확히 맞물립니다.
///
/// 반면 트랙은 눕혀 놓은 띠여서, 레인 폭을 그대로 노트 폭으로 쓰면 원근을 거치며 화면에서는 다른 폭이 됩니다.
/// 특히 안쪽 레인은 205px로 맺혀 시안의 218px보다 13px 좁아지고, 그만큼 노트가 구분선에 닿지 못합니다.
/// 그래서 폭을 순방향으로 정하지 않고, 화면에 맺힐 폭을 목표로 두고 트랙 좌표의 폭을 거꾸로 구합니다.
///
/// 레인 경계는 트랙 좌표에서 세로 비율과 무관하므로 노트 폭도 레인마다 상수이며, 한 번만 구해 두고 재사용합니다.
/// </summary>
public class LiveNoteLaneFit
{
    // 시안을 그린 화면 세로 해상도입니다. 다른 해상도에서는 이 비율로 목표 폭을 환산합니다.
    private const float DESIGN_SCREEN_HEIGHT = 1080f;

    private const int SOLVE_ITERATION_COUNT = 24;
    private const float SOLVE_SEARCH_MARGIN = 200f;

    private readonly float[] _laneWidths = new float[LiveLane.COUNT];

    private bool _isSolved;

    /// <summary>
    /// 레인의 노트 폭입니다. 아직 구하지 못했으면 레인 폭을 그대로 돌려주어 표시가 끊기지 않게 합니다.
    /// </summary>
    public float GetNoteWidth(int lane, float fallbackWidth)
    {
        if (!_isSolved || !LiveLane.IsValid(lane))
        {
            return fallbackWidth;
        }

        return _laneWidths[lane - LiveLane.FIRST];
    }

    /// <summary>
    /// 트랙 크기·해상도·노트 두께가 정해진 뒤에 호출합니다. 이 셋 중 하나라도 바뀌면 다시 호출해야 합니다.
    /// 카메라가 없으면 아무것도 하지 않아, 호출하는 쪽은 레인 폭을 그대로 쓰게 됩니다.
    /// </summary>
    public void RefreshFit(UI_LiveTrackLanes lanes, RectTransform noteLayer, LiveNoteSpriteTable spriteTable,
        Camera camera, float noteHeight)
    {
        _isSolved = false;

        if (lanes == null || noteLayer == null || spriteTable == null || camera == null)
        {
            return;
        }

        float hitLineRatio = lanes.GetHitLineVerticalRatio();
        float screenScale = Screen.height / DESIGN_SCREEN_HEIGHT;

        for (int lane = LiveLane.FIRST; lane <= LiveLane.LAST; lane++)
        {
            Sprite sprite = spriteTable.GetSprite(lane);

            lanes.GetLaneBoundsAtRatio(lane, hitLineRatio, out float leftX, out float rightX);
            _laneWidths[lane - LiveLane.FIRST] = rightX - leftX;

            if (sprite == null)
            {
                continue;
            }

            float centerY = lanes.GetLaneCenterPosition(lane, hitLineRatio).y + noteHeight * 0.5f;
            float targetWidth = sprite.rect.width * screenScale;

            _laneWidths[lane - LiveLane.FIRST] =
                SolveWidth(noteLayer, camera, leftX, rightX, centerY, noteHeight, targetWidth);
        }

        _isSolved = true;
    }

    /// <summary>
    /// 화면에 맺히는 폭이 목표와 같아지는 트랙 좌표 폭을 이분법으로 찾습니다.
    /// 원근 투영이라 폭과 화면 폭의 관계가 선형이 아니므로 식으로 풀지 않고 좁혀 들어갑니다.
    /// </summary>
    private static float SolveWidth(RectTransform noteLayer, Camera camera, float leftX, float rightX,
        float centerY, float noteHeight, float targetWidth)
    {
        float laneWidth = rightX - leftX;
        float centerX = (leftX + rightX) * 0.5f;

        float low = Mathf.Max(1f, laneWidth - SOLVE_SEARCH_MARGIN);
        float high = laneWidth + SOLVE_SEARCH_MARGIN;

        for (int i = 0; i < SOLVE_ITERATION_COUNT; i++)
        {
            float mid = (low + high) * 0.5f;

            if (GetScreenWidth(noteLayer, camera, centerX, mid, centerY, noteHeight) < targetWidth)
            {
                low = mid;
                continue;
            }

            high = mid;
        }

        return (low + high) * 0.5f;
    }

    /// <summary>
    /// 트랙 위에 놓인 노트 사각형이 화면에서 차지하는 가로 폭입니다.
    /// 트랙이 기울어 있어 위·아래 변의 화면 폭이 다르므로 네 꼭짓점을 모두 재야 합니다.
    /// </summary>
    private static float GetScreenWidth(RectTransform noteLayer, Camera camera, float centerX, float width,
        float centerY, float height)
    {
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        float minX = float.MaxValue;
        float maxX = float.MinValue;

        for (int i = 0; i < 4; i++)
        {
            float localX = centerX + ((i < 2) ? -halfWidth : halfWidth);
            float localY = centerY + ((i % 2 == 0) ? -halfHeight : halfHeight);

            float screenX = camera.WorldToScreenPoint(noteLayer.TransformPoint(new Vector3(localX, localY, 0f))).x;

            minX = Mathf.Min(minX, screenX);
            maxX = Mathf.Max(maxX, screenX);
        }

        return maxX - minX;
    }
}
