using Cysharp.Threading.Tasks;
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
    /// 대사 상자의 프레임 그림입니다. 대사가 없는 줄에서 감춥니다.
    ///
    /// 이 그림은 상자 전체를 덮는 진행 버튼이기도 해서, 컴포넌트를 끄거나 오브젝트를 비활성으로 두면
    /// 클릭이 통하지 않아 다음 줄로 넘어갈 수 없습니다. 그래서 투명도만 낮춥니다.
    /// </summary>
    [SerializeField]
    private Image _boxBackground;

    /// <summary>
    /// 대화창 없이 화면 한가운데에 띄우는 문장입니다(연출표의 "화면 중앙 텍스트").
    /// 본문과 별도 오브젝트인 이유는 자리와 정렬이 다르기 때문입니다. 같은 TMP를 옮겨 쓰면
    /// 줄마다 앵커와 정렬을 되돌려야 하고, 되돌리기를 빠뜨리면 다음 줄이 가운데에 남습니다.
    /// </summary>
    [SerializeField]
    private TMP_Text _centerText;

    [Foldout("Project")]
    [SerializeField]
    private StoryTextStyleBinder _textStyleBinder;

    /// <summary>
    /// 이번 줄의 글자가 찍힐 자리입니다. Refresh가 정한 위치를 따릅니다.
    /// 타이핑은 이 TMP 하나의 maxVisibleCharacters로 이루어지므로, 위치가 바뀌면 대상도 함께 바뀌어야 합니다.
    /// </summary>
    public TMP_Text BodyText => _isCenterPlacement ? _centerText : _bodyText;

    private bool _isCenterPlacement;

    /// <summary>
    /// 스타일에 글꼴이 지정되지 않은 줄이 돌아올 자리입니다.
    /// 프리팹에 물려 둔 글꼴을 한 번만 기억해 두고, 그 뒤로는 스타일이 이 값을 덮어씁니다.
    /// </summary>
    private TMP_FontAsset _defaultBodyFont;

    /// <summary>
    /// 글자 떨림을 돌리는 쪽입니다. 줄마다 다시 시작하므로 상자가 하나만 들고 돌려 씁니다.
    /// </summary>
    private readonly StoryTextTrembler _trembler = new StoryTextTrembler();

    private void Awake()
    {
        _defaultBodyFont = _bodyText.font;
    }

    /// <summary>
    /// 화면을 떠날 때 떨림을 멈춥니다. 취소 토큰으로도 멈추지만, 흔들린 자리를 되돌리는 것은 Stop뿐입니다.
    /// </summary>
    private void OnDestroy()
    {
        _trembler.Stop();
    }

    /// <summary>
    /// 한 줄분으로 대사 상자를 갈아 끼웁니다. 타이핑이 시작되기 전에 불러야 합니다.
    ///
    /// 세 가지를 한 번에 받는 이유는 하나를 빠뜨리기 쉽기 때문입니다.
    /// 화자만 갱신하고 글꼴을 두면 앞 줄의 크기와 색이 그대로 남습니다.
    /// </summary>
    public void Refresh(StoryLineData line)
    {
        RefreshPlacement(line);
        RefreshBox(line);
        RefreshSpeaker(line);
        RefreshBodyStyle(line.TextStyleId);
    }

    /// <summary>
    /// 이번 줄을 하단 대화창에 낼지 화면 가운데에 낼지 정합니다.
    ///
    /// 쓰지 않는 쪽의 글자를 반드시 비웁니다. 남겨 두면 가운데 문장이 뜬 채로 다음 줄이
    /// 대화창에 찍혀 두 문장이 동시에 보입니다.
    /// </summary>
    private void RefreshPlacement(StoryLineData line)
    {
        _isCenterPlacement = line.TextPlacement == EStoryTextPlacement.SCREEN_CENTER;

        _centerText.gameObject.SetActive(_isCenterPlacement);
        _bodyText.gameObject.SetActive(!_isCenterPlacement);

        _bodyText.text = string.Empty;
        _centerText.text = string.Empty;
    }

    /// <summary>
    /// 대사가 없는 줄은 배경만 넘기는 컷입니다(05·11화). 상자 프레임을 감춰 배경을 가리지 않게 합니다.
    /// </summary>
    private void RefreshBox(StoryLineData line)
    {
        // 화면 중앙 줄은 대화창을 보여 주지 않습니다. 다만 이 그림이 진행 버튼을 겸하므로
        // 오브젝트를 끄지 않고 투명도만 낮춥니다. 끄면 다음 줄로 넘어갈 수 없습니다.
        bool hideBox = _isCenterPlacement || string.IsNullOrEmpty(line.Text);

        Color color = _boxBackground.color;
        color.a = hideBox ? 0f : 1f;
        _boxBackground.color = color;
    }

    /// <summary>
    /// 본문 글꼴을 이번 줄의 스타일로 갈아 끼웁니다(연출표의 [디자인] 열).
    /// 글자를 찍는 도중에 크기가 바뀌면 이미 찍힌 글자까지 다시 배치되므로 타이핑 전에 끝냅니다.
    /// </summary>
    private void RefreshBodyStyle(string textStyleId)
    {
        StoryTextStyle style = _textStyleBinder.Get(textStyleId);
        style.ApplyTo(BodyText, _defaultBodyFont);

        // 떨지 않는 줄에서도 부릅니다. Play가 앞 줄의 떨림을 멈추고 자리를 되돌려 주기 때문입니다.
        _trembler.Play(BodyText.rectTransform, style.TrembleAmplitude, style.TrembleFrequency,
            this.GetCancellationTokenOnDestroy());
    }

    /// <summary>
    /// 화자명 박스를 갱신합니다.
    /// 지문(NARRATION)은 화자가 없으므로 박스를 통째로 끕니다(ELineType 주석).
    /// 독백(MONOLOGUE)은 화자명을 표시하되 대사와 구분되는 연출 대상이라, 지금은 이름만 같은 방식으로 냅니다.
    /// </summary>
    private void RefreshSpeaker(StoryLineData line)
    {
        string speakerName = StoryLineSpeaker.GetDisplayName(line);

        // 화면 중앙 줄은 대화창 자체를 감추므로 화자명도 함께 감춥니다.
        if (_isCenterPlacement || line.LineType == ELineType.NARRATION || string.IsNullOrEmpty(speakerName))
        {
            _speakerRoot.SetActive(false);
            return;
        }

        _speakerRoot.SetActive(true);
        _speakerText.text = speakerName;
    }
}
