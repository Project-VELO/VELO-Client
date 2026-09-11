using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 한글이 구워진 글자 그림을 TMP 텍스트로 바꿉니다.
///
/// 그림에 글자가 박혀 있으면 언어를 바꿔도 한글이 그대로 남습니다. 일본어판 그림을 따로 그리는
/// 대신 TMP로 바꾸면 UiText의 번역 표에 얹혀 글자가 함께 바뀝니다.
///
/// 바꾸는 것은 글자만 든 그림뿐입니다. 아이콘이나 배경이 함께 구워진 그림은 TMP로 바꾸면
/// 그 그림이 사라지므로 표에 넣지 않았습니다(홈 내비게이션 세 장, 스크립트 확인 버튼,
/// 자동 편성과 초기화 버튼 네 장). 그쪽은 일본어판 그림이 필요합니다.
///
/// Image와 TextMeshProUGUI는 둘 다 Graphic이라 한 오브젝트에 함께 둘 수 없습니다.
/// 반드시 지운 뒤에 답니다.
///
/// 일정 카드의 '바로가기'와 '완료'도 표에 없습니다. 그 둘은 UI_ScheduleShortcutButton이
/// 완료 여부에 따라 Image의 스프라이트를 갈아 끼우고 SetNativeSize로 자리까지 잡는 구조라,
/// Image만 떼면 _label 참조가 끊겨 완료 상태에서도 '바로가기'가 그대로 남습니다.
/// 그쪽을 옮기려면 컴포넌트를 글자 기준으로 다시 써야 합니다.
/// </summary>
public static class UiTextImageConverter
{
    private const string FONT_PATH = "Assets/10. Fonts/NotoSansKR-Regular SDF.asset";

    /// <summary>
    /// 자동 크기 조절의 하한입니다. 이보다 작아지면 읽히지 않으므로, 여기까지 줄여도 넘치면
    /// 상자가 좁다는 뜻이고 자리를 손봐야 합니다.
    /// </summary>
    private const int MIN_FONT_SIZE = 10;

    [MenuItem("VELO/Localization/글자 그림을 TMP로 바꾸기")]
    public static void ConvertAll()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);

        if (font == null)
        {
            Debug.LogError($"[UiTextImageConverter] 폰트 에셋을 찾지 못했습니다: {FONT_PATH}");
            return;
        }

        int changed = ConvertPrefabs(font) + ConvertScenes(font);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[UiTextImageConverter] {changed}곳을 바꿨습니다.");
    }

    private static int ConvertPrefabs(TMP_FontAsset font)
    {
        Dictionary<string, List<UiTextImageTarget>> byAsset = GroupByAsset(UiTextImageTargetTable.Prefabs);
        int changed = 0;

        foreach (KeyValuePair<string, List<UiTextImageTarget>> pair in byAsset)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(pair.Key);

            if (root == null)
            {
                Debug.LogError($"[UiTextImageConverter] 프리팹을 열지 못했습니다: {pair.Key}");
                continue;
            }

            int applied = 0;

            foreach (UiTextImageTarget target in pair.Value)
            {
                applied += Convert(root.transform.Find(target.NodePath), target, font) ? 1 : 0;
            }

            if (0 < applied)
            {
                PrefabUtility.SaveAsPrefabAsset(root, pair.Key);
                changed += applied;
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        return changed;
    }

    private static int ConvertScenes(TMP_FontAsset font)
    {
        Dictionary<string, List<UiTextImageTarget>> byAsset = GroupByAsset(UiTextImageTargetTable.Scenes);
        int changed = 0;

        foreach (KeyValuePair<string, List<UiTextImageTarget>> pair in byAsset)
        {
            Scene scene = EditorSceneManager.OpenScene(pair.Key, OpenSceneMode.Additive);
            int applied = 0;

            foreach (UiTextImageTarget target in pair.Value)
            {
                applied += Convert(FindInScene(scene, target.NodePath), target, font) ? 1 : 0;
            }

            if (0 < applied)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                changed += applied;
            }

            EditorSceneManager.CloseScene(scene, true);
        }

        return changed;
    }

    /// <summary>
    /// 씬은 루트가 여럿이라 경로의 첫 칸으로 루트를 고른 뒤 나머지를 따라 내려갑니다.
    /// </summary>
    private static Transform FindInScene(Scene scene, string nodePath)
    {
        int split = nodePath.IndexOf('/');
        string rootName = 0 <= split ? nodePath.Substring(0, split) : nodePath;
        string rest = 0 <= split ? nodePath.Substring(split + 1) : string.Empty;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != rootName)
            {
                continue;
            }

            return string.IsNullOrEmpty(rest) ? root.transform : root.transform.Find(rest);
        }

        return null;
    }

    /// <summary>
    /// 이미 찾아 둔 오브젝트를 바꿉니다. 찾는 방식이 프리팹과 씬에서 다르므로 부르는 쪽이 찾아 넘깁니다.
    /// </summary>
    private static bool Convert(Transform node, UiTextImageTarget target, TMP_FontAsset font)
    {
        if (node == null)
        {
            Debug.LogWarning($"[UiTextImageConverter] 오브젝트를 찾지 못했습니다: {target.AssetPath} / {target.NodePath}");
            return false;
        }

        Image image = node.GetComponent<Image>();

        if (image != null)
        {
            Object.DestroyImmediate(image, true);
        }

        // 이미 바꿔 둔 자리는 값만 다시 맞춥니다. 크기나 색을 고쳐 다시 돌릴 때
        // 붙였다 떼면 다른 컴포넌트가 들고 있던 참조가 끊깁니다.
        TextMeshProUGUI text = node.GetComponent<TextMeshProUGUI>();

        if (text == null)
        {
            text = node.gameObject.AddComponent<TextMeshProUGUI>();
        }

        text.font = font;
        text.text = target.Korean;
        text.color = target.Color;
        // TMP의 정렬은 가로와 세로 값을 비트로 겹쳐 씁니다. 512는 세로 가운데입니다.
        text.alignment = (TextAlignmentOptions)(target.HorizontalAlignment | 512);
        text.enableWordWrapping = false;
        text.raycastTarget = false;

        // 원본 그림은 글자를 좁게 그려 두었고 일본어는 한국어보다 글자 수가 많습니다.
        // 고정 크기로 두면 어느 한쪽에서 반드시 상자를 넘칩니다. 상자에 맞춰 줄어들게 둡니다.
        text.enableAutoSizing = true;
        text.fontSizeMax = target.FontSize;
        text.fontSizeMin = MIN_FONT_SIZE;
        text.fontSize = target.FontSize;

        if (target.IsBold)
        {
            text.fontStyle = FontStyles.Bold;
        }

        return true;
    }

    private static Dictionary<string, List<UiTextImageTarget>> GroupByAsset(UiTextImageTarget[] targets)
    {
        Dictionary<string, List<UiTextImageTarget>> byAsset = new Dictionary<string, List<UiTextImageTarget>>();

        for (int i = 0; i < targets.Length; i++)
        {
            if (!byAsset.TryGetValue(targets[i].AssetPath, out List<UiTextImageTarget> list))
            {
                list = new List<UiTextImageTarget>();
                byAsset[targets[i].AssetPath] = list;
            }

            list.Add(targets[i]);
        }

        return byAsset;
    }
}
