using UnityEngine;
using UnityEngine.UI;

namespace VELO.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(CanvasRenderer))]
    public class UI_TrapezoidLanes : MaskableGraphic
    {
        [Header("Trapezoid Dimensions")]
        [SerializeField] private float _topWidth = 340f;
        [SerializeField] private float _bottomWidth = 1447.17f;

        [Header("Hit Line Specifications")]
        [SerializeField] private bool _useHitLineSpecs = true;
        [SerializeField] private float _hitLineYFromBottom = 125.99f; // Distance from screen bottom (125.99px)
        [SerializeField] private float _targetHitLineWidth = 1318.01f; // Length of hit line (1318.01px, Left/Right margin 301px)

        [Header("Lane Colors (6 Lanes)")]
        [SerializeField]
        private Color[] _laneColors = new Color[6]
        {
            new Color(1.0f, 0.67f, 0.65f, 0.85f),  // 1: Pink / Light Salmon
            new Color(0.89f, 0.89f, 0.89f, 0.85f), // 2: Light Grey
            new Color(0.85f, 1.0f, 1.0f, 0.85f),   // 3: Light Cyan
            new Color(0.64f, 0.62f, 0.62f, 0.85f),  // 4: Grey
            new Color(0.56f, 0.53f, 0.53f, 0.85f),  // 5: Darker Grey
            new Color(1.0f, 0.67f, 0.65f, 0.85f)   // 6: Pink / Light Salmon
        };

        [Header("Dividers")]
        [SerializeField] private bool _showDividers = true;
        [SerializeField] private Color _dividerColor = new Color(1f, 1f, 1f, 0.9f);
        [SerializeField] private float _dividerWidth = 2f;

        [Header("Hit Line Rendering")]
        [SerializeField] private bool _showHitLine = true;
        [SerializeField] private Color _hitLineColor = new Color(1f, 1f, 1f, 0.95f);
        [SerializeField] private float _hitLineWidth = 4f;

        [Header("Side Borders")]
        [SerializeField] private bool _showSideBorders = true;
        [SerializeField] private Color _sideBorderColor = new Color(0.77f, 0.37f, 1.0f, 0.95f); // Neon purple
        [SerializeField] private float _sideBorderWidth = 6f;

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

            float effectiveBottomWidth = _bottomWidth;
            if (_useHitLineSpecs && tHit < 1f)
            {
                effectiveBottomWidth = (_targetHitLineWidth - _topWidth * tHit) / (1f - tHit);
            }

            // Draw 6 Lanes
            int numLanes = 6;
            for (int i = 0; i < numLanes; i++)
            {
                float t1 = (float)i / numLanes;
                float t2 = (float)(i + 1) / numLanes;

                float topX1 = Mathf.Lerp(-_topWidth * 0.5f, _topWidth * 0.5f, t1);
                float topX2 = Mathf.Lerp(-_topWidth * 0.5f, _topWidth * 0.5f, t2);

                float botX1 = Mathf.Lerp(-effectiveBottomWidth * 0.5f, effectiveBottomWidth * 0.5f, t1);
                float botX2 = Mathf.Lerp(-effectiveBottomWidth * 0.5f, effectiveBottomWidth * 0.5f, t2);

                Color laneColor = (i < _laneColors.Length) ? _laneColors[i] : color;

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
                    float t = (float)i / numLanes;
                    float topX = Mathf.Lerp(-_topWidth * 0.5f, _topWidth * 0.5f, t);
                    float botX = Mathf.Lerp(-effectiveBottomWidth * 0.5f, effectiveBottomWidth * 0.5f, t);

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
            if (dir == Vector2.zero) return;
            Vector2 normal = new Vector2(-dir.y, dir.x) * (width * 0.5f);

            AddQuad(vh,
                start - normal,
                end - normal,
                end + normal,
                start + normal,
                col);
        }
    }
}
