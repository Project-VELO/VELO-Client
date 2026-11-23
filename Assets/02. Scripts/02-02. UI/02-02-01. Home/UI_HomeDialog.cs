using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VInspector;

/// <summary>
/// 홈 오른쪽 아래 대사창(H07)입니다. 화자 이름과 홈 대사를 보이고, 누를 때마다 다음 대사로 넘깁니다
/// (개발 기획서 2장 "말풍선 클릭 시 다음 홈 대사로 순환"). 끝 줄 다음은 첫 줄입니다.
///
/// 화자는 가운데 홈 캐릭터입니다(검수 1 답, 기획서 16장 권고 "H06 중앙 인물").
/// 대사 목록과 처음 보일 대사는 기획자 입력이라, 목록이 비면 글을 비우고 클릭해도 그대로 둡니다.
///
/// 클릭은 자식 이미지(대사 배경)에서 올라온 포인터 이벤트를 받습니다. 캐릭터 클릭(H06)과는 서로 부르지 않습니다.
/// </summary>
public class UI_HomeDialog : MonoBehaviour, IPointerClickHandler
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _dialogText;

    private HomeCharacterEntry _speaker;
    private int _lineIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        // 팝업이 열려 있는 동안 뒤 화면 입력은 막습니다(개발 기획서 3장 "팝업 열린 상태: 뒤 메뉴 입력 차단").
        if (UIManager.Instance != null && UIManager.Instance.HasPopups)
        {
            return;
        }

        ShowNextLine();
    }

    /// <summary>
    /// 화자가 바뀔 때만 첫 대사로 돌아갑니다. 언어만 바꾼 갱신에서도 불리는데, 그때 읽던 줄이 처음으로 튀지 않게 하기 위해서입니다.
    /// </summary>
    public void SetSpeaker(HomeCharacterEntry speaker)
    {
        if (!ReferenceEquals(_speaker, speaker))
        {
            _speaker = speaker;
            _lineIndex = 0;
        }

        Refresh();
    }

    private void ShowNextLine()
    {
        if (ReferenceEquals(_speaker, null) || _speaker.DialogLines.Count < 2)
        {
            return;
        }

        _lineIndex = (_lineIndex + 1) % _speaker.DialogLines.Count;
        RefreshDialog();
    }

    private void Refresh()
    {
        if (ReferenceEquals(_speaker, null))
        {
            return;
        }

        string displayName = MasterDataProvider.Instance.GetCharacterDisplayName(_speaker.CharacterId);
        _nameText.text = UiText.Localize(displayName);
        RefreshDialog();
    }

    private void RefreshDialog()
    {
        List<string> lines = _speaker.DialogLines;
        _dialogText.text = _lineIndex < lines.Count ? UiText.Localize(lines[_lineIndex]) : string.Empty;
    }
}
