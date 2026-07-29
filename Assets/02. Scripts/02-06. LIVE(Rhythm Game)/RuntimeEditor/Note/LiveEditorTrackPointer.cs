using UnityEngine;
using UnityEngine.EventSystems;
using VInspector;

/// <summary>
/// 화면 좌표를 트랙 위의 레인과 격자 셀로 변환하는 것을 전담합니다.
/// 사다리꼴 트랙은 높이에 따라 레인 폭이 달라지므로, 세로 비율을 먼저 구한 뒤 그 높이에서의 레인 경계와 비교합니다.
/// </summary>
public class LiveEditorTrackPointer : MonoBehaviour
{
    private const int LANE_COUNT = 6;

    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorTimeline _timeline;

    [SerializeField]
    private RectTransform _laneAreaRect;

    [SerializeField]
    private Canvas _canvas;

    /// <summary>
    /// 클릭 지점이 트랙 안쪽일 때만 레인/셀을 돌려줍니다. 트랙 바깥 클릭은 좌표를 보정하지 않고 그대로 실패 처리합니다.
    /// </summary>
    public bool TryGetCell(Vector2 screenPosition, out int lane, out int barIndex, out int cellIndex)
    {
        lane = 0;
        barIndex = 0;
        cellIndex = 0;

        if (_timeline.Lanes == null || IsPointerOverUI())
        {
            return false;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_laneAreaRect, screenPosition, GetEventCamera(), out Vector2 localPoint))
        {
            return false;
        }

        Rect rect = _laneAreaRect.rect;
        if (!rect.Contains(localPoint))
        {
            return false;
        }

        float verticalRatio = (localPoint.y - rect.yMin) / rect.height;

        if (!TryGetLaneAtPoint(localPoint.x, verticalRatio, out lane))
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
    /// </summary>
    private Camera GetEventCamera()
    {
        return _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
    }

    private bool TryGetLaneAtPoint(float localX, float verticalRatio, out int lane)
    {
        for (int candidateLane = 1; candidateLane <= LANE_COUNT; candidateLane++)
        {
            _timeline.Lanes.GetLaneBoundsAtRatio(candidateLane, verticalRatio, out float leftX, out float rightX);

            if (localX >= leftX && localX <= rightX)
            {
                lane = candidateLane;
                return true;
            }
        }

        lane = 0;
        return false;
    }
}
