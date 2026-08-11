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

    [Foldout("Project")]
    [SerializeField]
    private StoryTextStyleBinder _textStyleBinder;

    public TMP_Text BodyText => _bodyText;

    /// <summary>
    /// 스타일에 글꼴이 지정되지 않은 줄이 돌아올 자리입니다.
    /// 프리팹에 물려 둔 글꼴을 한 번만 기억해 두고, 그 뒤로는 스타일이 이 값을 덮어씁니다.
    /// </summary>
    private TMP_FontAsset _defaultBodyFont;

    private void Awake()
    {
        _defaultBodyFont = _bodyText.font;
    }

    /// <summary>
    /// 본문 글꼴을 이번 줄의 스타일로 갈아 끼웁니다(연출표의 [디자인] 열).
    /// 타이핑이 시작되기 전에 불러야 합니다. 글자를 찍는 도중에 크기가 바뀌면 이미 찍힌 글자까지 다시 배치됩니다.
    /// </summary>
    public void RefreshBodyStyle(string textStyleId)
    {
        _textStyleBinder.Get(textStyleId).ApplyTo(_bodyText, _defaultBodyFont);
    }

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
