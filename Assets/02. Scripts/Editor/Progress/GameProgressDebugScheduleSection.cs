using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 진행 상태 디버그 창의 스케줄 영역을 그립니다.
/// 오늘의 스케줄 강제 완료와 하루 마무리, 주차 스케줄 표가 여기에 속합니다.
/// </summary>
public class GameProgressDebugScheduleSection
{
    public void Draw()
    {
        DrawTodaySchedules();
        EditorGUILayout.Space();
        DrawWeekTable();
    }

    private void DrawTodaySchedules()
    {
        EditorGUILayout.LabelField("오늘의 스케줄", EditorStyles.boldLabel);

        List<ScheduleData> schedules = GameProgressService.Instance.GetTodaySchedules();

        if (schedules.Count == 0)
        {
            EditorGUILayout.HelpBox("이 날짜에 등록된 스케줄이 없습니다.", MessageType.Warning);
            return;
        }

        foreach (ScheduleData schedule in schedules)
        {
            DrawScheduleRow(schedule);
        }

        EditorGUILayout.Space();
        DrawFinishDayButton();
    }

    private void DrawScheduleRow(ScheduleData schedule)
    {
        bool isCompleted = GameProgressService.Instance.IsScheduleCompleted(schedule.ScheduleId);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField($"[{schedule.ScheduleType}] {schedule.Title}", GUILayout.MinWidth(220f));

            if (isCompleted)
            {
                EditorGUILayout.LabelField("완료", GUILayout.Width(80f));
                return;
            }

            if (GUILayout.Button("완료 처리", GUILayout.Width(80f)))
            {
                CompleteSchedule(schedule);
            }
        }
    }

    /// <summary>
    /// 스토리 감상 스케줄은 대상 스토리를 완료시켜야 자연스럽게 완료됩니다.
    /// 스케줄만 강제로 완료시키면 스토리는 미완료로 남아 실제 플레이와 다른 상태가 되기 때문입니다.
    /// </summary>
    private void CompleteSchedule(ScheduleData schedule)
    {
        if (schedule.ScheduleType == EScheduleType.STORY && !string.IsNullOrEmpty(schedule.ClearCondition.TargetStoryId))
        {
            GameProgressService.Instance.CompleteStory(schedule.ClearCondition.TargetStoryId);
            return;
        }

        GameProgressService.Instance.ForceCompleteSchedule(schedule.ScheduleId);
    }

    private void DrawFinishDayButton()
    {
        bool canFinish = GameProgressService.Instance.IsDayCompletable();

        using (new EditorGUI.DisabledScope(!canFinish))
        {
            if (GUILayout.Button(canFinish ? "하루 마무리" : "하루 마무리 (필수 스케줄이 남았습니다)"))
            {
                FinishDay();
            }
        }
    }

    private void FinishDay()
    {
        DayFinishResult result = GameProgressService.Instance.FinishDay();

        if (!result.IsSuccess)
        {
            Debug.LogWarning("[GameProgressDebug] 하루를 마무리하지 못했습니다.");
            return;
        }

        if (!result.IsWeekFinished)
        {
            Debug.Log($"[GameProgressDebug] 다음 날짜로 이동했습니다: {result.NextDayId}");
            return;
        }

        Debug.Log($"[GameProgressDebug] 주차를 마무리했습니다. 해금된 스토리 {result.UnlockedStoryIds.Count}편");
    }

    private void DrawWeekTable()
    {
        EditorGUILayout.LabelField("주차 스케줄 표", EditorStyles.boldLabel);

        PlayerData data = PlayerDataProvider.Instance.Data;

        using (new EditorGUILayout.HorizontalScope())
        {
            foreach (string dayId in GameProgressService.Instance.GetCurrentWeekDayIds())
            {
                EDayViewState state = GameProgressService.Instance.GetDayViewState(data.CurrentWeekId, dayId);
                EditorGUILayout.LabelField($"{dayId}\n{state}", EditorStyles.miniLabel, GUILayout.Width(78f), GUILayout.Height(30f));
            }
        }
    }
}
