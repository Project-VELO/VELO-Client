using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 하이스피드를 미세(±0.1)/거친(±1.0) 단위 버튼으로 조절하고 현재 값을 가운데에 표시하는 UGUI 컨트롤입니다.
/// 하이스피드는 채보 데이터가 아니라 작업자의 화면 설정이므로 PlayerPrefs에 보관합니다.
/// </summary>
public class UI_LiveEditorHiSpeedControl : MonoBehaviour
{
    private const string HI_SPEED_PREFS_KEY = "LiveEditor.HiSpeed";
    private const float DEFAULT_HI_SPEED = 1.0f;

    [Header("Step")]
    [SerializeField]
    private float _fineStep = 0.1f;

    [SerializeField]
    private float _coarseStep = 1.0f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorTimeline _timeline;

    [SerializeField]
    private TMP_Text _valueText;

    [SerializeField]
    private Button _coarseDownButton;

    [SerializeField]
    private Button _fineDownButton;

    [SerializeField]
    private Button _fineUpButton;

    [SerializeField]
    private Button _coarseUpButton;

    private void Awake()
    {
        _coarseDownButton.onClick.AddListener(DecreaseCoarse);
        _fineDownButton.onClick.AddListener(DecreaseFine);
        _fineUpButton.onClick.AddListener(IncreaseFine);
        _coarseUpButton.onClick.AddListener(IncreaseCoarse);
    }

    private void Start()
    {
        SetHiSpeed(PlayerPrefs.GetFloat(HI_SPEED_PREFS_KEY, DEFAULT_HI_SPEED));
    }

    private void DecreaseCoarse()
    {
        SetHiSpeed(_timeline.HiSpeed - _coarseStep);
    }

    private void DecreaseFine()
    {
        SetHiSpeed(_timeline.HiSpeed - _fineStep);
    }

    private void IncreaseFine()
    {
        SetHiSpeed(_timeline.HiSpeed + _fineStep);
    }

    private void IncreaseCoarse()
    {
        SetHiSpeed(_timeline.HiSpeed + _coarseStep);
    }

    /// <summary>
    /// 0.1씩 더하다 보면 부동소수점 오차가 쌓여 4.9999 같은 값이 되므로 소수점 둘째 자리로 반올림해 고정합니다.
    /// 상·하한 제한 자체는 LiveScrollMapper가 담당합니다.
    /// </summary>
    private void SetHiSpeed(float hiSpeed)
    {
        _timeline.HiSpeed = Mathf.Round(hiSpeed * 100f) / 100f;

        PlayerPrefs.SetFloat(HI_SPEED_PREFS_KEY, _timeline.HiSpeed);
        RefreshHiSpeed();
    }

    private void RefreshHiSpeed()
    {
        _valueText.text = $"{_timeline.HiSpeed:0.00}x";

        bool isAtMin = _timeline.HiSpeed <= LiveScrollMapper.MIN_HI_SPEED;
        bool isAtMax = _timeline.HiSpeed >= LiveScrollMapper.MAX_HI_SPEED;

        _coarseDownButton.interactable = !isAtMin;
        _fineDownButton.interactable = !isAtMin;
        _fineUpButton.interactable = !isAtMax;
        _coarseUpButton.interactable = !isAtMax;
    }
}
