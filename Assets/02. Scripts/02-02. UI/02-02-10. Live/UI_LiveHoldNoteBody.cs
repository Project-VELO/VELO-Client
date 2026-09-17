using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 롱노트 몸통을 트랙 사다리꼴을 따라가는 띠로 그립니다.
/// 그림 매핑을 UV로만 처리하므로 노트마다 머티리얼 인스턴스가 생기지 않습니다.
///
/// 몸통 그림은 화면에 고정된 레인 띠이고, 줄마다 그 그림 안의 픽셀 좌표를 넘겨받아 UV로 바꾸기만 합니다.
/// 그림 속 자리를 정하는 일은 LiveHoldNoteRenderer가, 판정선에서 잘라 내는 일은 LiveNoteRenderer가 시작 깊이를
/// 줄여 넘기는 것으로 이미 끝내므로 여기서는 하지 않습니다.
/// </summary>
public class UI_LiveHoldNoteBody : MaskableGraphic
{
    [Tooltip("레인 띠를 화면 높이 1080 전체에 보이는 모양 그대로 1:1로 그린 그림입니다. 반복하지 않고 화면에 고정된 채 몸통 구간만 드러나므로 Wrap Mode는 Clamp로 둡니다. 비워 두면 흰 텍스처로 그려져 Color 값이 그대로 띠 색이 되므로, 그림을 쓰는 동안 Color는 흰색으로 둡니다.")]
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

        GetUvMapping(out Vector2 uvOrigin, out Vector2 uvPerPixel);

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        for (int i = 0; i < _samples.Count; i++)
        {
            LiveHoldBodySample sample = _samples[i];
            float v = uvOrigin.y + sample.ArtY * uvPerPixel.y;

            AddVertex(vh, ref vertex, sample.LeftX, sample.LocalY, uvOrigin.x + sample.LeftArtX * uvPerPixel.x, v);
            AddVertex(vh, ref vertex, sample.RightX, sample.LocalY, uvOrigin.x + sample.RightArtX * uvPerPixel.x, v);
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

    /// <summary>
    /// 그림 안의 픽셀 좌표를 텍스처 UV로 바꾸는 기준입니다. 텍스처 전체가 아니라 스프라이트가 차지한 영역을 원점으로 삼습니다.
    /// 그림이 없으면 흰 텍스처 한 점만 찍으면 되므로 모두 0으로 둡니다.
    /// </summary>
    private void GetUvMapping(out Vector2 uvOrigin, out Vector2 uvPerPixel)
    {
        if (_bodySprite == null)
        {
            uvOrigin = Vector2.zero;
            uvPerPixel = Vector2.zero;
            return;
        }

        Texture texture = _bodySprite.texture;
        Rect textureRect = _bodySprite.textureRect;

        uvPerPixel = new Vector2(1f / texture.width, 1f / texture.height);
        uvOrigin = new Vector2(textureRect.x * uvPerPixel.x, textureRect.y * uvPerPixel.y);
    }

    private static void AddVertex(VertexHelper vh, ref UIVertex vertex, float x, float y, float u, float v)
    {
        vertex.position = new Vector3(x, y);
        vertex.uv0 = new Vector2(u, v);
        vh.AddVert(vertex);
    }
}
