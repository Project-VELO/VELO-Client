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

        MoveAsync(sceneName, cancellationToken).Forget();
    }

    /// <summary>
    /// 진행 중인 전환이 끝나기를 먼저 기다립니다.
    ///
    /// 곡이나 채보를 읽지 못해 화면이 뜨자마자 되돌아가는 경우(LiveGameController.Start), 이 화면을 띄운
    /// 전환이 아직 끝나지 않아 SceneTransitionManager의 중복 방지 가드에 걸립니다. 그러면 요청이 조용히
    /// 버려져 검은 리듬게임 화면에 갇힙니다. 기획서 16-15는 이 상황을 "진입 차단"으로 정하고 있습니다.
    /// </summary>
    private static async UniTaskVoid MoveAsync(ESceneNames sceneName, CancellationToken cancellationToken)
    {
        while (SceneTransitionManager.Instance.IsTransitioning)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        SceneTransitionManager.Instance.LoadSceneAsync(sceneName, cancellationToken).Forget();
    }
}
