using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 일일 스케줄 한 건을 표시하는 행입니다.
///
/// 홈과 사무실이 같은 클래스를 씁니다(기획서 9.2). 각자 구현하면 완료 표시 규칙이 갈라져
/// 같은 스케줄이 화면마다 다르게 보입니다. 같은 이유로 목적지도 이 클래스가 정하지 않고
/// ScheduleShortcutRouter에 맡깁니다.
///
/// 두 가지 모드가 있습니다. SetSchedule은 오늘의 스케줄을, SetPreview는 홈 화면의 내일 예고를 그립니다.
/// 내일 스케줄은 아직 진행할 수 없으므로 완료 여부라는 개념이 없고 바로가기도 열리지 않습니다.
///
/// _completedRoot와 _lockIcon을 뺀 참조는 인스펙터 필수이며 null 검사를 하지 않습니다. 검사로 감싸면
/// 예외 대신 빈 행이 조용히 그려져 배선 누락을 한참 뒤에야 발견하게 됩니다.
/// </summary>
public class UI_ScheduleRow : MonoBehaviour
{
    private const string SHORTCUT_LABEL = "바로가기";
    private const string COMPLETED_LABEL = "완료";

    /// <summary>
    /// 내일 예고 행의 버튼 자리 표시입니다. 하루는 반드시 하루 마무리를 거쳐야 넘어가므로
    /// 홈 화면에서 미리 보이는 다음 날짜는 항상 하루 뒤입니다.
    /// </summary>
    private const string PREVIEW_LABEL = "D-1";

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

    /// <summary>
    /// 잠금 표시입니다. 내일 예고 행에서만 켭니다. 오늘의 세 스케줄은 순서 없이 모두
    /// 처음부터 진행할 수 있으므로(기획서 3-E-2-2) 오늘 행에는 잠금이 없습니다.
    /// </summary>
    [SerializeField]
    private GameObject _lockIcon;

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
    /// 오늘의 스케줄 하나를 행에 채웁니다.
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

        RefreshCommon(schedule);
        SetLocked(false);
        SetCompleted(isCompleted);
    }

    /// <summary>
    /// 내일 예정된 스케줄을 예고로 채웁니다. 아직 진행할 수 없으므로 바로가기를 열지 않습니다.
    /// _schedule을 비워 두어 버튼이 눌리더라도 이동이 일어나지 않게 막습니다.
    /// </summary>
    public void SetPreview(ScheduleData schedule)
    {
        _schedule = null;

        gameObject.SetActive(true);

        if (ReferenceEquals(schedule, null))
        {
            Clear();
            return;
        }

        RefreshCommon(schedule);
        SetLocked(true);

        _shortcutButton.interactable = false;
        _shortcutLabel.text = PREVIEW_LABEL;

        if (_completedRoot != null)
        {
            _completedRoot.SetActive(false);
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
    /// 두 모드가 공통으로 그리는 제목과 보상입니다.
    /// </summary>
    private void RefreshCommon(ScheduleData schedule)
    {
        _titleText.text = schedule.Title;
        _enNameText.text = string.Empty;

        RefreshReward(schedule.RewardId);
    }

    /// <summary>
    /// 완료 시 받을 보상입니다. 지급이 아니라 표시이므로 RewardService가 아니라 마스터 데이터를 직접 읽습니다.
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

    private void SetLocked(bool isLocked)
    {
        if (_lockIcon != null)
        {
            _lockIcon.SetActive(isLocked);
        }
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
