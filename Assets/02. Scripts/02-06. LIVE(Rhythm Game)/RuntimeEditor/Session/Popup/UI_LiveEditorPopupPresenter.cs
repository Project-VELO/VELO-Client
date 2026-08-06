using UnityEngine;

/// <summary>
/// UIManager의 팝업 스택을 다루는 호출을 한곳에 모아, 에디터 쪽 클래스들이 UIManager 유무를 매번 확인하지 않게 합니다.
///
/// 자체 상태가 전혀 없고 UIManager에 위임만 하므로 컴포넌트일 이유가 없습니다.
/// 팝업을 여는 클래스마다 각자 new로 만들어 쓰며, 어느 인스턴스로 호출하든 결과는 같습니다.
/// </summary>
public class UI_LiveEditorPopupPresenter
{
    public bool HasPopups => UIManager.Instance != null && UIManager.Instance.HasPopups;

    public void Open(UI_Popup popup)
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("[UI_LiveEditorPopupPresenter] 씬에 UIManager가 없어 팝업을 열 수 없습니다.");
            return;
        }

        UIManager.Instance.OpenPopup(popup);
    }

    public void CloseLatest()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.CloseLatestPopup();
    }

    /// <summary>
    /// 편집 화면으로 넘어갈 때처럼 남은 팝업을 한 번에 정리해야 하는 경우에 사용합니다.
    /// 팝업 스택이 비면 InputHandler의 입력 차단도 함께 풀립니다.
    /// </summary>
    public void CloseAll()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.ClearAllPopups();
    }
}
