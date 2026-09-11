using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 프리팹에 박힌 UI 글자를 지금 언어로 갈아 끼웁니다.
///
/// 글자가 프리팹 안에 있어 화면 코드가 건드리지 않는 자리가 아흔 곳 넘습니다.
/// 그 자리마다 컴포넌트를 붙이는 대신, 화면이 열릴 때 한 번 훑어 바꿉니다.
/// 프리팹을 고치지 않으므로 다른 작업과 충돌하지 않고, 앞으로 글자가 늘어도 손댈 곳이 없습니다.
///
/// 훑는 시점은 두 곳입니다. 씬이 올라올 때 그 씬의 화면 전체를, 팝업이 열릴 때 그 팝업을 봅니다.
/// 풀에서 꺼내 쓰는 항목은 여기서 훑지 않아도 됩니다. 그런 자리의 글자는 마스터 데이터에서
/// 오고, 마스터 데이터는 읽는 순간 이미 번역되어 있습니다.
/// </summary>
public static class UiTextLocalizer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// 화면 하나를 훑어 글자를 바꿉니다. 꺼져 있는 것까지 봅니다.
    /// 탭이나 접힌 패널은 꺼진 채로 올라왔다가 나중에 켜지는데, 그때는 훑을 기회가 없습니다.
    /// </summary>
    public static void Apply(GameObject root)
    {
        if (root == null || LanguageSetting.Current == ELanguage.KOREAN)
        {
            return;
        }

        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);

        for (int i = 0; i < texts.Length; i++)
        {
            Apply(texts[i]);
        }
    }

    private static void Apply(TMP_Text target)
    {
        if (target == null || string.IsNullOrEmpty(target.text))
        {
            return;
        }

        string translated = UiText.Localize(target.text);

        // 같은 글자를 다시 대입하면 TMP가 메시를 다시 만듭니다. 바뀐 것만 건드립니다.
        if (!ReferenceEquals(translated, target.text) && translated != target.text)
        {
            target.text = translated;
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (LanguageSetting.Current == ELanguage.KOREAN)
        {
            return;
        }

        GameObject[] roots = scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            Apply(roots[i]);
        }
    }
}
