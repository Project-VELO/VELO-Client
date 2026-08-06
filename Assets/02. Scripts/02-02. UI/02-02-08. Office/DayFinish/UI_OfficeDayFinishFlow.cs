using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

/// <summary>
/// 하루 마무리 전환 시퀀스입니다(기획서 3-E-3-1 "다음 날짜 전환 순서" 10단계).
///
/// 상태·저장 커밋(FinishDay)을 연출보다 먼저 수행합니다. 기획서 순서로는 연출이 상태 변경보다 앞이지만,
/// 16-7이 "연출 도중 게임이 종료되더라도 재실행 시 다음 날짜"를 요구하므로 커밋을 뒤로 미루면
/// 배너 도중 종료 시 하루가 되돌아갑니다. 보이는 순서는 화면 갱신 시점만 조절해 그대로 유지됩니다.
/// </summary>
public class UI_OfficeDayFinishFlow : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_OfficeTodayPanel _todayPanel;

    [SerializeField]
    private UI_OfficeWeeklyPanel _weeklyPanel;

    /// <summary>
    /// 화면 전체를 덮는 투명 레이캐스트 블로커입니다. 전환 중 날짜 재클릭·바로가기·뒤로가기·
    /// 마우스 오버를 버튼을 하나씩 찾아다니지 않고 한 번에 차단합니다(기획서 9.5).
    /// </summary>
    [SerializeField]
    private GameObject _inputBlocker;

    [SerializeField]
    private UI_OfficeDayFinishBanner _banner;

    /// <summary>
    /// 주차 완료 팝업입니다. 씬에 놓인 오브젝트라 씬 오버라이드로 배선합니다(화면 전용 팝업 원칙).
    /// </summary>
    [SerializeField]
    private UI_WeekCompletePopup _weekCompletePopup;

    /// <summary>
    /// 재진입 방지 1차 관문입니다. 연속 클릭이 와도 시퀀스는 한 번만 돕니다(기획서 16-7).
    /// DayProgressService의 완료 날짜 재마무리 차단이 2차 방어선입니다.
    /// </summary>
    private bool _isRunning;

    private UniTaskCompletionSource _popupClosedSource;

    public async UniTask RunAsync(CancellationToken cancellationToken)
    {
        if (_isRunning)
        {
            return;
        }

        _isRunning = true;
        _inputBlocker.SetActive(true);

        try
        {
            // 커밋 선행 — 이 줄이 끝나면 다음 날짜와 저장까지 확정입니다. 이후는 전부 시각 처리입니다.
            DayFinishResult result = GameProgressService.Instance.FinishDay();

            if (!result.IsSuccess)
            {
                return;
            }

            await _banner.PlayAsync(cancellationToken);

            // 이전 날짜 칸을 완료 표시로, 하이라이트 점멸은 정지. 위치는 아직 이전 칸입니다.
            _weeklyPanel.RefreshDays();
            _weeklyPanel.StopHighlightPulse();

            if (result.IsWeekFinished)
            {
                // 마지막 날짜는 슬라이드 없이 주차 마무리로 이어집니다(기획서 9.5).
                await WaitWeekCompletePopupAsync(result, cancellationToken);
                _weeklyPanel.RefreshHighlight();
                return;
            }

            await _weeklyPanel.SlideHighlightToCurrentAsync(cancellationToken);
            _weeklyPanel.RefreshHighlight();

            // 하이라이트 도착 후에 오늘의 스케줄을 다음 날짜 내용으로 교체합니다(기획서 9.5).
            await _todayPanel.SwapAsync(cancellationToken);
        }
        finally
        {
            if (_inputBlocker != null)
            {
                _inputBlocker.SetActive(false);
            }

            _isRunning = false;
        }
    }

    /// <summary>
    /// 주차 완료 팝업을 열고 닫힐 때까지 기다립니다. 해금된 스토리로의 강제 이동은 하지 않습니다(기획서 3-F-2).
    /// </summary>
    private async UniTask WaitWeekCompletePopupAsync(DayFinishResult result, CancellationToken cancellationToken)
    {
        _weekCompletePopup.SetResult(result.UnlockedStoryIds.Count);

        _popupClosedSource = new UniTaskCompletionSource();
        _weekCompletePopup.OnClosed = NotifyPopupClosed;

        UIManager.Instance.OpenPopup(_weekCompletePopup);

        try
        {
            await _popupClosedSource.Task.AttachExternalCancellation(cancellationToken);
        }
        finally
        {
            // 구독을 건 쪽에서 거둡니다. 팝업은 씬에 배치되어 재사용되므로 통지 뒤에도 구독이 남으면
            // 다음에 어떤 경로로 닫히든 이미 끝난 시퀀스의 콜백이 다시 불립니다.
            // 씬 언로드로 취소된 경우에도 같은 자리에서 정리되므로 경로마다 따로 지울 필요가 없습니다.
            if (_weekCompletePopup != null)
            {
                _weekCompletePopup.OnClosed = null;
            }

            _popupClosedSource = null;
        }
    }

    private void NotifyPopupClosed()
    {
        _popupClosedSource?.TrySetResult();
    }
}
