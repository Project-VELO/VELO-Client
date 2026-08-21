using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VInspector;

/// <summary>
/// 결과 화면의 루트입니다(SCREEN-010).
/// 진입과 동시에 보상 지급과 최고 기록 갱신을 마친 뒤 결과를 표시하고, 두 버튼의 이동만 처리합니다.
/// </summary>
public class UI_LiveResult : MonoBehaviour
{
    private const string CLEAR_LABEL = "CLEAR";
    private const string FAILED_LABEL = "FAILED";

    [Foldout("Hierarchy")]
    [Header("Summary")]
    [SerializeField]
    private TMP_Text _clearStatusText;

    [SerializeField]
    private TMP_Text _scoreText;

    [SerializeField]
    private TMP_Text _songTitleText;

    [Foldout("Hierarchy")]
    [Header("Sub UI Panels")]
    [SerializeField]
    private UI_RankIcon _rankIcon;

    [SerializeField]
    private UI_LiveResultJudgementPanel _judgementPanel;

    [SerializeField]
    private UI_LiveResultRewardPanel _rewardPanel;

    [SerializeField]
    private UI_LiveResultTags _tags;

    [Foldout("Hierarchy")]
    [Header("Buttons")]
    [SerializeField]
    private Button _retryButton;

    [SerializeField]
    private Button _confirmButton;

    private void Awake()
    {
        _retryButton.onClick.AddListener(RetryLive);
        _confirmButton.onClick.AddListener(ReturnToEntryScreen);
    }

    private void Start()
    {
        InitResult();
    }

    private void InitResult()
    {
        LiveResultData result = LiveResultContext.Instance.Result;

        if (ReferenceEquals(result, null))
        {
            Debug.LogError("[UI_LiveResult] 표시할 플레이 결과가 없습니다. 리듬게임 씬을 거쳐 진입했는지 확인해 주세요.");
            return;
        }

        // 보상은 결과 화면이 표시되는 순간 지급합니다(3-J-4). 표시 전에 끝내야 획득 수치를 그대로 그릴 수 있습니다.
        LiveResultProcessor.ApplyResult(result);

        RefreshResult(result);
    }

    private void RefreshResult(LiveResultData result)
    {
        SetText(_clearStatusText, result.IsClear ? CLEAR_LABEL : FAILED_LABEL);
        SetText(_scoreText, result.Score.ToString("N0"));
        SetText(_songTitleText, GetSongTitle(result.SongId));

        if (_rankIcon != null)
        {
            _rankIcon.RefreshRank(result.Rank);
        }

        if (_judgementPanel != null)
        {
            _judgementPanel.RefreshResult(result);
        }

        if (_rewardPanel != null)
        {
            _rewardPanel.RefreshReward(result);
        }

        if (_tags != null)
        {
            _tags.RefreshTags(result);
        }
    }

    /// <summary>
    /// 같은 곡·난이도로 다시 시작합니다. 진입 컨텍스트는 그대로 두고 이전 결과만 비웁니다(3-J-5).
    /// </summary>
    private void RetryLive()
    {
        LiveResultContext.Instance.Clear();
        LoadScene(ESceneNames.LiveScene);
    }

    /// <summary>
    /// 확인 버튼입니다. 이번 결과로 스케줄이 완료되었으면 사무실, 아니면 곡 선택 화면입니다(기획서 9.4, 3-J-6).
    /// 목적지를 결과에서 먼저 읽은 뒤에 컨텍스트를 비웁니다. 순서를 바꾸면 읽을 결과가 없습니다.
    /// </summary>
    private void ReturnToEntryScreen()
    {
        LiveResultData result = LiveResultContext.Instance.Result;
        bool hasCompletedSchedule = !ReferenceEquals(result, null) && result.HasCompletedSchedule;

        // 확인은 카드·의상·악세사리를 기본 편성으로 되돌리는 경로입니다(기획서 3-J-5).
        // 다시 플레이하기(RetryLive)는 컨텍스트를 남겨 직전 편성을 유지합니다.
        LiveLoadoutContext.Instance.Clear();

        LiveResultContext.Instance.Clear();
        LoadScene(LiveResultReturnTarget.GetReturnScene(hasCompletedSchedule));
    }

    private static string GetSongTitle(string songId)
    {
        return LiveSongCatalog.Instance.TryGetSong(songId, out SongData song) ? song.Title : songId;
    }

    private void LoadScene(ESceneNames sceneName)
    {
        LiveSceneNavigator.LoadScene(sceneName, this.GetCancellationTokenOnDestroy(), nameof(UI_LiveResult));
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text == null)
        {
            return;
        }

        text.text = value;
    }
}
