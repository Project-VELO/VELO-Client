using UnityEngine;

/// <summary>
/// 스케줄 행 왼쪽 아이콘의 공통 계약입니다.
///
/// 홈은 진행 상태(잠김·완료)를, 사무실은 스케줄 유형(LIVE·STORY)을 보여 줍니다. 규칙이 화면마다
/// 다르므로 한 클래스에 담지 않고 구현을 나누되, 행 클래스(UI_ScheduleItemToday)는 어느 쪽이
/// 붙었는지 모른 채 같은 호출만 하도록 이 추상 클래스로 묶습니다.
///
/// 인터페이스가 아니라 MonoBehaviour 추상 클래스인 것은 인스펙터가 인터페이스 필드를
/// 직렬화하지 못하기 때문입니다.
/// </summary>
public abstract class UI_ScheduleIconBase : MonoBehaviour
{
    /// <summary>
    /// 행이 표시 중인 스케줄에 맞춰 아이콘을 갱신합니다.
    /// 구현은 두 인자 중 자기에게 필요한 것만 씁니다.
    /// </summary>
    public abstract void Refresh(EScheduleType scheduleType, bool isCompleted);
}
