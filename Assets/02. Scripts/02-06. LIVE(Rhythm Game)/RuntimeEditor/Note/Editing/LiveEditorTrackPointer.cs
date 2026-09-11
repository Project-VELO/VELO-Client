using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 화면 좌표를 트랙 위의 레인과 격자 셀로 변환하는 것을 전담합니다.
///
/// 변환 기준은 트랙 자신입니다. 클릭 지점을 트랙의 로컬 좌표로 옮긴 뒤 그 높이의 깊이 비율을 트랙에게 되물어,
/// 노트와 격자선을 그릴 때 쓰는 것과 똑같은 비율로 레인과 셀을 찾습니다.
///
/// 사각형 안에서의 세로 위치를 그대로 깊이 비율로 쓰면 안 됩니다. 트랙은 사다리꼴이라 깊이와 화면 높이가
/// 곡선 관계이고(LiveTrackShape), 화면 중간을 클릭하면 깊이로는 0.15에 지나지 않습니다. 이 둘을 같다고 보면
/// 마디가 최대 한 마디까지 밀리고, 그 높이의 트랙 폭도 실제보다 좁게 잡혀 바깥 레인이 한 칸씩 어긋납니다.
///
/// 유니티 생명주기를 쓰지 않으므로 컴포넌트가 아니며, LiveEditorNoteEditing이 씬 참조를 넘겨 생성합니다.
/// </summary>
public class LiveEditorTrackPointer
{
    private readonly LiveEditorTimeline _timeline;
    private readonly Canvas _canvas;

    public LiveEditorTrackPointer(LiveEditorTimeline timeline, Canvas canvas)
    {
        _timeline = timeline;
        _canvas = canvas;
    }

    /// <summary>
    /// 클릭 지점이 트랙 안쪽일 때만 레인/셀을 돌려줍니다. 트랙 바깥 클릭은 좌표를 보정하지 않고 그대로 실패 처리합니다.
    /// </summary>
    public bool TryGetCell(Vector2 screenPosition, out int lane, out int barIndex, out int cellIndex)
    {
        lane = 0;
        barIndex = 0;
        cellIndex = 0;

        UI_LiveTrackLanes lanes = _timeline.Lanes;

        if (lanes == null || IsPointerOverUI())
        {
            return false;
        }

        RectTransform laneAreaRect = lanes.rectTransform;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(laneAreaRect, screenPosition, GetEventCamera(), out Vector2 localPoint))
        {
            return false;
        }

        if (!laneAreaRect.rect.Contains(localPoint))
        {
            return false;
        }

        float verticalRatio = lanes.GetRatioAtLocalY(localPoint.y);

        if (!TryGetLaneAtPoint(lanes, localPoint.x, verticalRatio, out lane))
        {
            return false;
        }

        return _timeline.TryGetCellAtVerticalRatio(verticalRatio, out barIndex, out cellIndex);
    }

    /// <summary>
    /// 화면 좌표를 레인과 격자 셀의 절대 시각으로 한 번에 변환합니다.
    /// </summary>
    public bool TryGetCellTime(Vector2 screenPosition, out int lane, out int timeMs)
    {
        timeMs = 0;

        if (!TryGetCell(screenPosition, out lane, out int barIndex, out int cellIndex))
        {
            return false;
        }

        timeMs = _timeline.GetCellTimeMs(barIndex, cellIndex);
        return true;
    }

    /// <summary>
    /// 트랙 위의 레인/격자선/노트 마커는 모두 레이캐스트 대상에서 제외되어 있으므로,
    /// 이 검사에 걸리는 것은 패널의 드롭다운·버튼 같은 실제 UI 컨트롤뿐입니다.
    /// </summary>
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// Screen Space - Overlay 캔버스는 카메라를 함께 넘기면 좌표 변환 결과가 어긋나므로 반드시 null을 전달해야 합니다.
    /// 월드 스페이스 캔버스에서는 반대로 카메라를 넘겨야 평면과의 교점을 제대로 구합니다.
    /// </summary>
    private Camera GetEventCamera()
    {
        return _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
    }

    private static bool TryGetLaneAtPoint(UI_LiveTrackLanes lanes, float localX, float verticalRatio, out int lane)
    {
        for (int candidateLane = LiveLane.FIRST; candidateLane <= LiveLane.COUNT; candidateLane++)
        {
            lanes.GetLaneBoundsAtRatio(candidateLane, verticalRatio, out float leftX, out float rightX);

            if (leftX <= localX && localX <= rightX)
            {
                lane = candidateLane;
                return true;
            }
        }

        lane = 0;
        return false;
    }
}
