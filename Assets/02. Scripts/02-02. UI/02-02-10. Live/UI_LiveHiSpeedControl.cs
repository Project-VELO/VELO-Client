using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 일시정지 팝업 설정 탭에서 하이스피드를 미세(±0.1)/거친(±1.0) 단위 버튼으로 조절하고
/// 현재 값을 가운데에 표시하는 UGUI 컨트롤입니다.
///
/// 값의 보관과 상·하한은 LiveHiSpeedSetting이, 트랙 반영은 LiveHiSpeedController가 맡습니다.
/// 이 클래스가 트랙을 직접 참조하지 않으므로 팝업이 게임플레이 객체와 얽히지 않습니다.
/// </summary>
public class UI_LiveHiSpeedControl : MonoBehaviour
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
        RefreshHiSpeed();
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
        SetHiSpeed(LiveHiSpeedSetting.Current - LiveHiSpeedSetting.COARSE_STEP);
    }

    private void DecreaseFine()
    {
        SetHiSpeed(LiveHiSpeedSetting.Current - LiveHiSpeedSetting.FINE_STEP);
    }

    private void IncreaseFine()
    {
        SetHiSpeed(LiveHiSpeedSetting.Current + LiveHiSpeedSetting.FINE_STEP);
    }

    private void IncreaseCoarse()
    {
        SetHiSpeed(LiveHiSpeedSetting.Current + LiveHiSpeedSetting.COARSE_STEP);
    }

    /// <summary>
    /// 거친 ±1.0도 0.1의 배수에 떨어지므로 어느 버튼을 눌러도 값은 0.1 단위를 벗어나지 않습니다.
    /// 반올림과 상·하한 제한은 LiveHiSpeedSetting이 담당합니다.
    /// </summary>
    private void SetHiSpeed(float hiSpeed)
    {
        LiveHiSpeedSetting.Set(hiSpeed);

        RefreshHiSpeed();
    }

    private void RefreshHiSpeed()
    {
        float hiSpeed = LiveHiSpeedSetting.Current;

        _valueText.text = $"{hiSpeed:0.0}x";

        bool isAtMin = hiSpeed <= LiveHiSpeedSetting.MIN_HI_SPEED;
        bool isAtMax = LiveHiSpeedSetting.MAX_HI_SPEED <= hiSpeed;

        _coarseDownButton.interactable = !isAtMin;
        _fineDownButton.interactable = !isAtMin;
        _fineUpButton.interactable = !isAtMax;
        _coarseUpButton.interactable = !isAtMax;
    }
}
