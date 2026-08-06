using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 공용 오류 안내 팝업(SCREEN-011)의 표시 순서를 맡습니다. 곡 데이터 없음·스토리 대사 없음·저장 실패가
/// 모두 팝업 하나를 쓰므로, 안내가 떠 있는 동안 들어온 다음 오류를 큐에 쌓아 차례로 보여 줍니다.
/// UIManager가 팝업 위임 창구 역할과 오류 순번 관리라는 두 책임을 겸하지 않도록 분리했습니다.
/// </summary>
public class PendingErrorPresenter
{
    private readonly UI_ErrorPopup _errorPopup;
    private readonly Action<UI_Popup> _openPopup;
    private readonly CancellationToken _lifetimeToken;

    /// <summary>
    /// 안내가 떠 있는 동안 들어온 다음 오류들입니다. 팝업이 하나뿐이라 차례로 보여 줍니다.
    /// </summary>
    private readonly Queue<(string Message, Action OnConfirmed)> _pendingErrors = new Queue<(string, Action)>();

    public PendingErrorPresenter(UI_ErrorPopup errorPopup, Action<UI_Popup> openPopup, CancellationToken lifetimeToken)
    {
        _errorPopup = errorPopup;
        _openPopup = openPopup;
        _lifetimeToken = lifetimeToken;

        if (_errorPopup != null)
        {
            _errorPopup.OnClosed = ShowNextPendingError;
        }
    }

    /// <summary>
    /// 오류를 안내합니다(기획서 3-L). onConfirmed는 안내를 닫은 뒤에 이어서 할 일이며,
    /// "오류 팝업 후 스토리 목록 복귀"처럼 복귀가 뒤따르는 경우에 넘깁니다.
    ///
    /// 팝업이 없으면(PersistentScene 없이 단독 실행) 안내를 건너뛰더라도 뒤처리는 그대로 진행합니다.
    /// 그러지 않으면 대본을 읽지 못한 감상 화면에 갇힙니다.
    /// </summary>
    public void OpenErrorPopup(string message, Action onConfirmed = null)
    {
        if (_errorPopup == null)
        {
            Debug.LogWarning($"[PendingErrorPresenter] 오류 안내 팝업이 없어 문구를 표시하지 못했습니다: {message}");
            onConfirmed?.Invoke();
            return;
        }

        // 이미 떠 있는 안내를 덮어쓰지 않고 차례를 기다립니다. 덮어쓰면 문구만 바뀌는 것이 아니라
        // 앞선 요청의 콜백까지 사라져, "오류 팝업 후 스토리 목록 복귀"처럼 닫기에 매인 뒤처리가 끊깁니다.
        // (감상 화면에서 대본을 읽지 못한 안내가 떠 있는 동안 저장 실패가 겹치는 경우)
        if (_errorPopup.gameObject.activeSelf)
        {
            _pendingErrors.Enqueue((message, onConfirmed));
            return;
        }

        ShowError(message, onConfirmed);
    }

    private void ShowError(string message, Action onConfirmed)
    {
        _errorPopup.SetError(message, onConfirmed);
        _openPopup(_errorPopup);
    }

    /// <summary>
    /// 밀려 있던 다음 안내를 엽니다. 이 통지는 UI_ErrorPopup.CloseAsync 안에서 오므로,
    /// 같은 프레임에 다시 열면 아직 끝나지 않은 닫기 뒤처리(UI_PopupHandler의 형제 순서 복원)가
    /// 새로 연 팝업을 도로 뒤로 밀어냅니다. 그래서 한 프레임을 기다린 뒤 엽니다.
    /// </summary>
    private void ShowNextPendingError()
    {
        if (_pendingErrors.Count == 0)
        {
            return;
        }

        ShowNextPendingErrorAsync().Forget();
    }

    private async UniTaskVoid ShowNextPendingErrorAsync()
    {
        await UniTask.NextFrame(_lifetimeToken);

        if (!_pendingErrors.TryDequeue(out (string Message, Action OnConfirmed) request))
        {
            return;
        }

        ShowError(request.Message, request.OnConfirmed);
    }
}
