using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

public class SceneEnumGenerator : EnumGeneratorBase
{
    [MenuItem(MENU_PREFIX + "Generate Scene Enums(ESceneNames)", priority = 3)]
    public static void GenerateSceneEnum()
    {
        Generate("ESceneNames.cs", "ESceneNames", (writer) =>
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            foreach (var scene in scenes)
            {
                if (!scene.enabled) continue;

                string sceneName = Path.GetFileNameWithoutExtension(scene.path);
                string cleanedName = Regex.Replace(sceneName, @"^\d+_", "");
                writer.WriteLine($"    {cleanedName.Replace(" ", "_")},");
            }
        });
    }
}