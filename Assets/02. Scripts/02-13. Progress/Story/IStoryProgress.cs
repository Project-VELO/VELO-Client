/// <summary>
/// 화면이 스토리 진행 상태를 읽을 때 쓰는 읽기 전용 창구입니다.
///
/// StoryProgress를 그대로 넘기면 화면에서 값을 직접 바꿀 수 있고, 그러면 저장이 함께 이루어지지 않아
/// 게임을 다시 켰을 때 되돌아갑니다. 상태 변경은 GameProgressService를 통해야 합니다.
///
/// 방어적 복사 대신 인터페이스를 쓰는 것은 목록의 행마다 조회가 일어나기 때문입니다.
/// 복사본을 돌려주면 스크롤할 때마다 GC 할당이 생깁니다.
/// </summary>
public interface IStoryProgress
{
    EStoryStatus Status { get; }

    bool IsNew { get; }

    bool IsCompleted { get; }

    bool CanEnter { get; }
}
