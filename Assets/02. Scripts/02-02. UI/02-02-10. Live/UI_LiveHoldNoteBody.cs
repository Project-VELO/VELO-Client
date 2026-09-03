using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 롱노트 몸통을 쿼드 하나로 그립니다.
/// 타일링과 판정선 클리핑을 UV로만 처리하므로 노트마다 머티리얼 인스턴스가 생기지 않습니다.
/// </summary>
public class UI_LiveHoldNoteBody : MaskableGraphic
{
    [Tooltip("세로로 반복되므로 아틀라스에 묶이지 않은 Wrap Mode = Repeat 텍스처여야 합니다.")]
    [SerializeField]
    private Sprite _bodySprite;

    private float _uvStart;
    private float _uvEnd = 1f;

    public override Texture mainTexture => _bodySprite == null ? s_WhiteTexture : _bodySprite.texture;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        AddVertex(vh, ref vertex, rect.xMin, rect.yMin, 0f, _uvStart);
        AddVertex(vh, ref vertex, rect.xMin, rect.yMax, 0f, _uvEnd);
        AddVertex(vh, ref vertex, rect.xMax, rect.yMax, 1f, _uvEnd);
        AddVertex(vh, ref vertex, rect.xMax, rect.yMin, 1f, _uvStart);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(0, 2, 3);
    }

    public void SetBodySprite(Sprite sprite)
    {
        _bodySprite = sprite;
        SetMaterialDirty();
        SetVerticesDirty();
    }

    /// <summary>
    /// 판정선에 먹힌 만큼 uvStart가 밀리므로, 몸통이 줄어드는 동안에도 텍스처가 미끄러지지 않습니다.
    /// </summary>
    public void RefreshBody(float length, float uvStart, float uvEnd)
    {
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, length);
        _uvStart = uvStart;
        _uvEnd = uvEnd;
        SetVerticesDirty();
    }

    private static void AddVertex(VertexHelper vh, ref UIVertex vertex, float x, float y, float u, float v)
    {
        vertex.position = new Vector3(x, y);
        vertex.uv0 = new Vector2(u, v);
        vh.AddVert(vertex);
    }
}
