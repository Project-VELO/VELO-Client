using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 잠긴 스토리를 클릭했을 때 뜨는 안내 팝업입니다(기획서 5.4).
///
/// 닫기만 하면 되므로 동작이 없습니다. 닫기 버튼은 UI_Popup이 이미 처리합니다.
/// 문구를 인스펙터가 아니라 여기에 두는 것은 잠금 사유가 늘어나도 한 곳만 고치면 되게 하기 위함입니다.
/// </summary>
public class UI_LockedStoryPopup : UI_Popup
{
    private const string MESSAGE = "아직 해금되지 않은 스토리입니다.";

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _messageText;

    public override void InitPopup()
    {
        base.InitPopup();

        if (_messageText != null)
        {
            _messageText.text = MESSAGE;
        }
    }
}
