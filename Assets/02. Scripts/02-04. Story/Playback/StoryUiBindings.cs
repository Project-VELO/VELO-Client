using System;

/// <summary>
/// 감상 화면과 팝업이 재생 컨트롤러에게 보내는 통지를 걸고 걷습니다.
///
/// 컨트롤러에서 떼어낸 이유는 배선과 해제가 파일의 양 끝에 떨어져 있었기 때문입니다.
/// 무엇을 걸었는지는 위에서, 무엇을 걷는지는 OnDestroy에서 봐야 해 한쪽만 고치기 쉬웠습니다.
///
/// 남의 오브젝트에 건 배선은 스스로 걷습니다. 지금은 같은 씬에서 함께 파괴되어 문제가 드러나지
/// 않지만, 팝업이 화면보다 오래 사는 구조로 바뀌는 순간 조용히 누수가 됩니다.
/// </summary>
public class StoryUiBindings : IDisposable
{
    private readonly UI_Story _ui;

    public StoryUiBindings(UI_Story ui)
    {
        _ui = ui;
    }

    public void Bind(StoryVisualBinder visualBinder, Action onNext, Action onLog, Action onBack,
        Action onPopupClosed, Action onExitConfirmed)
    {
        _ui.OnNextRequested = onNext;
        _ui.OnLogRequested = onLog;
        _ui.OnBackRequested = onBack;

        _ui.LogPopup.Init(visualBinder);

        // 팝업이 닫혔다는 사실은 팝업만 알 수 있습니다. 통지를 받지 않으면 멈춘 상태에서 깨어나지 못합니다.
        _ui.LogPopup.OnClosed = onPopupClosed;
        _ui.ExitConfirmPopup.OnConfirmed = onExitConfirmed;
        _ui.ExitConfirmPopup.OnCancelled = onPopupClosed;
    }

    public void Dispose()
    {
        if (_ui == null)
        {
            return;
        }

        _ui.OnNextRequested = null;
        _ui.OnLogRequested = null;
        _ui.OnBackRequested = null;

        if (_ui.LogPopup != null)
        {
            _ui.LogPopup.OnClosed = null;
        }

        if (_ui.ExitConfirmPopup != null)
        {
            _ui.ExitConfirmPopup.OnConfirmed = null;
            _ui.ExitConfirmPopup.OnCancelled = null;
        }
    }
}
