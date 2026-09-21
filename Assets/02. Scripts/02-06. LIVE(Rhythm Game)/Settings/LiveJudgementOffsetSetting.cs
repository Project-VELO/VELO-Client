using System;
using UnityEngine;

/// <summary>
/// 플레이어가 고른 판정 보정값(ms)을 들고 있습니다.
///
/// 습관적으로 노트보다 일찍 또는 늦게 치는 사람의 입력을 판정 쪽에서 밀어 주기 위한 값입니다.
/// 입력 시각에 이 값을 더하므로, 일찍 치는 버릇이면 양수로, 늦게 치는 버릇이면 음수로 맞춥니다.
///
/// 하이스피드와 같은 이유로 세이브 데이터가 아니라 PlayerPrefs에 둡니다(LiveHiSpeedSetting 참고).
/// </summary>
public static class LiveJudgementOffsetSetting
{
    private const string PREFS_KEY = "Live.JudgementOffsetMs";

    /// <summary>
    /// 사람의 버릇만이 아니라 블루투스 이어폰처럼 소리가 늦게 나오는 기기까지 맞출 수 있도록 ±100ms까지 엽니다.
    /// PERFECT 판정창(±45ms, LiveJudgementRule)의 두 배가 넘어, 끝값 근처에서는 눈에 보이는 노트와 판정이 어긋나 보입니다.
    /// </summary>
    public const int MIN_OFFSET_MS = -100;
    public const int MAX_OFFSET_MS = 100;

    /// <summary>
    /// 보정하지 않은 상태입니다. 설정 탭의 0 버튼이 이 값으로 되돌립니다.
    /// </summary>
    public const int DEFAULT_OFFSET_MS = 0;

    /// <summary>
    /// 재생 시각이 정수 밀리초라(LiveConductor.SongTimeMs) 1ms보다 잘게 나눌 수 없습니다.
    /// 큰 폭은 슬라이더로 옮기고, 버튼은 그 뒤에 1ms·10ms 단위로 다듬는 데 씁니다.
    /// </summary>
    public const int FINE_STEP_MS = 1;
    public const int COARSE_STEP_MS = 10;

    /// <summary>
    /// 보정값이 바뀌었습니다. 판정에 쓰는 시각을 만드는 쪽이 구독합니다.
    /// </summary>
    public static event Action<int> OnChanged;

    private static int? _currentMs;

    public static int CurrentMs
    {
        get
        {
            if (_currentMs == null)
            {
                _currentMs = Load();
            }

            return _currentMs.Value;
        }
    }

    public static void Set(int offsetMs)
    {
        int clamped = Mathf.Clamp(offsetMs, MIN_OFFSET_MS, MAX_OFFSET_MS);

        if (CurrentMs == clamped)
        {
            return;
        }

        _currentMs = clamped;
        PlayerPrefs.SetInt(PREFS_KEY, clamped);
        PlayerPrefs.Save();

        OnChanged?.Invoke(clamped);
    }

    /// <summary>
    /// 저장된 값을 읽습니다. 범위를 좁히기 전에 저장된 값이 남아 있을 수 있어 읽을 때도 조입니다.
    /// </summary>
    private static int Load()
    {
        return Mathf.Clamp(PlayerPrefs.GetInt(PREFS_KEY, DEFAULT_OFFSET_MS), MIN_OFFSET_MS, MAX_OFFSET_MS);
    }
}
