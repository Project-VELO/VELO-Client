using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 홈 화면의 스케줄 패널입니다. 오늘 구역과 내일 예고 구역을 함께 관리합니다.
///
/// 사무실의 UI_OfficeTodayPanel과 같은 자리에 있는 클래스로, 날짜 헤더를 그리고 목록에 갱신을
/// 시킵니다. 목록 자체는 두 화면이 공유하는 UI_TodayScheduleList가 그립니다.
///
/// 주차의 마지막 날짜에는 내일이 없으므로 예고 구역을 통째로 숨깁니다. 비워 둔 채로 두면
/// 제목만 있고 내용이 없는 구역이 남습니다.
/// </summary>
public class UI_HomeSchedulePanel : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("Today")]
    [SerializeField]
    private TMP_Text _todayDateText;

    [SerializeField]
    private UI_TodayScheduleList _todayList;

    [Foldout("Hierarchy")]
    [Header("Tomorrow")]
    [SerializeField]
    private GameObject _tomorrowRoot;

    [SerializeField]
    private TMP_Text _tomorrowDateText;

    [SerializeField]
    private UI_TomorrowScheduleList _tomorrowList;

    /// <summary>
    /// 홈의 스케줄 바로가기는 HOME_LIVE로 진입합니다. 결과·뒤로가기의 복귀 표가 이 값을 봅니다.
    /// </summary>
    public void Init()
    {
        _todayList.Init(EEntryType.HOME_LIVE);
    }

    public void Refresh()
    {
        int weekOrder = GameProgressService.Instance.GetCurrentWeekOrder();

        _todayDateText.text = ScheduleDayLabel.Format(weekOrder, GameProgressService.Instance.GetCurrentDayNumber());
        _todayList.RefreshSchedules();

        RefreshTomorrow(weekOrder);
    }

    private void RefreshTomorrow(int weekOrder)
    {
        int tomorrowDayNumber = GameProgressService.Instance.GetTomorrowDayNumber();
        bool hasTomorrow = 0 < tomorrowDayNumber;

        _tomorrowRoot.SetActive(hasTomorrow);

        if (!hasTomorrow)
        {
            return;
        }

        _tomorrowDateText.text = ScheduleDayLabel.Format(weekOrder, tomorrowDayNumber);
        _tomorrowList.RefreshSchedules();
    }
}
