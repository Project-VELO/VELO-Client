using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 마스터 데이터의 참조 무결성을 확인하는 에디터 창입니다.
///
/// 화면이 붙기 전까지는 마스터 데이터가 제대로 읽히는지 눈으로 확인할 방법이 없으므로,
/// 이 창이 당분간 유일한 검증 수단입니다. 화면이 생긴 뒤에도 데이터를 추가할 때마다 계속 사용합니다.
/// </summary>
public class MasterDataValidateWindow : EditorWindow
{
    private readonly MasterDataValidator _validator = new MasterDataValidator();

    private MasterDataValidationReport _report;
    private string _runtimeLoadResult = string.Empty;
    private Vector2 _scrollPosition;

    [MenuItem("VELO/MasterData/검증")]
    private static void Open()
    {
        GetWindow<MasterDataValidateWindow>("마스터 데이터 검증");
    }

    private void OnGUI()
    {
        DrawToolbar();
        EditorGUILayout.Space();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        DrawReport();
        DrawRuntimeLoadResult();
        EditorGUILayout.EndScrollView();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.LabelField($"경로: {MasterDataPaths.MasterDataRoot}", EditorStyles.miniLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("검증 실행"))
            {
                _report = _validator.Validate();
            }

            if (GUILayout.Button("런타임 로드 테스트"))
            {
                _runtimeLoadResult = RunLoadTest();
            }
        }
    }

    private void DrawReport()
    {
        if (_report == null)
        {
            EditorGUILayout.HelpBox("검증 실행을 눌러 주세요.", MessageType.Info);
            return;
        }

        foreach (string summary in _report.Summaries)
        {
            EditorGUILayout.LabelField(summary, EditorStyles.miniLabel);
        }

        EditorGUILayout.Space();

        if (!_report.HasError && _report.Warnings.Count == 0)
        {
            EditorGUILayout.HelpBox("오류와 경고가 없습니다.", MessageType.Info);
            return;
        }

        DrawMessages("오류", _report.Errors, MessageType.Error);
        DrawMessages("경고", _report.Warnings, MessageType.Warning);
    }

    private void DrawMessages(string title, IReadOnlyList<string> messages, MessageType messageType)
    {
        if (messages.Count == 0)
        {
            return;
        }

        EditorGUILayout.LabelField($"{title} {messages.Count}건", EditorStyles.boldLabel);

        foreach (string message in messages)
        {
            EditorGUILayout.HelpBox(message, messageType);
        }

        EditorGUILayout.Space();
    }

    private void DrawRuntimeLoadResult()
    {
        if (string.IsNullOrEmpty(_runtimeLoadResult))
        {
            return;
        }

        EditorGUILayout.LabelField("런타임 로드 테스트", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(_runtimeLoadResult, MessageType.None);
    }

    /// <summary>
    /// 실제 게임 코드가 쓰는 경로로 조회해 봅니다.
    /// 파일이 읽히는 것과 화면이 원하는 형태로 조회되는 것은 다른 문제이므로 따로 확인합니다.
    /// </summary>
    private string RunLoadTest()
    {
        MasterDataProvider provider = MasterDataProvider.Instance;
        provider.Rebuild();

        NewGameConfigData config = provider.NewGameConfig;

        if (ReferenceEquals(config, null))
        {
            return "신규 게임 설정을 읽지 못했습니다.";
        }

        List<ScheduleData> firstDay = MasterDataQuery.GetSchedulesByDay(config.StartWeekId, config.StartDayId);
        List<CharacterData> members = MasterDataQuery.GetMembersInSlotOrder();
        List<StoryData> stories = MasterDataQuery.GetAllStoriesInDisplayOrder();

        var builder = new System.Text.StringBuilder();
        builder.AppendLine($"시작 시점: {config.StartWeekId} / {config.StartDayId}");
        builder.AppendLine($"첫날 스케줄 {firstDay.Count}건");

        foreach (ScheduleData schedule in firstDay)
        {
            builder.AppendLine($"  - [{schedule.ScheduleType}] {schedule.Title}");
        }

        builder.AppendLine($"편성 멤버 {members.Count}명: {string.Join(", ", members.ConvertAll(member => member.DisplayName))}");
        builder.AppendLine($"스토리 {stories.Count}편 (표시 순서 첫 항목: {(stories.Count > 0 ? stories[0].Title : "없음")})");

        return builder.ToString();
    }
}
