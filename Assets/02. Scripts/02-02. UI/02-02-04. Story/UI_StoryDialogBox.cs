using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 감상 화면의 대사 상자입니다. 화자명 박스와 본문 텍스트를 담당합니다(기획서 6.2).
///
/// 본문 TMP를 밖으로 내주는 것은 타이핑이 이 TMP 하나의 maxVisibleCharacters로 이루어지기 때문입니다.
/// 타이핑 로직까지 여기 두면 대사 상자가 재생 상태를 알게 되어 책임이 섞입니다.
/// </summary>
public class UI_StoryDialogBox : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private GameObject _speakerRoot;

    [SerializeField]
    private TMP_Text _speakerText;

    [SerializeField]
    private TMP_Text _bodyText;

    public TMP_Text BodyText => _bodyText;

    /// <summary>
    /// 화자명 박스를 갱신합니다.
    /// 지문(NARRATION)은 화자가 없으므로 박스를 통째로 끕니다(ELineType 주석).
    /// 독백(MONOLOGUE)은 화자명을 표시하되 대사와 구분되는 연출 대상이라, 지금은 이름만 같은 방식으로 냅니다.
    /// </summary>
    public void RefreshSpeaker(StoryLineData line)
    {
        string speakerName = StoryLineSpeaker.GetDisplayName(line);

        if (line.LineType == ELineType.NARRATION || string.IsNullOrEmpty(speakerName))
        {
            _speakerRoot.SetActive(false);
            return;
        }

        _speakerRoot.SetActive(true);
        _speakerText.text = speakerName;
    }
}
