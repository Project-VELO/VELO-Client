using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 언어를 고른 순간 이미 떠 있는 화면을 그 자리에서 다시 맞춥니다.
///
/// 고르는 자리가 종료 확인 팝업이라, 바뀌어야 할 것은 팝업이 아니라 그 뒤에 깔린 화면입니다.
/// 팝업을 닫고 화면을 다시 열 때까지 기다리게 하면 방금 누른 것이 먹히지 않은 것처럼 보입니다.
///
/// 씬을 다시 읽지 않는 것은 진행 중인 상태를 잃지 않기 위해서입니다. 고른 곡, 스크롤 위치,
/// 열려 있는 팝업이 그대로 있어야 합니다.
///
/// 떠 있는 씬을 모두 봅니다. 화면 한 장이 아니라 PersistentScene의 공용 팝업까지 함께 떠 있고,
/// 어느 씬이 화면을 맡고 있는지는 이 자리에서 알 수 없기 때문입니다.
/// </summary>
public static class LanguageChangeRefresher
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        LanguageSetting.OnApplied -= OnLanguageApplied;
        LanguageSetting.OnApplied += OnLanguageApplied;
    }

    private static void OnLanguageApplied(ELanguage language)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (!scene.isLoaded)
            {
                continue;
            }

            Refresh(scene);
        }
    }

    private static void Refresh(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            UiTextLocalizer.Apply(roots[i]);
        }

        // 훑기를 끝낸 뒤에 부릅니다. 스스로 다시 그리는 쪽이 내놓는 글자가 최종이라,
        // 순서가 뒤바뀌면 훑기가 그 위에 덮어씁니다.
        for (int i = 0; i < roots.Length; i++)
        {
            Notify(roots[i]);
        }
    }

    /// <summary>
    /// 꺼져 있는 것까지 부릅니다. 탭 뒤에 숨은 패널은 지금 안 보일 뿐 다음에 켜질 때 그대로
    /// 드러나고, 그 시점에는 다시 그릴 기회가 없습니다.
    /// </summary>
    private static void Notify(GameObject root)
    {
        ILanguageRefreshable[] targets = root.GetComponentsInChildren<ILanguageRefreshable>(true);

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].RefreshLanguage();
        }
    }
}
