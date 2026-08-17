using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스케줄 유형(LIVE·STORY)을 나타내는 아이콘입니다. 사무실의 오늘의 스케줄 행이 씁니다.
///
/// 완료 여부는 이 아이콘이 표현하지 않습니다. 완료된 행도 무엇을 하는 스케줄이었는지 남아야 해서,
/// 완료 표시는 바로가기 버튼(UI_ScheduleShortcutButton)이 배경 교체로 맡습니다.
/// </summary>
public class UI_ScheduleTypeIcon : UI_ScheduleIconBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _icon;

    [Foldout("Project")]
    [SerializeField]
    private Sprite _liveSprite;

    [SerializeField]
    private Sprite _storySprite;

    public override void Refresh(EScheduleType scheduleType, bool isCompleted)
    {
        _icon.sprite = GetSprite(scheduleType);
    }

    /// <summary>
    /// SYSTEM은 하루 마무리 같은 내부 처리용이라 목록에 오르지 않습니다. 그래도 들어왔다면
    /// 아이콘을 비워 빈 사각형을 남기는 대신 경고를 남겨 데이터 쪽 문제를 드러냅니다.
    /// </summary>
    private Sprite GetSprite(EScheduleType scheduleType)
    {
        switch (scheduleType)
        {
            case EScheduleType.LIVE: return _liveSprite;
            case EScheduleType.STORY: return _storySprite;
            default:
                Debug.LogWarning($"[UI_ScheduleTypeIcon] 아이콘 규칙이 없는 스케줄 유형입니다: {scheduleType}");
                return _liveSprite;
        }
    }
}
