using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스케줄 행 왼쪽의 상태 아이콘입니다. 잠김·수행 가능·완료 세 상태의 스프라이트를 바꿔 끼웁니다.
/// 홈 화면과 내일 예고가 씁니다. 사무실은 유형 아이콘(UI_ScheduleTypeIcon)을 대신 붙입니다.
///
/// 오브젝트를 껐다 켜지 않고 스프라이트만 교체합니다. 상태에 따라 아이콘이 사라지면 행마다
/// 제목이 시작되는 위치가 달라져 목록이 들쭉날쭉해집니다.
///
/// 스프라이트는 아직 없어 비어 있어도 됩니다. 비면 Image가 단색 사각형으로 남아 자리만 지키므로,
/// 아트가 들어오기 전까지 세 상태가 화면에서는 같아 보입니다. 상태 전환 자체는 그대로 동작합니다.
/// </summary>
public class UI_ScheduleStateIcon : UI_ScheduleIconBase
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _icon;

    [Foldout("Project")]
    [SerializeField]
    private Sprite _lockedSprite;

    [SerializeField]
    private Sprite _availableSprite;

    [SerializeField]
    private Sprite _completedSprite;

    /// <summary>
    /// 오늘의 행에는 잠김이 없습니다. 세 스케줄 모두 처음부터 진행할 수 있어(기획서 3-E-2-2)
    /// 완료 여부만 상태로 옮기면 충분합니다.
    /// </summary>
    public override void Refresh(EScheduleType scheduleType, bool isCompleted)
    {
        SetState(isCompleted ? EScheduleViewState.COMPLETED : EScheduleViewState.AVAILABLE);
    }

    /// <summary>
    /// 내일 예고처럼 완료 여부가 아니라 잠김을 직접 지정해야 하는 곳이 씁니다.
    /// </summary>
    public void SetState(EScheduleViewState state)
    {
        _icon.sprite = GetSprite(state);
    }

    private Sprite GetSprite(EScheduleViewState state)
    {
        switch (state)
        {
            case EScheduleViewState.LOCKED: return _lockedSprite;
            case EScheduleViewState.COMPLETED: return _completedSprite;
            default: return _availableSprite;
        }
    }
}
