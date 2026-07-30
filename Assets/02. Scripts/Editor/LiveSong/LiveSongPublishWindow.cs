using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 완성된 곡·채보를 챕터 폴더로 수록하는 에디터 창입니다.
/// 곡 선택 화면은 수록 공간만 읽으므로, 이 창을 거치지 않은 채보는 게임에 노출되지 않습니다.
/// </summary>
public class LiveSongPublishWindow : EditorWindow
{
    private readonly LiveSongPublisher _publisher = new LiveSongPublisher();
    private readonly LiveEditorSongIO _songIO = new LiveEditorSongIO();
    private readonly LiveEditorChartIO _chartIO = new LiveEditorChartIO();

    private readonly Dictionary<string, HashSet<EDifficulty>> _selectedDifficulties = new Dictionary<string, HashSet<EDifficulty>>();

    private List<string> _chapterFolders = new List<string>();
    private int _chapterIndex;
    private int _newChapterOrder;
    private string _newChapterId = "PROLOGUE";
    private Vector2 _scrollPosition;

    [MenuItem("VELO/Live/곡 수록(Publish)")]
    private static void Open()
    {
        GetWindow<LiveSongPublishWindow>("곡 수록");
    }

    private void OnEnable()
    {
        RefreshChapters();
    }

    private void OnGUI()
    {
        DrawChapterSection();
        EditorGUILayout.Space();

        if (_chapterFolders.Count == 0)
        {
            EditorGUILayout.HelpBox("먼저 챕터 폴더를 만들어 주세요.", MessageType.Info);
            return;
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        DrawWorkingSongs();
        EditorGUILayout.Space();
        DrawPublishedSongs();
        EditorGUILayout.EndScrollView();
    }

    private void DrawChapterSection()
    {
        EditorGUILayout.LabelField("챕터", EditorStyles.boldLabel);

        if (_chapterFolders.Count > 0)
        {
            string[] names = _chapterFolders.Select(Path.GetFileName).ToArray();
            _chapterIndex = EditorGUILayout.Popup("수록 대상", Mathf.Clamp(_chapterIndex, 0, names.Length - 1), names);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            _newChapterOrder = EditorGUILayout.IntField("표시 순서", _newChapterOrder);
            _newChapterId = EditorGUILayout.TextField(_newChapterId);

            if (GUILayout.Button("챕터 폴더 생성", GUILayout.Width(120f)) && !string.IsNullOrEmpty(_newChapterId))
            {
                _publisher.CreateChapterFolder(_newChapterOrder, _newChapterId);
                RefreshChapters();
            }
        }
    }

    private void DrawWorkingSongs()
    {
        EditorGUILayout.LabelField("작업 공간의 곡", EditorStyles.boldLabel);

        List<string> songIds = _songIO.GetAllSongIds();
        if (songIds.Count == 0)
        {
            EditorGUILayout.HelpBox("StreamingAssets/Songs에 등록된 곡이 없습니다.", MessageType.Info);
            return;
        }

        foreach (string songId in songIds)
        {
            List<EDifficulty> savedDifficulties = _chartIO.GetSavedDifficulties(songId);
            if (savedDifficulties.Count == 0)
            {
                continue;
            }

            DrawWorkingSong(songId, savedDifficulties);
        }
    }

    private void DrawWorkingSong(string songId, List<EDifficulty> savedDifficulties)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField(songId, EditorStyles.boldLabel);

            HashSet<EDifficulty> selected = GetSelectedDifficulties(songId, savedDifficulties);

            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (EDifficulty difficulty in savedDifficulties)
                {
                    bool isSelected = EditorGUILayout.ToggleLeft(difficulty.ToString(), selected.Contains(difficulty), GUILayout.Width(90f));
                    if (isSelected)
                    {
                        selected.Add(difficulty);
                    }
                    else
                    {
                        selected.Remove(difficulty);
                    }
                }

                if (GUILayout.Button("수록", GUILayout.Width(80f)))
                {
                    PublishSong(songId, selected);
                }
            }
        }
    }

    private void DrawPublishedSongs()
    {
        string chapterFolder = _chapterFolders[Mathf.Clamp(_chapterIndex, 0, _chapterFolders.Count - 1)];
        EditorGUILayout.LabelField($"수록된 곡 ({Path.GetFileName(chapterFolder)})", EditorStyles.boldLabel);

        List<string> publishedSongIds = _publisher.GetPublishedSongIds(chapterFolder);
        if (publishedSongIds.Count == 0)
        {
            EditorGUILayout.HelpBox("이 챕터에 수록된 곡이 없습니다.", MessageType.Info);
            return;
        }

        foreach (string songId in publishedSongIds)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(songId);

                if (GUILayout.Button("수록 취소", GUILayout.Width(80f)))
                {
                    _publisher.Unpublish(chapterFolder, songId);
                    GUIUtility.ExitGUI();
                }
            }
        }
    }

    private void PublishSong(string songId, HashSet<EDifficulty> selectedDifficulties)
    {
        string chapterFolder = _chapterFolders[Mathf.Clamp(_chapterIndex, 0, _chapterFolders.Count - 1)];

        if (_publisher.Publish(songId, chapterFolder, selectedDifficulties.ToList(), out string error))
        {
            Debug.Log($"[LiveSongPublishWindow] 수록 완료: {songId} → {Path.GetFileName(chapterFolder)}");
        }
        else
        {
            EditorUtility.DisplayDialog("수록 실패", error, "확인");
        }
    }

    /// <summary>
    /// 처음 그릴 때는 저장된 난이도를 모두 선택해 둡니다. 대부분 만들어 둔 채보를 전부 수록하기 때문입니다.
    /// </summary>
    private HashSet<EDifficulty> GetSelectedDifficulties(string songId, List<EDifficulty> savedDifficulties)
    {
        if (!_selectedDifficulties.TryGetValue(songId, out HashSet<EDifficulty> selected))
        {
            selected = new HashSet<EDifficulty>(savedDifficulties);
            _selectedDifficulties[songId] = selected;
        }

        return selected;
    }

    private void RefreshChapters()
    {
        _chapterFolders = LiveSongPaths.GetPublishedChapterFolders();
        _chapterIndex = Mathf.Clamp(_chapterIndex, 0, Mathf.Max(0, _chapterFolders.Count - 1));
    }
}
