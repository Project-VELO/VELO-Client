using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스크립트 확인 팝업의 대사 한 줄입니다(기획서 7.2 "대사 리스트 = 화자명 + 대사 내용").
/// 풀에서 재사용되므로 표시 상태는 SetItem에서 매번 새로 덮어씁니다.
/// </summary>
public class UI_StoryLogItem : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _iconImage;

    [SerializeField]
    private TMP_Text _speakerText;

    [SerializeField]
    private TMP_Text _lineText;

    public void SetItem(StoryLineData line, StoryVisualBinder visualBinder)
    {
        _lineText.text = line.Text;

        string speakerName = StoryLineSpeaker.GetDisplayName(line);
        bool hasSpeaker = !string.IsNullOrEmpty(speakerName);

        _speakerText.gameObject.SetActive(hasSpeaker);
        _iconImage.gameObject.SetActive(hasSpeaker);

        if (!hasSpeaker)
        {
            return;
        }

        _speakerText.text = speakerName;

        // 감상 화면과 같은 색을 써야 목록에서도 누구 대사인지 알아볼 수 있습니다.
        // 단역은 SpeakerId가 없어 색 표에도 없으므로 중립 회색으로 떨어집니다.
        Sprite portrait = visualBinder.GetCharacter(line.SpeakerId, line.ExpressionId);
        _iconImage.sprite = portrait;
        _iconImage.color = portrait == null ? visualBinder.GetCharacterPlaceholderColor(line.SpeakerId) : Color.white;
    }

    /// <summary>
    /// 풀로 반환되기 전에 호출해 이전 대사의 흔적을 지웁니다.
    /// 초상 스프라이트와 색까지 되돌리는 것은, 다음에 꺼내 쓸 때 화자가 없는 줄이면 SetItem이
    /// 일찍 반환해 이미지를 건드리지 않기 때문입니다. 남겨 두면 다른 화자의 색을 물려받은 채 남습니다.
    /// </summary>
    public void ResetItem()
    {
        _lineText.text = string.Empty;
        _speakerText.text = string.Empty;
        _speakerText.gameObject.SetActive(false);

        _iconImage.sprite = null;
        _iconImage.color = Color.white;
        _iconImage.gameObject.SetActive(false);
    }
}
