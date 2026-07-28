using UnityEngine;

/// <summary>
/// 마디 좌표(마디 인덱스 + 마디 내 비율)와 트랙의 화면 세로 비율 사이를 상호 변환합니다.
/// 화면에 보이는 마디 수를 직접 지정하는 대신 리듬게임의 하이스피드 배율로 표현하며,
/// 시간이 아닌 마디를 기준으로 정규화하므로 곡의 BPM이 달라도 같은 하이스피드에서 체감 속도가 동일합니다.
/// </summary>
public class LiveEditorScrollMapper
{
    // 하이스피드 1.0배일 때 히트라인부터 트랙 최상단까지 보이는 마디 수입니다.
    private const float BASE_VISIBLE_BAR_COUNT = 4f;

    public const float MIN_HI_SPEED = 0.5f;
    public const float MAX_HI_SPEED = 10f;

    private float _hiSpeed = 1f;

    public float HiSpeed
    {
        get => _hiSpeed;
        set => _hiSpeed = Mathf.Clamp(value, MIN_HI_SPEED, MAX_HI_SPEED);
    }

    public float VisibleBarCount => BASE_VISIBLE_BAR_COUNT / _hiSpeed;

    public float ToVerticalRatio(double barPosition, double currentBarPosition, float hitLineRatio)
    {
        double barDelta = barPosition - currentBarPosition;
        return hitLineRatio + (float)(barDelta / VisibleBarCount) * (1f - hitLineRatio);
    }

    public double ToBarPosition(float verticalRatio, double currentBarPosition, float hitLineRatio)
    {
        float denominator = 1f - hitLineRatio;
        if (denominator <= 0f)
        {
            return currentBarPosition;
        }

        double barDelta = (verticalRatio - hitLineRatio) / denominator * VisibleBarCount;
        return currentBarPosition + barDelta;
    }

    public bool IsRatioVisible(float verticalRatio)
    {
        return verticalRatio >= 0f && verticalRatio <= 1f;
    }

    /// <summary>
    /// 현재 재생 위치를 기준으로 화면에 걸치는 마디 인덱스 범위를 구합니다.
    /// 히트라인 아래(이미 지나간) 영역까지 포함하기 위해 시작 마디를 한 칸 앞당깁니다.
    /// </summary>
    public void GetVisibleBarRange(double currentBarPosition, int barCount, out int startBarIndex, out int endBarIndex)
    {
        startBarIndex = Mathf.Max(0, Mathf.FloorToInt((float)currentBarPosition) - 1);
        endBarIndex = Mathf.Min(barCount - 1, Mathf.CeilToInt((float)(currentBarPosition + VisibleBarCount)));
    }
}
