using System;

public static class InputHandler
{
    public static Action OnCancelEvent;
    public static Action OnNavigateEvent;
    public static Action OnPointEvent;

    public enum EInputMode
    {
        Player,
        UI
    }

    public static EInputMode CurrentMode { get; private set; } = EInputMode.Player;
    public static bool IsInputBlocked { get; private set; }

    public static void BlockInput()
    {
        IsInputBlocked = true;
    }

    public static void UnblockInput()
    {
        IsInputBlocked = false;
    }

    public static void ChangeToUIInput()
    {
        CurrentMode = EInputMode.UI;
        BlockInput();
    }

    public static void ChangeToPlayerInput()
    {
        CurrentMode = EInputMode.Player;
        UnblockInput();
    }

    public static void TriggerCancelEvent() => OnCancelEvent?.Invoke();
    public static void TriggerNavigateEvent() => OnNavigateEvent?.Invoke();
    public static void TriggerPointEvent() => OnPointEvent?.Invoke();
}
