using UnityEngine;
using TMPro;
using VInspector;

/// <summary>
/// 결과 화면에서 이번 플레이로 획득한 MONEY / HYPE / GEM을 표시합니다.
/// 실제 지급은 LiveResultProcessor가 이미 끝냈으므로 이 패널은 표시만 담당합니다.
///
/// EXP도 지급되지만 여기서는 보여 주지 않습니다. 시안이 세 칸을 재화로 채우고,
/// 경험치는 프로필의 레벨 바에서 보는 값이라 성격이 다릅니다.
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
    private TMP_Text _gemText;

    public void RefreshReward(LiveResultData result)
    {
        // FAILED에서도 칸을 남깁니다. 시안이 실패 화면에도 세 칸을 그리고 있고,
        // 보상이 없다는 사실은 0으로 보여 주는 편이 칸이 통째로 사라지는 것보다 읽힙니다(3-J-3).
        if (_root != null)
        {
            _root.SetActive(true);
        }

        SetAmount(_moneyText, result.EarnedMoney);
        SetAmount(_hypeText, result.EarnedHype);
        SetAmount(_gemText, result.EarnedGem);
    }

    /// <summary>
    /// 획득량이므로 부호를 붙여 보여 줍니다(시안 "+ 1,000").
    /// 여기 오는 값은 이번 판으로 늘어난 양뿐이라 음수가 될 일이 없습니다.
    /// </summary>
    private static void SetAmount(TMP_Text text, int amount)
    {
        if (text == null)
        {
            return;
        }

        text.text = $"+ {amount:N0}";
    }
}
