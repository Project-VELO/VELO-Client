using UnityEngine;

/// <summary>
/// 노트 마커를 시안 크기 그대로 그리기 위해, 레인별 마커 크기(노트 레이어의 로컬 좌표)를 구합니다.
///
/// 외주 시안(Figma Section 4 - 리듬게임 진행 화면)은 판정선 위에 노트를 원본 픽셀 크기 그대로
/// 올려 두었고(239/242/218/218/243/239 x 56, 1920x1080 기준), 그 상태에서 노트가 레인 좌우
/// 구분선과 빈틈없이 맞물립니다.
///
/// 마커를 트랙 평면에 눕혀 두면 이 그림이 나오지 않습니다. 아트에 이미 원근 기울기가 그려져 있는데
/// 카메라가 한 번 더 기울이고 세로로 눌러, 폭과 높이를 맞춰도 모양이 달라지기 때문입니다.
/// 그래서 마커는 카메라를 향해 세워(빌보드) 원근 변형에서 빼고, 거리에 따라 크기만 줄어들게 합니다.
///
/// 카메라를 향한 면은 화면 크기가 거리에 반비례하므로, 판정선에서 한 번만 시안 크기에 맞춰 두면
/// 나머지 깊이는 저절로 맞습니다. 그래서 레인마다 크기 상수 하나로 끝납니다.
/// </summary>
public class LiveNoteLaneFit
{
    // 시안을 그린 화면 세로 해상도입니다. 이 값으로 재므로 실행 해상도가 달라도 월드 크기는 같습니다.
    private const float DESIGN_SCREEN_HEIGHT = 1080f;

    /// <summary>
    /// 시안에서 6개 노트가 판정선 높이에 차지하는 가로 범위입니다(화면 x 308~1612).
    /// 아래 좌표들의 기준 폭이며, 실제 트랙 폭에 이 비율 그대로 옮깁니다.
    /// </summary>
    private const float DESIGN_NOTE_SPAN = 1304f;

    /// <summary>
    /// 시안의 노트 왼쪽 좌표입니다(맨 왼쪽 노트를 0으로 둔 값).
    /// 레인 중심이 아니라 이 값을 쓰는 이유는, 노트가 기울어진 평행사변형이라 그림이 레인을 채우는 자리와
    /// 사각형의 자리가 다르기 때문입니다. 바깥 레인일수록 기울기가 커서 11px까지 벌어집니다.
    /// </summary>
    private static readonly float[] DESIGN_NOTE_LEFTS = { 0f, 218f, 443f, 646f, 846f, 1065f };

    private readonly Vector2[] _noteSizes = new Vector2[LiveLane.COUNT];
    private readonly float[] _noteCenterRatios = new float[LiveLane.COUNT];

    private bool _isSolved;
    private float _fittedTrackWidth;
    private Camera _camera;

    public bool IsSolved => _isSolved;

    /// <summary>
    /// 크기를 맞출 때 기준으로 삼은 카메라입니다. 마커를 세울 때도 같은 것을 써야 크기와 방향이 어긋나지 않습니다.
    /// </summary>
    public Camera Camera => _camera;

    /// <summary>
    /// 레인의 마커 크기입니다. 아직 구하지 못했으면 넘겨받은 값을 그대로 돌려주어 표시가 끊기지 않게 합니다.
    /// </summary>
    public Vector2 GetNoteSize(int lane, Vector2 fallbackSize)
    {
        if (!_isSolved || !LiveLane.IsValid(lane))
        {
            return fallbackSize;
        }

        return _noteSizes[lane - LiveLane.FIRST];
    }

    /// <summary>
    /// 노트 가로 중심의 트랙 로컬 좌표입니다. 트랙 폭에 대한 비율로 갖고 있으므로 깊이와 무관하게 같습니다.
    /// </summary>
    public float GetNoteCenterX(int lane, float trackWidth, float fallbackCenterX)
    {
        if (!_isSolved || !LiveLane.IsValid(lane))
        {
            return fallbackCenterX;
        }

        return (_noteCenterRatios[lane - LiveLane.FIRST] - 0.5f) * trackWidth;
    }

    /// <summary>
    /// 트랙 배치가 바뀐 프레임에만 다시 맞춥니다. 리그가 트랙을 배치하는 시점이 렌더러가 만들어지는 시점보다
    /// 늦을 수 있어, 한 번만 구해 두면 어긋납니다. 트랙 크기가 그대로면 결과도 같으므로 매 프레임 불러도 됩니다.
    /// </summary>
    public void RefreshIfChanged(UI_LiveTrackLanes lanes, RectTransform noteLayer, LiveNoteSpriteTable spriteTable)
    {
        if (lanes == null)
        {
            return;
        }

        lanes.GetTrackEdgesAtRatio(0f, out float leftX, out float rightX, out _);
        float trackWidth = rightX - leftX;

        if (Mathf.Approximately(trackWidth, _fittedTrackWidth) && _camera != null)
        {
            return;
        }

        _fittedTrackWidth = trackWidth;
        _camera = Camera.main;

        RefreshFit(lanes, noteLayer, spriteTable, _camera);
    }

    /// <summary>
    /// 카메라나 스프라이트가 없으면 아무것도 정하지 않아, 호출하는 쪽은 레인 폭을 그대로 쓰게 됩니다.
    /// </summary>
    private void RefreshFit(UI_LiveTrackLanes lanes, RectTransform noteLayer, LiveNoteSpriteTable spriteTable,
        Camera camera)
    {
        _isSolved = false;

        if (lanes == null || noteLayer == null || spriteTable == null || camera == null)
        {
            return;
        }

        float pixelsPerLocalUnit = GetPixelsPerLocalUnitAtHitLine(lanes, noteLayer, camera);

        if (pixelsPerLocalUnit <= 0f)
        {
            return;
        }

        lanes.GetTrackEdgesAtRatio(lanes.GetHitLineVerticalRatio(), out float trackLeftX, out float trackRightX, out _);
        float trackWidth = trackRightX - trackLeftX;

        if (trackWidth <= 0f)
        {
            return;
        }

        // 시안은 폭 1304 안에 그려져 있습니다. 실제 트랙이 그보다 넓거나 좁으면 같은 비율로 늘리고 줄입니다.
        float designToTrack = trackWidth / DESIGN_NOTE_SPAN;

        for (int lane = LiveLane.FIRST; lane <= LiveLane.LAST; lane++)
        {
            Sprite sprite = spriteTable.GetSprite(lane);

            if (sprite == null)
            {
                return;
            }

            int index = lane - LiveLane.FIRST;

            _noteSizes[index] = sprite.rect.size * designToTrack / pixelsPerLocalUnit;
            _noteCenterRatios[index] =
                (DESIGN_NOTE_LEFTS[index] + sprite.rect.width * 0.5f) / DESIGN_NOTE_SPAN;
        }

        _isSolved = true;
    }

    /// <summary>
    /// 판정선 높이에서 노트 레이어의 로컬 1단위가 화면 몇 픽셀로 맺히는지입니다.
    /// 카메라를 향해 세운 면은 시야각과 거리만으로 화면 크기가 정해지므로 이 값 하나면 됩니다.
    /// </summary>
    private static float GetPixelsPerLocalUnitAtHitLine(UI_LiveTrackLanes lanes, RectTransform noteLayer,
        Camera camera)
    {
        Vector2 hitLinePoint = lanes.GetLaneCenterPosition(LiveLane.FIRST, lanes.GetHitLineVerticalRatio());
        Vector3 worldPoint = noteLayer.TransformPoint(new Vector3(hitLinePoint.x, hitLinePoint.y, 0f));

        // 화면 좌표 변환의 z가 곧 광축 방향 거리입니다. 카메라 뒤에 있으면 크기를 정할 수 없습니다.
        float viewDepth = camera.WorldToScreenPoint(worldPoint).z;

        if (viewDepth <= 0f)
        {
            return 0f;
        }

        float halfFrustumHeight = viewDepth * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        if (halfFrustumHeight <= 0f)
        {
            return 0f;
        }

        float pixelsPerWorldUnit = DESIGN_SCREEN_HEIGHT * 0.5f / halfFrustumHeight;

        return pixelsPerWorldUnit * noteLayer.lossyScale.x;
    }
}
