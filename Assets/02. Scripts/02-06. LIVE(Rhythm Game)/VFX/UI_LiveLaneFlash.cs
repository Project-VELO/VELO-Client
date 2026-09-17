using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타격이 들어온 레인을 화면 맨 아래부터 트랙 끝까지 한 번 번쩍이게 그립니다.
///
/// 외주 시안은 레인마다 사다리꼴 그래픽을 따로 두고 폭과 기울기를 손으로 적었는데(아래 239/245/221, 위 42/39/45),
/// 그 수치는 UI_LiveTrackLanes의 레인 모양을 화면 맨 아래와 맨 위에서 잰 값과 같습니다. 손으로 옮긴 수치는 트랙 모양이
/// 바뀌면 혼자 어긋나므로 레인 모양은 트랙에서 직접 받고, 여섯 레인을 메시 하나로 그립니다.
/// 레인 경계는 화면 높이에 대해 직선이라 레인마다 사각형 하나면 모양이 그대로 나옵니다.
///
/// 트랙과 이 그래픽은 서로 다른 캔버스에 있으므로 꼭짓점은 월드 좌표를 거쳐 옮깁니다.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class UI_LiveLaneFlash : MaskableGraphic
{
    [Header("Fade")]
    [Tooltip("번쩍임이 사라지는 속도입니다(초당 알파 감소량). 2면 0.5초에 걸쳐 사라집니다. 일시정지와 상관없이 실제 시간으로 흐릅니다.")]
    [SerializeField]
    private float _fadeSpeed = 2f;

    private readonly List<float> _laneAlphas = new List<float>(LiveLane.COUNT);

    private UI_LiveTrackLanes _lanes;

    protected override void Awake()
    {
        base.Awake();

        InitLaneAlphas();
    }

    private void Update()
    {
        RefreshFade();
    }

    public void Init(UI_LiveTrackLanes lanes)
    {
        _lanes = lanes;
        SetVerticesDirty();
    }

    public void RefreshFlash(int lane)
    {
        int index = lane - LiveLane.FIRST;

        if (index < 0 || _laneAlphas.Count <= index)
        {
            return;
        }

        _laneAlphas[index] = 1f;
        SetVerticesDirty();
    }

    public void ClearFlashes()
    {
        for (int i = 0; i < _laneAlphas.Count; i++)
        {
            _laneAlphas[i] = 0f;
        }

        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (_lanes == null)
        {
            return;
        }

        for (int i = 0; i < _laneAlphas.Count; i++)
        {
            // 다 사라진 레인은 꼭짓점을 넣지 않습니다. CanvasRenderer가 빈 메시를 건너뛰므로 평소에는 그리는 비용이 없습니다.
            if (_laneAlphas[i] <= 0f)
            {
                continue;
            }

            AddLaneQuad(vh, LiveLane.FIRST + i, _laneAlphas[i]);
        }
    }

    private void InitLaneAlphas()
    {
        _laneAlphas.Clear();

        for (int i = 0; i < LiveLane.COUNT; i++)
        {
            _laneAlphas.Add(0f);
        }
    }

    private void RefreshFade()
    {
        float fadeAmount = _fadeSpeed * Time.unscaledDeltaTime;
        bool isFading = false;

        for (int i = 0; i < _laneAlphas.Count; i++)
        {
            if (_laneAlphas[i] <= 0f)
            {
                continue;
            }

            _laneAlphas[i] = Mathf.Max(0f, _laneAlphas[i] - fadeAmount);
            isFading = true;
        }

        if (isFading)
        {
            SetVerticesDirty();
        }
    }

    /// <summary>
    /// 깊이 비율 0이 화면 맨 아래, 1이 화면 맨 위라 두 높이의 레인 경계만 이으면 됩니다.
    /// UV는 외주 사다리꼴과 같게 왼쪽 아래가 (0, 0), 오른쪽 위가 (1, 1)이라 셰이더가 아래에서 위로 흐리는 그라데이션을 그대로 씁니다.
    /// </summary>
    private void AddLaneQuad(VertexHelper vh, int lane, float alpha)
    {
        _lanes.GetLaneBoundsAtRatio(lane, 0f, out float bottomLeftX, out float bottomRightX);
        _lanes.GetLaneBoundsAtRatio(lane, 1f, out float topLeftX, out float topRightX);
        float bottomY = _lanes.GetLocalY(0f);
        float topY = _lanes.GetLocalY(1f);

        Color32 vertexColor = color;
        vertexColor.a = (byte)Mathf.RoundToInt(color.a * alpha * 255f);

        int startIndex = vh.currentVertCount;
        vh.AddVert(ToLocal(bottomLeftX, bottomY), vertexColor, new Vector2(0f, 0f));
        vh.AddVert(ToLocal(topLeftX, topY), vertexColor, new Vector2(0f, 1f));
        vh.AddVert(ToLocal(topRightX, topY), vertexColor, new Vector2(1f, 1f));
        vh.AddVert(ToLocal(bottomRightX, bottomY), vertexColor, new Vector2(1f, 0f));

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }

    private Vector3 ToLocal(float lanesLocalX, float lanesLocalY)
    {
        Vector3 world = _lanes.rectTransform.TransformPoint(new Vector3(lanesLocalX, lanesLocalY, 0f));
        Vector3 local = rectTransform.InverseTransformPoint(world);

        return new Vector3(local.x, local.y, 0f);
    }
}
