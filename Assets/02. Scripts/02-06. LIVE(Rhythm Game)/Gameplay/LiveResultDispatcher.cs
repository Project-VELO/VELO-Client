using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

/// <summary>
/// 판정이 끝난 집계를 결과 데이터로 굳혀 결과 화면으로 넘깁니다.
/// 마지막 판정 연출을 볼 시간을 준 뒤 이동하므로(SCREEN-009 목업: 완주 3초 후 자동 이동),
/// 이동 시점을 아는 것이 이 클래스의 유일한 책임입니다.
/// </summary>
public class LiveResultDispatcher : MonoBehaviour
{
    private const float RESULT_DELAY_SECONDS = 3f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveJudgementProcessor _judgementProcessor;

    public void Dispatch()
    {
        DispatchAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid DispatchAsync(CancellationToken cancellationToken)
    {
        LiveResultContext.Instance.SetResult(LiveResultBuilder.Build(
            _judgementProcessor.ScoreTracker,
            _judgementProcessor.TotalNoteCount,
            _judgementProcessor.HasGhostFailed));

        await UniTask.Delay(TimeSpan.FromSeconds(RESULT_DELAY_SECONDS), DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken);

        LiveSceneNavigator.LoadScene(ESceneNames.LiveResultScene, cancellationToken, nameof(LiveResultDispatcher));
    }
}
