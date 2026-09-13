using UnityEngine;

/// <summary>
/// 마디 좌표(마디 인덱스 + 마디 내 비율)와 트랙의 깊이 비율 사이를 상호 변환합니다.
/// 화면에 보이는 마디 수를 직접 지정하는 대신 리듬게임의 하이스피드 배율로 표현하며,
/// 시간이 아닌 마디를 기준으로 정규화하므로 곡의 BPM이 달라도 같은 하이스피드에서 체감 속도가 동일합니다.
///
/// 여기서 다루는 비율은 화면 높이가 아니라 깊이입니다. 마디 좌표를 깊이에 정비례하는 값으로만 옮기면
/// 노트가 등속으로 다가오게 되고, 다가오며 빨라 보이는 가속과 확대는 사다리꼴 형태 쪽에서 나옵니다.
///
/// 깊이 비율을 화면 세로 좌표로 옮기는 것은 LiveTrackShape와 UI_LiveTrackLanes.GetLocalY의 몫이며,
/// 둘은 선형 관계가 아닙니다. 이 클래스가 돌려준 비율을 화면 높이 비율처럼 다루면 안 됩니다.
/// </summary>
public class LiveScrollMapper
{
    // 하이스피드 1.0배일 때 히트라인부터 트랙 최상단까지 보이는 마디 수입니다.
    private const float BASE_VISIBLE_BAR_COUNT = 4f;

    public const float MIN_HI_SPEED = 0.5f;
    public const float MAX_HI_SPEED = 10f;

    /// <summary>
    /// 값을 따로 정하지 않은 모든 곳에서 쓰는 하이스피드입니다.
    /// 리듬게임과 채보 에디터가 같은 속도로 보여야 에디터에서 맞춘 배치가 실제 플레이와 어긋나지 않으므로,
    /// 기본값을 각자 적지 않고 여기 한 곳에 둡니다.
    ///
    /// 1.0배는 판정선부터 트랙 최상단까지 4마디가 한꺼번에 보여 노트가 너무 느리게 다가옵니다.
    /// </summary>
    public const float DEFAULT_HI_SPEED = 1.5f;

    public const float MIN_SPAWN_RATIO = 0.5f;
    public const float MAX_SPAWN_RATIO = 1f;

    private float _hiSpeed = DEFAULT_HI_SPEED;
    private float _spawnRatio = MAX_SPAWN_RATIO;

    public float HiSpeed
    {
        get => _hiSpeed;
        set => _hiSpeed = Mathf.Clamp(value, MIN_HI_SPEED, MAX_HI_SPEED);
    }

    /// <summary>
    /// 노트가 처음 나타나는 트랙 높이입니다. 1이면 트랙 최상단에서 등장하며, 낮출수록 노트가 판정선까지 오면서
    /// 겪는 확대 배율이 줄어들어 낙하가 안정적으로 보입니다. 리드 타임은 그대로이고 등장 높이만 내려오므로
    /// 트랙 아트웍 자체는 전혀 바뀌지 않습니다.
    /// </summary>
    public float SpawnRatio
    {
        get => _spawnRatio;
        set => _spawnRatio = Mathf.Clamp(value, MIN_SPAWN_RATIO, MAX_SPAWN_RATIO);
    }

    public float VisibleBarCount => BASE_VISIBLE_BAR_COUNT / _hiSpeed;

    /// <summary>
    /// 마디 좌표를 트랙 위의 세로 비율로 옮깁니다. 비율은 깊이에 정비례하므로, 시간에 정비례하는 이 변환이
    /// 곧 월드 공간에서의 등속 이동이 됩니다.
    /// </summary>
    public float ToVerticalRatio(double barPosition, double currentBarPosition, float hitLineRatio)
    {
        double progress = (barPosition - currentBarPosition) / VisibleBarCount;
        return hitLineRatio + (float)progress * (_spawnRatio - hitLineRatio);
    }

    /// <summary>
    /// ToVerticalRatio의 역변환입니다. 트랙을 클릭한 지점이 어느 시각에 해당하는지 되짚을 때 사용합니다.
    /// </summary>
    public double ToBarPosition(float verticalRatio, double currentBarPosition, float hitLineRatio)
    {
        float denominator = _spawnRatio - hitLineRatio;
        if (denominator <= 0f)
        {
            return currentBarPosition;
        }

        double progress = (verticalRatio - hitLineRatio) / denominator;
        return currentBarPosition + progress * VisibleBarCount;
    }

    public bool IsRatioVisible(float verticalRatio)
    {
        return 0f <= verticalRatio && verticalRatio <= _spawnRatio;
    }

    /// <summary>
    /// 길이가 있는 대상이 트랙에 걸치는지 봅니다. 롱노트는 머리가 지나가도 몸통이 남아 있어 한 점으로 판단할 수 없습니다.
    /// </summary>
    public bool IsSpanVisible(float startRatio, float endRatio)
    {
        return 0f <= endRatio && startRatio <= _spawnRatio;
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
