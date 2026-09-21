using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 일시정지 팝업 설정 탭의 판정 타이밍 조절 UI입니다.
/// 슬라이더로 큰 폭을 옮기고, -10 / -1 / +1 / +10 버튼으로 지금 값에서 다듬으며, 0 버튼으로 보정을 없앱니다.
///
/// 값의 보관과 상·하한은 LiveJudgementOffsetSetting이, 판정 반영은 LiveConductor.JudgementTimeMs가 맡습니다.
/// </summary>
public class UI_LiveJudgementOffsetControl : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("Value")]
    [SerializeField]
    private TMP_Text _valueText;

    [SerializeField]
    private Slider _slider;

    [Header("Range Labels")]
    [SerializeField]
    private TMP_Text _minLabel;

    [SerializeField]
    private TMP_Text _maxLabel;

    [Header("Buttons")]
    [SerializeField]
    private Button _coarseDownButton;

    [SerializeField]
    private Button _fineDownButton;

    [SerializeField]
    private Button _resetButton;

    [SerializeField]
    private Button _fineUpButton;

    [SerializeField]
    private Button _coarseUpButton;

    private void Awake()
    {
        InitRange();

        _slider.onValueChanged.AddListener(SetOffsetFromSlider);
        _coarseDownButton.onClick.AddListener(DecreaseCoarse);
        _fineDownButton.onClick.AddListener(DecreaseFine);
        _resetButton.onClick.AddListener(ResetOffset);
        _fineUpButton.onClick.AddListener(IncreaseFine);
        _coarseUpButton.onClick.AddListener(IncreaseCoarse);
    }

    // 팝업은 닫아도 오브젝트가 남습니다. 설정 탭을 열 때마다 지금 값을 다시 비춰야 합니다.
    private void OnEnable()
    {
        RefreshOffset();
    }

    private void OnDestroy()
    {
        _slider.onValueChanged.RemoveListener(SetOffsetFromSlider);
        _coarseDownButton.onClick.RemoveListener(DecreaseCoarse);
        _fineDownButton.onClick.RemoveListener(DecreaseFine);
        _resetButton.onClick.RemoveListener(ResetOffset);
        _fineUpButton.onClick.RemoveListener(IncreaseFine);
        _coarseUpButton.onClick.RemoveListener(IncreaseCoarse);
    }

    /// <summary>
    /// 슬라이더 범위와 양끝 표기를 상·하한 상수에서 가져옵니다. 인스펙터에 따로 적어 두면
    /// 범위를 바꿀 때 한쪽만 고쳐져, 표기는 ±100인데 실제로는 다른 값에서 멈추는 일이 생깁니다.
    /// </summary>
    private void InitRange()
    {
        _slider.minValue = LiveJudgementOffsetSetting.MIN_OFFSET_MS;
        _slider.maxValue = LiveJudgementOffsetSetting.MAX_OFFSET_MS;
        _slider.wholeNumbers = true;

        _minLabel.text = FormatOffset(LiveJudgementOffsetSetting.MIN_OFFSET_MS);
        _maxLabel.text = FormatOffset(LiveJudgementOffsetSetting.MAX_OFFSET_MS);
    }

    private void SetOffsetFromSlider(float value)
    {
        SetOffset(Mathf.RoundToInt(value));
    }

    private void DecreaseCoarse()
    {
        SetOffset(LiveJudgementOffsetSetting.CurrentMs - LiveJudgementOffsetSetting.COARSE_STEP_MS);
    }

    private void DecreaseFine()
    {
        SetOffset(LiveJudgementOffsetSetting.CurrentMs - LiveJudgementOffsetSetting.FINE_STEP_MS);
    }

    private void ResetOffset()
    {
        SetOffset(LiveJudgementOffsetSetting.DEFAULT_OFFSET_MS);
    }

    private void IncreaseFine()
    {
        SetOffset(LiveJudgementOffsetSetting.CurrentMs + LiveJudgementOffsetSetting.FINE_STEP_MS);
    }

    private void IncreaseCoarse()
    {
        SetOffset(LiveJudgementOffsetSetting.CurrentMs + LiveJudgementOffsetSetting.COARSE_STEP_MS);
    }

    private void SetOffset(int offsetMs)
    {
        LiveJudgementOffsetSetting.Set(offsetMs);

        RefreshOffset();
    }

    /// <summary>
    /// 버튼으로 바꾼 값을 슬라이더에 되비출 때는 알림 없이 옮깁니다.
    /// 알림을 켜 두면 onValueChanged가 다시 돌아 같은 값을 한 번 더 저장합니다.
    /// </summary>
    private void RefreshOffset()
    {
        int offsetMs = LiveJudgementOffsetSetting.CurrentMs;

        _valueText.text = FormatOffset(offsetMs);
        _slider.SetValueWithoutNotify(offsetMs);

        bool isAtMin = offsetMs <= LiveJudgementOffsetSetting.MIN_OFFSET_MS;
        bool isAtMax = LiveJudgementOffsetSetting.MAX_OFFSET_MS <= offsetMs;

        _coarseDownButton.interactable = !isAtMin;
        _fineDownButton.interactable = !isAtMin;
        _fineUpButton.interactable = !isAtMax;
        _coarseUpButton.interactable = !isAtMax;
    }

    /// <summary>
    /// 부호를 항상 붙여 어느 쪽으로 밀었는지 한눈에 보이게 합니다.
    /// 양수(빠르게)는 입력을 늦게 들어온 것으로 쳐 주므로 일찍 치는 버릇을, 음수(느리게)는 그 반대를 보정합니다.
    /// </summary>
    private static string FormatOffset(int offsetMs)
    {
        return $"{offsetMs:+0;-0;0}ms";
    }
}
