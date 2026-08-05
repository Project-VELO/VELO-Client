using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스케줄 행 왼쪽의 상태 아이콘입니다. 잠김·수행 가능·완료 세 상태의 스프라이트를 바꿔 끼웁니다.
///
/// 오브젝트를 껐다 켜지 않고 스프라이트만 교체합니다. 상태에 따라 아이콘이 사라지면 행마다
/// 제목이 시작되는 위치가 달라져 목록이 들쭉날쭉해집니다.
///
/// 스프라이트는 아직 없어 비어 있어도 됩니다. 비면 Image가 단색 사각형으로 남아 자리만 지키므로,
/// 아트가 들어오기 전까지 세 상태가 화면에서는 같아 보입니다. 상태 전환 자체는 그대로 동작합니다.
/// </summary>
public class UI_ScheduleStateIcon : MonoBehaviour
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
