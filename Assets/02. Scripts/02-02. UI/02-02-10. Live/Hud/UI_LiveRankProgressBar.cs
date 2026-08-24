using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 플레이 도중의 랭크 진행 상태를 보여 주는 게이지입니다.
/// C·B·A·S·PS 다섯 구간의 점 중 지금 도달한 랭크의 점만 밝히고, 막대는 그 점 위치까지 채웁니다.
///
/// 시안의 점은 등간격인데 랭크 경계 정확도(70/85/95/97%)는 등간격이 아닙니다.
/// 정확도를 그대로 채우면 C랭크인데 막대가 S 점을 넘어서는 일이 생기므로, 구간별로 선형 보간해 점 위치로 옮깁니다.
/// </summary>
public class UI_LiveRankProgressBar : MonoBehaviour
{
    private const float MAX_ACCURACY = 100f;

    /// <summary>
    /// 정확도를 막대 비율로 옮기는 꺾은점입니다. 앞의 값들은 랭크 경계 정확도이고, 마지막은 만점입니다.
    /// </summary>
    private static readonly List<float> BREAKPOINT_ACCURACIES = new List<float>
    {
        0f,
        LiveRankEvaluator.CLEAR_ACCURACY,
        LiveRankEvaluator.B_ACCURACY,
        LiveRankEvaluator.A_ACCURACY,
        LiveRankEvaluator.S_ACCURACY,
        MAX_ACCURACY
    };

    /// <summary>
    /// 위 꺾은점에 대응하는 막대 비율입니다. 가운데 네 값은 시안에서 C·B·A·S 점이 놓인 자리이고,
    /// 마지막은 막대 오른쪽 끝입니다. 프리팹의 점 배치를 바꾸면 이 값도 함께 맞춰야 합니다.
    /// </summary>
    private static readonly List<float> BREAKPOINT_BAR_RATIOS = new List<float>
    {
        0f,
        0.181f,
        0.340f,
        0.498f,
        0.664f,
        1f
    };

    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _progressBarFill;

    [Tooltip("C, B, A, S, PERFECT_S 순서로 넣습니다.")]
    [SerializeField]
    private List<Image> _rankDots = new List<Image>();

    [Foldout("Project")]
    [Header("Dot Sprites")]
    [SerializeField]
    private Sprite _reachedDotSprite;

    [SerializeField]
    private Sprite _unreachedDotSprite;

    public void RefreshProgress(float accuracy, ELiveRank rank)
    {
        _progressBarFill.fillAmount = ToBarRatio(accuracy);

        int reachedIndex = GetReachedDotIndex(rank);

        for (int i = 0; i < _rankDots.Count; i++)
        {
            SetDotSprite(i, i == reachedIndex);
        }
    }

    private void SetDotSprite(int index, bool isReached)
    {
        Image dot = _rankDots[index];

        if (dot == null)
        {
            return;
        }

        Sprite sprite = isReached ? _reachedDotSprite : _unreachedDotSprite;

        // 밝은 점과 어두운 점의 원본 크기가 다르므로 바뀔 때만 크기까지 맞춥니다.
        if (dot.sprite == sprite)
        {
            return;
        }

        dot.sprite = sprite;
        dot.SetNativeSize();
    }

    private static float ToBarRatio(float accuracy)
    {
        for (int i = 1; i < BREAKPOINT_ACCURACIES.Count; i++)
        {
            if (BREAKPOINT_ACCURACIES[i] <= accuracy)
            {
                continue;
            }

            float segmentRatio = Mathf.InverseLerp(BREAKPOINT_ACCURACIES[i - 1], BREAKPOINT_ACCURACIES[i], accuracy);
            return Mathf.Lerp(BREAKPOINT_BAR_RATIOS[i - 1], BREAKPOINT_BAR_RATIOS[i], segmentRatio);
        }

        return 1f;
    }

    /// <summary>
    /// 아직 클리어 기준에 못 미친 FAILED는 밝힐 점이 없으므로 -1을 돌려줍니다.
    /// </summary>
    private static int GetReachedDotIndex(ELiveRank rank)
    {
        switch (rank)
        {
            case ELiveRank.C:
                return 0;

            case ELiveRank.B:
                return 1;

            case ELiveRank.A:
                return 2;

            case ELiveRank.S:
                return 3;

            case ELiveRank.PERFECT_S:
                return 4;

            default:
                return -1;
        }
    }
}
