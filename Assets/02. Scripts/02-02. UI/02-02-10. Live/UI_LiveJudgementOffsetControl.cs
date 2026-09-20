using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 일시정지 팝업 설정 탭에서 판정 보정을 1ms/10ms 단위 버튼으로 조절하고 현재 값을 가운데에 표시하는 UGUI 컨트롤입니다.
///
/// 값의 보관과 상·하한은 LiveJudgementOffsetSetting이, 판정 반영은 LiveConductor.JudgementTimeMs가 맡습니다.
/// UI_LiveHiSpeedControl과 구조가 같지만 다루는 값과 표기가 달라 따로 둡니다.
/// </summary>
public class UI_LiveJudgementOffsetControl : MonoBehaviour
{
    [Foldout("Hierarchy")]
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

    // 팝업은 닫아도 오브젝트가 남습니다. 설정 탭을 열 때마다 지금 값을 다시 비춰야 합니다.
    private void OnEnable()
    {
        RefreshOffset();
    }

    private void OnDestroy()
    {
        _coarseDownButton.onClick.RemoveListener(DecreaseCoarse);
        _fineDownButton.onClick.RemoveListener(DecreaseFine);
        _fineUpButton.onClick.RemoveListener(IncreaseFine);
        _coarseUpButton.onClick.RemoveListener(IncreaseCoarse);
    }

    private void DecreaseCoarse()
    {
        SetOffset(LiveJudgementOffsetSetting.CurrentMs - LiveJudgementOffsetSetting.COARSE_STEP_MS);
    }

    private void DecreaseFine()
    {
        SetOffset(LiveJudgementOffsetSetting.CurrentMs - LiveJudgementOffsetSetting.FINE_STEP_MS);
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
    /// 부호를 항상 붙여 어느 쪽으로 밀었는지 한눈에 보이게 합니다.
    /// 양수는 입력을 늦게 들어온 것으로 쳐 주므로 일찍 치는 버릇을, 음수는 그 반대를 보정합니다.
    /// </summary>
    private void RefreshOffset()
    {
        int offsetMs = LiveJudgementOffsetSetting.CurrentMs;

        _valueText.text = $"{offsetMs:+0;-0;0}ms";

        bool isAtMin = offsetMs <= LiveJudgementOffsetSetting.MIN_OFFSET_MS;
        bool isAtMax = LiveJudgementOffsetSetting.MAX_OFFSET_MS <= offsetMs;

        _coarseDownButton.interactable = !isAtMin;
        _fineDownButton.interactable = !isAtMin;
        _fineUpButton.interactable = !isAtMax;
        _coarseUpButton.interactable = !isAtMax;
    }
}
