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
    /// 문구는 홈 화면과 같아야 하므로 ScheduleDayLabel이 만듭니다.
    /// </summary>
    private void RefreshHeader()
    {
        // 날짜는 주간 표가 칸마다 보여 주므로 이 패널에서는 비워 둘 수 있습니다.
        if (_dateText == null || !_dateText.gameObject.activeInHierarchy)
        {
            return;
        }

        int weekOrder = GameProgressService.Instance.GetCurrentWeekOrder();
        int dayNumber = GameProgressService.Instance.GetCurrentDayNumber();

        _dateText.text = ScheduleDayLabel.Format(weekOrder, dayNumber);
    }

    /// <summary>
    /// KillAndCancelAwait는 취소 시 트윈을 끊고 await도 함께 취소시킵니다.
    /// 기본값 Kill은 취소를 정상 완료로 처리하므로, 씬 언로드 중에도 SwapAsync가 다음 줄로 진행됩니다.
    /// </summary>
    private async UniTask FadeAsync(float from, float to, CancellationToken cancellationToken)
    {
        _canvasGroup.alpha = from;

        await _canvasGroup.DOFade(to, _swapFadeDuration)
            .SetUpdate(true)
            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
    }
}
