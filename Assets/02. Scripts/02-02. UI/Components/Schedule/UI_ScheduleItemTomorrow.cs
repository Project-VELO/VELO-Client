using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 홈 화면의 내일 스케줄 예고 한 건을 표시하는 항목입니다.
///
/// UI_ScheduleItemToday와 겹치는 것은 스케줄명(한글·영문)뿐입니다. 내일 스케줄은 아직 진행할 수
/// 없어 완료 여부도, 보상 표시도, 바로가기도 없습니다. 대신 잠금 아이콘과 공개까지 남은 날짜만
/// 보여 줍니다. 두 항목을 한 클래스로 묶으면 오늘 항목에 쓰이지 않는 잠김 분기가, 내일 항목에
/// 동작하지 않는 보상·이동 코드가 각각 남습니다.
///
/// 참조는 인스펙터 필수이며 null 검사를 하지 않습니다. 검사로 감싸면 예외 대신 빈 행이 조용히
/// 그려져 배선 누락을 한참 뒤에야 발견하게 됩니다.
/// </summary>
public class UI_ScheduleItemTomorrow : MonoBehaviour
{
    /// <summary>
    /// 공개까지 남은 날짜입니다. 하루는 반드시 하루 마무리를 거쳐야 넘어가므로 홈 화면에서
    /// 미리 보이는 다음 날짜는 항상 하루 뒤이고, 따라서 이 값은 상수입니다.
    /// </summary>
    private const string D_DAY_LABEL = "D-1";

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _titleText;

    /// <summary>
    /// 제목 아래 영문 표기 자리입니다. ScheduleData에 영문 필드가 아직 없어 비워 두지만,
    /// 행 높이를 잡아 두어야 데이터가 들어와도 배치가 흔들리지 않습니다.
    /// </summary>
    [SerializeField]
    private TMP_Text _enNameText;

    [SerializeField]
    private TMP_Text _dDayText;

    [SerializeField]
    private UI_ScheduleStateIcon _stateIcon;

    /// <summary>
    /// 내일 예정된 스케줄 하나를 예고로 채웁니다.
    /// </summary>
    public void SetSchedule(ScheduleData schedule)
    {
        gameObject.SetActive(true);

        if (ReferenceEquals(schedule, null))
        {
            Clear();
            return;
        }

        RefreshTitle(schedule);

        _stateIcon.SetState(EScheduleViewState.LOCKED);
        _dDayText.text = D_DAY_LABEL;
    }

    /// <summary>
    /// 스케줄이 없는 남는 항목을 숨깁니다.
    /// </summary>
    public void Clear()
    {
        gameObject.SetActive(false);
    }

    private void RefreshTitle(ScheduleData schedule)
    {
        _titleText.text = schedule.Title;
        _enNameText.text = string.Empty;
    }
}
