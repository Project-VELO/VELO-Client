using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 리듬게임 플레이테스트에서 쌓인 노트별 판정 통계를 채보 단위로 보여 주는 에디터 창입니다.
/// GOOD/BAD가 반복되는 노트와, 이르게·늦게 치우친 노트를 골라 채보 타이밍을 고치는 데 씁니다.
///
/// 통계는 리듬게임 씬을 떠날 때 저장되므로, 플레이 모드가 끝나 에디터로 돌아오면 목록과 내용을 자동으로 다시 읽습니다.
/// </summary>
public class LiveNoteHitStatWindow : EditorWindow
{
    private const string READING_GUIDE =
        "편차 = 노트 평균 오차 − 채보 평균 오차. 오프셋·기기 지연처럼 전체에 걸린 치우침을 뺀 값이라 옮겨야 할 양으로 읽습니다.\n"
        + "편차가 −40ms면 40ms 이르게 친 것이므로 노트를 40ms 앞당깁니다(TimeMs에 편차를 더함).\n"
        + "이른 BAD가 많으면 늦게 찍혔을 가능성, MISS가 많으면 이르게 찍혔거나(늦게 친 입력은 GOOD 창을 넘기면 들어오지 않음) 읽기 어려운 배치를 의심합니다.";

    private readonly LiveNoteHitStatReport _report = new LiveNoteHitStatReport();
    private readonly LiveNoteHitStatTable _table = new LiveNoteHitStatTable();

    private List<string> _filePaths = new List<string>();

    // EditorGUILayout.Popup이 배열을 받으므로 목록을 읽을 때 한 번 만들어 둡니다.
    private string[] _fileNames = new string[0];
    private int _fileIndex;
    private bool _hasReport;

    [MenuItem("VELO/Live/노트 판정 통계")]
    private static void Open()
    {
        GetWindow<LiveNoteHitStatWindow>("노트 판정 통계");
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += RefreshOnEditMode;
        RefreshFiles();
    }

    private void OnGUI()
    {
        DrawFileSection();
        EditorGUILayout.Space();

        if (_filePaths.Count == 0)
        {
            EditorGUILayout.HelpBox("아직 기록된 판정 통계가 없습니다. 리듬게임 씬에서 곡을 플레이하면 채보별로 쌓입니다(개발 빌드·에디터 전용).", MessageType.Info);
            return;
        }

        if (!_hasReport)
        {
            EditorGUILayout.HelpBox("선택한 파일을 읽지 못했습니다. 사유는 콘솔 경고를 확인하세요. 필요 없으면 '이 채보 기록 초기화'로 지울 수 있습니다.", MessageType.Warning);
            return;
        }

        DrawSummary();
        EditorGUILayout.Space();
        _table.Draw();
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= RefreshOnEditMode;
    }

    private void DrawFileSection()
    {
        EditorGUILayout.LabelField("판정 통계 파일", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(LiveChartHitStatsStorage.RootPath, EditorStyles.miniLabel);

        bool isResetRequested = false;

        using (new EditorGUILayout.HorizontalScope())
        {
            DrawFilePopup();

            if (GUILayout.Button("새로고침", GUILayout.Width(80f)))
            {
                RefreshFiles();
            }

            if (GUILayout.Button("폴더 열기", GUILayout.Width(80f)))
            {
                Directory.CreateDirectory(LiveChartHitStatsStorage.RootPath);
                EditorUtility.RevealInFinder(LiveChartHitStatsStorage.RootPath);
            }

            // 플레이 중에 지우면 씬을 떠날 때 메모리에 남은 누적값이 다시 저장되어 초기화가 되살아납니다.
            // 읽지 못한 파일도 창에서 지울 수 있도록 내용이 아니라 선택 여부로 막습니다.
            using (new EditorGUI.DisabledScope(!HasSelectedFile() || EditorApplication.isPlaying))
            {
                isResetRequested = GUILayout.Button("이 채보 기록 초기화", GUILayout.Width(130f));
            }
        }

        // 확인 창은 가로 그룹 밖에서 띄웁니다. 모달이 OnGUI 도중 이벤트를 가져가면 레이아웃 경고가 납니다.
        if (isResetRequested && EditorUtility.DisplayDialog(
                "판정 통계 초기화",
                $"{_fileNames[_fileIndex]}의 누적 판정 통계를 지웁니다. 계속하시겠습니까?",
                "초기화",
                "취소"))
        {
            LiveChartHitStatsStorage.Delete(_filePaths[_fileIndex]);
            RefreshFiles();
            GUIUtility.ExitGUI();
        }
    }

    private void DrawFilePopup()
    {
        if (_filePaths.Count == 0)
        {
            EditorGUILayout.LabelField("파일 없음");
            return;
        }

        int fileIndex = EditorGUILayout.Popup(_fileIndex, _fileNames);

        if (fileIndex != _fileIndex)
        {
            _fileIndex = fileIndex;
            RefreshReport();
        }
    }

    private void DrawSummary()
    {
        EditorGUILayout.LabelField($"{_report.Stats.SongId} / {_report.Stats.Difficulty}", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(_report.OverviewSummary);
        EditorGUILayout.LabelField(_report.JudgementSummary);
        EditorGUILayout.LabelField(_report.TimingSummary);

        if (!_report.HasWorkingChart)
        {
            EditorGUILayout.HelpBox("작업본 채보(StreamingAssets/Charts)를 찾지 못해 마디:박과 수정 여부를 표시하지 않습니다.", MessageType.Warning);
        }

        EditorGUILayout.HelpBox(READING_GUIDE, MessageType.None);
    }

    private void RefreshOnEditMode(PlayModeStateChange stateChange)
    {
        if (stateChange != PlayModeStateChange.EnteredEditMode)
        {
            return;
        }

        RefreshFiles();
        Repaint();
    }

    /// <summary>
    /// 파일 목록을 다시 읽고, 보고 있던 파일이 남아 있으면 그 파일을 다시 엽니다.
    /// </summary>
    private void RefreshFiles()
    {
        string selectedPath = HasSelectedFile() ? _filePaths[_fileIndex] : null;

        _filePaths = LiveChartHitStatsStorage.GetAllFilePaths();
        _fileNames = new string[_filePaths.Count];
        _fileIndex = Mathf.Max(0, _filePaths.IndexOf(selectedPath));

        for (int i = 0; i < _filePaths.Count; i++)
        {
            _fileNames[i] = Path.GetFileNameWithoutExtension(_filePaths[i]);
        }

        RefreshReport();
    }

    private void RefreshReport()
    {
        _hasReport = false;

        if (!HasSelectedFile() || !LiveChartHitStatsStorage.TryReadWithBackup(_filePaths[_fileIndex], out LiveChartHitStats stats))
        {
            return;
        }

        _report.Init(stats);
        _table.SetReport(_report);
        _hasReport = true;
    }

    private bool HasSelectedFile()
    {
        return _fileIndex < _filePaths.Count;
    }
}
