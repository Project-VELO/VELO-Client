using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 사무실의 "오늘의 스케줄" 영역입니다(기획서 9.2).
///
/// 스케줄 행 자체는 홈과 공유하는 UI_TodayScheduleList가 그리고,
/// 이 클래스는 주차·날짜 헤더와 날짜 전환 시의 페이드 교체만 더합니다.
/// </summary>
public class UI_OfficeTodayPanel : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _dateText;

    [SerializeField]
    private UI_TodayScheduleList _scheduleList;

    /// <summary>
    /// 페이드 교체용입니다. 기존 내용과 다음 내용이 겹쳐 보이면 안 되므로(기획서 16-7)
    /// 완전히 사라진 뒤에 내용을 바꾸고 다시 나타냅니다.
    /// </summary>
    [SerializeField]
    private CanvasGroup _canvasGroup;

    [Foldout("Settings")]
    [SerializeField]
    [Range(0.05f, 1f)]
    private float _swapFadeDuration = 0.25f;

    public void Init()
    {
        // 사무실의 LIVE 바로가기는 SCHEDULE_LIVE로 진입합니다. 결과·뒤로가기의 복귀 표가 이 값을 봅니다.
        _scheduleList.Init(EEntryType.SCHEDULE_LIVE);
    }

    public void Refresh()
    {
        RefreshHeader();
        _scheduleList.RefreshSchedules();
    }

    /// <summary>
    /// 하루 마무리 후 다음 날짜의 내용으로 교체합니다. 페이드 아웃이 끝난 뒤에 갱신하므로
    /// CurrentDayId가 이미 다음 날짜로 바뀐 상태에서 불러야 합니다.
    /// </summary>
    public async UniTask SwapAsync(CancellationToken cancellationToken)
    {
        try
        {
            await FadeAsync(1f, 0f, cancellationToken);
            Refresh();
            await FadeAsync(0f, 1f, cancellationToken);
        }
        finally
        {
            // 취소돼도 패널이 투명한 채 남지 않게 합니다. 파괴 중이면 건드리지 않습니다.
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }
    }

    /// <summary>
    /// 현재 주차·날짜 표시입니다(기획서 16-6 "현재 주차와 현재 날짜가 표시된다").
    /// 주차 번호는 마스터 데이터의 WeekData가 따로 없어 오늘 스케줄의 WeekOrder에서 얻고,
    /// 일차 번호는 DayOrder 정렬 목록의 위치에서 얻습니다(기획 확정 전 잠정 표기).
    /// </summary>
    private void RefreshHeader()
    {
        List<string> dayIds = GameProgressService.Instance.GetCurrentWeekDayIds();
        int dayIndex = dayIds.IndexOf(PlayerDataProvider.Instance.Data.CurrentDayId);

        if (dayIndex < 0)
        {
            Debug.LogWarning("[UI_OfficeTodayPanel] 현재 날짜를 주차 목록에서 찾지 못해 헤더를 비웁니다.");
            _dateText.text = string.Empty;
            return;
        }

        List<ScheduleData> schedules = GameProgressService.Instance.GetTodaySchedules();
        int weekOrder = schedules.Count > 0 ? schedules[0].WeekOrder : 1;

        _dateText.text = $"{weekOrder}주차 {dayIndex + 1}일차";
    }

    private async UniTask FadeAsync(float from, float to, CancellationToken cancellationToken)
    {
        _canvasGroup.alpha = from;

        Tween tween = _canvasGroup.DOFade(to, _swapFadeDuration).SetUpdate(true);

        try
        {
            await UniTask.WaitUntil(() => !tween.IsActive() || !tween.IsPlaying(), cancellationToken: cancellationToken);
        }
        finally
        {
            if (tween.IsActive())
            {
                tween.Kill();
            }
        }
    }
}
