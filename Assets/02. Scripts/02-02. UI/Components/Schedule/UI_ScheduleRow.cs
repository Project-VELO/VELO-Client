using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 일일 스케줄 한 건을 표시하는 행입니다.
///
/// 홈과 사무실이 같은 클래스를 씁니다(기획서 9.2). 각자 구현하면 완료 표시 규칙이 갈라져
/// 같은 스케줄이 화면마다 다르게 보입니다. 같은 이유로 목적지도 이 클래스가 정하지 않고
/// ScheduleShortcutRouter에 맡깁니다.
///
/// _completedRoot를 뺀 참조는 인스펙터 필수이며 null 검사를 하지 않습니다. 검사로 감싸면 예외 대신
/// 빈 행이 조용히 그려져 배선 누락을 한참 뒤에야 발견하게 됩니다.
/// </summary>
public class UI_ScheduleRow : MonoBehaviour
{
    private const string SHORTCUT_LABEL = "바로가기";
    private const string COMPLETED_LABEL = "완료";

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _shortcutButton;

    [SerializeField]
    private TMP_Text _titleText;

    [SerializeField]
    private TMP_Text _typeText;

    [SerializeField]
    private TMP_Text _shortcutLabel;

    /// <summary>
    /// 완료 체크 표시입니다. 위 필드들과 달리 이것만 비어 있어도 됩니다.
    /// 프리팹에 체크용 오브젝트가 없고, 완료 표시는 라벨 교체와 클릭 차단으로 이미 충족되기 때문입니다.
    /// </summary>
    [SerializeField]
    private GameObject _completedRoot;

    private ScheduleData _schedule;
    private EEntryType _entryType = EEntryType.HOME_LIVE;

    private void Awake()
    {
        _shortcutButton.onClick.AddListener(MoveToSchedule);
    }

    /// <summary>
    /// 스케줄 하나를 행에 채웁니다.
    /// entryType은 이 행이 놓인 화면이 결정합니다. 홈은 HOME_LIVE, 사무실은 SCHEDULE_LIVE입니다.
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
        _typeText.text = GetTypeLabel(schedule.ScheduleType);

        SetCompleted(isCompleted);
    }

    /// <summary>
    /// 유형의 화면 표기입니다. 기획서가 "유형 표시"만 요구하고 문구를 정하지 않아 임시로 둡니다.
    /// 확정되면 이 메서드만 고치면 됩니다.
    /// </summary>
    private string GetTypeLabel(EScheduleType scheduleType)
    {
        switch (scheduleType)
        {
            case EScheduleType.LIVE: return "LIVE";
            case EScheduleType.STORY: return "스토리 감상";
            default: return string.Empty;
        }
    }

    /// <summary>
    /// 스케줄이 없는 남는 행을 숨깁니다.
    /// </summary>
    public void Clear()
    {
        _schedule = null;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 완료 여부에 따라 같은 자리의 표시를 바꿉니다.
    /// 완료된 행은 버튼을 없애지 않고 라벨만 "완료"로 바꿉니다. 행 높이가 변하면 표가 흔들립니다.
    ///
    /// interactable을 끄는 것으로 마우스오버 강조와 중복 완료 진입이 함께 막힙니다.
    /// </summary>
    private void SetCompleted(bool isCompleted)
    {
        _shortcutButton.interactable = !isCompleted;
        _shortcutLabel.text = isCompleted ? COMPLETED_LABEL : SHORTCUT_LABEL;

        if (_completedRoot != null)
        {
            _completedRoot.SetActive(isCompleted);
        }
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
