using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 기획이 작성한 대본 TSV를 회차별 JSON으로 변환하는 에디터 창입니다.
///
/// 스토리 31화 분량의 대사를 손으로 JSON에 옮기는 것은 현실적이지 않으므로 이 경로를 통해서만 대본을 만듭니다.
/// 반면 스케줄 21개와 카드 10장은 손으로 쓸 수 있는 분량이라 별도 변환 도구를 만들지 않습니다.
/// </summary>
public class StoryScriptImportWindow : EditorWindow
{
    private readonly StoryScriptTsvParser _parser = new StoryScriptTsvParser();

    private string _tsvPath = string.Empty;
    private string _resultMessage = string.Empty;
    private Vector2 _scrollPosition;

    [MenuItem("VELO/MasterData/스토리 대본 가져오기")]
    private static void Open()
    {
        GetWindow<StoryScriptImportWindow>("스토리 대본 가져오기");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Google Sheets에서 '파일 > 다운로드 > 탭으로 구분된 값(.tsv)'으로 내보낸 파일을 선택하세요.\n" +
            $"저장 위치: {MasterDataPaths.StoryScriptsRoot}",
            MessageType.Info);

        DrawFileSelector();
        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_tsvPath)))
        {
            if (GUILayout.Button("변환 실행"))
            {
                _resultMessage = Import();
            }
        }

        EditorGUILayout.Space();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        if (!string.IsNullOrEmpty(_resultMessage))
        {
            EditorGUILayout.HelpBox(_resultMessage, MessageType.None);
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawFileSelector()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("TSV 파일", _tsvPath, EditorStyles.textField);

            if (GUILayout.Button("찾기", GUILayout.Width(60f)))
            {
                string selected = EditorUtility.OpenFilePanel("대본 TSV 선택", Application.dataPath, "tsv");

                if (!string.IsNullOrEmpty(selected))
                {
                    _tsvPath = selected;
                }
            }
        }
    }

    private string Import()
    {
        if (!File.Exists(_tsvPath))
        {
            return $"파일을 찾을 수 없습니다: {_tsvPath}";
        }

        var errors = new List<string>();
        List<StoryScriptData> scripts = _parser.Parse(File.ReadAllText(_tsvPath), errors);

        var builder = new System.Text.StringBuilder();

        // 오류가 있어도 정상적인 회차는 저장합니다. 한 줄의 오타 때문에 전체 변환이 막히면 작업이 더뎌집니다.
        if (0 < errors.Count)
        {
            builder.AppendLine($"건너뛴 행 {errors.Count}건");

            foreach (string error in errors)
            {
                builder.AppendLine($"  - {error}");
            }

            builder.AppendLine();
        }

        if (scripts.Count == 0)
        {
            builder.AppendLine("변환된 회차가 없습니다.");
            return builder.ToString();
        }

        Directory.CreateDirectory(MasterDataPaths.StoryScriptsRoot);

        foreach (StoryScriptData script in scripts)
        {
            string path = MasterDataPaths.GetStoryScriptPath(script.StoryId);
            File.WriteAllText(path, JsonUtility.ToJson(script, true));
            builder.AppendLine($"{script.StoryId}: {script.Lines.Count}줄 → {Path.GetFileName(path)}");
        }

        AssetDatabase.Refresh();

        return builder.ToString();
    }
}
