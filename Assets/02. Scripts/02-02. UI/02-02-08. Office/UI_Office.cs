using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 사무실 화면입니다(기획서 SCREEN-006).
///
/// 스케줄·날짜 판정은 전부 GameProgressService에서 읽기만 하고, 이 화면은 하위 패널 조립과
/// 입력 배선만 맡습니다. 판정을 여기서 다시 짜면 홈 화면과 표시가 어긋납니다(기획서 16-6).
/// </summary>
public class UI_Office : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("Panels")]
    [SerializeField]
    private UI_OfficeTodayPanel _todayPanel;

    [SerializeField]
    private UI_OfficeWeeklyPanel _weeklyPanel;

    [SerializeField]
    private UI_CurrencyHud _currencyHud;

    [Foldout("Hierarchy")]
    [Header("Buttons")]
    [SerializeField]
    private Button _backButton;

    [Foldout("Hierarchy")]
    [Header("연출")]
    [SerializeField]
    private UI_OfficeDayFinishFlow _dayFinishFlow;

    private void Start()
    {
        // 스케줄 목록을 그리기 전에 자동 완료를 먼저 반영합니다(기획서 3-E-2-3).
        // 순서가 뒤바뀌면 이미 본 스토리의 스케줄이 미완료로 한 번 보였다가 다음 진입에야 완료로 바뀝니다.
        GameProgressService.Instance.SyncStorySchedules();

        // 배선을 갱신보다 먼저 합니다. 표시할 데이터나 참조 하나가 잘못돼 갱신이 중간에 끊겨도
        // 뒤로가기만은 살아 있어야 화면을 빠져나갈 수 있습니다. 사무실은 홈으로 돌아가는 경로가
        // 이 버튼뿐이라, 여기서 막히면 게임을 다시 켜는 수밖에 없습니다.
        InitButtons();
        RefreshScreen();
    }

    private void RefreshScreen()
    {
        _currencyHud.Refresh();

        _todayPanel.Init();
        _todayPanel.Refresh();

        _weeklyPanel.RefreshDays();
        _weeklyPanel.RefreshHighlight();
    }

    private void InitButtons()
    {
        _backButton.onClick.AddListener(MoveToSpace);
        _weeklyPanel.OnDayClicked = OnDayClicked;
    }

    /// <summary>
    /// 날짜 칸 클릭입니다. COMPLETABLE 칸만 interactable이므로 여기 도달하면 하루 마무리 요청입니다.
    /// 어느 날짜인지는 받지 않습니다. 마무리는 항상 현재 날짜에만 일어나고(DayProgressService),
    /// 다른 날짜는 애초에 클릭될 수 없기 때문입니다(기획서 3-E-3-1).
    /// </summary>
    private void OnDayClicked()
    {
        _dayFinishFlow.RunAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private void MoveToSpace()
    {
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogWarning("[UI_Office] SceneTransitionManager가 없어 스페이스로 이동하지 못했습니다.");
            return;
        }

        SceneTransitionManager.Instance.LoadSceneAsync(ESceneNames.SpaceScene, this.GetCancellationTokenOnDestroy()).Forget();
    }
}
