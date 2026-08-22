using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 화면에 걸린 TMP 폰트를 한꺼번에 갈아 끼웁니다.
///
/// TMP 텍스트는 폰트 에셋과 머티리얼을 따로 들고 있고, 머티리얼은 폰트 에셋 안의 하위 에셋입니다.
/// 그래서 파일에서 guid만 바꾸면 머티리얼이 없는 fileID를 가리켜 글자가 분홍색으로 깨집니다.
/// 에디터에서 font 프로퍼티로 넣어야 머티리얼까지 맞게 붙습니다.
///
/// 스토리 감상 화면은 NotoSerifKR을 그대로 쓰므로 여기서 건드리지 않습니다.
/// 바꾸는 대상은 고운바탕 두 벌뿐입니다.
/// </summary>
public static class FontAssetReplacer
{
    private const string TARGET_PATH = "Assets/10. Fonts/NotoSansKR-Regular SDF.asset";

    private static readonly string[] SOURCE_PATHS =
    {
        "Assets/10. Fonts/GowunBatang-Bold SDF.asset",
        "Assets/10. Fonts/GowunBatang-Regular SDF.asset"
    };

    private static readonly string[] PREFAB_FOLDERS = { "Assets/03. Prefabs" };
    private static readonly string[] SCENE_FOLDERS = { "Assets/01. Scenes" };

    [MenuItem("VELO/Font/본문 폰트를 NotoSansKR로 교체")]
    public static void ReplaceAll()
    {
        TMP_FontAsset target = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TARGET_PATH);

        if (target == null)
        {
            Debug.LogError($"[FontAssetReplacer] 먼저 'VELO/Font/NotoSansKR SDF 만들기'로 폰트 에셋을 만들어 주세요: {TARGET_PATH}");
            return;
        }

        HashSet<TMP_FontAsset> sources = LoadSources();

        if (sources.Count == 0)
        {
            Debug.LogError("[FontAssetReplacer] 바꿀 원본 폰트 에셋을 찾지 못했습니다.");
            return;
        }

        int prefabCount = ReplaceInPrefabs(sources, target);
        int sceneCount = ReplaceInScenes(sources, target);

        AssetDatabase.SaveAssets();
        Debug.Log($"[FontAssetReplacer] 프리팹 {prefabCount}개, 씬 {sceneCount}개에서 폰트를 바꿨습니다.");
    }

    private static HashSet<TMP_FontAsset> LoadSources()
    {
        HashSet<TMP_FontAsset> sources = new HashSet<TMP_FontAsset>();

        foreach (string path in SOURCE_PATHS)
        {
            TMP_FontAsset asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);

            if (asset != null)
            {
                sources.Add(asset);
            }
        }

        return sources;
    }

    /// <summary>
    /// 프리팹을 먼저 바꿉니다. 씬에 놓인 프리팹 인스턴스는 원본이 바뀌면 따라오므로,
    /// 순서를 뒤집으면 씬마다 불필요한 오버라이드가 생깁니다.
    /// </summary>
    private static int ReplaceInPrefabs(HashSet<TMP_FontAsset> sources, TMP_FontAsset target)
    {
        int changed = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", PREFAB_FOLDERS))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);

            if (Replace(root.GetComponentsInChildren<TMP_Text>(true), sources, target))
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                changed++;
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        return changed;
    }

    private static int ReplaceInScenes(HashSet<TMP_FontAsset> sources, TMP_FontAsset target)
    {
        string openedScene = SceneManager.GetActiveScene().path;
        int changed = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:Scene", SCENE_FOLDERS))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            bool dirty = false;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                dirty |= Replace(root.GetComponentsInChildren<TMP_Text>(true), sources, target);
            }

            if (dirty)
            {
                EditorSceneManager.SaveScene(scene);
                changed++;
            }
        }

        if (!string.IsNullOrEmpty(openedScene))
        {
            EditorSceneManager.OpenScene(openedScene, OpenSceneMode.Single);
        }

        return changed;
    }

    /// <summary>
    /// font에 넣으면 TMP가 그 폰트의 기본 머티리얼을 함께 물려 줍니다.
    /// fontSharedMaterial까지 명시하는 것은, 앞서 다른 머티리얼로 덮어 둔 텍스트를 되돌리기 위해서입니다.
    /// </summary>
    private static bool Replace(TMP_Text[] texts, HashSet<TMP_FontAsset> sources, TMP_FontAsset target)
    {
        bool changed = false;

        foreach (TMP_Text text in texts)
        {
            if (text == null || !sources.Contains(text.font))
            {
                continue;
            }

            Undo.RecordObject(text, "Replace Font");
            text.font = target;
            text.fontSharedMaterial = target.material;
            EditorUtility.SetDirty(text);
            changed = true;
        }

        return changed;
    }
}
