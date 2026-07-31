using UnityEngine;
using TMPro;
using VInspector;

/// <summary>
/// 결과 화면의 판정 상세(PERFECT/GREAT/GOOD/BAD 개수, 최대 콤보, 정확도)를 표시합니다.
/// </summary>
public class UI_LiveResultJudgementPanel : MonoBehaviour
{
    private const string ACCURACY_FORMAT = "0.00";

    [Foldout("Hierarchy")]
    [Header("Judgement Counts")]
    [SerializeField]
    private TMP_Text _perfectCountText;

    [SerializeField]
    private TMP_Text _greatCountText;

    [SerializeField]
    private TMP_Text _goodCountText;

    [SerializeField]
    private TMP_Text _badCountText;

    [Foldout("Hierarchy")]
    [Header("Summary")]
    [SerializeField]
    private TMP_Text _maxComboText;

    [SerializeField]
    private TMP_Text _accuracyText;

    public void RefreshResult(LiveResultData result)
    {
        SetText(_perfectCountText, result.PerfectCount.ToString());
        SetText(_greatCountText, result.GreatCount.ToString());
        SetText(_goodCountText, result.GoodCount.ToString());
        SetText(_badCountText, result.BadCount.ToString());

        SetText(_maxComboText, result.MaxCombo.ToString());
        SetText(_accuracyText, $"{result.Accuracy.ToString(ACCURACY_FORMAT)}%");
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text == null)
        {
            return;
        }

        text.text = value;
    }
}
