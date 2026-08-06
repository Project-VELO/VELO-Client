using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UI_PopupHandler
{
    /// <summary>
    /// 팝업 하나와, 그 팝업을 맨 앞으로 끌어올리기 전의 형제 순서를 함께 기억합니다.
    /// 스택이 LIFO로 풀리므로 닫을 때 기억한 순서를 그대로 되돌리면 열기 전 계층으로 복원됩니다.
    /// </summary>
    private readonly struct PopupEntry
    {
        public readonly UI_Popup Popup;
        public readonly int SiblingIndex;

        public PopupEntry(UI_Popup popup, int siblingIndex)
        {
            Popup = popup;
            SiblingIndex = siblingIndex;
        }
    }

    private Stack<PopupEntry> _popups = new Stack<PopupEntry>();
    private bool _isClosing = false;

    public bool HasPopups => 0 < _popups.Count;

    public void ClearAllPopups()
    {
        while (0 < _popups.Count)
        {
            PopupEntry entry = _popups.Pop();
            if (entry.Popup != null)
            {
                entry.Popup.OnCloseRequested = null;
                entry.Popup.gameObject.SetActive(false);
                RestoreSiblingIndex(entry);
            }
        }
        _isClosing = false;
        InputHandler.Instance.ChangeToPlayerInput();
    }

    public void OpenPopup(UI_Popup popup)
    {
        if (popup.gameObject.activeSelf)
        {
            return;
        }

        SetTopPopupFocusState(false);

        // 스택 맨 위에 올라간 팝업이 화면에서도 맨 위에 그려지도록 형제 순서를 맨 뒤로 옮깁니다.
        // Canvas는 형제 순서대로 그리고 레이캐스트도 그 순서를 따르므로, 이 처리가 없으면 씬에 먼저
        // 배치된 팝업은 나중에 열려도 이미 열려 있는 팝업의 전체화면 딤에 가려져 보이지도 눌리지도 않습니다.
        var entry = new PopupEntry(popup, popup.transform.GetSiblingIndex());
        popup.transform.SetAsLastSibling();

        // 팝업이 UIManager를 역참조하지 않도록, 닫기 요청 경로를 여는 쪽에서 주입합니다.
        // 닫힐 때 도로 비우므로, 콜백이 남아 있다는 것은 곧 이 핸들러가 열어 둔 팝업이라는 뜻입니다.
        popup.OnCloseRequested = ClosePopup;

        _popups.Push(entry);
        popup.OpenAsync().Forget();

        InputHandler.Instance.ChangeToUIInput();
        SetPopupFocusState(popup, true);
    }

    public void ClosePopup(UI_Popup popup)
    {
        if (_popups.TryPeek(out PopupEntry latestEntry) && latestEntry.Popup == popup)
        {
            CloseLatestPopup();
        }
    }

    public void CloseLatestPopup()
    {
        if (_isClosing) return;
        CloseLatestPopupAsync().Forget();
    }

    private async UniTaskVoid CloseLatestPopupAsync()
    {
        if (_popups.TryPop(out PopupEntry latestEntry))
        {
            _isClosing = true;
            latestEntry.Popup.OnCloseRequested = null;
            SetPopupFocusState(latestEntry.Popup, false);

            try
            {
                await latestEntry.Popup.CloseAsync();
            }
            finally
            {
                _isClosing = false;
            }

            // 닫는 연출이 끝난 뒤에 되돌려야, 사라지는 동안에도 맨 위에 그려집니다.
            RestoreSiblingIndex(latestEntry);

            if (_popups.Count <= 0)
            {
                InputHandler.Instance.ChangeToPlayerInput();
                return;
            }

            SetTopPopupFocusState(true);
        }
    }

    /// <summary>
    /// 팝업을 열기 전 형제 순서로 되돌립니다.
    /// 팝업이 열려 있는 동안 형제가 늘거나 줄었을 수 있으므로, 범위를 넘어서는 값은 SetSiblingIndex의 클램프에 맡깁니다.
    /// </summary>
    private static void RestoreSiblingIndex(PopupEntry entry)
    {
        if (entry.Popup == null)
        {
            return;
        }

        entry.Popup.transform.SetSiblingIndex(entry.SiblingIndex);
    }

    private void SetTopPopupFocusState(bool state)
    {
        if (_popups.TryPeek(out PopupEntry topEntry))
        {
            SetPopupFocusState(topEntry.Popup, state);
        }
    }

    private void SetPopupFocusState(UI_Popup popup, bool state)
    {
    }
}
