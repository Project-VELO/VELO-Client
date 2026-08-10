using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 오늘의 일일 스케줄 한 건을 표시하는 항목입니다.
///
/// 홈과 사무실이 같은 클래스를 씁니다(기획서 9.2). 각자 구현하면 완료 표시 규칙이 갈라져
/// 같은 스케줄이 화면마다 다르게 보입니다. 같은 이유로 목적지도 이 클래스가 정하지 않고
/// ScheduleShortcutRouter에 맡깁니다.
///
/// 내일 예고는 UI_ScheduleItemTomorrow가 맡습니다. 스케줄명(한글·영문)만 같을 뿐 보여 주는 정보가
/// 달라, 한 클래스에 두 규칙을 담으면 오늘 항목에도 쓰이지 않는 분기가 남습니다.
///
/// 참조는 인스펙터 필수이며 null 검사를 하지 않습니다. 검사로 감싸면 예외 대신 빈 행이 조용히
/// 그려져 배선 누락을 한참 뒤에야 발견하게 됩니다.
/// </summary>
public class UI_ScheduleItemToday : MonoBehaviour
{
    private const string SHORTCUT_LABEL = "바로가기";
    private const string COMPLETED_LABEL = "완료";

    private const string MONEY_FORMAT = "N0";
    private const string EXP_FORMAT = "EXP {0}";

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _shortcutButton;

    [SerializeField]
    private TMP_Text _titleText;

    /// <summary>
    /// 제목 아래 영문 표기 자리입니다. ScheduleData에 영문 필드가 아직 없어 비워 두지만,
    /// 행 높이를 잡아 두어야 데이터가 들어와도 배치가 흔들리지 않습니다.
    /// </summary>
    [SerializeField]
    private TMP_Text _enNameText;

    /// <summary>
    /// 이전에 스케줄 유형을 표시하던 자리입니다. 디자인이 이 칸을 보상 금액으로 바꾸어 이름만 옮겼습니다.
    /// FormerlySerializedAs가 없으면 홈·사무실 프리팹의 기존 배선이 전부 끊깁니다.
    /// </summary>
    [FormerlySerializedAs("_typeText")]
    [SerializeField]
    private TMP_Text _rewardMoneyText;

    [SerializeField]
    private TMP_Text _rewardExpText;

    [SerializeField]
    private TMP_Text _shortcutLabel;

    [SerializeField]
    private UI_ScheduleStateIcon _stateIcon;

    private ScheduleData _schedule;
    private EEntryType _entryType = EEntryType.HOME_LIVE;

    private void Awake()
    {
        _shortcutButton.onClick.AddListener(MoveToSchedule);
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

        RefreshTitle(schedule);
        RefreshReward(schedule.RewardId);
        SetCompleted(isCompleted);
    }

    /// <summary>
    /// 스케줄이 없는 남는 항목을 숨깁니다.
    /// </summary>
    public void Clear()
    {
        _schedule = null;
        gameObject.SetActive(false);
    }

    private void RefreshTitle(ScheduleData schedule)
    {
        _titleText.text = schedule.Title;
        _enNameText.text = string.Empty;
    }

    /// <summary>
    /// 완료 시 받을 보상입니다. 지급이 아니라 표시이므로 지급 로직(GameProgressService)이 아니라 마스터 데이터를 직접 읽습니다.
    /// 보상 데이터가 없으면 숫자 대신 빈 칸을 두어, 0을 실제 보상으로 오해하지 않게 합니다.
    /// </summary>
    private void RefreshReward(string rewardId)
    {
        if (!MasterDataProvider.Instance.TryGetReward(rewardId, out RewardData reward))
        {
            _rewardMoneyText.text = string.Empty;
            _rewardExpText.text = string.Empty;
            return;
        }

        _rewardMoneyText.text = reward.Money.ToString(MONEY_FORMAT);
        _rewardExpText.text = string.Format(EXP_FORMAT, reward.Exp);
    }

    /// <summary>
    /// 완료 여부에 따라 상태 아이콘과 버튼 표시를 함께 바꿉니다.
    /// 완료된 항목은 버튼을 없애지 않고 라벨만 "완료"로 바꿉니다. 행 높이가 변하면 표가 흔들립니다.
    ///
    /// interactable을 끄는 것으로 마우스오버 강조와 중복 완료 진입이 함께 막힙니다.
    /// 오늘 항목에는 잠김이 없습니다. 세 스케줄 모두 처음부터 진행할 수 있기 때문입니다(기획서 3-E-2-2).
    /// </summary>
    private void SetCompleted(bool isCompleted)
    {
        _stateIcon.SetState(isCompleted ? EScheduleViewState.COMPLETED : EScheduleViewState.AVAILABLE);

        _shortcutButton.interactable = !isCompleted;
        _shortcutLabel.text = isCompleted ? COMPLETED_LABEL : SHORTCUT_LABEL;
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
