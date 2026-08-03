using UnityEditor;
using UnityEngine;

/// <summary>
/// 진행 상태 디버그 창의 스토리 영역을 그립니다.
/// 해금·완료·NEW 상태를 한눈에 보고, 감상 화면 없이 완료 처리를 할 수 있습니다.
/// </summary>
public class GameProgressDebugStorySection
{
    public void Draw()
    {
        EditorGUILayout.LabelField("스토리", EditorStyles.boldLabel);

        foreach (StoryData story in MasterDataQuery.GetAllStoriesInDisplayOrder())
        {
            DrawStoryRow(story);
        }
    }

    private void DrawStoryRow(StoryData story)
    {
        IStoryProgress progress = GameProgressService.Instance.GetStoryProgress(story.StoryId);
        string state = ReferenceEquals(progress, null)
            ? "기록 없음"
            : $"{progress.Status}{(progress.IsNew ? " · NEW" : string.Empty)}";

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField($"{story.StoryId}  ({state})", GUILayout.MinWidth(280f));

            using (new EditorGUI.DisabledScope(ReferenceEquals(progress, null) || progress.IsCompleted))
            {
                if (GUILayout.Button("완료 처리", GUILayout.Width(80f)))
                {
                    GameProgressService.Instance.CompleteStory(story.StoryId);
                }
            }
        }
    }
}
