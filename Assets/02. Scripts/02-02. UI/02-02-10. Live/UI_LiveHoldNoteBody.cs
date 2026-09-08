using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 롱노트 몸통을 트랙 사다리꼴을 따라가는 띠로 그립니다.
/// 타일링과 판정선 클리핑을 UV로만 처리하므로 노트마다 머티리얼 인스턴스가 생기지 않습니다.
/// </summary>
public class UI_LiveHoldNoteBody : MaskableGraphic
{
    [Tooltip("세로로 반복되므로 아틀라스에 묶이지 않은 Wrap Mode = Repeat 텍스처여야 합니다.")]
    [SerializeField]
    private Sprite _bodySprite;

    // 넘겨받은 목록을 그대로 들고 있지 않고 자기 것에 옮겨 담습니다.
    // 메시 생성은 캔버스 갱신 시점으로 미뤄지는데, 그 사이 호출자의 버퍼는 이미 다음 노트의 것으로 덮이기 때문입니다.
    private readonly List<LiveHoldBodySample> _samples = new List<LiveHoldBodySample>();

    public override Texture mainTexture => _bodySprite == null ? s_WhiteTexture : _bodySprite.texture;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (_samples.Count < 2)
        {
            return;
        }

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        for (int i = 0; i < _samples.Count; i++)
        {
            LiveHoldBodySample sample = _samples[i];

            AddVertex(vh, ref vertex, sample.LeftX, sample.LocalY, 0f, sample.V);
            AddVertex(vh, ref vertex, sample.RightX, sample.LocalY, 1f, sample.V);
        }

        for (int i = 0; i + 1 < _samples.Count; i++)
        {
            int lowerLeft = i * 2;

            vh.AddTriangle(lowerLeft, lowerLeft + 2, lowerLeft + 3);
            vh.AddTriangle(lowerLeft, lowerLeft + 3, lowerLeft + 1);
        }
    }

    public void SetBodySprite(Sprite sprite)
    {
        _bodySprite = sprite;
        SetMaterialDirty();
        SetVerticesDirty();
    }

    public void RefreshBody(List<LiveHoldBodySample> samples)
    {
        _samples.Clear();

        for (int i = 0; i < samples.Count; i++)
        {
            _samples.Add(samples[i]);
        }

        SetVerticesDirty();
    }

    private static void AddVertex(VertexHelper vh, ref UIVertex vertex, float x, float y, float u, float v)
    {
        vertex.position = new Vector3(x, y);
        vertex.uv0 = new Vector2(u, v);
        vh.AddVert(vertex);
    }
}
