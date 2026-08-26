using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 결과 화면 우측의 퍼포먼스 패널입니다. 왕관·퍼펙트 비율·별 다섯으로 이번 판을 요약합니다.
///
/// 판정표의 ACCURACY와 다른 값을 보여 줍니다. 정확도는 GREAT·GOOD까지 점수로 쳐 주지만,
/// 여기 비율은 PERFECT만 셉니다. 둘을 나란히 두면 "얼마나 맞췄나"와 "얼마나 정확했나"가 갈립니다.
/// </summary>
public class UI_LiveResultPerformancePanel : MonoBehaviour
{
    private const string RATE_FORMAT = "0.0";

    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _crown;

    [SerializeField]
    private TMP_Text _rateText;

    /// <summary>
    /// 클리어했을 때만 보여 주는 것들입니다. 시안의 실패 화면에는 제목과 비율이 없습니다.
    /// </summary>
    [SerializeField]
    private List<GameObject> _clearOnlyObjects = new List<GameObject>();

    /// <summary>
    /// 왼쪽부터 채워지는 별입니다. 개수는 프리팹이 정하므로 코드가 다섯으로 못 박지 않습니다.
    /// </summary>
    [SerializeField]
    private List<Image> _stars = new List<Image>();

    [Foldout("Project")]
    [SerializeField]
    private Sprite _clearCrown;

    [SerializeField]
    private Sprite _failedCrown;

    [SerializeField]
    private Sprite _filledStar;

    [SerializeField]
    private Sprite _emptyStar;

    public void RefreshResult(LiveResultData result)
    {
        SetClearOnlyVisible(result.IsClear);

        if (_crown != null)
        {
            _crown.sprite = result.IsClear ? _clearCrown : _failedCrown;
        }

        if (_rateText != null)
        {
            _rateText.text = $"{GetPerfectRate(result).ToString(RATE_FORMAT)}%";
        }

        SetStars(LivePerformanceRule.GetStarCount(result.Rank));
    }

    private void SetClearOnlyVisible(bool isVisible)
    {
        for (int i = 0; i < _clearOnlyObjects.Count; i++)
        {
            if (_clearOnlyObjects[i] != null)
            {
                _clearOnlyObjects[i].SetActive(isVisible);
            }
        }
    }

    /// <summary>
    /// 채워진 별과 빈 별을 앞에서부터 나눠 칠합니다.
    /// </summary>
    private void SetStars(int filledCount)
    {
        for (int i = 0; i < _stars.Count; i++)
        {
            if (_stars[i] != null)
            {
                _stars[i].sprite = i < filledCount ? _filledStar : _emptyStar;
            }
        }
    }

    /// <summary>
    /// 전체 노트 중 PERFECT로 친 비율입니다. 채보를 모르면 0으로 둡니다.
    /// </summary>
    private static float GetPerfectRate(LiveResultData result)
    {
        if (result.TotalNoteCount <= 0)
        {
            return 0f;
        }

        return (float)result.PerfectCount / result.TotalNoteCount * 100f;
    }
}
