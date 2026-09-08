using System;
using UnityEngine;

/// <summary>
/// 지금 표시할 언어를 들고 있습니다.
///
/// PlayerPrefs에 두는 것은 이 값이 플레이어의 진행이 아니라 기기의 취향이기 때문입니다.
/// 세이브 데이터(PlayerDataProvider)에 넣으면 계정을 옮길 때 따라다니고, 세이브가 없는
/// 첫 실행이나 초기화 직후에는 읽을 수 없습니다.
///
/// 매니저가 아니라 정적 클래스인 것은 씬에 존재하지 않아도 읽을 수 있어야 하기 때문입니다.
/// 대본을 읽는 시점(StoryScriptLoader)은 화면이 뜨기 전입니다.
/// </summary>
public static class LanguageSetting
{
    private const string PREFS_KEY = "Language";

    /// <summary>
    /// 언어가 바뀌었습니다. 이미 화면에 떠 있는 글자를 다시 그리는 쪽이 구독합니다.
    /// </summary>
    public static event Action<ELanguage> OnChanged;

    private static ELanguage? _current;

    public static ELanguage Current
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

    public static void Set(ELanguage language)
    {
        if (Current == language)
        {
            return;
        }

        _current = language;
        PlayerPrefs.SetInt(PREFS_KEY, (int)language);
        PlayerPrefs.Save();

        OnChanged?.Invoke(language);
    }

    /// <summary>
    /// 저장된 값을 읽습니다. 정의에 없는 값이면 한국어로 둡니다.
    /// 열거형이 줄었을 때 저장된 옛 값이 그대로 형변환되면 어느 언어도 아닌 상태가 됩니다.
    /// </summary>
    private static ELanguage Load()
    {
        int saved = PlayerPrefs.GetInt(PREFS_KEY, (int)ELanguage.KOREAN);

        return Enum.IsDefined(typeof(ELanguage), saved) ? (ELanguage)saved : ELanguage.KOREAN;
    }
}
