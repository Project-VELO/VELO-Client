using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 프로필 초상 선택 팝업(P09~P11)입니다. 후보(P10)를 고르면 크게 미리 보이고(P11), 확인하면 상위 프로필 팝업의
/// 임시값만 바꿉니다. 저장은 상위 팝업의 확인(P08)에서만 합니다(검수 1 답).
///
/// 후보는 초상 그림이 있는 인물입니다. 후보 목록과 그림은 기획자 입력이라(그림은 15칸) 지금은 저장소에 있는
/// 초상만 나옵니다. 칸은 프리팹에 미리 놓고 후보 수만큼 켭니다.
/// </summary>
public class UI_ProfilePortraitPopup : UI_Popup
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<UI_ProfileCharacterSlot> _portraitSlots = new List<UI_ProfileCharacterSlot>();

    [SerializeField]
    private Image _previewImage;

    [SerializeField]
    private Button _confirmButton;

    private readonly List<HomeCharacterEntry> _candidates = new List<HomeCharacterEntry>();
    private Action<string> _onConfirmed;
    private string _pendingPortraitId;

    protected override void Awake()
    {
        base.Awake();

        _confirmButton.onClick.AddListener(Confirm);

        for (int i = 0; i < _portraitSlots.Count; i++)
        {
            _portraitSlots[i].OnSelected = SelectPortrait;
        }
    }

    public void SetPortrait(List<HomeCharacterEntry> entries, string portraitId, Action<string> onConfirmed)
    {
        _candidates.Clear();

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].Portrait != null)
            {
                _candidates.Add(entries[i]);
            }
        }

        _pendingPortraitId = portraitId;
        _onConfirmed = onConfirmed;
    }

    /// <summary>
    /// 켜진 뒤에 그립니다. 기반 OpenAsync는 첫 대기(열기 연출) 전에 켜기까지 마칩니다.
    /// </summary>
    public override async UniTask OpenAsync()
    {
        UniTask openTask = base.OpenAsync();
        Refresh();
        await openTask;
    }

    /// <summary>
    /// 확인하지 않고 닫으면 고른 값을 버립니다(개발 기획서 3장 "확인하지 않은 편집값 폐기").
    /// </summary>
    public override async UniTask CloseAsync()
    {
        _onConfirmed = null;
        await base.CloseAsync();
    }

    private void Refresh()
    {
        for (int i = 0; i < _portraitSlots.Count; i++)
        {
            bool hasCandidate = i < _candidates.Count;
            _portraitSlots[i].gameObject.SetActive(hasCandidate);

            if (hasCandidate)
            {
                _portraitSlots[i].SetCharacter(_candidates[i].CharacterId, _candidates[i].Portrait);
                _portraitSlots[i].SetSelected(_candidates[i].CharacterId == _pendingPortraitId);
            }
        }

        HomeCharacterEntry selected = HomeCharacterEntry.Find(_candidates, _pendingPortraitId);
        _previewImage.gameObject.SetActive(!ReferenceEquals(selected, null));

        if (!ReferenceEquals(selected, null))
        {
            _previewImage.sprite = selected.Portrait;
        }
    }

    private void SelectPortrait(string portraitId)
    {
        _pendingPortraitId = portraitId;
        Refresh();
    }

    private void Confirm()
    {
        _onConfirmed?.Invoke(_pendingPortraitId);
        RequestClose();
    }
}
