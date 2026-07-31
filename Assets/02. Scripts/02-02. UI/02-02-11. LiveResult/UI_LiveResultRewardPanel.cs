using UnityEngine;
using TMPro;
using VInspector;

/// <summary>
/// 결과 화면에서 이번 플레이로 획득한 MONEY / HYPE / EXP를 표시합니다.
/// 실제 지급은 LiveRewardService가 이미 끝냈으므로 이 패널은 표시만 담당합니다.
/// </summary>
public class UI_LiveResultRewardPanel : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("Components")]
    [SerializeField]
    private GameObject _root;

    [SerializeField]
    private TMP_Text _moneyText;

    [SerializeField]
    private TMP_Text _hypeText;

    [SerializeField]
    private TMP_Text _expText;

    public void RefreshReward(LiveResultData result)
    {
        // FAILED와 중도 종료는 보상이 없으므로 패널 자체를 숨깁니다(3-J-3).
        if (_root != null)
        {
            _root.SetActive(result.IsClear);
        }

        SetText(_moneyText, result.EarnedMoney.ToString("N0"));
        SetText(_hypeText, result.EarnedHype.ToString("N0"));
        SetText(_expText, result.EarnedExp.ToString("N0"));
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
