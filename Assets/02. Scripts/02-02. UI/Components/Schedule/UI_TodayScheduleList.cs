using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 오늘의 일일 스케줄 목록입니다. 홈 화면과 사무실 화면이 같은 클래스를 씁니다.
///
/// 행은 프리팹에 미리 배치된 것을 씁니다. 하루 스케줄이 3개 고정이라(기획서 3-E-1) 풀에서
/// 꺼낼 이유가 없고, 고정 배치가 레이아웃도 안정적입니다.
/// </summary>
public class UI_TodayScheduleList : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<UI_ScheduleItemToday> _rows = new List<UI_ScheduleItemToday>();

    private EEntryType _entryType = EEntryType.HOME_LIVE;

    /// <summary>
    /// 이 목록이 놓인 화면의 LIVE 진입 유형을 정합니다.
    /// 홈은 HOME_LIVE, 사무실은 SCHEDULE_LIVE입니다. RefreshSchedules보다 먼저 호출해야 합니다.
    /// </summary>
    public void Init(EEntryType entryType)
    {
        _entryType = entryType;
    }

    /// <summary>
    /// 오늘의 스케줄을 다시 읽어 행에 채웁니다.
    /// 화면에 들어올 때와 다른 화면에서 돌아왔을 때만 호출합니다. 매 프레임 부르는 용도가 아닙니다.
    /// </summary>
    public void RefreshSchedules()
    {
        List<ScheduleData> schedules = GameProgressService.Instance.GetTodaySchedules();

        for (int i = 0; i < _rows.Count; i++)
        {
            UI_ScheduleItemToday row = _rows[i];

            if (row == null)
            {
                continue;
            }

            if (schedules.Count <= i)
            {
                row.Clear();
                continue;
            }

            ScheduleData schedule = schedules[i];
            bool isCompleted = GameProgressService.Instance.IsScheduleCompleted(schedule.ScheduleId);

            row.SetSchedule(schedule, isCompleted, _entryType);
        }

        if (_rows.Count < schedules.Count)
        {
            Debug.LogWarning($"[UI_TodayScheduleList] 표시할 행이 모자랍니다. 스케줄 {schedules.Count}건 / 행 {_rows.Count}개");
        }
    }
}
