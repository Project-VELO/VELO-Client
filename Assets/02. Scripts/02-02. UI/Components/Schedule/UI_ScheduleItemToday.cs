using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 오늘의 일일 스케줄 한 건을 표시하는 항목입니다.
///
/// 홈과 사무실이 같은 클래스를 씁니다(기획서 9.2). 각자 구현하면 완료 표시 규칙이 갈라져
/// 같은 스케줄이 화면마다 다르게 보입니다. 같은 이유로 목적지도 이 클래스가 정하지 않고
/// ScheduleShortcutRouter에 맡깁니다.
///
/// 화면에 그리는 일은 아이콘(UI_ScheduleIconBase)과 버튼(UI_ScheduleShortcutButton)이 나눠 맡고,
/// 이 클래스는 어떤 스케줄을 어떤 상태로 넘길지만 정합니다. 아이콘 규칙이 화면마다 다른데도
/// 여기에 분기가 없는 이유입니다.
///
/// 내일 예고는 UI_ScheduleItemTomorrow가 맡습니다. 스케줄명만 같을 뿐 보여 주는 정보가
/// 달라, 한 클래스에 두 규칙을 담으면 오늘 항목에도 쓰이지 않는 분기가 남습니다.
///
/// 참조는 인스펙터 필수이며 null 검사를 하지 않습니다. 검사로 감싸면 예외 대신 빈 행이 조용히
/// 그려져 배선 누락을 한참 뒤에야 발견하게 됩니다.
/// </summary>
public class UI_ScheduleItemToday : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_ScheduleShortcutButton _shortcutButton;

    [SerializeField]
    private TMP_Text _titleText;

    [SerializeField]
    private UI_ScheduleIconBase _icon;

    private ScheduleData _schedule;
    private EEntryType _entryType = EEntryType.HOME_LIVE;

    private void Awake()
    {
        _shortcutButton.OnClicked = MoveToSchedule;
    }

    /// <summary>
    /// 오늘의 스케줄 하나를 항목에 채웁니다.
    /// entryType은 이 항목이 놓인 화면이 결정합니다. 홈은 HOME_LIVE, 사무실은 SCHEDULE_LIVE입니다.
    /// </summary>
    public void SetSchedule(ScheduleData schedule, bool isCompleted, EEntryType entryType)
    {
        _schedule = schedule;
        _entryType = entryType;

        gameObject.SetActive(true);

        if (ReferenceEquals(schedule, null))
        {
            Clear();
            return;
        }

        _titleText.text = schedule.Title;
        SetCompleted(schedule.ScheduleType, isCompleted);
    }

    /// <summary>
    /// 스케줄이 없는 남는 항목을 숨깁니다.
    /// </summary>
    public void Clear()
    {
        _schedule = null;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 완료 여부에 따라 아이콘과 버튼 표시를 함께 바꿉니다.
    /// 오늘 항목에는 잠김이 없습니다. 세 스케줄 모두 처음부터 진행할 수 있기 때문입니다(기획서 3-E-2-2).
    /// </summary>
    private void SetCompleted(EScheduleType scheduleType, bool isCompleted)
    {
        _icon.Refresh(scheduleType, isCompleted);
        _shortcutButton.SetCompleted(isCompleted);
    }

    private void MoveToSchedule()
    {
        if (ReferenceEquals(_schedule, null))
        {
            return;
        }

        ScheduleShortcutRouter.MoveToSchedule(_schedule, _entryType, this.GetCancellationTokenOnDestroy());
    }
}
