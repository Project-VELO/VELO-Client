using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 롱노트 몸통을 트랙 사다리꼴을 따라가는 띠로 그립니다.
/// 타일링과 판정선 클리핑을 UV로만 처리하므로 노트마다 머티리얼 인스턴스가 생기지 않습니다.
///
/// 몸통 아트가 아직 없어 지금은 텍스처 없이 단색 띠로 그려집니다. 그동안은 프리팹 Color가 곧 띠 색이며,
/// 머리 노트 그림과 같은 계열로 맞춰 두었습니다. 판정선에서 잘라 내는 일은 LiveNoteRenderer가 시작 깊이를
/// 이미 줄여 넘기므로 여기서는 하지 않습니다.
/// </summary>
public class UI_LiveHoldNoteBody : MaskableGraphic
{
    [Tooltip("세로로 반복되므로 아틀라스에 묶이지 않은 Wrap Mode = Repeat 텍스처여야 합니다. 비워 두면 흰 텍스처로 그려져 Color 값이 그대로 띠 색이 됩니다. 몸통 아트가 들어오면 Color를 흰색으로 되돌려 텍스처 색이 그대로 나오게 하십시오.")]
    [SerializeField]
    private Sprite _bodySprite;

    // 넘겨받은 목록을 그대로 들고 있지 않고 자기 것에 옮겨 담습니다.
    // 메시 생성은 캔버스 갱신 시점으로 미뤄지는데, 그 사이 호출자의 버퍼는 이미 다음 노트의 것으로 덮이기 때문입니다.
    // 용량을 처음부터 최대치로 잡아 두어야 첫 롱노트를 채우는 동안 내부 배열이 여러 번 할당되지 않습니다.
    private readonly List<LiveHoldBodySample> _samples = new List<LiveHoldBodySample>(LiveHoldBodySample.MAX_COUNT);

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
