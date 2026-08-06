using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 라이브 준비 단계에서 편성을 확인·변경하는 팝업입니다(SCREEN-008).
/// 무엇을 플레이할지는 이미 LiveEntryContext에 확정돼 있으므로 편성 편집과 LIVE 시작만 책임집니다.
///
/// 편성 값 자체는 LiveLoadoutContext가 들고 있습니다. 열릴 때 기본 편성에서 초기화하고,
/// 뒤로가기·ESC로 닫히면 폐기합니다(기획서 3-H-4). 플레이로 씬을 떠날 때는 CloseAsync가 불리지 않아
/// 변경분이 그대로 현재 LIVE에 유지됩니다.
/// </summary>
public class UI_PhotocardSelectPopup : UI_Popup
{
    [Foldout("Hierarchy")]
    [Header("Sub Panels")]
    [SerializeField]
    private UI_PhotocardSelectTabs _settingTabs;

    [SerializeField]
    private UI_PhotocardSlotList _slotList;

    [SerializeField]
    private UI_OwnedCardList _ownedCardList;

    [SerializeField]
    private UI_ItemSettingPanel _itemSettingPanel;

    /// <summary>
    /// 편성 미달 안내 팝업입니다. 이 팝업 위에 겹쳐 열리므로 편성을 고치던 상태가 유지됩니다.
    /// </summary>
    [SerializeField]
    private UI_CardShortagePopup _cardShortagePopup;

    [Foldout("Hierarchy")]
    [Header("Buttons")]
    // 포토카드 세팅 패널과 의상 & 악세서리 패널이 같은 버튼을 각자 하나씩 들고 있어, 두 벌을 모두 받아 같은 동작에 묶습니다.
    [SerializeField]
    private List<Button> _liveStartButtons = new List<Button>();

    [SerializeField]
    private List<Button> _resetButtons = new List<Button>();

    protected override void Awake()
    {
        // 뒤로가기(닫기) 버튼 연결이 기반 클래스에 있으므로 먼저 호출합니다.
        base.Awake();

        _settingTabs.OnTabSelected = SetTab;
        _ownedCardList.OnCardClicked = SwapCard;

        InitButtons();
    }

    /// <summary>
    /// 팝업은 항상 첫 번째 탭에서 시작합니다.
    /// 열기 연출이 끝난 뒤에 탭을 맞추면 직전에 보던 패널이 한 프레임 스쳐 보이므로, 연출 전에 정리합니다.
    /// </summary>
    public override async UniTask OpenAsync()
    {
        // 보통은 여기서 기본 편성이 복사됩니다. 이미 초기화되어 있으면 그대로 유지됩니다(InitFromPlayerData 참고).
        LiveLoadoutContext.Instance.InitFromPlayerData();

        _settingTabs.SetSelectedTab(EPhotocardSelectTab.PHOTOCARD);
        _ownedCardList.ResetPage();
        RefreshPhotocards();
        _itemSettingPanel.RefreshItems();

        await base.OpenAsync();
    }

    /// <summary>
    /// 뒤로가기·ESC로 닫히는 경로입니다. 이 화면에서 변경한 편성을 폐기해 기본 편성으로 복원합니다(기획서 3-H-4).
    /// </summary>
    public override async UniTask CloseAsync()
    {
        LiveLoadoutContext.Instance.Clear();

        await base.CloseAsync();
    }

    private void InitButtons()
    {
        foreach (Button liveStartButton in _liveStartButtons)
        {
            liveStartButton.onClick.AddListener(StartLive);
        }

        foreach (Button resetButton in _resetButtons)
        {
            resetButton.onClick.AddListener(ResetSetting);
        }
    }

    private void SetTab(EPhotocardSelectTab tab)
    {
        _settingTabs.SetSelectedTab(tab);
    }

    private void SwapCard(string cardId)
    {
        if (LiveLoadoutContext.Instance.TrySwapCard(cardId))
        {
            RefreshPhotocards();
        }
    }

    /// <summary>
    /// 초기화 버튼입니다. 보고 있는 탭의 편성만 기본값으로 되돌리고 즉시 반영합니다(기획서 3-H-3-1).
    /// </summary>
    private void ResetSetting()
    {
        if (_settingTabs.SelectedTab == EPhotocardSelectTab.PHOTOCARD)
        {
            LiveLoadoutContext.Instance.ResetPhotocards();
            RefreshPhotocards();
            return;
        }

        LiveLoadoutContext.Instance.ResetItems();
        _itemSettingPanel.RefreshItems();
    }

    private void RefreshPhotocards()
    {
        _slotList.RefreshSlots();
        _ownedCardList.RefreshCards();
    }

    /// <summary>
    /// 곡과 난이도는 팝업을 열기 전에 이미 확정되어 있으므로 여기서는 씬만 넘깁니다.
    /// 팝업을 따로 닫지 않아도 SceneTransitionManager가 전환 직전에 열린 팝업을 모두 정리합니다.
    /// </summary>
    private void StartLive()
    {
        // 편성이 5장 규칙을 채우지 못하면 여기서 진입을 막습니다(기획서 16-15). 준비 화면 자체는 막지 않으므로,
        // 안내를 닫고 빈 슬롯을 채운 뒤 다시 시작할 수 있습니다.
        if (!LiveLoadoutRule.IsCurrentLoadoutPlayable())
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenPopup(_cardShortagePopup);
            }

            return;
        }

        // 전환 매니저는 PersistentScene에 있으므로, 이 씬만 단독으로 열어 확인할 때는 존재하지 않을 수 있습니다.
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogWarning($"[UI_PhotocardSelectPopup] SceneTransitionManager가 없어 {ESceneNames.LiveScene}으로 이동하지 못했습니다. PersistentScene이 로드되어 있는지 확인해 주세요.");
            return;
        }

        SceneTransitionManager.Instance.LoadSceneAsync(ESceneNames.LiveScene, this.GetCancellationTokenOnDestroy()).Forget();
    }
}
