using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 프로필 팝업의 인물 원 한 칸입니다. 홈 캐릭터 선택(P06)과 초상 선택(P10)이 함께 씁니다.
///
/// 칸은 프리팹에 미리 놓아 두고 인물 수만큼만 켭니다. 동적 생성(Instantiate)을 쓰지 않기 위해서입니다.
/// </summary>
public class UI_ProfileCharacterSlot : MonoBehaviour
{
    public Action<string> OnSelected;

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _button;

    [SerializeField]
    private Image _portraitImage;

    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private GameObject _selectedMark;

    private string _characterId;

    public string CharacterId => _characterId;

    private void Awake()
    {
        _button.onClick.AddListener(Select);
    }

    /// <summary>
    /// 초상 그림을 넘기지 않으면 이름만 보입니다. 홈 캐릭터 칸은 초상이 아니라 인물을 고르는 칸이기 때문입니다.
    /// </summary>
    public void SetCharacter(string characterId, Sprite portrait)
    {
        _characterId = characterId;
        _nameText.text = UiText.Localize(MasterDataProvider.Instance.GetCharacterDisplayName(characterId));

        _portraitImage.gameObject.SetActive(portrait != null);
        _portraitImage.sprite = portrait;
    }

    public void SetSelected(bool isSelected)
    {
        _selectedMark.SetActive(isSelected);
    }

    private void Select()
    {
        OnSelected?.Invoke(_characterId);
    }
}
