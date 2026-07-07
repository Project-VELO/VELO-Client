using System;
using UnityEngine;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    public PopupHandler PopupHandler { get; } = new PopupHandler();

    public void FadeInLoadingPanel(Action callback)
    {
        callback?.Invoke();
    }

    public void FadeOutLoadingPanel(Action callback)
    {
        callback?.Invoke();
    }
}

public class PopupHandler
{
    public void ClearAllPopups()
    {
    }
}
