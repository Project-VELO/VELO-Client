using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// TMP SDF 폰트 에셋을 만듭니다.
///
/// Font Asset Creator 창에서 손으로 값을 맞추면 폰트마다 설정이 어긋납니다. 기존
/// NotoSerifKR-Regular SDF와 같은 값으로 고정해 두고 메뉴 한 번으로 만듭니다.
///
/// 동적(Dynamic) 모드로 만듭니다. 정적으로 구우면 GowunBatang처럼 에셋 하나가 40MB에 육박해
/// 저장소에 그대로 쌓입니다. 한글은 글자 수가 많아 정적 아틀라스가 특히 큽니다.
/// </summary>
public static class FontSdfAssetBuilder
{
    private const string FONT_FOLDER = "Assets/10. Fonts";

    /// <summary>
    /// 아래 값은 기존 NotoSerifKR-Regular SDF에서 그대로 가져왔습니다. 폰트마다 다르면
    /// 같은 크기로 찍어도 두께와 번짐이 달라 보입니다.
    /// </summary>
    private const int SAMPLING_POINT_SIZE = 28;

    private const int ATLAS_PADDING = 5;
    private const int ATLAS_SIZE = 1024;

    [MenuItem("VELO/Font/NotoSansKR SDF 만들기")]
    public static void CreateNotoSansKr()
    {
        Create("NotoSansKR-Regular.ttf");
    }

    private static void Create(string sourceFileName)
    {
        string sourcePath = Path.Combine(FONT_FOLDER, sourceFileName).Replace('\\', '/');
        Font source = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);

        if (source == null)
        {
            Debug.LogError($"[FontSdfAssetBuilder] 원본 폰트를 찾지 못했습니다: {sourcePath}");
            return;
        }

        string assetPath = $"{FONT_FOLDER}/{Path.GetFileNameWithoutExtension(sourceFileName)} SDF.asset";

        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null)
        {
            Debug.LogWarning($"[FontSdfAssetBuilder] 이미 있습니다. 다시 만들려면 먼저 지워 주세요: {assetPath}");
            return;
        }

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            source,
            SAMPLING_POINT_SIZE,
            ATLAS_PADDING,
            GlyphRenderMode.SDFAA,
            ATLAS_SIZE,
            ATLAS_SIZE,
            AtlasPopulationMode.Dynamic,
            enableMultiAtlasSupport: true);

        if (fontAsset == null)
        {
            Debug.LogError($"[FontSdfAssetBuilder] 폰트 에셋 생성에 실패했습니다: {sourcePath}");
            return;
        }

        AssetDatabase.CreateAsset(fontAsset, assetPath);
        AttachSubAssets(fontAsset, assetPath);
        ClearDynamicDataOnBuild(fontAsset);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[FontSdfAssetBuilder] 만들었습니다: {assetPath}");
        EditorGUIUtility.PingObject(fontAsset);
    }

    /// <summary>
    /// 아틀라스 텍스처와 머티리얼은 에셋 파일 안에 함께 들어가야 합니다.
    /// 붙이지 않으면 저장 시점에 사라져, 다음에 열 때 글자가 나오지 않습니다.
    /// </summary>
    private static void AttachSubAssets(TMP_FontAsset fontAsset, string assetPath)
    {
        string assetName = Path.GetFileNameWithoutExtension(assetPath);

        if (fontAsset.atlasTextures != null && 0 < fontAsset.atlasTextures.Length)
        {
            Texture2D atlas = fontAsset.atlasTextures[0];
            atlas.name = $"{assetName} Atlas";
            AssetDatabase.AddObjectToAsset(atlas, fontAsset);
        }

        if (fontAsset.material != null)
        {
            fontAsset.material.name = $"{assetName} Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }
    }

    /// <summary>
    /// 빌드에 동적으로 채워진 글자를 들고 가지 않게 합니다.
    ///
    /// 켜 두지 않으면 에디터에서 띄워 본 글자가 아틀라스에 남아 에셋 파일이 계속 커지고,
    /// 그때마다 형상 관리에 잡혀 사람마다 다른 내용이 올라갑니다.
    /// 프로퍼티가 TMP 버전마다 있고 없고 해서 직렬화 필드로 직접 켭니다.
    /// </summary>
    private static void ClearDynamicDataOnBuild(TMP_FontAsset fontAsset)
    {
        SerializedObject serialized = new SerializedObject(fontAsset);
        SerializedProperty property = serialized.FindProperty("m_ClearDynamicDataOnBuild");

        if (property == null)
        {
            Debug.LogWarning("[FontSdfAssetBuilder] m_ClearDynamicDataOnBuild 필드를 찾지 못했습니다. 인스펙터에서 직접 켜 주세요.");
            return;
        }

        property.boolValue = true;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
