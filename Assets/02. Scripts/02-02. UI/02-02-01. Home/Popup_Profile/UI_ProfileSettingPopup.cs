using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 프로필 설정 팝업(P01~P08)입니다. 초상(P01, P02 → P09), 프로듀서 이름(P03, P04 → P12), 홈 캐릭터(P06, P07)를
/// 고르고, 확인(P08)을 눌렀을 때만 세 값을 함께 저장합니다(개발 기획서 3장, 15장).
///
/// 고른 값은 확인 전까지 이 팝업의 임시값입니다. 하위 팝업(P09, P12)의 확인도 이 임시값만 바꿉니다(검수 1 답).
/// 닫기나 Esc로 닫으면 임시값은 버려지고, 다음에 열 때 저장값부터 다시 읽습니다(3장 "확인하지 않은 편집값 폐기").
/// UID(P05)는 형식이 정해지지 않아(기획자 입력) 칸을 두지 않습니다.
/// </summary>
public class UI_ProfileSettingPopup : UI_Popup
{
    [Foldout("Hierarchy")]
    [Header("Portrait")]
    [SerializeField]
    private Image _portraitImage;

    [SerializeField]
    private Button _portraitChangeButton;

    [Header("Name")]
    [SerializeField]
    private TMP_Text _producerNameText;

    [SerializeField]
    private Button _nameChangeButton;

    [Header("Home Character")]
    [SerializeField]
    private UI_ProfileHomeCharacterPicker _characterPicker;

    [SerializeField]
    private Button _confirmButton;

    [Header("Sub Popups")]
    [SerializeField]
    private UI_ProfilePortraitPopup _portraitPopup;

    [SerializeField]
    private UI_ProducerNamePopup _namePopup;

    private List<HomeCharacterEntry> _entries;
    private Action _onApplied;

    private string _pendingPortraitId;
    private string _pendingProducerName;

    /// <summary>
    /// 열 때의 초상, 홈 캐릭터(기본값 해석 뒤)입니다. 확인 때 이 값에서 바뀌지 않았으면 저장값을 그대로 넘깁니다.
    /// </summary>
    private string _openedPortraitId;
    private string _openedCharacterId;

    protected override void Awake()
    {
        base.Awake();

        _portraitChangeButton.onClick.AddListener(OpenPortraitPopup);
        _nameChangeButton.onClick.AddListener(OpenNamePopup);
        _confirmButton.onClick.AddListener(Confirm);
    }

    /// <summary>
    /// 열기 직전에 부릅니다. 저장값을 임시값으로 옮겨 두고, 그리기는 팝업이 켜진 뒤(OpenAsync)에 합니다.
    /// </summary>
    public void SetProfile(List<HomeCharacterEntry> entries, Action onApplied)
    {
        HomeProfileService profile = HomeProfileService.Instance;

        _entries = entries;
        _onApplied = onApplied;
        _openedPortraitId = profile.PortraitId;
        _openedCharacterId = profile.HomeCharacterId;
        _pendingPortraitId = _openedPortraitId;
        _pendingProducerName = profile.SavedProducerName;
        _characterPicker.SetCharacters(entries, _openedCharacterId);
    }

    /// <summary>
    /// 기반 클래스가 켜면서 프리팹 글자를 번역하므로, 사용자가 정한 이름이 번역표의 낱말과 같아도 바뀌지 않게
    /// 켜진 뒤에 그립니다. 기반 OpenAsync는 첫 대기(열기 연출) 전에 켜기까지 마칩니다.
    /// </summary>
    public override async UniTask OpenAsync()
    {
        UniTask openTask = base.OpenAsync();

        RefreshPortrait();
        RefreshProducerName();
        _characterPicker.Refresh();

        await openTask;
    }

    private void RefreshPortrait()
    {
        HomeCharacterEntry entry = HomeCharacterEntry.Find(_entries, _pendingPortraitId);

        if (!ReferenceEquals(entry, null) && entry.Portrait != null)
        {
            _portraitImage.sprite = entry.Portrait;
        }
    }

    private void RefreshProducerName()
    {
        _producerNameText.text = string.IsNullOrEmpty(_pendingProducerName)
            ? UiText.Localize(HomeProfileService.DEFAULT_PRODUCER_NAME)
            : _pendingProducerName;
    }

    private void OpenPortraitPopup()
    {
        _portraitPopup.SetPortrait(_entries, _pendingPortraitId, SetPendingPortrait);
        UIManager.Instance.OpenPopup(_portraitPopup);
    }

    private void OpenNamePopup()
    {
        _namePopup.SetProducerName(_producerNameText.text, SetPendingProducerName);
        UIManager.Instance.OpenPopup(_namePopup);
    }

    private void SetPendingPortrait(string portraitId)
    {
        _pendingPortraitId = portraitId;
        RefreshPortrait();
    }

    private void SetPendingProducerName(string producerName)
    {
        _pendingProducerName = producerName;
        RefreshProducerName();
    }

    /// <summary>
    /// 홈 캐릭터 리소스를 먼저 확인합니다. 읽지 못하면 저장하지 않고 팝업을 그대로 둡니다
    /// (3장 "리소스 로딩 실패: 기존 저장값, 홈 표시 유지"). 재선택 안내 문구는 기획자 입력이라 아직 띄우지 않습니다.
    /// 저장에 실패해도 팝업을 닫지 않습니다. 실패 안내 뒤에 고른 값을 그대로 다시 확인할 수 있어야 하기 때문입니다.
    /// </summary>
    private void Confirm()
    {
        string characterId = _characterPicker.SelectedCharacterId;
        HomeCharacterEntry entry = HomeCharacterEntry.Find(_entries, characterId);

        if (ReferenceEquals(entry, null) || !entry.CanLoad())
        {
            Debug.LogWarning($"[UI_ProfileSettingPopup] 홈 캐릭터 {characterId}의 Spine 자료를 읽지 못해 저장하지 않습니다.");
            return;
        }

        HomeProfileService profile = HomeProfileService.Instance;
        string portraitToSave = _pendingPortraitId == _openedPortraitId ? profile.SavedPortraitId : _pendingPortraitId;
        string characterToSave = characterId == _openedCharacterId ? profile.SavedHomeCharacterId : characterId;

        if (!profile.Apply(portraitToSave, characterToSave, _pendingProducerName))
        {
            return;
        }

        _onApplied?.Invoke();
        RequestClose();
    }
}
