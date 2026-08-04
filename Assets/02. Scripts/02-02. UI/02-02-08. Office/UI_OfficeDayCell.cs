using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 주간 스케줄 표의 날짜 한 칸입니다(기획서 3-E-3-1).
///
/// 네 가지 화면 상태(잠금/진행/마무리 가능/완료)의 시각 요소를 켜고 끄는 일만 합니다.
/// 상태 판정은 GetDayViewState가 하고, 현재 날짜 하이라이트 테두리는 표에서 분리된
/// UI_OfficeDayHighlight가 따로 그립니다 — 슬라이드 이동 때문에 칸이 소유할 수 없습니다.
/// </summary>
public class UI_OfficeDayCell : MonoBehaviour
{
    public Action OnClicked;

    /// <summary>
    /// 이 칸이 표시 중인 날짜입니다. 주간 패널이 하이라이트를 놓을 칸을 찾을 때 씁니다.
    /// </summary>
    public string DayId { get; private set; }

    public RectTransform CellRect => (RectTransform)transform;

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _button;

    [SerializeField]
    private TMP_Text _dayText;

    /// <summary>
    /// 아직 진행할 수 없는 날짜를 덮는 반투명 오버레이입니다(기획서 3-E-3-1의 4).
    /// </summary>
    [SerializeField]
    private GameObject _lockOverlay;

    /// <summary>
    /// 하루 마무리를 끝낸 날짜의 완료 체크입니다(기획서 3-E-3-1의 3).
    /// </summary>
    [SerializeField]
    private GameObject _completedRoot;

    /// <summary>
    /// "하루 마무리" 안내 표시입니다. 마무리 가능 상태에서만 켭니다(기획서 3-E-3-1의 2).
    /// </summary>
    [SerializeField]
    private GameObject _finishGuideRoot;

    private void Awake()
    {
        _button.onClick.AddListener(NotifyClicked);
    }

    public void SetDay(string dayId, int dayNumber)
    {
        DayId = dayId;
        gameObject.SetActive(true);
        _dayText.text = $"{dayNumber}일차";
    }

    /// <summary>
    /// interactable을 끄는 것으로 클릭 차단과 마우스 오버 강조 억제가 함께 해결됩니다.
    /// 클릭 가능한 것은 마무리 가능 상태의 현재 날짜뿐입니다(기획서 16-7).
    /// </summary>
    public void RefreshState(EDayViewState state)
    {
        _lockOverlay.SetActive(state == EDayViewState.LOCKED);
        _completedRoot.SetActive(state == EDayViewState.COMPLETED);
        _finishGuideRoot.SetActive(state == EDayViewState.COMPLETABLE);
        _button.interactable = state == EDayViewState.COMPLETABLE;
    }

    /// <summary>
    /// 표시할 날짜가 없는 남는 칸을 숨깁니다. 1차 MVP는 7일 고정이라 정상 데이터에서는 불리지 않습니다.
    /// </summary>
    public void Clear()
    {
        DayId = string.Empty;
        gameObject.SetActive(false);
    }

    private void NotifyClicked()
    {
        OnClicked?.Invoke();
    }
}
