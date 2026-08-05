using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 상단 재화 표시입니다(MONEY / GEM / HYPE).
///
/// P_UI_CurrencyInfos 프리팹은 9개 화면이 공유하므로, 화면마다 값을 읽는 코드를 따로 두지 않고
/// 이 컴포넌트 하나가 담당합니다.
///
/// EXP는 여기 넣지 않습니다. 프로필의 레벨 경험치 바에 표시되는 값이라 성격이 다릅니다.
/// GEM은 증감 경로가 아직 없어(상점·가챠 비활성) 항상 0으로 표시됩니다. 정상입니다.
/// </summary>
public class UI_CurrencyHud : MonoBehaviour
{
    /// <summary>
    /// 천 단위 구분 기호를 넣습니다("MONEY 1,000").
    /// </summary>
    private const string CURRENCY_FORMAT = "N0";

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _moneyText;

    [SerializeField]
    private TMP_Text _gemText;

    [SerializeField]
    private TMP_Text _hypeText;

    /// <summary>
    /// 현재 재화를 다시 읽어 표시합니다.
    ///
    /// 이벤트를 구독하거나 Update에서 폴링하지 않습니다. 재화는 리듬게임 결과와 스토리 보상에서만 변하고
    /// 두 경우 모두 화면을 다시 불러오므로, 진입 시 한 번 호출하면 충분합니다.
    /// </summary>
    public void Refresh()
    {
        PlayerData data = PlayerDataProvider.Instance.Data;

        SetText(_moneyText, data.Money);
        SetText(_gemText, data.Gem);
        SetText(_hypeText, data.Hype);
    }

    private void SetText(TMP_Text target, int value)
    {
        if (target == null)
        {
            return;
        }

        target.text = value.ToString(CURRENCY_FORMAT);
    }
}
