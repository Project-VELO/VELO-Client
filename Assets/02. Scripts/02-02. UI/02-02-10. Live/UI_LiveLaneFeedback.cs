using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 하단 리듬 버튼이 눌린 것처럼 잠깐 켜지는 연출입니다.
/// 판정은 키보드 입력으로만 이루어지므로 이 컴포넌트는 표시만 담당하며, 버튼 클릭을 받지 않습니다.
///
/// 켜진 시간을 레인별로 세어 Update에서 함께 끄기 때문에, 연타해도 대기 작업이 쌓이지 않습니다.
/// </summary>
public class UI_LiveLaneFeedback : MonoBehaviour
{
    [Header("Feedback")]
    [Tooltip("버튼이 켜져 있는 시간(초)입니다.")]
    [SerializeField]
    private float _litSeconds = 0.08f;

    [Foldout("Hierarchy")]
    [Tooltip("레인 1번부터 순서대로 넣습니다. 비어 있는 레인은 연출을 건너뜁니다.")]
    [SerializeField]
    private List<GameObject> _laneHighlights = new List<GameObject>();

    private readonly float[] _remainingSeconds = new float[LiveLane.COUNT];

    private void Update()
    {
        for (int i = 0; i < LiveLane.COUNT; i++)
        {
            if (_remainingSeconds[i] <= 0f)
            {
                continue;
            }

            _remainingSeconds[i] -= Time.unscaledDeltaTime;

            if (_remainingSeconds[i] <= 0f)
            {
                SetHighlight(i, false);
            }
        }
    }

    public void RefreshLanePress(int lane)
    {
        int index = lane - LiveLane.FIRST;

        if (index < 0 || index >= LiveLane.COUNT)
        {
            return;
        }

        _remainingSeconds[index] = _litSeconds;
        SetHighlight(index, true);
    }

    public void ClearHighlights()
    {
        for (int i = 0; i < LiveLane.COUNT; i++)
        {
            _remainingSeconds[i] = 0f;
            SetHighlight(i, false);
        }
    }

    private void SetHighlight(int index, bool isLit)
    {
        if (index >= _laneHighlights.Count || _laneHighlights[index] == null)
        {
            return;
        }

        _laneHighlights[index].SetActive(isLit);
    }
}
