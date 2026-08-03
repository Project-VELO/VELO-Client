using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 스케줄 바로가기를 눌렀을 때 어디로 갈지 정하는 유일한 창구입니다.
///
/// 홈과 사무실이 같은 스케줄 목록을 보여주므로(기획서.txt:1998), 목적지 판단이 두 화면에 흩어지면
/// 같은 스케줄이 화면마다 다른 곳으로 이동하게 됩니다. 판단을 여기 한 곳에 모읍니다.
///
/// 완료 판정이 아니라 화면 이동 정책이라 ScheduleCompletionRule과 나란히 두지 않고 스케줄 도메인에 둡니다.
/// </summary>
public static class ScheduleShortcutRouter
{
    public static void MoveToSchedule(ScheduleData schedule, EEntryType entryType, CancellationToken cancellationToken = default)
    {
        if (ReferenceEquals(schedule, null))
        {
            return;
        }

        switch (schedule.ScheduleType)
        {
            case EScheduleType.LIVE:
                MoveToLive(schedule, entryType, cancellationToken);
                break;

            case EScheduleType.STORY:
                MoveToStory(schedule, cancellationToken);
                break;

            default:
                Debug.LogWarning($"[ScheduleShortcutRouter] 이동 규칙이 없는 스케줄 유형입니다: {schedule.ScheduleId} / {schedule.ScheduleType}");
                break;
        }
    }

    /// <summary>
    /// 지정곡을 고정한 채 곡 선택 화면으로 보냅니다(기획서.txt:490-495).
    /// </summary>
    private static void MoveToLive(ScheduleData schedule, EEntryType entryType, CancellationToken cancellationToken)
    {
        LiveEntryStarter.StartLive(entryType, schedule.ClearCondition.TargetSongId, schedule.ScheduleId, cancellationToken);
    }

    /// <summary>
    /// 스토리 목록 화면으로 보냅니다(기획서.txt:565-566).
    ///
    /// 대상 스토리를 컨텍스트에 실어 보내야 목록 화면이 해당 회차로 자동 스크롤하고 강조할 수 있습니다.
    /// 여기서 넣어 주지 않으면 이전에 남은 값 때문에 엉뚱한 스토리가 강조됩니다.
    /// </summary>
    private static void MoveToStory(ScheduleData schedule, CancellationToken cancellationToken)
    {
        StoryEntryContext.Instance.SetShortcutEntry(schedule.ClearCondition.TargetStoryId);

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogWarning("[ScheduleShortcutRouter] SceneTransitionManager가 없어 스토리 목록으로 이동하지 못했습니다.");
            return;
        }

        SceneTransitionManager.Instance.LoadSceneAsync(ESceneNames.SelectStoryScene, cancellationToken).Forget();
    }
}
