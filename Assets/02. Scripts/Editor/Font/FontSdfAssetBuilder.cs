using System.Collections.Generic;
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
    /// 구글 폰트에서 받은 폴더 구조 그대로입니다. Create가 FONT_FOLDER 아래로 경로를 이어 붙이므로,
    /// 하위 폴더에 있는 굵기도 파일명 자리에 그대로 넘기면 됩니다.
    /// </summary>
    private const string SANS_KR_STATIC = "Noto_Sans_KR/static";

    private const string SANS_JP_STATIC = "Noto_Sans_JP/static";
    private const string SERIF_KR_STATIC = "Noto_Serif_KR/static";
    private const string SERIF_JP_STATIC = "Noto_Serif_JP/static";

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
        Create("NotoSansKR-Medium.ttf");
    }

    /// <summary>
    /// 일본어 대사가 쓰는 신자체 한자는 한국어 폰트에 없습니다(NotoSerifKR 기준 104자).
    /// 이 에셋을 한국어 폰트의 폴백으로 걸어 두면 없는 글자만 여기서 나옵니다.
    ///
    /// 파일명이 아니라 짧은 이름으로 만드는 것은 원본이 가변 폰트라 이름에 축 정보가
    /// 붙어 있기 때문입니다("NotoSansJP-VariableFont_wght SDF"는 읽기 어렵습니다).
    /// </summary>
    [MenuItem("VELO/Font/일본어 SDF 만들기")]
    public static void CreateJapanese()
    {
        Create("NotoSansJP-VariableFont_wght.ttf", "NotoSansJP-Regular");
        Create("NotoSerifJP-VariableFont_wght.ttf", "NotoSerifJP-Regular");
    }

    /// <summary>
    /// 결과 화면의 점수와 판정 수치가 가늘다는 피드백에 쓸 굵기입니다.
    ///
    /// NotoSansKR-Regular SDF의 굵기 표 700 칸에 물려 두면, 이미 Bold로 지정돼 있던 수치들이
    /// TMP의 가짜 굵게 대신 진짜 Bold 글리프로 그려집니다.
    ///
    /// 일본어 쪽을 함께 만드는 것은 한국어 Bold의 폴백으로 걸기 위해서입니다. Regular 폴백을
    /// 그대로 두면 한 문장 안에서 일본어 구간만 얇게 튑니다.
    /// </summary>
    [MenuItem("VELO/Font/굵은 SDF 만들기")]
    public static void CreateBold()
    {
        Create($"{SANS_KR_STATIC}/NotoSansKR-Bold.ttf");
        Create($"{SANS_JP_STATIC}/NotoSansJP-Bold.ttf");
    }

    /// <summary>
    /// 스토리 본문이 가녀리다는 피드백에 쓸, Regular(400)와 Bold(700) 사이 굵기입니다.
    /// 어느 쪽이 맞는지는 눈으로 비교해야 정해지므로 Medium(500)과 SemiBold(600)를 함께 만듭니다.
    /// </summary>
    [MenuItem("VELO/Font/스토리 중간 굵기 SDF 만들기")]
    public static void CreateStoryMidWeights()
    {
        Create($"{SERIF_KR_STATIC}/NotoSerifKR-Medium.ttf");
        Create($"{SERIF_KR_STATIC}/NotoSerifKR-SemiBold.ttf");
        Create($"{SERIF_JP_STATIC}/NotoSerifJP-Medium.ttf");
        Create($"{SERIF_JP_STATIC}/NotoSerifJP-SemiBold.ttf");
    }

    private static void Create(string sourceFileName, string assetName = null)
    {
        string sourcePath = Path.Combine(FONT_FOLDER, sourceFileName).Replace('\\', '/');
        Font source = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);

        if (source == null)
        {
            Debug.LogError($"[FontSdfAssetBuilder] 원본 폰트를 찾지 못했습니다: {sourcePath}");
            return;
        }

        string name = string.IsNullOrEmpty(assetName) ? Path.GetFileNameWithoutExtension(sourceFileName) : assetName;
        string assetPath = $"{FONT_FOLDER}/{name} SDF.asset";

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
        ClearFallbacks(fontAsset);

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
    /// 폴백을 비워 둡니다.
    ///
    /// 고운바탕이 NotoSerifKR을 폴백으로 물고 있는 것은 한자 글리프가 없어서입니다(書·怪·談 등 16자).
    /// NotoSansKR은 프로젝트가 쓰는 글자를 전부 담고 있어 혼자로 충분하고, 동적 모드라 원본 TTF에
    /// 있는 글자는 필요할 때 스스로 구워 냅니다. 폴백을 걸면 서체가 섞여 나올 위험만 생깁니다.
    /// </summary>
    private static void ClearFallbacks(TMP_FontAsset fontAsset)
    {
        fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();
        EditorUtility.SetDirty(fontAsset);
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
