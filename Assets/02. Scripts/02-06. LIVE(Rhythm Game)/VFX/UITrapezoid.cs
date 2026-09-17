using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(CanvasRenderer))]
public class UITrapezoid : MaskableGraphic
{
    [Header("사다리꼴 크기 (픽셀 단위)")]
    public float topWidth = 100f;    // 윗면 가로 길이 (px)
    public float bottomWidth = 200f; // 아랫면 가로 길이 (px)
    public float height = 150f;      // 높이 (px)

    [Header("비대칭 위치 조절 (px)")]
    public float topOffset = 0f;     // 0: 좌우 대칭, +: 우측 쏠림, -: 좌측 쏠림

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float halfTop = topWidth * 0.5f;
        float halfBottom = bottomWidth * 0.5f;
        float halfHeight = height * 0.5f;

        // topOffset 값을 가산하여 윗면 전체를 좌/우로 이동
        Vector3 v0 = new Vector3(-halfBottom, -halfHeight, 0);             // 좌측 하단
        Vector3 v1 = new Vector3(-halfTop + topOffset, halfHeight, 0);    // 좌측 상단
        Vector3 v2 = new Vector3(halfTop + topOffset, halfHeight, 0);     // 우측 상단
        Vector3 v3 = new Vector3(halfBottom, -halfHeight, 0);              // 우측 하단

        Vector2 uv0 = new Vector2(0, 0);
        Vector2 uv1 = new Vector2(0, 1);
        Vector2 uv2 = new Vector2(1, 1);
        Vector2 uv3 = new Vector2(1, 0);

        vh.AddVert(v0, color, uv0);
        vh.AddVert(v1, color, uv1);
        vh.AddVert(v2, color, uv2);
        vh.AddVert(v3, color, uv3);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}