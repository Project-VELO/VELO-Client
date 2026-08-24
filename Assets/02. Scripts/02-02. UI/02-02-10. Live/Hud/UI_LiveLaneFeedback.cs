using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 레인 하단 판정 링의 연출입니다. 링은 항상 보이고, 입력이 들어온 레인만 잠깐 밝아졌다가 기본 밝기로 돌아옵니다.
/// 판정은 키보드 입력으로만 이루어지므로 이 컴포넌트는 표시만 담당하며, 버튼 클릭을 받지 않습니다.
///
/// 남은 시간을 레인별로 세어 Update에서 함께 되돌리기 때문에, 연타해도 대기 작업이 쌓이지 않습니다.
/// 기본 밝기로 돌아온 뒤에는 아무것도 쓰지 않으므로 캔버스가 다시 만들어지지 않습니다.
/// </summary>
public class UI_LiveLaneFeedback : MonoBehaviour
{
    [Header("Feedback")]
    [Tooltip("입력 직후 강조가 기본 밝기로 돌아가기까지의 시간(초)입니다.")]
    [SerializeField]
    private float _litSeconds = 0.08f;

    [Tooltip("입력이 없을 때 링의 불투명도입니다.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _idleAlpha = 0.45f;

    [Tooltip("입력 순간 링의 불투명도입니다.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _litAlpha = 1f;

    [Tooltip("입력 순간 링이 커지는 배율입니다. 1이면 크기 변화 없이 밝기만 바뀝니다.")]
    [Range(1f, 1.5f)]
    [SerializeField]
    private float _litScale = 1.12f;

    [Foldout("Hierarchy")]
    [Tooltip("레인 1번부터 순서대로 넣습니다. 비어 있는 레인은 연출을 건너뜁니다.")]
    [SerializeField]
    private List<Image> _laneRings = new List<Image>();

    private readonly float[] _remainingSeconds = new float[LiveLane.COUNT];

    private void Awake()
    {
        InitRings();
    }

    private void Update()
    {
        for (int i = 0; i < LiveLane.COUNT; i++)
        {
            if (_remainingSeconds[i] <= 0f)
            {
                continue;
            }

            _remainingSeconds[i] -= Time.unscaledDeltaTime;

            // 0 이하로 떨어진 프레임에 기본 밝기가 정확히 한 번 쓰이고, 다음 프레임부터는 위에서 걸러집니다.
            SetRingIntensity(i, Mathf.Max(0f, _remainingSeconds[i]) / _litSeconds);
        }
    }

    public void RefreshLanePress(int lane)
    {
        int index = lane - LiveLane.FIRST;

        if (index < 0 || LiveLane.COUNT <= index)
        {
            return;
        }

        _remainingSeconds[index] = _litSeconds;
        SetRingIntensity(index, 1f);
    }

    public void ClearHighlights()
    {
        InitRings();
    }

    private void InitRings()
    {
        for (int i = 0; i < LiveLane.COUNT; i++)
        {
            _remainingSeconds[i] = 0f;
            SetRingIntensity(i, 0f);
        }
    }

    /// <summary>
    /// 강조 정도를 0(기본)에서 1(입력 순간) 사이로 반영합니다.
    /// Color와 Vector3는 값 타입이라 매 프레임 호출해도 가비지가 생기지 않습니다.
    /// </summary>
    private void SetRingIntensity(int index, float litRatio)
    {
        if (_laneRings.Count <= index || _laneRings[index] == null)
        {
            return;
        }

        Image ring = _laneRings[index];

        Color color = ring.color;
        color.a = Mathf.Lerp(_idleAlpha, _litAlpha, litRatio);
        ring.color = color;

        float scale = Mathf.Lerp(1f, _litScale, litRatio);
        ring.rectTransform.localScale = new Vector3(scale, scale, 1f);
    }
}
