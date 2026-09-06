using UnityEngine;

/// <summary>
/// 리듬게임 트랙 사다리꼴의 형태를 화면 좌표만으로 정의합니다.
///
/// 예전에는 평평한 직사각형 메시를 원근 카메라로 비스듬히 봐서 사다리꼴을 만들었습니다. 그런데 화면에 실제로
/// 보이는 트랙은 시안에서 받은 2D 그림(Image_Live_TrackPlate)이고, 메시는 좌표 계산에만 쓰이고 있었습니다.
/// 카메라를 눕히는 것은 그 좌표를 그림에 맞추기 위한 우회였을 뿐이라, 형태를 여기서 직접 정의하고
/// 카메라는 프로젝트 기본값인 직교 투영 그대로 둡니다.
///
/// 사다리꼴은 시안이 준 네 수치로 완전히 정해집니다. 폭은 화면 높이에 정비례해 줄어듭니다.
/// 판정선 높이에서 <see cref="_hitLineWidth"/>이고 화면 최상단에서 <see cref="_topScreenWidth"/>이므로,
/// 그 두 점을 지나는 직선이 곧 트랙의 좌우 경계입니다.
///
/// 세로는 깊이 기준(0이 앞쪽 끝, 1이 뒤쪽 끝)을 그대로 유지합니다. 화면 기준으로 바꾸면 노트가 다가오며
/// 빨라지는 느낌과 격자가 멀수록 촘촘해지는 모습이 사라집니다. 겉보기 폭은 깊이에 반비례하므로,
/// 깊이 비율과 화면 높이 사이의 변환은 폭의 역수를 선형 보간하는 것으로 끝납니다.
/// </summary>
[System.Serializable]
public class LiveTrackShape
{
    [Tooltip("시안을 그린 화면 세로 해상도입니다.")]
    [SerializeField]
    private float _designScreenHeight = 1080f;

    [Tooltip("화면 아래에서 판정선까지의 높이입니다.")]
    [SerializeField]
    private float _hitLineScreenY = 94.5f;

    [Tooltip("판정선 높이에서의 트랙 폭입니다.")]
    [SerializeField]
    private float _hitLineWidth = 1308.675f;

    [Tooltip("화면 최상단에서의 트랙 폭입니다.")]
    [SerializeField]
    private float _topScreenWidth = 252f;

    public float DesignScreenHeight => _designScreenHeight;

    /// <summary>
    /// 화면 맨 아래에서의 트랙 폭입니다. 레인 경계 비율(CUMULATIVE_LANE_RATIOS)이 이 폭을 기준으로 정규화되어 있습니다.
    /// </summary>
    public float BottomWidth => GetWidthAtScreenY(0f);

    public float TopWidth => GetWidthAtScreenY(_designScreenHeight);

    /// <summary>
    /// 판정선이 놓이는 깊이 비율입니다. 폭에서 거꾸로 구하므로 시안 수치가 바뀌면 저절로 따라옵니다.
    /// </summary>
    public float HitLineRatio => GetRatioAtScreenY(_hitLineScreenY);

    /// <summary>
    /// 화면 높이에 따른 트랙 폭입니다. 좌우 경계가 직선이므로 폭도 높이에 정비례합니다.
    /// </summary>
    public float GetWidthAtScreenY(float screenY)
    {
        float widthSlope = (_topScreenWidth - _hitLineWidth) / (_designScreenHeight - _hitLineScreenY);
        return _hitLineWidth + widthSlope * (screenY - _hitLineScreenY);
    }

    /// <summary>
    /// 깊이 비율에서의 트랙 폭입니다. 겉보기 폭은 깊이에 반비례하므로 역수를 선형 보간합니다.
    /// 비율을 0~1로 자르지 않습니다. 판정선을 지나 화면 밖으로 흘러가는 노트가 트랙 끝에 멈춰 서면 안 됩니다.
    /// </summary>
    public float GetWidthAtRatio(float verticalRatio)
    {
        float inverseWidth = Mathf.LerpUnclamped(1f / BottomWidth, 1f / TopWidth, verticalRatio);

        // 소실선 너머는 폭이 음수가 되므로, 화면 밖으로 밀어내되 뒤집히지는 않도록 아주 얇은 양수로 막습니다.
        return inverseWidth <= 0f ? MIN_WIDTH : Mathf.Max(MIN_WIDTH, 1f / inverseWidth);
    }

    /// <summary>
    /// 깊이 비율을 화면 높이로 옮깁니다. 폭이 높이에 정비례하므로 폭을 구한 뒤 높이로 되돌리면 됩니다.
    /// </summary>
    public float GetScreenYAtRatio(float verticalRatio)
    {
        return GetScreenYAtWidth(GetWidthAtRatio(verticalRatio));
    }

    /// <summary>
    /// 화면 높이를 깊이 비율로 옮깁니다. GetScreenYAtRatio의 역변환이며,
    /// 트랙을 클릭한 지점이 어느 시각인지 되짚는 채보 에디터가 이 방향을 씁니다.
    /// </summary>
    public float GetRatioAtScreenY(float screenY)
    {
        float width = GetWidthAtScreenY(screenY);

        if (width <= MIN_WIDTH)
        {
            return 1f;
        }

        float inverseBottom = 1f / BottomWidth;
        float denominator = 1f / TopWidth - inverseBottom;

        if (Mathf.Approximately(denominator, 0f))
        {
            return 0f;
        }

        return (1f / width - inverseBottom) / denominator;
    }

    private float GetScreenYAtWidth(float width)
    {
        float bottomWidth = BottomWidth;
        float span = bottomWidth - TopWidth;

        if (Mathf.Approximately(span, 0f))
        {
            return 0f;
        }

        return (bottomWidth - width) / span * _designScreenHeight;
    }

    // 소실선에 가까워질수록 폭이 0으로 수렴해 나눗셈이 터지므로, 화면 밖에서 쓰는 하한을 둡니다.
    private const float MIN_WIDTH = 0.01f;
}
