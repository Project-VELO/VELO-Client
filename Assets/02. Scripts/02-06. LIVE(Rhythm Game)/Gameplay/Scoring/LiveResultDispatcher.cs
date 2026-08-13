using System;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// 판정이 끝난 집계를 결과 데이터로 굳혀 결과 화면으로 넘깁니다.
/// 마지막 판정 연출을 볼 시간을 준 뒤 이동하므로(SCREEN-009 목업: 완주 3초 후 자동 이동),
/// 이동 시점을 아는 것이 이 클래스의 유일한 책임입니다.
///
/// 결과 데이터 조립(구 LiveResultBuilder)은 이 클래스에서만 쓰여 병합했습니다.
/// 보상과 최고 기록은 결과 화면 진입 시점에 계산하므로(3-J-4) 여기서는 채우지 않습니다.
///
/// 유니티 이벤트 메서드를 쓰지 않으므로 컴포넌트가 아니라 컨트롤러가 생성해 쓰는 일반 클래스입니다.
/// </summary>
public class LiveResultDispatcher
{
    public const string FAIL_REASON_LOW_ACCURACY = "LOW_ACCURACY";

    private const float RESULT_DELAY_SECONDS = 3f;

    private readonly LiveJudgementProcessor _judgementProcessor;

    public LiveResultDispatcher(LiveJudgementProcessor judgementProcessor)
    {
        _judgementProcessor = judgementProcessor;
    }

    public void Dispatch(CancellationToken cancellationToken)
    {
        DispatchAsync(cancellationToken).Forget();
    }

    private async UniTaskVoid DispatchAsync(CancellationToken cancellationToken)
    {
        LiveResultContext.Instance.SetResult(BuildResult(
            _judgementProcessor.ScoreTracker,
            _judgementProcessor.TotalNoteCount));

        await UniTask.Delay(TimeSpan.FromSeconds(RESULT_DELAY_SECONDS), DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken);

        LiveSceneNavigator.LoadScene(ESceneNames.LiveResultScene, cancellationToken, nameof(LiveResultDispatcher));
    }

    private static LiveResultData BuildResult(LiveScoreTracker tracker, int totalNoteCount)
    {
        LiveEntryContext context = LiveEntryContext.Instance;

        float accuracy = LiveRankEvaluator.GetAccuracy(tracker.Score, totalNoteCount);
        ELiveRank rank = LiveRankEvaluator.Evaluate(accuracy, tracker.PerfectCount, totalNoteCount);
        bool isClear = LiveRankEvaluator.IsClear(rank);

        return new LiveResultData
        {
            PlayResultId = Guid.NewGuid().ToString(),
            EntryType = context.EntryType,
            ScheduleId = context.ScheduleId,
            SongId = context.SelectedSongId,
            Difficulty = context.SelectedDifficulty,

            IsClear = isClear,
            FailReason = GetFailReason(isClear),

            Score = tracker.Score,
            Rank = rank,
            Accuracy = accuracy,
            TotalNoteCount = totalNoteCount,

            PerfectCount = tracker.PerfectCount,
            GreatCount = tracker.GreatCount,
            GoodCount = tracker.GoodCount,
            BadCount = tracker.BadCount,

            MaxCombo = tracker.MaxCombo,
            IsFullCombo = tracker.IsFullCombo,
        };
    }

    /// <summary>
    /// 귀신 노트 실패가 즉시 종료 대신 감점으로 바뀐 뒤로 실패 경로는 정확도 미달 하나뿐입니다(3-I-6).
    /// </summary>
    private static string GetFailReason(bool isClear)
    {
        return isClear ? null : FAIL_REASON_LOW_ACCURACY;
    }
}
