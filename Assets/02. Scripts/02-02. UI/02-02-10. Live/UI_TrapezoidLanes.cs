using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using VInspector;

[ExecuteAlways]
[RequireComponent(typeof(CanvasRenderer))]
public class UI_TrapezoidLanes : MaskableGraphic
{
    private static readonly List<float> CUMULATIVE_TOP_RATIOS = new List<float>
    {
        0f,
        42f / 252f,
        81f / 252f,
        126f / 252f,
        171f / 252f,
        210f / 252f,
        1f
    };

    private static readonly List<float> CUMULATIVE_BOTTOM_RATIOS = new List<float>
    {
        0f,
        239f / 1410f,
        484f / 1410f,
        705f / 1410f,
        926f / 1410f,
        1171f / 1410f,
        1f
    };

    [Foldout("Hierarchy")]
    [Header("Trapezoid Dimensions")]
    [SerializeField]
    private float _topWidth = 252f;

    [SerializeField]
    private float _bottomWidth = 1410f;

    [Header("Hit Line Specifications")]
    [SerializeField]
    private bool _useHitLineSpecs = true;

    [SerializeField]
    private float _hitLineYFromBottom = 125.99f;

    [SerializeField]
    private float _targetHitLineWidth = 1274.91f;

    [Header("Lane Colors (6 Lanes)")]
    [SerializeField]
    private List<Color> _laneColors = new List<Color>
    {
        new Color(1.0f, 0.67f, 0.65f, 0.85f),  // 1: Pink / Light Salmon
        new Color(0.89f, 0.89f, 0.89f, 0.85f), // 2: Light Grey
        new Color(0.85f, 1.0f, 1.0f, 0.85f),   // 3: Light Cyan
        new Color(0.64f, 0.62f, 0.62f, 0.85f),  // 4: Grey
        new Color(0.56f, 0.53f, 0.53f, 0.85f),  // 5: Darker Grey
        new Color(1.0f, 0.67f, 0.65f, 0.85f)   // 6: Pink / Light Salmon
    };

    [Header("Dividers")]
    [SerializeField]
    private bool _showDividers = true;

    [SerializeField]
    private Color _dividerColor = new Color(1f, 1f, 1f, 0.9f);

    [SerializeField]
    private float _dividerWidth = 2f;

    [Header("Hit Line Rendering")]
    [SerializeField]
    private bool _showHitLine = true;

    [SerializeField]
    private Color _hitLineColor = new Color(1f, 1f, 1f, 0.95f);

    [SerializeField]
    private float _hitLineWidth = 4f;

    [Header("Side Borders")]
    [SerializeField]
    private bool _showSideBorders = true;

    [SerializeField]
    private Color _sideBorderColor = new Color(0.77f, 0.37f, 1.0f, 0.95f);

    [SerializeField]
    private float _sideBorderWidth = 6f;

    public float TopWidth { get => _topWidth; set { _topWidth = value; SetVerticesDirty(); } }
    public float BottomWidth { get => _bottomWidth; set { _bottomWidth = value; SetVerticesDirty(); } }
    public float HitLineYFromBottom { get => _hitLineYFromBottom; set { _hitLineYFromBottom = value; SetVerticesDirty(); } }
    public float TargetHitLineWidth { get => _targetHitLineWidth; set { _targetHitLineWidth = value; SetVerticesDirty(); } }

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
        float height = rect.height > 0 ? rect.height : 1080f;
        float halfH = height * 0.5f;

        float tHit = Mathf.Clamp01(_hitLineYFromBottom / height);

        float effectiveBottomWidth = GetEffectiveBottomWidth();

        // Draw 6 Lanes
        int numLanes = 6;
        for (int i = 0; i < numLanes; i++)
        {
            float topT1 = CUMULATIVE_TOP_RATIOS[i];
            float topT2 = CUMULATIVE_TOP_RATIOS[i + 1];

            float botT1 = CUMULATIVE_BOTTOM_RATIOS[i];
            float botT2 = CUMULATIVE_BOTTOM_RATIOS[i + 1];

            float topX1 = Mathf.Lerp(-_topWidth * 0.5f, _topWidth * 0.5f, topT1);
            float topX2 = Mathf.Lerp(-_topWidth * 0.5f, _topWidth * 0.5f, topT2);

            float botX1 = Mathf.Lerp(-effectiveBottomWidth * 0.5f, effectiveBottomWidth * 0.5f, botT1);
            float botX2 = Mathf.Lerp(-effectiveBottomWidth * 0.5f, effectiveBottomWidth * 0.5f, botT2);

            Color laneColor = (i < _laneColors.Count) ? _laneColors[i] : color;

            AddQuad(vh,
                new Vector2(botX1, -halfH),
                new Vector2(topX1, halfH),
                new Vector2(topX2, halfH),
                new Vector2(botX2, -halfH),
                laneColor);
        }

        // Draw Lane Dividers
        if (_showDividers)
        {
            for (int i = 1; i < numLanes; i++)
            {
                float topT = CUMULATIVE_TOP_RATIOS[i];
                float botT = CUMULATIVE_BOTTOM_RATIOS[i];
                float topX = Mathf.Lerp(-_topWidth * 0.5f, _topWidth * 0.5f, topT);
                float botX = Mathf.Lerp(-effectiveBottomWidth * 0.5f, effectiveBottomWidth * 0.5f, botT);

                Vector2 topPos = new Vector2(topX, halfH);
                Vector2 botPos = new Vector2(botX, -halfH);

                AddThickLine(vh, botPos, topPos, _dividerWidth, _dividerColor);
            }
        }

        // Draw Hit Line
        if (_showHitLine)
        {
            float hitY = Mathf.Lerp(-halfH, halfH, tHit);
            float hitXLeft = Mathf.Lerp(-effectiveBottomWidth * 0.5f, -_topWidth * 0.5f, tHit);
            float hitXRight = Mathf.Lerp(effectiveBottomWidth * 0.5f, _topWidth * 0.5f, tHit);

            AddThickLine(vh, new Vector2(hitXLeft, hitY), new Vector2(hitXRight, hitY), _hitLineWidth, _hitLineColor);
        }

        // Draw Side Borders
        if (_showSideBorders)
        {
            Vector2 leftTop = new Vector2(-_topWidth * 0.5f, halfH);
            Vector2 leftBot = new Vector2(-effectiveBottomWidth * 0.5f, -halfH);
            Vector2 rightTop = new Vector2(_topWidth * 0.5f, halfH);
            Vector2 rightBot = new Vector2(effectiveBottomWidth * 0.5f, -halfH);

            AddThickLine(vh, leftBot, leftTop, _sideBorderWidth, _sideBorderColor);
            AddThickLine(vh, rightBot, rightTop, _sideBorderWidth, _sideBorderColor);
        }
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

        AddQuad(vh,
            start - normal,
            end - normal,
            end + normal,
            start + normal,
            col);
    }


    private float GetEffectiveBottomWidth()
    {
        Rect rect = GetPixelAdjustedRect();
        float height = rect.height > 0 ? rect.height : 1080f;
        float tHit = Mathf.Clamp01(_hitLineYFromBottom / height);

        if (_useHitLineSpecs && tHit < 1f)
        {
            return (_targetHitLineWidth - _topWidth * tHit) / (1f - tHit);
        }

        return _bottomWidth;
    }

    public Vector2 GetLaneCenterPosition(int laneIndex, float verticalRatio)
    {
        int laneArrayIndex = laneIndex - 1;
        float topT1 = CUMULATIVE_TOP_RATIOS[laneArrayIndex];
        float topT2 = CUMULATIVE_TOP_RATIOS[laneArrayIndex + 1];
        float botT1 = CUMULATIVE_BOTTOM_RATIOS[laneArrayIndex];
        float botT2 = CUMULATIVE_BOTTOM_RATIOS[laneArrayIndex + 1];

        float effectiveBottomWidth = GetEffectiveBottomWidth();
        float topXCenter = Mathf.Lerp(-_topWidth * 0.5f, _topWidth * 0.5f, (topT1 + topT2) * 0.5f);
        float botXCenter = Mathf.Lerp(-effectiveBottomWidth * 0.5f, effectiveBottomWidth * 0.5f, (botT1 + botT2) * 0.5f);

        Rect rect = GetPixelAdjustedRect();
        float height = rect.height > 0 ? rect.height : 1080f;
        float halfH = height * 0.5f;

        float x = Mathf.Lerp(botXCenter, topXCenter, verticalRatio);
        float y = Mathf.Lerp(-halfH, halfH, verticalRatio);

        return new Vector2(x, y);
    }

    public float GetHitLineVerticalRatio()
    {
        Rect rect = GetPixelAdjustedRect();
        float height = rect.height > 0 ? rect.height : 1080f;
        return Mathf.Clamp01(_hitLineYFromBottom / height);
    }


    public void GetTrackEdgesAtRatio(float verticalRatio, out float leftX, out float rightX, out float y)
    {
        float effectiveBottomWidth = GetEffectiveBottomWidth();

        Rect rect = GetPixelAdjustedRect();
        float height = rect.height > 0 ? rect.height : 1080f;
        float halfH = height * 0.5f;

        float topLeftX = -_topWidth * 0.5f;
        float topRightX = _topWidth * 0.5f;
        float botLeftX = -effectiveBottomWidth * 0.5f;
        float botRightX = effectiveBottomWidth * 0.5f;

        leftX = Mathf.Lerp(botLeftX, topLeftX, verticalRatio);
        rightX = Mathf.Lerp(botRightX, topRightX, verticalRatio);
        y = Mathf.Lerp(-halfH, halfH, verticalRatio);
    }


    public void GetLaneBoundsAtRatio(int laneIndex, float verticalRatio, out float leftX, out float rightX)
    {
        int laneArrayIndex = laneIndex - 1;
        float topT1 = CUMULATIVE_TOP_RATIOS[laneArrayIndex];
        float topT2 = CUMULATIVE_TOP_RATIOS[laneArrayIndex + 1];
        float botT1 = CUMULATIVE_BOTTOM_RATIOS[laneArrayIndex];
        float botT2 = CUMULATIVE_BOTTOM_RATIOS[laneArrayIndex + 1];

        float effectiveBottomWidth = GetEffectiveBottomWidth();

        float topLeftX = Mathf.Lerp(-_topWidth * 0.5f, _topWidth * 0.5f, topT1);
        float topRightX = Mathf.Lerp(-_topWidth * 0.5f, _topWidth * 0.5f, topT2);
        float botLeftX = Mathf.Lerp(-effectiveBottomWidth * 0.5f, effectiveBottomWidth * 0.5f, botT1);
        float botRightX = Mathf.Lerp(-effectiveBottomWidth * 0.5f, effectiveBottomWidth * 0.5f, botT2);

        leftX = Mathf.Lerp(botLeftX, topLeftX, verticalRatio);
        rightX = Mathf.Lerp(botRightX, topRightX, verticalRatio);
    }
}
