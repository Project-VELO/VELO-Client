using TMPro;
using UnityEngine;
using UnityEngine.UI;
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

    /// <summary>
    /// 대사 상자의 배경 그림입니다. 대사가 없는 줄에서 이것만 끕니다.
    /// NEXT 버튼이 상자 안에 들어 있어 오브젝트를 통째로 끄면 진행이 막힙니다.
    /// </summary>
    [SerializeField]
    private Image _boxBackground;

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
    /// 한 줄분으로 대사 상자를 갈아 끼웁니다. 타이핑이 시작되기 전에 불러야 합니다.
    ///
    /// 세 가지를 한 번에 받는 이유는 하나를 빠뜨리기 쉽기 때문입니다.
    /// 화자만 갱신하고 글꼴을 두면 앞 줄의 크기와 색이 그대로 남습니다.
    /// </summary>
    public void Refresh(StoryLineData line)
    {
        RefreshBox(line);
        RefreshSpeaker(line);
        RefreshBodyStyle(line.TextStyleId);
    }

    /// <summary>
    /// 대사가 없는 줄은 배경만 넘기는 컷입니다(05·11화). 상자 그림을 감춰 배경을 가리지 않게 합니다.
    /// </summary>
    private void RefreshBox(StoryLineData line)
    {
        _boxBackground.enabled = !string.IsNullOrEmpty(line.Text);
    }

    /// <summary>
    /// 본문 글꼴을 이번 줄의 스타일로 갈아 끼웁니다(연출표의 [디자인] 열).
    /// 글자를 찍는 도중에 크기가 바뀌면 이미 찍힌 글자까지 다시 배치되므로 타이핑 전에 끝냅니다.
    /// </summary>
    private void RefreshBodyStyle(string textStyleId)
    {
        _textStyleBinder.Get(textStyleId).ApplyTo(_bodyText, _defaultBodyFont);
    }

    /// <summary>
    /// 화자명 박스를 갱신합니다.
    /// 지문(NARRATION)은 화자가 없으므로 박스를 통째로 끕니다(ELineType 주석).
    /// 독백(MONOLOGUE)은 화자명을 표시하되 대사와 구분되는 연출 대상이라, 지금은 이름만 같은 방식으로 냅니다.
    /// </summary>
    private void RefreshSpeaker(StoryLineData line)
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
