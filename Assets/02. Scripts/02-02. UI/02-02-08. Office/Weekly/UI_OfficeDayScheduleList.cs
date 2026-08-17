using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 주간 스케줄 표의 한 칸이 품는 스케줄 목록입니다.
///
/// 줄은 프리팹에 미리 배치된 것을 씁니다. 하루 스케줄이 3개 고정이라(기획서 3-E-1) 풀에서
/// 꺼낼 이유가 없고, 고정 배치가 레이아웃도 안정적입니다(UI_TodayScheduleList와 같은 방식).
///
/// 완료 여부는 목록을 넘기는 쪽이 아니라 여기서 조회합니다. 칸마다 스케줄 수만큼 반복되는
/// 조회라 호출부에 널어 두면 주간 표를 그릴 때마다 같은 코드가 일곱 벌로 늘어납니다.
/// </summary>
public class UI_OfficeDayScheduleList : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<UI_OfficeDayScheduleItem> _items = new List<UI_OfficeDayScheduleItem>();

    public void SetSchedules(List<ScheduleData> schedules)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (schedules.Count <= i)
            {
                _items[i].Clear();
                continue;
            }

            ScheduleData schedule = schedules[i];
            bool isCompleted = GameProgressService.Instance.IsScheduleCompleted(schedule.ScheduleId);

            _items[i].SetSchedule(schedule.Title, isCompleted);
        }

        if (_items.Count < schedules.Count)
        {
            Debug.LogWarning($"[UI_OfficeDayScheduleList] 표시할 줄이 모자랍니다. 스케줄 {schedules.Count}건 / 줄 {_items.Count}개");
        }
    }
}
