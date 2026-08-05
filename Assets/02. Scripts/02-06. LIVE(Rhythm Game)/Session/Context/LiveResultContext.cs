/// <summary>
/// 방금 끝난 플레이 결과를 결과 화면으로 옮기는 싱글톤입니다.
/// LiveEntryContext와 같은 이유로 MonoBehaviour가 아니며, 리듬게임 씬이 언로드되어도 값이 유지됩니다.
/// </summary>
public class LiveResultContext : POCOSingleton<LiveResultContext>
{
    public LiveResultData Result { get; private set; }

    public bool HasResult => !ReferenceEquals(Result, null);

    public void SetResult(LiveResultData result)
    {
        Result = result;
    }

    public void Clear()
    {
        Result = null;
    }
}
