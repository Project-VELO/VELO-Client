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

        _titleText.text = title;
        _stateImage.sprite = isCompleted ? _completedSprite : _notCompletedSprite;
    }

    /// <summary>
    /// 스케줄이 없는 남는 줄을 숨깁니다. 하루가 3개 고정이라 정상 데이터에서는 불리지 않습니다.
    /// </summary>
    public void Clear()
    {
        gameObject.SetActive(false);
    }
}
