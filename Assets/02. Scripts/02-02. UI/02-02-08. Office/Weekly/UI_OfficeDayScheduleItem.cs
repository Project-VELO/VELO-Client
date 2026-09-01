using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 주간 스케줄 표의 한 칸 안에 적히는 스케줄 한 줄입니다(기획서 3-E-3-1).
///
/// 오늘의 스케줄 행(UI_ScheduleItemToday)과 달리 바로가기가 없습니다. 주간 표는 그 날 무엇을
/// 했는지 훑어보는 용도라 진입은 오늘 날짜에서만 열립니다.
/// </summary>
public class UI_OfficeDayScheduleItem : MonoBehaviour
{
    /// <summary>
    /// 이 말들 앞에서 줄을 끊습니다. 칸 너비가 90뿐이라 자동 줄바꿈에 맡기면
    /// "till the end 1회" / "클리어"처럼 뜻이 붙어 있어야 할 말이 둘로 쪼개집니다.
    ///
    /// 대상 이름(곡·화)과 완료 조건을 갈라 놓는 지점이라, 조건 문구가 늘면 여기에 더합니다.
    /// </summary>
    private static readonly string[] LINE_BREAK_MARKERS =
    {
        "감상",
        "1회 클리어",
        "연속 클리어",
        "→",
    };

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _titleText;

    [SerializeField]
    private Image _stateImage;

    [Foldout("Project")]
    [SerializeField]
    private Sprite _completedSprite;

    [SerializeField]
    private Sprite _notCompletedSprite;

    public void SetSchedule(string title, bool isCompleted)
    {
        gameObject.SetActive(true);

        _titleText.text = InsertLineBreaks(title);
        _stateImage.sprite = isCompleted ? _completedSprite : _notCompletedSprite;
    }

    /// <summary>
    /// 정해진 말 앞에서 줄을 끊습니다. 주간 표에서만 손보는 것은 오늘의 스케줄 행처럼
    /// 넉넉한 곳에서는 한 줄로 두는 편이 읽기 좋기 때문입니다.
    ///
    /// 마스터 데이터의 제목을 그대로 두고 표시할 때만 바꿉니다. 원본에 줄바꿈을 넣으면
    /// 같은 제목을 쓰는 다른 화면까지 함께 끊깁니다.
    ///
    /// 끊을 때마다 IndexOf를 다시 재는 것은 앞선 줄바꿈으로 뒤쪽 위치가 밀리기 때문입니다.
    /// 화면에 들어올 때와 날짜가 바뀔 때만 부르므로 문자열이 몇 번 더 생기는 것은 문제되지 않습니다.
    /// </summary>
    private static string InsertLineBreaks(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return title;
        }

        for (int i = 0; i < LINE_BREAK_MARKERS.Length; i++)
        {
            int index = title.IndexOf(LINE_BREAK_MARKERS[i]);

            // 제목에 없거나(-1) 그 말로 시작하면(0) 앞에서 끊을 것이 없습니다.
            if (index <= 0)
            {
                continue;
            }

            title = title.Substring(0, index).TrimEnd() + "\n" + title.Substring(index);
        }

        return title;
    }

    /// <summary>
    /// 스케줄이 없는 남는 줄을 숨깁니다. 하루가 3개 고정이라 정상 데이터에서는 불리지 않습니다.
    /// </summary>
    public void Clear()
    {
        gameObject.SetActive(false);
    }
}
