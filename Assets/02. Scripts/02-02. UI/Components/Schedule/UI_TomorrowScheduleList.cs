using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 홈 화면의 내일 스케줄 예고 목록입니다.
///
/// UI_TodayScheduleList와 나누어 둔 것은 그리는 규칙이 다르기 때문입니다. 내일 스케줄은 아직
/// 진행할 수 없어 완료 여부를 묻지 않고, 바로가기 대신 잠금과 D-1만 표시합니다. 한 클래스에
/// 두 규칙을 넣으면 오늘 목록에도 잠금 분기가 섞여 사무실 화면까지 함께 복잡해집니다.
/// 같은 이유로 항목 스크립트도 UI_ScheduleItemToday와 UI_ScheduleItemTomorrow로 나뉘어 있습니다.
///
/// 행은 프리팹에 미리 배치된 것을 씁니다. 하루 스케줄이 3개 고정이라(기획서 3-E-1) 풀에서
/// 꺼낼 이유가 없고, 고정 배치가 레이아웃도 안정적입니다.
///
/// 다만 예고 구역은 패널 높이에 맞춰 배치된 행 수만큼만 보여 줍니다. 하루 스케줄은 3개인데
/// 디자인의 예고 구역은 2행이므로 마지막 하나는 표시되지 않습니다. 미리보기라 의도된 생략이며,
/// 전부 보여 주려면 행을 늘리는 것만으로는 안 되고 패널 높이부터 다시 잡아야 합니다.
/// </summary>
public class UI_TomorrowScheduleList : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<UI_ScheduleItemTomorrow> _rows = new List<UI_ScheduleItemTomorrow>();

    /// <summary>
    /// 내일 스케줄을 다시 읽어 행에 채웁니다.
    /// 화면에 들어올 때와 다른 화면에서 돌아왔을 때만 호출합니다. 매 프레임 부르는 용도가 아닙니다.
    /// </summary>
    public void RefreshSchedules()
    {
        List<ScheduleData> schedules = GameProgressService.Instance.GetTomorrowSchedules();

        for (int i = 0; i < _rows.Count; i++)
        {
            UI_ScheduleItemTomorrow row = _rows[i];

            if (row == null)
            {
                continue;
            }

            if (schedules.Count <= i)
            {
                row.Clear();
                continue;
            }

            row.SetSchedule(schedules[i]);
        }
    }
}
