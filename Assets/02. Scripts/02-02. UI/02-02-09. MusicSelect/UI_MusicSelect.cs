using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 라이브 곡 선택 화면(09_MusicSelectScene)의 총괄 컨트롤러입니다.
/// 수록 목록을 읽어 챕터 탭·곡 목록·곡 상세를 이어 주고, 진입 경로에 따른 지정곡 고정과 뒤로가기 위치를 처리합니다.
/// </summary>
public class UI_MusicSelect : MonoBehaviour
{
    // 기획서 SCREEN-007: 카드 편성이 플레이 필수 조건이며 편성 슬롯은 5장입니다.
    private const int REQUIRED_CARD_COUNT = 5;
    private const string PRACTICE_TITLE = "연습실 라이브";
    private const string DEFAULT_TITLE = "라이브 선택";

    [Foldout("Hierarchy")]
    [Header("Sub Panels")]
    [SerializeField]
    private UI_MusicSelectChapterTabs _chapterTabs;

    [SerializeField]
    private UI_MusicSelectSongList _songList;

    [SerializeField]
    private UI_MusicSelectSongDetail _songDetail;

    [SerializeField]
    private UI_MusicSelectDifficultyTabs _difficultyTabs;

    [SerializeField]
    private UI_MusicSelectRecordPanel _recordPanel;

    [Foldout("Hierarchy")]
    [Header("Buttons")]
    [SerializeField]
    private Button _backButton;

    [SerializeField]
    private Button _liveReadyButton;

    [Foldout("Hierarchy")]
    [Header("Texts")]
    [SerializeField]
    private TMP_Text _subTitleText;

    [Foldout("Hierarchy")]
    [Header("Popups")]
    // 이 화면에서만 열고 닫는 팝업이라 PersistentScene이 아니라 09_MusicSelectScene에 두고 여기서 참조합니다.
    [SerializeField]
    private UI_PhotocardSelectPopup _photocardSelectPopup;

    private readonly LiveChartSummaryReader _chartSummaryReader = new LiveChartSummaryReader();
    private readonly SongCoverLoader _coverLoader = new SongCoverLoader();

    private int _chapterIndex = -1;
    private EDifficulty _difficulty = EDifficulty.NORMAL;
    private SongData _selectedSong;

    private void Awake()
    {
        _songList.Init(_coverLoader);
        _songDetail.Init(_coverLoader);

        _chapterTabs.OnChapterSelected = chapterIndex => SetChapter(chapterIndex, 0);
        _songList.OnSongSelected = SetSong;
        _difficultyTabs.OnDifficultySelected = SetDifficulty;

        InitButtons();
    }

    private void Start()
    {
        InitCatalog();
    }

    // 커버는 씬에 매이지 않는 리소스라, 화면을 떠날 때 캐시가 직접 정리해 주어야 드나들 때마다 쌓이지 않습니다.
    private void OnDestroy()
    {
        _coverLoader.Clear();
    }

    private void InitButtons()
    {
        // 뒤로가기 위치는 진입 경로에 따라 달라지므로 UI_SceneTransitionButton(고정 대상)을 쓸 수 없습니다.
        _backButton.onClick.AddListener(() => LoadScene(LiveEntryBackTarget.GetBackScene(LiveEntryContext.Instance.EntryType)));
        _liveReadyButton.onClick.AddListener(OpenPhotocardSelect);
    }

    /// <summary>
    /// 채보 에디터에서 곡을 새로 수록한 직후에도 최신 목록이 보이도록, 화면 진입 때마다 수록 공간을 다시 읽습니다.
    /// </summary>
    private void InitCatalog()
    {
        LiveSongCatalog catalog = LiveSongCatalog.Instance;
        catalog.Rebuild();

        // 연습실 LIVE만 화면 이름을 달리 표기합니다.
        _subTitleText.text = LiveEntryContext.Instance.EntryType == EEntryType.PRACTICE_LIVE ? PRACTICE_TITLE : DEFAULT_TITLE;

        if (catalog.Chapters.Count == 0)
        {
            Debug.LogWarning($"[UI_MusicSelect] 수록된 곡이 없습니다. {LiveSongPaths.PublishedRoot} 아래에 챕터 폴더와 곡을 수록해 주세요.");
            RefreshEmptyState();
            return;
        }

        _chapterTabs.RefreshChapters(catalog.Chapters);

        if (!LiveSongSelectionResolver.TryResolve(catalog, LiveEntryContext.Instance, out int chapterIndex, out int songIndex))
        {
            Debug.LogWarning("[UI_MusicSelect] 해금된 챕터가 없습니다.");
            RefreshEmptyState();
            return;
        }

        SetChapter(chapterIndex, songIndex);
    }

    private void SetChapter(int chapterIndex, int songIndex)
    {
        _chapterIndex = chapterIndex;
        _chapterTabs.SetSelectedIndex(chapterIndex);

        IReadOnlyList<SongData> songs = LiveSongCatalog.Instance.Chapters[chapterIndex].Songs;

        // 스케줄 LIVE는 지정곡 외의 곡을 고를 수 없으므로 목록 자체를 잠급니다.
        _songList.RefreshSongs(songs, !LiveEntryContext.Instance.HasDesignatedSong);

        SetSong(Mathf.Clamp(songIndex, 0, songs.Count - 1));
    }

    private void SetSong(int songIndex)
    {
        _songList.SetSelectedIndex(songIndex);
        _selectedSong = LiveSongCatalog.Instance.Chapters[_chapterIndex].Songs[songIndex];

        _difficultyTabs.RefreshDifficulties(_selectedSong, _chartSummaryReader);

        if (!_difficultyTabs.TryGetDefaultDifficulty(out EDifficulty difficulty))
        {
            Debug.LogWarning($"[UI_MusicSelect] 수록된 채보가 없는 곡이라 플레이할 수 없습니다: {_selectedSong.SongId}");
        }

        SetDifficulty(difficulty);
    }

    private void SetDifficulty(EDifficulty difficulty)
    {
        _difficulty = difficulty;
        _difficultyTabs.SetSelectedDifficulty(difficulty);

        LiveChartSummary summary = _chartSummaryReader.GetSummary(_selectedSong, difficulty);

        _songDetail.RefreshSong(_selectedSong, summary);
        _recordPanel.RefreshRecord(PlayerDataProvider.Instance.GetSongRecord(_selectedSong.SongId, difficulty));

        // 카드 편성이 갖춰지지 않았거나 채보가 없으면 플레이할 수 없습니다.
        _liveReadyButton.interactable = summary.HasChart && PlayerDataProvider.Instance.Data.SelectedCardIds.Count >= REQUIRED_CARD_COUNT;
    }

    private void RefreshEmptyState()
    {
        _selectedSong = null;
        _chapterIndex = -1;

        _songList.RefreshSongs(null, false);
        _difficultyTabs.RefreshDifficulties(null, _chartSummaryReader);
        _songDetail.Clear();
        _recordPanel.Clear();
        _liveReadyButton.interactable = false;
    }

    /// <summary>
    /// 곡 선택 화면의 책임은 "무엇을 플레이할지" 확정하는 데까지입니다.
    /// 실제 LIVE 시작(리듬게임 씬 이동)은 이 팝업 안의 라이브 스타트 버튼이 담당하므로,
    /// 여기서는 선택 결과만 컨텍스트에 남기고 팝업을 엽니다.
    /// </summary>
    private void OpenPhotocardSelect()
    {
        if (ReferenceEquals(_selectedSong, null))
        {
            return;
        }

        LiveEntryContext.Instance.SetSelection(_selectedSong.SongId, _difficulty);

        // 팝업 스택은 PersistentScene의 UIManager가 들고 있으므로, 이 씬만 단독으로 열어 확인할 때는 존재하지 않을 수 있습니다.
        if (UIManager.Instance == null)
        {
            Debug.LogWarning("[UI_MusicSelect] UIManager가 없어 포토카드 선택 팝업을 열지 못했습니다. PersistentScene이 로드되어 있는지 확인해 주세요.");
            return;
        }

        UIManager.Instance.PopupHandler.OpenPopup(_photocardSelectPopup);
    }

    // 전환 매니저는 PersistentScene에 있으므로, 이 씬만 단독으로 열어 확인할 때는 존재하지 않을 수 있습니다.
    private void LoadScene(ESceneNames sceneName)
    {
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogWarning($"[UI_MusicSelect] SceneTransitionManager가 없어 {sceneName}으로 이동하지 못했습니다. PersistentScene이 로드되어 있는지 확인해 주세요.");
            return;
        }

        SceneTransitionManager.Instance.LoadSceneAsync(sceneName, this.GetCancellationTokenOnDestroy()).Forget();
    }
}
