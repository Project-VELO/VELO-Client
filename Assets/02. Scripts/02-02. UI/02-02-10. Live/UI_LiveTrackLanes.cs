using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 노트가 흐르는 6레인 트랙입니다. 월드 공간에 눕혀 놓는 평평한 띠이며, LiveTrackRig가 배치와 크기를 정해 줍니다.
///
/// 화면에 보이는 사다리꼴 모양은 이 메시가 아니라 원근 카메라가 만들어 냅니다.
/// 그 덕분에 레인 경계가 실제로 평행해지고, 노트는 크기를 바꾸지 않은 채 등속으로 다가오기만 하면 됩니다.
///
/// 세로 비율은 트랙 앞쪽 끝이 0, 뒤쪽 끝이 1이며 깊이에 정비례합니다.
/// 노트/격자/마디번호 렌더러와 트랙 포인터는 모두 이 클래스의 좌표 조회 기능만 사용합니다.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(CanvasRenderer))]
public class UI_LiveTrackLanes : MaskableGraphic
{
    private const int LANE_COUNT = 6;

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

    [Header("Hit Line")]
    [Tooltip("트랙 길이 중 판정선이 놓이는 비율입니다. LiveTrackRig가 카메라 지오메트리에서 계산해 넣어 줍니다.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _hitLineRatio = 0.023f;

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

    [Header("Dividers")]
    [SerializeField]
    private Color _dividerColor = new Color(1f, 1f, 1f, 0.9f);

    [SerializeField]
    private float _dividerWidth = 2f;

    [Header("Hit Line Rendering")]
    [SerializeField]
    private Color _hitLineColor = new Color(0.09f, 0.32f, 0.85f, 1f);

    [SerializeField]
    private float _hitLineThickness = 10f;

    [Header("Side Borders")]
    [SerializeField]
    private Color _sideBorderColor = new Color(0.77f, 0.37f, 1.0f, 0.95f);

    [SerializeField]
    private float _sideBorderWidth = 6f;

    [Header("Perspective (LiveTrackRig가 채워 줍니다)")]
    [Tooltip("트랙 앞쪽 끝의 광축 방향 거리입니다. 노트 두께의 원근 단축을 되돌리는 데 사용합니다.")]
    [SerializeField]
    private float _nearViewDepth = 1f;

    [Tooltip("트랙 뒤쪽 끝의 광축 방향 거리입니다.")]
    [SerializeField]
    private float _farViewDepth = 1f;

    public float HitLineRatio
    {
        get => _hitLineRatio;
        set
        {
            _hitLineRatio = Mathf.Clamp01(value);
            SetVerticesDirty();
        }
    }

    /// <summary>
    /// 카메라에서 트랙 양 끝까지의 광축 방향 거리입니다. 리그가 카메라 자세를 정한 뒤 넣어 줍니다.
    /// </summary>
    public void SetViewDepths(float nearViewDepth, float farViewDepth)
    {
        _nearViewDepth = nearViewDepth;
        _farViewDepth = farViewDepth;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        float halfWidth = rect.width * 0.5f;
        float halfLength = rect.height * 0.5f;

        for (int i = 0; i < LANE_COUNT; i++)
        {
            float leftX = Mathf.Lerp(-halfWidth, halfWidth, CUMULATIVE_LANE_RATIOS[i]);
            float rightX = Mathf.Lerp(-halfWidth, halfWidth, CUMULATIVE_LANE_RATIOS[i + 1]);
            Color laneColor = (i < _laneColors.Count) ? _laneColors[i] : color;

            AddQuad(vh,
                new Vector2(leftX, -halfLength),
                new Vector2(leftX, halfLength),
                new Vector2(rightX, halfLength),
                new Vector2(rightX, -halfLength),
                laneColor);
        }

        for (int i = 1; i < LANE_COUNT; i++)
        {
            float x = Mathf.Lerp(-halfWidth, halfWidth, CUMULATIVE_LANE_RATIOS[i]);
            AddThickLine(vh, new Vector2(x, -halfLength), new Vector2(x, halfLength), _dividerWidth, _dividerColor);
        }

        float hitY = Mathf.Lerp(-halfLength, halfLength, _hitLineRatio);
        AddThickLine(vh, new Vector2(-halfWidth, hitY), new Vector2(halfWidth, hitY), _hitLineThickness, _hitLineColor);

        AddThickLine(vh, new Vector2(-halfWidth, -halfLength), new Vector2(-halfWidth, halfLength), _sideBorderWidth, _sideBorderColor);
        AddThickLine(vh, new Vector2(halfWidth, -halfLength), new Vector2(halfWidth, halfLength), _sideBorderWidth, _sideBorderColor);
    }

    public float GetHitLineVerticalRatio()
    {
        return _hitLineRatio;
    }

    /// <summary>
    /// 누운 선의 화면 두께는 거리의 제곱에 반비례하므로, 거리 비율의 제곱만큼 두께를 늘려 되돌립니다.
    /// 격자선이 서브픽셀로 떨어져 깜빡이는 것을 막습니다.
    /// </summary>
    public float GetFlatThicknessCompensationAtRatio(float verticalRatio)
    {
        float depthScale = GetViewDepthScaleAtRatio(verticalRatio);
        return depthScale * depthScale;
    }

    /// <summary>
    /// 바닥에 누운 노트는 거리의 제곱으로 얇아지므로, 깊이에 비례해 두께를 미리 늘려 둡니다.
    /// 그 결과 화면에서는 폭과 같은 1/거리 비율로 줄어들어, 멀리서도 노트가 사라지지 않습니다.
    /// </summary>
    public float GetPerspectiveThicknessScaleAtRatio(float verticalRatio)
    {
        return GetViewDepthScaleAtRatio(verticalRatio);
    }

    /// <summary>
    /// 판정선을 1로 봤을 때 그 높이의 광축 방향 거리 비율입니다. 겉보기 크기가 이 값에 반비례합니다.
    /// </summary>
    private float GetViewDepthScaleAtRatio(float verticalRatio)
    {
        float hitLineViewDepth = Mathf.Lerp(_nearViewDepth, _farViewDepth, _hitLineRatio);

        if (hitLineViewDepth <= 0f)
        {
            return 1f;
        }

        return Mathf.Lerp(_nearViewDepth, _farViewDepth, verticalRatio) / hitLineViewDepth;
    }

    public Vector2 GetLaneCenterPosition(int laneIndex, float verticalRatio)
    {
        GetLaneBoundsAtRatio(laneIndex, verticalRatio, out float leftX, out float rightX);

        Rect rect = GetPixelAdjustedRect();
        float y = Mathf.Lerp(-rect.height * 0.5f, rect.height * 0.5f, verticalRatio);

        return new Vector2((leftX + rightX) * 0.5f, y);
    }

    public void GetLaneBoundsAtRatio(int laneIndex, float verticalRatio, out float leftX, out float rightX)
    {
        Rect rect = GetPixelAdjustedRect();
        float halfWidth = rect.width * 0.5f;
        int laneArrayIndex = laneIndex - 1;

        leftX = Mathf.Lerp(-halfWidth, halfWidth, CUMULATIVE_LANE_RATIOS[laneArrayIndex]);
        rightX = Mathf.Lerp(-halfWidth, halfWidth, CUMULATIVE_LANE_RATIOS[laneArrayIndex + 1]);
    }

    public void GetTrackEdgesAtRatio(float verticalRatio, out float leftX, out float rightX, out float y)
    {
        Rect rect = GetPixelAdjustedRect();
        float halfWidth = rect.width * 0.5f;

        leftX = -halfWidth;
        rightX = halfWidth;
        y = Mathf.Lerp(-rect.height * 0.5f, rect.height * 0.5f, verticalRatio);
    }

    private void AddQuad(VertexHelper vh, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Color col)
    {
        int count = vh.currentVertCount;
        UIVertex v = UIVertex.simpleVert;
        v.color = col;

        v.position = p0; vh.AddVert(v);
        v.position = p1; vh.AddVert(v);
        v.position = p2; vh.AddVert(v);
        v.position = p3; vh.AddVert(v);

        vh.AddTriangle(count, count + 1, count + 2);
        vh.AddTriangle(count, count + 2, count + 3);
    }

    private void AddThickLine(VertexHelper vh, Vector2 start, Vector2 end, float width, Color col)
    {
        Vector2 dir = (end - start).normalized;
        if (dir == Vector2.zero)
        {
            return;
        }

        Vector2 normal = new Vector2(-dir.y, dir.x) * (width * 0.5f);
        AddQuad(vh, start - normal, end - normal, end + normal, start + normal, col);
    }
}
