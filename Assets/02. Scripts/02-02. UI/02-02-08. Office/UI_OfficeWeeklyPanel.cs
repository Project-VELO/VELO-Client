using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 주간 스케줄 표입니다(기획서 9.2 "7일의 잠금·현재·완료 상태 표시").
///
/// 칸은 프리팹에 미리 배치된 7개를 씁니다. 1차 MVP의 주차는 7일 고정이라(기획서 3-E-1)
/// 풀에서 꺼낼 이유가 없고, 고정 배치가 레이아웃도 안정적입니다(UI_MusicSelectChapterTabs와 같은 방식).
/// </summary>
public class UI_OfficeWeeklyPanel : MonoBehaviour
{
    public Action OnDayClicked;

    [Foldout("Hierarchy")]
    [SerializeField]
    private List<UI_OfficeDayCell> _dayCells = new List<UI_OfficeDayCell>();

    [SerializeField]
    private UI_OfficeDayHighlight _highlight;

    /// <summary>
    /// 칸들을 배치하는 레이아웃 루트입니다. 하이라이트를 놓기 전에 레이아웃을 확정해야
    /// 칸 위치가 0이 아닌 실제 값으로 잡힙니다(레이아웃 그룹은 같은 프레임에 크기를 확정하지 않습니다).
    /// </summary>
    [SerializeField]
    private RectTransform _layoutRoot;

    private void Awake()
    {
        for (int i = 0; i < _dayCells.Count; i++)
        {
            _dayCells[i].OnClicked = NotifyDayClicked;
        }
    }

    /// <summary>
    /// 7칸의 날짜와 상태를 다시 읽어 그립니다. 하이라이트 위치는 건드리지 않습니다 —
    /// 하루 마무리 연출이 슬라이드 도중에 상태만 먼저 갱신해야 하기 때문입니다.
    /// </summary>
    public void RefreshDays()
    {
        List<string> dayIds = GameProgressService.Instance.GetCurrentWeekDayIds();
        string weekId = PlayerDataProvider.Instance.Data.CurrentWeekId;

        for (int i = 0; i < _dayCells.Count; i++)
        {
            if (i >= dayIds.Count)
            {
                _dayCells[i].Clear();
                continue;
            }

            _dayCells[i].SetDay(dayIds[i], i + 1);
            _dayCells[i].RefreshState(GameProgressService.Instance.GetDayViewState(weekId, dayIds[i]));
        }

        if (dayIds.Count > _dayCells.Count)
        {
            Debug.LogWarning($"[UI_OfficeWeeklyPanel] 표시할 칸이 모자랍니다. 날짜 {dayIds.Count}건 / 칸 {_dayCells.Count}개");
        }
    }

    /// <summary>
    /// 하이라이트를 현재 날짜 칸 위로 즉시 배치합니다. 마무리 가능 상태면 점멸을 켭니다.
    /// 현재 날짜가 완료 상태(주차 마무리 직후)면 하이라이트를 숨깁니다(기획서 3-E-3-1의 3).
    /// </summary>
    public void RefreshHighlight()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutRoot);

        UI_OfficeDayCell currentCell = FindCell(PlayerDataProvider.Instance.Data.CurrentDayId);

        if (currentCell == null)
        {
            _highlight.Hide();
            return;
        }

        EDayViewState state = GameProgressService.Instance.GetDayViewState(
            PlayerDataProvider.Instance.Data.CurrentWeekId, currentCell.DayId);

        if (state == EDayViewState.COMPLETED)
        {
            _highlight.Hide();
            return;
        }

        _highlight.SnapTo(currentCell.CellRect);
        _highlight.SetPulse(state == EDayViewState.COMPLETABLE);
    }

    /// <summary>
    /// 하루 마무리 연출용 — 하이라이트를 현재 날짜 칸까지 슬라이드합니다.
    /// FinishDay가 끝난 뒤 불러야 CurrentDayId가 다음 날짜를 가리킵니다.
    /// </summary>
    public async UniTask SlideHighlightToCurrentAsync(CancellationToken cancellationToken)
    {
        UI_OfficeDayCell currentCell = FindCell(PlayerDataProvider.Instance.Data.CurrentDayId);

        if (currentCell == null)
        {
            Debug.LogWarning("[UI_OfficeWeeklyPanel] 이동할 현재 날짜 칸을 찾지 못했습니다.");
            return;
        }

        await _highlight.SlideToAsync(currentCell.CellRect, cancellationToken);
    }

    public void StopHighlightPulse()
    {
        _highlight.SetPulse(false);
    }

    private UI_OfficeDayCell FindCell(string dayId)
    {
        for (int i = 0; i < _dayCells.Count; i++)
        {
            if (_dayCells[i].DayId == dayId)
            {
                return _dayCells[i];
            }
        }

        return null;
    }

    private void NotifyDayClicked()
    {
        OnDayClicked?.Invoke();
    }
}
