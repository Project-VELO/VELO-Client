using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 편집 중인 곡을 채보 에디터 안에서 곡 선택 화면으로 올리는 버튼입니다.
///
/// 저장 시 자동 갱신(LivePublishedChartSync)은 이미 수록된 채보를 최신으로 맞추는 데까지만 합니다.
/// 새 곡과 새 난이도는 의도적으로 올려야 하는 것이라 저장만으로 따라가게 두지 않았는데, 그렇다고 Unity 메뉴의
/// 수록 창까지 열게 하면 기획자가 채보를 만들다 말고 에디터 밖으로 나가야 합니다. 그 한 걸음을 여기로 옮깁니다.
///
/// 수록은 저장된 파일을 복사하는 것이므로 저장하지 않은 편집이 있으면 막습니다.
/// 그대로 진행하면 화면에 보이는 채보와 실제로 올라가는 채보가 달라집니다.
/// </summary>
public class UI_LiveEditorSongPublish : MonoBehaviour
{
    private const string NO_SONG_TOAST = "편집 중인 곡이 없습니다.";
    private const string UNSAVED_TOAST = "저장하지 않은 편집이 있습니다. 먼저 저장해 주세요.";
    private const string NO_CHART_TOAST = "저장된 채보가 없어 수록할 수 없습니다.";
    private const string NO_CHAPTER_TOAST = "수록할 챕터가 없습니다. 수록 창에서 챕터 폴더를 먼저 만들어 주세요.";
    private const string PUBLISHED_TOAST = "수록을 완료했습니다. 곡 선택 화면에서 확인할 수 있습니다.";

    private const string CHAPTER_SELECT_TITLE = "수록할 챕터 선택";
    private const string CONFIRM_LABEL = "수록";

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _publishButton;

    [SerializeField]
    private LiveEditorController _controller;

    [SerializeField]
    private UI_LiveEditorToast _toast;

    [Tooltip("수록할 챕터를 고르는 목록 팝업입니다. 곡 선택에 쓰는 목록 팝업을 그대로 함께 씁니다.")]
    [SerializeField]
    private UI_LiveEditorSongSelectPopup _chapterSelectPopup;

    [SerializeField]
    private UI_LiveEditorConfirmPopup _confirmPopup;

    private readonly LiveSongPublishWriter _publishWriter = new LiveSongPublishWriter();
    private readonly UI_LiveEditorPopupPresenter _popupPresenter = new UI_LiveEditorPopupPresenter();
    private readonly List<string> _chapterFolders = new List<string>();
    private readonly List<string> _chapterNames = new List<string>();
    private readonly List<EDifficulty> _publishTargets = new List<EDifficulty>();

    private void Awake()
    {
        _publishButton.onClick.AddListener(OnPublishClicked);
    }

    private void OnPublishClicked()
    {
        if (!TryCollectPublishTargets(out string songId))
        {
            return;
        }

        // 이미 수록된 곡은 챕터를 옮길 일이 아니라 갱신이므로, 고르게 하지 않고 원래 자리로 보냅니다.
        if (_publishWriter.TryFindPublishedChapterFolder(songId, out string chapterFolder))
        {
            OpenConfirm(songId, chapterFolder, true);
            return;
        }

        OpenChapterSelect(songId);
    }

    /// <summary>
    /// 수록할 수 있는 상태인지 확인하고 올릴 난이도를 모읍니다. 저장된 채보가 있는 난이도만 대상입니다.
    /// </summary>
    private bool TryCollectPublishTargets(out string songId)
    {
        songId = null;
        SongData song = _controller.CurrentSong;

        if (ReferenceEquals(song, null))
        {
            ShowToast(NO_SONG_TOAST);
            return false;
        }

        if (_controller.HasUnsavedChanges)
        {
            ShowToast(UNSAVED_TOAST);
            return false;
        }

        _publishTargets.Clear();
        _publishTargets.AddRange(_controller.ChartIO.GetSavedDifficulties(song.SongId));

        if (_publishTargets.Count == 0)
        {
            ShowToast(NO_CHART_TOAST);
            return false;
        }

        songId = song.SongId;
        return true;
    }

    private void OpenChapterSelect(string songId)
    {
        _chapterFolders.Clear();
        _chapterFolders.AddRange(LiveSongPaths.GetPublishedChapterFolders());

        if (_chapterFolders.Count == 0)
        {
            ShowToast(NO_CHAPTER_TOAST);
            return;
        }

        _chapterNames.Clear();
        foreach (string chapterFolder in _chapterFolders)
        {
            _chapterNames.Add(Path.GetFileName(chapterFolder));
        }

        // 목록 팝업은 곡 선택 흐름과 함께 쓰므로, 열기 직전에 이쪽 처리를 다시 걸어 둡니다.
        _chapterSelectPopup.OnSongSelected = chapterName => SelectChapter(songId, chapterName);
        _chapterSelectPopup.OnBackClicked = _popupPresenter.CloseLatest;
        _chapterSelectPopup.RefreshSongs(CHAPTER_SELECT_TITLE, _chapterNames);

        _popupPresenter.Open(_chapterSelectPopup);
    }

    private void SelectChapter(string songId, string chapterName)
    {
        _popupPresenter.CloseLatest();

        foreach (string chapterFolder in _chapterFolders)
        {
            if (Path.GetFileName(chapterFolder) == chapterName)
            {
                OpenConfirm(songId, chapterFolder, false);
                return;
            }
        }
    }

    /// <summary>
    /// 수록은 곡 선택 화면에 바로 드러나는 작업이라 한 번 묻습니다.
    /// 처음 올리는 것인지 이미 올라간 것을 덮는 것인지에 따라 결과가 다르므로 문구를 나눕니다.
    /// </summary>
    private void OpenConfirm(string songId, string chapterFolder, bool isAlreadyPublished)
    {
        string difficulties = string.Join(", ", _publishTargets);
        string message = isAlreadyPublished
            ? $"{songId}의 수록본을 지금 작업본으로 덮어씁니다.\n난이도: {difficulties}"
            : $"{songId}을(를) {Path.GetFileName(chapterFolder)}에 수록합니다.\n곡 선택 화면에 나타납니다.\n난이도: {difficulties}";

        _confirmPopup.OnConfirmed = () => Publish(songId, chapterFolder);
        _confirmPopup.OnCanceled = _popupPresenter.CloseLatest;
        _confirmPopup.SetMessage(message, CONFIRM_LABEL);

        _popupPresenter.Open(_confirmPopup);
    }

    private void Publish(string songId, string chapterFolder)
    {
        _popupPresenter.CloseLatest();

        if (!_publishWriter.Publish(songId, chapterFolder, _publishTargets, out string error))
        {
            Debug.LogError($"[UI_LiveEditorSongPublish] 수록에 실패했습니다: {error}");
            ShowToast(error);
            return;
        }

        ShowToast(PUBLISHED_TOAST);
    }

    private void ShowToast(string message)
    {
        if (_toast == null)
        {
            return;
        }

        _toast.Show(message);
    }
}
