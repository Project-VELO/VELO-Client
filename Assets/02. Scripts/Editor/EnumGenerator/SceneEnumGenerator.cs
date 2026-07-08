using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class SceneEnumGenerator : EnumGeneratorBase
{
    [MenuItem(MENU_PREFIX + "Generate Scene Enums(ESceneNames)", priority = 3)]
    public static void GenerateSceneEnum()
    {
        Generate("ESceneNames.cs", "ESceneNames", (writer) =>
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            HashSet<string> usedNames = new HashSet<string>();
            foreach (var scene in scenes)
            {
                if (!scene.enabled)
                {
                    continue;
                }

                string sceneName = Path.GetFileNameWithoutExtension(scene.path);
                string cleanedName = SceneTransitionManager.CleanSceneName(sceneName);
                if (usedNames.Add(cleanedName))
                {
                    writer.WriteLine($"    {cleanedName},");
                }
                else
                {
                    UnityEngine.Debug.LogError($"[SceneEnumGenerator] Duplicate scene enum name detected: {cleanedName} ({scene.path})");
                }
            }
        });
    }
}