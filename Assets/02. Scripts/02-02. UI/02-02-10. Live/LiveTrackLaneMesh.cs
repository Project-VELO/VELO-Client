using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 트랙 사다리꼴의 면·구분선·판정선·바깥 테두리를 정점으로 쌓습니다.
/// UI_LiveTrackLanes가 좌표 조회와 메시 생성을 함께 들면 200줄을 넘겨 두 책임이 섞이므로 그리는 쪽만 떼어 냈습니다.
///
/// 좌표는 모두 트랙 로컬(가로는 화면 맨 아래 폭, 세로는 디자인 화면 높이)입니다.
/// 레인 경계가 깊이에 따라 모이므로 각 레인은 직사각형이 아니라 사다리꼴이며, 구분선도 기울어집니다.
/// </summary>
public class LiveTrackLaneMesh
{
    private readonly List<Vector2> _leftEdges = new List<Vector2>(LiveLane.COUNT + 1);
    private readonly List<Vector2> _rightEdges = new List<Vector2>(LiveLane.COUNT + 1);

    /// <summary>
    /// 트랙 전체를 다시 쌓습니다. laneBoundsAtRatio는 레인 경계 인덱스(0~LiveLane.COUNT)와 세로 비율을 받아
    /// 그 높이의 가로 좌표를 돌려줍니다. 위·아래 두 줄만 있으면 사다리꼴이 정해지므로 비율은 0과 1만 씁니다.
    /// </summary>
    public void Rebuild(VertexHelper vh, LiveTrackLaneMeshSettings settings,
        System.Func<int, float, float> laneBoundsAtRatio, System.Func<float, float> localYAtRatio,
        float hitLineRatio, IReadOnlyList<Color> laneColors, Color fallbackColor)
    {
        vh.Clear();

        float bottomY = localYAtRatio(0f);
        float topY = localYAtRatio(1f);

        _leftEdges.Clear();
        _rightEdges.Clear();

        for (int i = 0; i <= LiveLane.COUNT; i++)
        {
            _leftEdges.Add(new Vector2(laneBoundsAtRatio(i, 0f), bottomY));
            _rightEdges.Add(new Vector2(laneBoundsAtRatio(i, 1f), topY));
        }

        AddLaneFaces(vh, laneColors, fallbackColor);
        AddDividers(vh, settings);
        AddHitLine(vh, settings, laneBoundsAtRatio, localYAtRatio, hitLineRatio);
        AddSideBorders(vh, settings);
    }

    private void AddLaneFaces(VertexHelper vh, IReadOnlyList<Color> laneColors, Color fallbackColor)
    {
        for (int i = 0; i < LiveLane.COUNT; i++)
        {
            Color laneColor = (i < laneColors.Count) ? laneColors[i] : fallbackColor;

            AddQuad(vh,
                new Vector2(_leftEdges[i].x, _leftEdges[i].y),
                new Vector2(_rightEdges[i].x, _rightEdges[i].y),
                new Vector2(_rightEdges[i + 1].x, _rightEdges[i + 1].y),
                new Vector2(_leftEdges[i + 1].x, _leftEdges[i + 1].y),
                laneColor);
        }
    }

    private void AddDividers(VertexHelper vh, LiveTrackLaneMeshSettings settings)
    {
        for (int i = 1; i < LiveLane.COUNT; i++)
        {
            AddThickLine(vh, _leftEdges[i], _rightEdges[i], settings.DividerWidth, settings.DividerColor);
        }
    }

    private void AddSideBorders(VertexHelper vh, LiveTrackLaneMeshSettings settings)
    {
        AddThickLine(vh, _leftEdges[0], _rightEdges[0], settings.SideBorderWidth, settings.SideBorderColor);
        AddThickLine(vh, _leftEdges[LiveLane.COUNT], _rightEdges[LiveLane.COUNT],
            settings.SideBorderWidth, settings.SideBorderColor);
    }

    private void AddHitLine(VertexHelper vh, LiveTrackLaneMeshSettings settings,
        System.Func<int, float, float> laneBoundsAtRatio, System.Func<float, float> localYAtRatio, float hitLineRatio)
    {
        float y = localYAtRatio(hitLineRatio);
        float left = laneBoundsAtRatio(0, hitLineRatio);
        float right = laneBoundsAtRatio(LiveLane.COUNT, hitLineRatio);

        AddThickLine(vh, new Vector2(left, y), new Vector2(right, y),
            settings.HitLineThickness, settings.HitLineColor);
    }

    private static void AddQuad(VertexHelper vh, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Color col)
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

    private static void AddThickLine(VertexHelper vh, Vector2 start, Vector2 end, float width, Color col)
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

/// <summary>
/// 트랙 선의 색과 두께 묶음입니다. 메시 생성기가 POCO라 인스펙터에 직접 노출될 수 없어
/// UI_LiveTrackLanes가 이 묶음을 직렬화해 넘깁니다.
/// </summary>
[System.Serializable]
public class LiveTrackLaneMeshSettings
{
    public Color DividerColor = new Color(1f, 1f, 1f, 0.9f);
    public float DividerWidth = 2f;

    public Color HitLineColor = new Color(0.09f, 0.32f, 0.85f, 1f);
    public float HitLineThickness = 10f;

    public Color SideBorderColor = new Color(0.77f, 0.37f, 1.0f, 0.95f);
    public float SideBorderWidth = 6f;
}
