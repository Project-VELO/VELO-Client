/// <summary>
/// 채보 편집 작업의 실행/취소를 위한 커맨드 패턴 인터페이스입니다.
/// </summary>
public interface ICommand
{
    void Execute();
    void Undo();
}
