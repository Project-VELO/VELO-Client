/// <summary>
/// 입력 모드(플레이어/UI)와 입력 차단 플래그를 관리합니다.
///
/// static 클래스가 아닌 싱글톤 인스턴스로 둔 것은 상태를 초기화할 수단이 필요하기 때문입니다.
/// 팝업이 걸어 둔 UI 모드가 씬 언로드로 풀리지 못한 채 남으면 다음 씬이 입력을 받지 못하므로,
/// SceneTransitionManager가 전환 시점마다 Reset()으로 되돌립니다.
/// </summary>
public class InputHandler : POCOSingleton<InputHandler>
{
    public enum EInputMode
    {
        Player,
        UI
    }

    public EInputMode CurrentMode { get; private set; } = EInputMode.Player;
    public bool IsInputBlocked { get; private set; }

    public void BlockInput()
    {
        IsInputBlocked = true;
    }

    public void UnblockInput()
    {
        IsInputBlocked = false;
    }

    public void ChangeToUIInput()
    {
        CurrentMode = EInputMode.UI;
        BlockInput();
    }

    public void ChangeToPlayerInput()
    {
        CurrentMode = EInputMode.Player;
        UnblockInput();
    }

    /// <summary>
    /// 이전 화면이 남긴 입력 상태가 다음 씬으로 새지 않도록 처음 상태로 되돌립니다.
    /// </summary>
    public void Reset()
    {
        CurrentMode = EInputMode.Player;
        IsInputBlocked = false;
    }
}
