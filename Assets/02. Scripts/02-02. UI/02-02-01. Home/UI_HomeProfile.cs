using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 홈 왼쪽 위 프로필(H01)입니다. 저장된 초상과 프로듀서 이름을 그리고, 초상 배지 버튼으로 프로필 팝업을 엽니다.
///
/// 클릭 범위는 배지 버튼 이미지뿐입니다(피그마 공통 주의 "클릭 가능한 요소는 버튼 이미지의 범위로 한정").
/// 팝업은 이 화면에서만 쓰므로 UIManager에 메서드를 더하지 않고 씬의 팝업을 직접 넘깁니다(AGENTS.md).
/// </summary>
public class UI_HomeProfile : MonoBehaviour
{
    /// <summary>
    /// 팝업에서 확인(P08)으로 저장이 끝났음을 알립니다. 초상, 홈 캐릭터, 대사창을 함께 다시 그리는 일은 홈 화면이 맡습니다.
    /// </summary>
    public Action OnProfileApplied;

    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _portraitImage;

    [SerializeField]
    private TMP_Text _producerNameText;

    [SerializeField]
    private Button _changeButton;

    [SerializeField]
    private UI_HomeCharacter _homeCharacter;

    /// <summary>
    /// 씬에 둔 팝업을 씬의 홈 인스턴스에서 연결합니다(UI_Home의 종료 팝업과 같은 방식).
    /// </summary>
    [SerializeField]
    private UI_ProfileSettingPopup _profilePopup;

    private void Awake()
    {
        _changeButton.onClick.AddListener(OpenProfilePopup);
    }

    /// <summary>
    /// 초상 그림이 없는 인물이 저장되어 있으면 지금 그림을 그대로 둡니다. 빈 원을 보이는 것보다 낫기 때문입니다.
    /// </summary>
    public void Refresh()
    {
        HomeProfileService profile = HomeProfileService.Instance;
        HomeCharacterEntry portraitEntry = _homeCharacter.FindEntry(profile.PortraitId);

        if (!ReferenceEquals(portraitEntry, null) && portraitEntry.Portrait != null)
        {
            _portraitImage.sprite = portraitEntry.Portrait;
        }

        _producerNameText.text = profile.ProducerName;
    }

    /// <summary>
    /// 팝업 스택은 PersistentScene의 UIManager가 들고 있으므로, 이 화면만 단독으로 열면 없을 수 있습니다.
    /// </summary>
    private void OpenProfilePopup()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        _profilePopup.SetProfile(_homeCharacter.Entries, HandleProfileApplied);
        UIManager.Instance.OpenPopup(_profilePopup);
    }

    private void HandleProfileApplied()
    {
        OnProfileApplied?.Invoke();
    }
}
