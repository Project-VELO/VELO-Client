using System;
using UnityEngine;

/// <summary>
/// 플레이 중 노트가 다가오는 속도(하이스피드)를 들고 있습니다.
///
/// PlayerPrefs에 두는 것은 이 값이 플레이어의 진행이 아니라 기기의 취향이기 때문입니다.
/// 세이브 데이터에 넣으면 계정을 옮길 때 따라다니고, 세이브가 없는 첫 실행에는 읽을 수 없습니다.
/// 매니저가 아니라 정적 클래스인 것도 LanguageSetting과 같은 이유입니다.
///
/// 채보 에디터의 하이스피드와 키를 나눕니다. 그쪽은 마디를 넓게 펴 보려고 10배까지 올리는
/// 작업자의 편집용 화면 설정이라, 플레이어가 고른 속도와 성격이 다릅니다.
/// </summary>
public static class LiveHiSpeedSetting
{
    private const string PREFS_KEY = "Live.HiSpeed";

    /// <summary>
    /// 플레이어에게 내보이는 범위입니다. LiveScrollMapper는 에디터를 위해 10배까지 열어 두므로
    /// 여기서 한 번 더 좁게 조입니다.
    /// </summary>
    public const float MIN_HI_SPEED = 0.5f;
    public const float MAX_HI_SPEED = 5f;

    public const float FINE_STEP = 0.1f;
    public const float COARSE_STEP = 1f;

    // 1.0 안에 든 FINE_STEP 칸 수(10)입니다. Round가 이 수로 곱했다 나눠 값을 0.1 단위에 맞춥니다.
    private const float STEPS_PER_UNIT = 1f / FINE_STEP;

    /// <summary>
    /// 하이스피드가 바뀌었습니다. 트랙에 물려 주는 쪽이 구독합니다.
    /// </summary>
    public static event Action<float> OnChanged;

    private static float? _current;

    public static float Current
    {
        get
        {
            if (_current == null)
            {
                _current = Load();
            }

            return _current.Value;
        }
    }

    public static void Set(float hiSpeed)
    {
        float clamped = Round(Mathf.Clamp(hiSpeed, MIN_HI_SPEED, MAX_HI_SPEED));

        if (Current == clamped)
        {
            return;
        }

        _current = clamped;
        PlayerPrefs.SetFloat(PREFS_KEY, clamped);
        PlayerPrefs.Save();

        OnChanged?.Invoke(clamped);
    }

    /// <summary>
    /// 저장된 값을 읽습니다. 범위를 좁히기 전에 저장된 값이 남아 있을 수 있어 읽을 때도 조입니다.
    /// 값을 정하지 않은 모든 곳과 같은 기본값을 쓰려고 LiveScrollMapper의 것을 그대로 가져옵니다.
    /// </summary>
    private static float Load()
    {
        float saved = PlayerPrefs.GetFloat(PREFS_KEY, LiveScrollMapper.DEFAULT_HI_SPEED);

        return Round(Mathf.Clamp(saved, MIN_HI_SPEED, MAX_HI_SPEED));
    }

    /// <summary>
    /// 값을 0.1 단위에 맞춥니다. 버튼으로만 바꾸면 늘 0.1 단위지만, 0.1씩 더하다 보면 부동소수점 오차가 쌓여
    /// 4.9999 같은 값이 되고, 코드에서 Set(1.23f)처럼 넣으면 화면에는 1.2x로 보이는데 트랙은 1.23으로 흐릅니다.
    ///
    /// 칸 수를 FINE_STEP으로 곱해 되돌리지 않고 STEPS_PER_UNIT으로 나눕니다. 9 × 0.1f는 0.90000004가 되어
    /// 0.9f와 어긋나는 칸이 생기지만(0.1은 float로 딱 떨어지지 않음), 9 / 10f는 0.9f 그대로 나옵니다.
    /// </summary>
    private static float Round(float hiSpeed)
    {
        return Mathf.Round(hiSpeed * STEPS_PER_UNIT) / STEPS_PER_UNIT;
    }
}
