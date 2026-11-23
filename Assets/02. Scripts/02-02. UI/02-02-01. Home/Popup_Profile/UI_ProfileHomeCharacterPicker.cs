using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using VInspector;

/// <summary>
/// 프로필 팝업의 홈 캐릭터 고르기(P06)와 미리보기(P07)입니다.
///
/// 고른 인물을 P07에 Idle로 보이기만 하고 홈 표시와 저장값은 바꾸지 않습니다(개발 기획서 3장 "미리보기 단계에서는
/// 홈 표시, 저장값 유지"). 고를 때마다 바로 바꾸므로 빠르게 여러 번 눌러도 마지막 선택만 남습니다(3장 "빠른 연속 선택").
/// 저장할 값은 프로필 팝업이 확인(P08) 때 SelectedCharacterId로 읽어 갑니다.
/// </summary>
public class UI_ProfileHomeCharacterPicker : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private List<UI_ProfileCharacterSlot> _characterSlots = new List<UI_ProfileCharacterSlot>();

    [SerializeField]
    private SkeletonGraphic _previewGraphic;

    private List<HomeCharacterEntry> _entries;

    public string SelectedCharacterId { get; private set; }

    private void Awake()
    {
        for (int i = 0; i < _characterSlots.Count; i++)
        {
            _characterSlots[i].OnSelected = SelectCharacter;
        }
    }

    public void SetCharacters(List<HomeCharacterEntry> entries, string selectedCharacterId)
    {
        _entries = entries;
        SelectedCharacterId = selectedCharacterId;
    }

    public void Refresh()
    {
        RefreshSlots();
        RefreshPreview();
    }

    private void SelectCharacter(string characterId)
    {
        SelectedCharacterId = characterId;
        Refresh();
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < _characterSlots.Count; i++)
        {
            bool hasEntry = i < _entries.Count;
            _characterSlots[i].gameObject.SetActive(hasEntry);

            if (hasEntry)
            {
                _characterSlots[i].SetCharacter(_entries[i].CharacterId, null);
                _characterSlots[i].SetSelected(_entries[i].CharacterId == SelectedCharacterId);
            }
        }
    }

    private void RefreshPreview()
    {
        HomeCharacterEntry entry = HomeCharacterEntry.Find(_entries, SelectedCharacterId);

        if (ReferenceEquals(entry, null) || !entry.CanLoad())
        {
            return;
        }

        if (_previewGraphic.skeletonDataAsset != entry.SkeletonDataAsset)
        {
            _previewGraphic.skeletonDataAsset = entry.SkeletonDataAsset;
            _previewGraphic.startingAnimation = UI_HomeCharacter.IDLE_ANIMATION_NAME;
            _previewGraphic.startingLoop = true;
            _previewGraphic.Initialize(true);
        }

        UI_SkeletonPreviewFitter.Fit(_previewGraphic);
    }
}
