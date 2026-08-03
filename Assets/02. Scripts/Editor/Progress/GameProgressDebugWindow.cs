using UnityEditor;
using UnityEngine;

/// <summary>
/// 진행 상태와 세이브를 조작·확인하는 에디터 창입니다.
///
/// 화면(홈·사무실·스토리)이 붙기 전까지는 진행을 확인할 유일한 수단이며,
/// 화면이 생긴 뒤에도 리듬게임을 실제로 B랭크로 클리어하지 않고 스케줄을 넘기는 QA 도구로 계속 씁니다.
///
/// 여기서 바꾼 내용은 곧바로 세이브 파일에 기록되므로, 플레이 모드에 들어가면 그 상태로 로드됩니다.
/// </summary>
public class GameProgressDebugWindow : EditorWindow
{
    private readonly GameProgressDebugScheduleSection _scheduleSection = new GameProgressDebugScheduleSection();
    private readonly GameProgressDebugStorySection _storySection = new GameProgressDebugStorySection();

    private Vector2 _scrollPosition;
    private string _saveJson = string.Empty;

    [MenuItem("VELO/Progress/진행 상태 디버그")]
    private static void Open()
    {
        GetWindow<GameProgressDebugWindow>("진행 상태 디버그");
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        DrawSummary();
        EditorGUILayout.Space();
        _scheduleSection.Draw();
        EditorGUILayout.Space();
        _storySection.Draw();
        EditorGUILayout.Space();
        DrawSaveTools();

        EditorGUILayout.EndScrollView();
    }

    private void DrawSummary()
    {
        PlayerData data = PlayerDataProvider.Instance.Data;

        EditorGUILayout.LabelField("현재 상태", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"{data.CurrentChapterId} / {data.CurrentWeekId} / {data.CurrentDayId}");
        EditorGUILayout.LabelField($"MONEY {data.Money:N0}   HYPE {data.Hype:N0}   EXP {data.Exp:N0}");
    }

    private void DrawSaveTools()
    {
        EditorGUILayout.LabelField("세이브", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(SavePaths.SaveFilePath, EditorStyles.miniLabel);

        bool isResetRequested;

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("JSON 보기"))
            {
                _saveJson = ReadSaveJson();
            }

            if (GUILayout.Button("폴더 열기"))
            {
                SavePaths.EnsureSaveRoot();
                EditorUtility.RevealInFinder(SavePaths.SaveRoot);
            }

            isResetRequested = GUILayout.Button("신규 게임 리셋");
        }

        // 세이브 파일을 즉시 지우는 되돌릴 수 없는 조작이라, 옆 버튼을 누르려다 잘못 눌리면 QA 진행분이 사라집니다.
        // 확인 창은 가로 그룹 밖에서 띄웁니다. 모달이 OnGUI 도중 이벤트를 가져가면 레이아웃 경고가 납니다.
        if (isResetRequested && EditorUtility.DisplayDialog(
                "신규 게임 리셋",
                "현재 세이브 데이터가 초기화됩니다. 계속하시겠습니까?",
                "리셋",
                "취소"))
        {
            PlayerDataProvider.Instance.ResetToNewGame();
            _saveJson = string.Empty;
        }

        if (!string.IsNullOrEmpty(_saveJson))
        {
            EditorGUILayout.TextArea(_saveJson, GUILayout.MinHeight(200f));
        }
    }

    private string ReadSaveJson()
    {
        if (!System.IO.File.Exists(SavePaths.SaveFilePath))
        {
            return "세이브 파일이 없습니다.";
        }

        return System.IO.File.ReadAllText(SavePaths.SaveFilePath);
    }
}
