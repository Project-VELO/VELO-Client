using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 스토리 최초 완료 보상을 알리는 팝업입니다(기획서 3-J-4 처리 순서).
///
/// 감상 화면이 아니라 목록 화면에서 뜹니다. 기획서가 "스토리 목록 화면으로 이동 → 보상 안내 팝업 표시"
/// 순서를 못박고 있기 때문입니다.
///
/// 이 팝업은 보상을 지급하지 않습니다. 지급은 GameProgressService.CompleteStory 안에서 이미 끝났고,
/// 확인 버튼은 팝업을 닫기만 합니다(기획서 16-5). 여기에 지급 코드를 붙이면 이중 지급이 됩니다.
/// </summary>
public class UI_StoryRewardPopup : UI_Popup
{
    /// <summary>
    /// 획득량이므로 부호를 붙여 보여 줍니다. 결과 화면의 재화 칸과 같은 양식입니다.
    /// 여기 오는 값은 보상으로 늘어난 양뿐이라 음수가 될 일이 없어 부호를 고정으로 씁니다.
    /// </summary>
    private const string AMOUNT_FORMAT = "+ #,##0";
    private const string MESSAGE = "스토리 감상 보상을 획득했습니다.";

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _messageText;

    [SerializeField]
    private TMP_Text _moneyText;

    [SerializeField]
    private TMP_Text _gemText;

    [SerializeField]
    private TMP_Text _hypeText;

    /// <summary>
    /// 표시할 보상을 채웁니다. 팝업을 열기 직전에 호출합니다.
    /// 등록되지 않은 보상 ID면 열지 않는 편이 낫기 때문에 false를 돌려줍니다.
    /// </summary>
    public bool SetReward(string rewardId)
    {
        if (!MasterDataProvider.Instance.TryGetReward(rewardId, out RewardData reward))
        {
            Debug.LogWarning($"[UI_StoryRewardPopup] 등록되지 않은 보상입니다: {rewardId}");
            return false;
        }

        if (_messageText != null)
        {
            _messageText.text = MESSAGE;
        }

        SetAmount(_moneyText, reward.Money);
        SetAmount(_gemText, reward.Gem);
        SetAmount(_hypeText, reward.Hype);

        return true;
    }

    private void SetAmount(TMP_Text target, int value)
    {
        if (target == null)
        {
            return;
        }

        target.text = value.ToString(AMOUNT_FORMAT);
    }
}
