using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// LIVE 관련 화면 이동을 한곳에서 처리합니다.
/// 전환 매니저는 PersistentScene에 있으므로 리듬게임·결과 씬을 단독으로 열어 확인할 때는 없을 수 있는데,
/// 그때마다 같은 방어 코드를 반복하지 않도록 모았습니다.
/// </summary>
public static class LiveSceneNavigator
{
    public static void LoadScene(ESceneNames sceneName, CancellationToken cancellationToken, string requesterName)
    {
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogWarning($"[{requesterName}] SceneTransitionManager가 없어 {sceneName}으로 이동하지 못했습니다. PersistentScene이 로드되어 있는지 확인해 주세요.");
            return;
        }

        SceneTransitionManager.Instance.LoadSceneAsync(sceneName, cancellationToken).Forget();
    }
}
