using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 음악 재생 배속을 미세(±0.1)/거친(±0.5) 단위 버튼으로 조절하고 현재 값을 가운데에 표시하는 UGUI 컨트롤입니다.
/// 배속은 작업 중 잠깐 늦춰 듣는 용도라 씬에 들어올 때마다 기본 1.0배로 되돌립니다.
/// </summary>
public class UI_LiveEditorPlaybackSpeedControl : MonoBehaviour
{
    public const float MIN_SPEED = 0.5f;
    public const float MAX_SPEED = 2.0f;
    private const float DEFAULT_SPEED = 1.0f;

    [Header("Step")]
    [SerializeField]
    private float _fineStep = 0.1f;

    [SerializeField]
    private float _coarseStep = 0.5f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveAudioPlayer _audioPlayer;

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

    private float _speed = DEFAULT_SPEED;
    private bool _isInteractable = true;

    private void Awake()
    {
        _coarseDownButton.onClick.AddListener(DecreaseCoarse);
        _fineDownButton.onClick.AddListener(DecreaseFine);
        _fineUpButton.onClick.AddListener(IncreaseFine);
        _coarseUpButton.onClick.AddListener(IncreaseCoarse);
    }

    private void Start()
    {
        SetSpeed(DEFAULT_SPEED);
    }

    /// <summary>
    /// 배속 조절 가능 여부를 바꿉니다. 테스트 플레이는 배속 1.0을 전제로 판정하므로 그동안 잠급니다.
    /// </summary>
    public void SetInteractable(bool isInteractable)
    {
        _isInteractable = isInteractable;
        RefreshSpeed();
    }

    public void ResetSpeed()
    {
        SetSpeed(DEFAULT_SPEED);
    }

    private void DecreaseCoarse()
    {
        SetSpeed(_speed - _coarseStep);
    }

    private void DecreaseFine()
    {
        SetSpeed(_speed - _fineStep);
    }

    private void IncreaseFine()
    {
        SetSpeed(_speed + _fineStep);
    }

    private void IncreaseCoarse()
    {
        SetSpeed(_speed + _coarseStep);
    }

    /// <summary>
    /// 0.1씩 더하다 보면 부동소수점 오차가 쌓여 1.2999 같은 값이 되므로 소수점 둘째 자리로 반올림해 고정합니다.
    /// </summary>
    private void SetSpeed(float speed)
    {
        _speed = Mathf.Clamp(Mathf.Round(speed * 100f) / 100f, MIN_SPEED, MAX_SPEED);

        _audioPlayer.SetSpeed(_speed);
        RefreshSpeed();
    }

    private void RefreshSpeed()
    {
        _valueText.text = $"{_speed:0.00}x";

        bool isAtMin = _speed <= MIN_SPEED;
        bool isAtMax = _speed >= MAX_SPEED;

        _coarseDownButton.interactable = _isInteractable && !isAtMin;
        _fineDownButton.interactable = _isInteractable && !isAtMin;
        _fineUpButton.interactable = _isInteractable && !isAtMax;
        _coarseUpButton.interactable = _isInteractable && !isAtMax;
    }
}
