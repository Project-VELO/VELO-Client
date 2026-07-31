using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 라이브 준비 단계에서 편성을 확인하는 팝업입니다.
/// 무엇을 플레이할지는 곡 선택 화면이 팝업을 열기 전에 이미 LiveEntryContext에 확정해 두므로,
/// 이 팝업은 편성 화면 전환과 LIVE 시작(리듬게임 씬 이동)만 책임집니다.
/// </summary>
public class UI_PhotocardSelectPopup : UI_Popup
{
    [Foldout("Hierarchy")]
    [Header("Sub Panels")]
    [SerializeField]
    private UI_PhotocardSelectTabs _settingTabs;

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

        InitButtons();
    }

    /// <summary>
    /// 팝업은 항상 첫 번째 탭에서 시작합니다.
    /// 열기 연출이 끝난 뒤에 탭을 맞추면 직전에 보던 패널이 한 프레임 스쳐 보이므로, 연출 전에 정리합니다.
    /// </summary>
    public override async UniTask OpenAsync()
    {
        _settingTabs.SetSelectedTab(EPhotocardSelectTab.PHOTOCARD);

        await base.OpenAsync();
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

    /// <summary>
    /// 곡과 난이도는 팝업을 열기 전에 이미 확정되어 있으므로 여기서는 씬만 넘깁니다.
    /// 팝업을 따로 닫지 않아도 SceneTransitionManager가 전환 직전에 열린 팝업을 모두 정리합니다.
    /// </summary>
    private void StartLive()
    {
        // 전환 매니저는 PersistentScene에 있으므로, 이 씬만 단독으로 열어 확인할 때는 존재하지 않을 수 있습니다.
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogWarning($"[UI_PhotocardSelectPopup] SceneTransitionManager가 없어 {ESceneNames.LiveScene}으로 이동하지 못했습니다. PersistentScene이 로드되어 있는지 확인해 주세요.");
            return;
        }

        SceneTransitionManager.Instance.LoadSceneAsync(ESceneNames.LiveScene, this.GetCancellationTokenOnDestroy()).Forget();
    }

    private void ResetSetting()
    {
        // TODO: 포토카드·의상 데이터 구조가 확정되면 _settingTabs.SelectedTab에 해당하는 편성을 초기화합니다.
    }
}
