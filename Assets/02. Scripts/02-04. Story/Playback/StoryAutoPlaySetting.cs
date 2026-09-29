using UnityEngine;

/// <summary>
/// 감상 화면의 AUTO(자동 진행) 켜짐 여부를 들고 있습니다.
///
/// PlayerPrefs에 두는 것은 언어(LanguageSetting)와 같은 이유입니다. 이 값은 플레이어의 진행이 아니라
/// 기기의 취향이라, 세이브 데이터에 넣으면 계정을 옮길 때 따라다니고 세이브가 없을 때 읽을 수 없습니다.
/// 회차를 넘어 기억해 두는 것은, AUTO를 켠 채 다음 회차에 들어갔을 때 다시 켜야 한다면
/// 켜 둔 의미가 없기 때문입니다.
/// </summary>
public static class StoryAutoPlaySetting
{
    private const string PREFS_KEY = "StoryAutoPlay";

    private static bool? _isOn;

    public static bool IsOn
    {
        get
        {
            if (_isOn == null)
            {
                _isOn = PlayerPrefs.GetInt(PREFS_KEY, 0) == 1;
            }

            return _isOn.Value;
        }
    }

    public static void Set(bool isOn)
    {
        if (IsOn == isOn)
        {
            return;
        }

        _isOn = isOn;
        PlayerPrefs.SetInt(PREFS_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}
