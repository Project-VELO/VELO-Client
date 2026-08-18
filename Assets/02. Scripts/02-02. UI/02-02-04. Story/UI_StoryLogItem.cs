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
    /// <summary>
    /// 원형 프레임과 초상을 함께 묶은 오브젝트입니다. 화자가 없는 줄에서 통째로 끕니다.
    /// 초상만 끄면 테두리 프레임이 빈 동그라미로 남습니다.
    /// </summary>
    [SerializeField]
    private GameObject _iconRoot;

    /// <summary>
    /// 프레임 안에서 원형으로 잘려 나오는 얼굴 초상입니다.
    /// 자르는 일은 부모의 Mask가 하므로 여기서는 그림과 색만 정합니다.
    /// </summary>
    [SerializeField]
    private Image _portraitImage;

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
        _iconRoot.SetActive(hasSpeaker);

        if (!hasSpeaker)
        {
            return;
        }

        _speakerText.text = speakerName;

        // 감상 화면의 전신이 아니라 얼굴 초상을 씁니다. 작은 원형 칸에 전신을 넣으면
        // 인물이 아주 작게 들어가거나 몸통만 잘려 누구인지 알아볼 수 없습니다.
        // 초상이 없는 화자는 감상 화면과 같은 색으로 떨어뜨려, 목록에서도 누구 대사인지 구분되게 합니다.
        Sprite face = visualBinder.GetCharacterFace(line.SpeakerId);
        _portraitImage.sprite = face;
        _portraitImage.color = face == null ? visualBinder.GetCharacterPlaceholderColor(line.SpeakerId) : Color.white;
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

        _portraitImage.sprite = null;
        _portraitImage.color = Color.white;
        _iconRoot.SetActive(false);
    }
}
