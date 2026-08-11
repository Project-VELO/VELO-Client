using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 라이브 곡 선택 화면(09_MusicSelectScene)의 총괄 컨트롤러입니다.
/// 수록 목록을 읽어 화면을 세우고, 진입 경로에 따른 뒤로가기 위치와 LIVE 진입 배선을 처리합니다.
///
/// 무엇을 고를지의 연쇄(챕터 → 곡 → 난이도 → 상세·기록)는 UI_MusicSelectSelection이,
/// LIVE 진입 게이트는 UI_MusicSelectLiveEntry가 맡습니다.
/// </summary>
public class UI_MusicSelect : MonoBehaviour
{
    private const string PRACTICE_TITLE = "연습실 라이브";
    private const string DEFAULT_TITLE = "라이브 선택";

    [Foldout("Hierarchy")]
    [Header("Sub Panels")]
    [SerializeField]
    private UI_MusicSelectSelection _selection;

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
    [Header("Live Entry")]
    // 편성 검사와 팝업·씬 진입은 같은 오브젝트의 진입 게이트가 맡습니다(200줄 규칙에 따른 분할).
    [SerializeField]
    private UI_MusicSelectLiveEntry _liveEntry;

    private SongData _selectedSong;
    private EDifficulty _difficulty = EDifficulty.NORMAL;

    private void Awake()
    {
        _selection.OnSelectionChanged = SetSelection;

        InitButtons();
    }

    private void Start()
    {
        // 곡 선택 화면에 온 시점에는 항상 기본 편성이 현재 편성이어야 합니다(기획서 3-H-4).
        // 결과 확인·준비 팝업 닫기가 이미 폐기하지만, 일시정지 그만두기처럼 LIVE를 우회 이탈한 경로의 잔존을 여기서 마저 정리합니다.
        LiveLoadoutContext.Instance.Clear();

        InitCatalog();
    }

    private void InitButtons()
    {
        // 뒤로가기 위치는 진입 경로에 따라 달라지므로 UI_SceneTransitionButton(고정 대상)을 쓸 수 없습니다.
        _backButton.onClick.AddListener(() => LoadScene(LiveEntryBackTarget.GetBackScene(LiveEntryContext.Instance.EntryType)));
        _liveReadyButton.onClick.AddListener(() => _liveEntry.OpenPreparePopup(_selectedSong, _difficulty));
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
            RefuseWhenSongDataMissing();
            return;
        }

        _selection.RefreshChapters(catalog.Chapters);

        if (!LiveSongSelectionResolver.TryResolve(catalog, LiveEntryContext.Instance, out int chapterIndex, out int songIndex))
        {
            Debug.LogWarning("[UI_MusicSelect] 해금된 챕터가 없습니다.");
            RefuseWhenSongDataMissing();
            return;
        }

        _selection.SetChapter(chapterIndex, songIndex);
    }

    /// <summary>
    /// 고를 수 있는 곡이 하나도 없을 때입니다. 기획서 3-L은 "오류 팝업 후 곡 선택 화면 유지"로 정하고 있어,
    /// 안내만 띄우고 다른 화면으로 밀어내지 않습니다.
    /// </summary>
    private void RefuseWhenSongDataMissing()
    {
        _selection.Clear();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenErrorPopup(ErrorMessages.SONG_DATA_MISSING);
        }
    }

    /// <summary>
    /// 채보 없음만 버튼 비활성 사유로 남깁니다. 카드 편성 미달은 조용한 비활성 대신
    /// 준비 팝업의 라이브 시작에서 안내 팝업으로 차단합니다(기획서 16-15 + SCREEN-011 포토카드 미편성 안내).
    /// </summary>
    private void SetSelection(SongData song, EDifficulty difficulty, bool hasChart)
    {
        _selectedSong = song;
        _difficulty = difficulty;

        _liveReadyButton.interactable = hasChart;
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
