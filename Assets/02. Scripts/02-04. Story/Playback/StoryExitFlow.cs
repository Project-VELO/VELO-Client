using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// 감상이 끝났을 때 무엇을 남기고 어디로 갈지를 담당합니다.
///
/// 재생 컨트롤러에서 떼어낸 이유는 "대사를 어떻게 보여 줄 것인가"와
/// "끝났을 때 무엇을 저장할 것인가"가 서로 다른 일이기 때문입니다.
/// 완료와 중간 이탈의 차이도 여기 한 곳에서만 갈립니다.
/// </summary>
public class StoryExitFlow
{
    /// <summary>
    /// 마지막 NEXT 처리입니다(기획서 3-F-4, 3-J-4).
    ///
    /// CompleteStory 하나가 완료 판정 → 보상 지급 → 스케줄 동기화 → 저장을 묶어 수행합니다.
    /// 보상 지급이나 SyncStorySchedules를 여기서 따로 부르면 이중 지급이 됩니다.
    /// 보상 안내 팝업은 감상 화면이 아니라 목록 화면이 띄우므로 결과만 컨텍스트에 남깁니다.
    /// </summary>
    public void CompleteAndReturn(StoryData story, CancellationToken cancellationToken)
    {
        bool isFirstCompletion = GameProgressService.Instance.CompleteStory(story.StoryId);
        StoryEntryContext.Instance.SetCompletionResult(story.StoryId, isFirstCompletion, story.RewardId);

        ReturnToStoryList(cancellationToken);
    }

    /// <summary>
    /// 중간 이탈과 대본 로드 실패에 씁니다(기획서 3-F-5, 3-L).
    ///
    /// CompleteStory를 부르지 않고 컨텍스트에도 아무것도 남기지 않습니다.
    /// 진행도를 저장하는 코드가 아예 없으므로 다시 감상하면 첫 대사부터 시작합니다.
    /// </summary>
    public void ReturnToStoryList(CancellationToken cancellationToken)
    {
        MoveAsync(cancellationToken).Forget();
    }

    /// <summary>
    /// 대본을 읽지 못했을 때입니다. 기획서 3-L은 "오류 팝업 <b>후</b> 스토리 목록 복귀"로 정하고 있어,
    /// 곧바로 되돌리지 않고 안내를 닫는 시점에 복귀시킵니다.
    ///
    /// 안내를 띄우지 못하는 상황(PersistentScene 없이 단독 실행)에서는 UIManager가 콜백을 그대로 실행하므로,
    /// 어느 경우에도 빈 감상 화면에 갇히지 않습니다.
    /// </summary>
    public void NotifyErrorAndReturn(string message, CancellationToken cancellationToken)
    {
        if (UIManager.Instance == null)
        {
            ReturnToStoryList(cancellationToken);
            return;
        }

        UIManager.Instance.OpenErrorPopup(message, () => ReturnToStoryList(cancellationToken));
    }

    /// <summary>
    /// 진행 중인 전환이 끝나기를 먼저 기다립니다.
    ///
    /// 대본을 읽지 못해 화면이 뜨자마자 되돌아가는 경우, 이 화면을 띄운 전환이 아직 끝나지 않아
    /// SceneTransitionManager의 중복 방지 가드에 걸립니다. 그러면 요청이 조용히 버려져
    /// 빈 감상 화면에 갇힙니다.
    /// </summary>
    private async UniTaskVoid MoveAsync(CancellationToken cancellationToken)
    {
        if (SceneTransitionManager.Instance == null)
        {
            return;
        }

        while (SceneTransitionManager.Instance.IsTransitioning)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        // 감상이 끝나면 홈이 아니라 반드시 스토리 목록으로 돌아갑니다(기획서 6.5).
        SceneTransitionManager.Instance.LoadSceneAsync(ESceneNames.SelectStoryScene, cancellationToken).Forget();
    }
}
