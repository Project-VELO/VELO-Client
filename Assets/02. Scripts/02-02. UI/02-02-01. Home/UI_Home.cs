using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 홈 화면입니다(기획서 SCREEN-001).
/// </summary>
public class UI_Home : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("활성 버튼")]
    [SerializeField]
    private Button _liveButton;

    [SerializeField]
    private Button _spaceButton;

    [SerializeField]
    private Button _enterStoryButton;

    [Foldout("Hierarchy")]
    [Header("Panels")]
    [SerializeField]
    private UI_CurrencyHud _currencyHud;

    [SerializeField]
    private UI_HomeSchedulePanel _schedulePanel;

    private void Start()
    {
        // 스케줄 목록을 그리기 전에 자동 완료를 먼저 반영합니다.
        // 순서가 뒤바뀌면 이미 본 스토리의 스케줄이 미완료로 한 번 보였다가 다음 진입에야 완료로 바뀝니다.
        GameProgressService.Instance.SyncStorySchedules();

        RefreshScreen();
        InitButtons();
    }

    private void RefreshScreen()
    {
        if (_currencyHud != null)
        {
            _currencyHud.Refresh();
        }

        if (_schedulePanel != null)
        {
            _schedulePanel.Init();
            _schedulePanel.Refresh();
        }
    }

    private void InitButtons()
    {
        if (_liveButton != null)
        {
            _liveButton.onClick.AddListener(StartFreeLive);
        }

        if (_spaceButton != null)
        {
            _spaceButton.onClick.AddListener(() => LoadScene(ESceneNames.SpaceScene));
        }

        if (_enterStoryButton != null)
        {
            _enterStoryButton.onClick.AddListener(MoveToStoryList);
        }

        // 상점·도감·프로필 등 비활성 대상은 클릭 동작을 등록하지 않는 것으로 막습니다.
        // 씬에 남아 있던 배선(01_HomeScene의 P_UI_Home 인스턴스 override)을 제거해야 실제로 막힙니다.
        // 기획서 4.6이 요구하는 "어둡게 표시"는 되살릴 때 UI_DisabledButton을 붙이면 됩니다.
    }

    /// <summary>
    /// 일반 LIVE 버튼입니다. 지정곡 없이 진입하므로 곡 선택 화면에서 첫 곡이 자동 선택되고
    /// 다른 곡으로 바꿀 수 있습니다.
    /// </summary>
    private void StartFreeLive()
    {
        LiveEntryStarter.StartLive(EEntryType.HOME_LIVE, cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    /// <summary>
    /// 스토리 버튼입니다. 스케줄 바로가기로 들어온 것이 아니므로 강조 대상을 지웁니다.
    /// 지우지 않으면 이전에 스케줄로 진입했을 때의 회차가 계속 강조됩니다.
    /// </summary>
    private void MoveToStoryList()
    {
        StoryEntryContext.Instance.ClearHighlight();
        LoadScene(ESceneNames.SelectStoryScene);
    }

    private void LoadScene(ESceneNames sceneName)
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneAsync(sceneName, this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}
