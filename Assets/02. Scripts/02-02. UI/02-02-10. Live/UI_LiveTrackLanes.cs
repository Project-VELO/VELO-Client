using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 노트가 흐르는 6레인 트랙입니다. 화면에 보이는 사다리꼴 모양을 좌표로 그대로 들고 있습니다.
///
/// 예전에는 평평한 직사각형을 원근 카메라로 비스듬히 봐서 사다리꼴을 만들었지만, 실제로 보이는 트랙은
/// 시안에서 받은 2D 그림이고 이 메시는 좌표 계산에만 쓰였습니다. 카메라를 눕히는 것은 그 좌표를 그림에
/// 맞추기 위한 우회였으므로, 형태는 LiveTrackShape가 화면 좌표로 직접 정의하고 카메라는 건드리지 않습니다.
///
/// 그 덕분에 레인 경계가 실제로 모이고, 어느 높이에서든 그 높이의 레인 폭을 그대로 물어볼 수 있습니다.
/// 노트/격자/마디번호 렌더러와 트랙 포인터는 모두 이 클래스의 좌표 조회 기능만 사용합니다.
///
/// 세로 비율은 트랙 앞쪽 끝이 0, 뒤쪽 끝이 1이며 깊이에 정비례합니다(화면 높이에는 정비례하지 않습니다).
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(CanvasRenderer))]
public class UI_LiveTrackLanes : MaskableGraphic
{
    /// <summary>
    /// 레인 경계를 트랙 맨 아래 폭에 대한 비율로 적어 둔 것입니다. 분모 1410은 화면 맨 아래에서의 트랙 폭이며
    /// LiveTrackShape가 시안 수치에서 구하는 값과 같습니다. 레인 폭이 서로 다르므로 균등 분할이 아닙니다.
    /// </summary>
    private static readonly List<float> CUMULATIVE_LANE_RATIOS = new List<float>
    {
        0f,
        239f / 1410f,
        484f / 1410f,
        705f / 1410f,
        926f / 1410f,
        1171f / 1410f,
        1f
    };

    [Header("Shape (시안 수치)")]
    [SerializeField]
    private LiveTrackShape _shape = new LiveTrackShape();

    [Header("Lane Colors (6 Lanes)")]
    [SerializeField]
    private List<Color> _laneColors = new List<Color>
    {
        new Color(1.0f, 0.67f, 0.65f, 0.85f),
        new Color(0.89f, 0.89f, 0.89f, 0.85f),
        new Color(0.85f, 1.0f, 1.0f, 0.85f),
        new Color(0.64f, 0.62f, 0.62f, 0.85f),
        new Color(0.56f, 0.53f, 0.53f, 0.85f),
        new Color(1.0f, 0.67f, 0.65f, 0.85f)
    };

    [Header("Lines")]
    [SerializeField]
    private LiveTrackLaneMeshSettings _lineSettings = new LiveTrackLaneMeshSettings();

    private readonly LiveTrackLaneMesh _mesh = new LiveTrackLaneMesh();

    public LiveTrackShape Shape => _shape;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        _mesh.Rebuild(vh, _lineSettings, GetLaneBoundaryX, GetLocalY, GetHitLineVerticalRatio(),
            _laneColors, color);
    }

    public float GetHitLineVerticalRatio()
    {
        return _shape.HitLineRatio;
    }

    /// <summary>
    /// 판정선에서를 1로 봤을 때 그 높이의 겉보기 배율입니다. 트랙 폭에 정비례하므로 노트와 격자선,
    /// 마디번호가 모두 이 값 하나로 같은 비율만큼 작아집니다.
    /// 원근 카메라를 쓰던 시절의 두 보정(거리 제곱 되돌리기, 거리 비례 늘리기)을 대신합니다.
    /// </summary>
    public float GetApparentScaleAtRatio(float verticalRatio)
    {
        float hitLineWidth = _shape.GetWidthAtRatio(GetHitLineVerticalRatio());

        if (hitLineWidth <= 0f)
        {
            return 1f;
        }

        return _shape.GetWidthAtRatio(verticalRatio) / hitLineWidth;
    }

    /// <summary>
    /// 트랙 밖(비율 0 미만)도 그대로 외삽합니다. 클램프하면 지나간 노트가 트랙 끝에 멈춰 선 채 사라지므로,
    /// 화면 밖까지 계속 미끄러져 나가도록 두어야 합니다.
    /// </summary>
    public Vector2 GetLaneCenterPosition(int laneIndex, float verticalRatio)
    {
        GetLaneBoundsAtRatio(laneIndex, verticalRatio, out float leftX, out float rightX);

        return new Vector2((leftX + rightX) * 0.5f, GetLocalY(verticalRatio));
    }

    public void GetLaneBoundsAtRatio(int laneIndex, float verticalRatio, out float leftX, out float rightX)
    {
        int laneArrayIndex = laneIndex - LiveLane.FIRST;

        leftX = GetLaneBoundaryX(laneArrayIndex, verticalRatio);
        rightX = GetLaneBoundaryX(laneArrayIndex + 1, verticalRatio);
    }

    public void GetTrackEdgesAtRatio(float verticalRatio, out float leftX, out float rightX, out float y)
    {
        leftX = GetLaneBoundaryX(0, verticalRatio);
        rightX = GetLaneBoundaryX(LiveLane.COUNT, verticalRatio);
        y = GetLocalY(verticalRatio);
    }

    /// <summary>
    /// 세로 비율을 이 사각형의 로컬 세로 좌표로 옮깁니다. 비율은 깊이 기준이고 화면 높이와는 곡선 관계라
    /// 단순 보간이 아니라 형태 정의를 거칩니다.
    /// </summary>
    public float GetLocalY(float verticalRatio)
    {
        Rect rect = GetPixelAdjustedRect();
        float screenRatio = _shape.GetScreenYAtRatio(verticalRatio) / _shape.DesignScreenHeight;

        return (screenRatio - 0.5f) * rect.height;
    }

    /// <summary>
    /// 로컬 세로 좌표를 세로 비율로 되짚습니다. 트랙을 클릭한 지점이 어느 시각인지 구할 때 씁니다.
    /// </summary>
    public float GetRatioAtLocalY(float localY)
    {
        Rect rect = GetPixelAdjustedRect();

        if (Mathf.Approximately(rect.height, 0f))
        {
            return 0f;
        }

        return _shape.GetRatioAtScreenY((localY / rect.height + 0.5f) * _shape.DesignScreenHeight);
    }

    /// <summary>
    /// 레인 경계(0이 트랙 왼쪽 끝, LiveLane.COUNT가 오른쪽 끝)의 그 높이 로컬 가로 좌표입니다.
    /// 경계 비율은 맨 아래 폭 기준이므로, 그 높이의 폭을 곱하면 모이는 경계가 그대로 나옵니다.
    /// </summary>
    private float GetLaneBoundaryX(int boundaryIndex, float verticalRatio)
    {
        int clamped = Mathf.Clamp(boundaryIndex, 0, LiveLane.COUNT);
        Rect rect = GetPixelAdjustedRect();

        float widthScale = rect.width / _shape.BottomWidth;
        float width = _shape.GetWidthAtRatio(verticalRatio) * widthScale;

        return (CUMULATIVE_LANE_RATIOS[clamped] - 0.5f) * width;
    }
}
