using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 일일 스케줄 한 건을 표시하는 행입니다.
///
/// 홈 화면과 사무실 화면이 같은 클래스를 씁니다. 기획서 9.2(기획서.txt:1998)가 사무실의 오늘의 스케줄 표를
/// "홈 화면과 동일"이라고 못박고 있고, 두 화면이 각자 구현하면 완료 표시 규칙(기획서 3-E-6-1)이 갈라져
/// 같은 스케줄이 화면마다 다르게 보입니다. 기획서 16-6이 금지하는 상황입니다.
///
/// 목적지는 이 클래스가 정하지 않고 ScheduleShortcutRouter에 맡깁니다.
/// 행이 목적지를 알면 홈과 사무실에서 이동 규칙이 어긋날 수 있습니다.
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

    /// <summary>
    /// 스케줄 유형입니다. 기획서 4.2가 제목·유형·완료 상태·바로가기 네 가지 표시를 요구합니다.
    /// </summary>
    [SerializeField]
    private TMP_Text _typeText;

    [SerializeField]
    private TMP_Text _shortcutLabel;

    /// <summary>
    /// 완료 체크 표시입니다. 완료된 행에서만 켭니다(기획서.txt:838).
    /// </summary>
    [SerializeField]
    private GameObject _completedRoot;

    private ScheduleData _schedule;
    private EEntryType _entryType = EEntryType.HOME_LIVE;

    private void Awake()
    {
        if (_shortcutButton != null)
        {
            _shortcutButton.onClick.AddListener(MoveToSchedule);
        }
    }

    /// <summary>
    /// 스케줄 하나를 행에 채웁니다.
    /// entryType은 이 행이 놓인 화면이 결정합니다. 홈은 HOME_LIVE, 사무실은 SCHEDULE_LIVE입니다
    /// (기획서.txt:440, 447-448).
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

        if (_typeText != null)
        {
            _typeText.text = GetTypeLabel(schedule.ScheduleType);
        }

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
    /// 완료 여부에 따라 같은 자리의 표시를 바꿉니다(기획서.txt:823-843).
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
