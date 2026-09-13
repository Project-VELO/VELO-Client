using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 굵기 표(m_FontWeightTable)와 폴백을 배선합니다.
///
/// TMP는 Bold로 지정된 글자를 그릴 때 이 표의 700 칸을 먼저 찾습니다. 비어 있으면 원본 글리프를
/// 기하학적으로 부풀린 가짜 굵게로 대신하는데, 한글은 획이 많아 속공간이 메워져 뭉개집니다.
/// 결과 화면의 판정 수치가 Bold로 지정돼 있는데도 또렷하지 않던 것이 이 때문입니다.
///
/// 표를 채워 두면 프리팹과 씬을 건드리지 않고도 이미 Bold로 지정된 자리가 전부 진짜 글리프로 바뀝니다.
/// </summary>
public static class FontWeightTableBinder
{
    private const string FONT_FOLDER = "Assets/10. Fonts";

    /// <summary>
    /// TMP는 굵기를 100 단위로 나눈 값을 표의 첨자로 씁니다(TMP_Text.GetFontAssetForWeight).
    /// Bold는 700이라 7번 칸입니다.
    /// </summary>
    private const int BOLD_WEIGHT_INDEX = 7;

    [MenuItem("VELO/Font/굵기 표에 Bold 물리기")]
    public static void SetBoldWeights()
    {
        SetFallback("NotoSansKR-Bold SDF", "NotoSansJP-Bold SDF");
        SetWeightSlot("NotoSansKR-Regular SDF", "NotoSansKR-Bold SDF");
        SetWeightSlot("NotoSansKR-Medium SDF", "NotoSansKR-Bold SDF");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 스토리 본문이 쓰는 SemiBold에도 같은 굵기의 일본어를 걸어 둡니다.
    /// 굵기 표는 건드리지 않습니다. 본문 자체가 SemiBold라 Bold 지정이 따로 없기 때문입니다.
    /// </summary>
    [MenuItem("VELO/Font/스토리 SemiBold 폴백 걸기")]
    public static void SetStoryFallback()
    {
        SetFallback("NotoSerifKR-SemiBold SDF", "NotoSerifJP-SemiBold SDF");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void SetWeightSlot(string baseAssetName, string boldAssetName)
    {
        TMP_FontAsset baseFont = Load(baseAssetName);
        TMP_FontAsset boldFont = Load(boldAssetName);

        if (baseFont == null || boldFont == null)
        {
            return;
        }

        SerializedObject serialized = new SerializedObject(baseFont);
        SerializedProperty table = serialized.FindProperty("m_FontWeightTable");

        if (table == null || table.arraySize <= BOLD_WEIGHT_INDEX)
        {
            Debug.LogError($"[FontWeightTableBinder] 굵기 표가 없거나 칸이 모자랍니다: {baseAssetName}");
            return;
        }

        SerializedProperty slot = table.GetArrayElementAtIndex(BOLD_WEIGHT_INDEX).FindPropertyRelative("regularTypeface");

        if (slot == null)
        {
            Debug.LogError($"[FontWeightTableBinder] regularTypeface 칸을 찾지 못했습니다: {baseAssetName}");
            return;
        }

        slot.objectReferenceValue = boldFont;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(baseFont);

        Debug.Log($"[FontWeightTableBinder] {baseAssetName}의 700 칸에 {boldAssetName}를 물렸습니다.");
    }

    /// <summary>
    /// 한국어 폰트에 없는 일본어 신자체 한자를 메웁니다.
    ///
    /// 굵은 에셋에는 굵은 일본어를 걸어야 합니다. Regular 폴백을 그대로 두면 같은 문장 안에서
    /// 일본어 구간만 얇게 튑니다.
    /// </summary>
    private static void SetFallback(string targetAssetName, string fallbackAssetName)
    {
        TMP_FontAsset target = Load(targetAssetName);
        TMP_FontAsset fallback = Load(fallbackAssetName);

        if (target == null || fallback == null)
        {
            return;
        }

        target.fallbackFontAssetTable = new List<TMP_FontAsset> { fallback };
        EditorUtility.SetDirty(target);

        Debug.Log($"[FontWeightTableBinder] {targetAssetName}의 폴백을 {fallbackAssetName}로 걸었습니다.");
    }

    private static TMP_FontAsset Load(string assetName)
    {
        string path = $"{FONT_FOLDER}/{assetName}.asset";
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);

        if (fontAsset == null)
        {
            Debug.LogError($"[FontWeightTableBinder] 폰트 에셋을 찾지 못했습니다: {path}");
        }

        return fontAsset;
    }
}
