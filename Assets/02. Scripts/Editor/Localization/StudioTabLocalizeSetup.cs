using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 스튜디오 탭의 글자를 그림에서 빼내 TMP로 돌립니다.
///
/// 탭 배경 넉 장(두 탭 x 선택·비선택)에 한글이 구워져 있었습니다. 글자를 지운 판으로 바꾸고,
/// 프리팹에 꺼진 채 놓여 있던 글자 오브젝트를 켜서 그 자리를 맡깁니다.
///
/// 배경을 네 장 모두 바꾸는 것은 선택 상태가 배경으로 드러나기 때문입니다. 한 장만 바꾸면
/// 탭을 누를 때마다 글자가 한 번은 그림에서, 한 번은 TMP에서 나옵니다.
/// </summary>
public static class StudioTabLocalizeSetup
{
    /// <summary>
    /// 스튜디오 화면과 라이브 준비 팝업 둘 다 손봐야 합니다. 팝업은 스튜디오 패널을 중첩한 것이
    /// 아니라 언팩한 복사본이라, 한쪽만 고치면 다른 쪽에 한글이 그대로 남습니다.
    /// </summary>
    private static readonly string[,] PANELS =
    {
        { "Assets/03. Prefabs/03-02. UI/03-02-07. Studio/P_UI_Panel_Studio.prefab", "Layoutgroup_Horizontal_Buttons" },
        { "Assets/03. Prefabs/03-02. UI/03-02-09. MusicSelect/P_UI_Popup_PhotocardSelect.prefab", "P_UI_Panel_Studio/Layoutgroup_Horizontal_Buttons" },
    };
    private const string FONT_PATH = "Assets/10. Fonts/NotoSansKR-Regular SDF.asset";
    private const string IMAGE_FOLDER = "Assets/04. Images/04-02. UI/04-02-09. MusicSelect/Setting/";

    private const int LABEL_FONT_SIZE = 30;
    private const int LABEL_MIN_FONT_SIZE = 14;

    /// <summary>탭 안에서 글자가 놓이는 자리입니다. 원본 그림(712x58)에서 가운데 정렬이었습니다.</summary>
    private const float LABEL_WIDTH = 420f;

    private const float LABEL_HEIGHT = 42f;

    private static readonly string[,] TABS =
    {
        { "P_UI_Button_PhotocardSetting", "_photocardTabLabel", "_photocardSelectedSprite", "Image_Setting_Tab_Photocard_Selected", "1. 포토카드 세팅" },
        { "P_UI_Button_PhotocardSetting", "_photocardTabLabel", "_photocardNormalSprite", "Image_Setting_Tab_Photocard_Normal", "1. 포토카드 세팅" },
        { "P_UI_Button_ItemSetting", "_itemTabLabel", "_itemSelectedSprite", "Image_Setting_Tab_Costume_Selected", "2. 의상 & 악세서리" },
        { "P_UI_Button_ItemSetting", "_itemTabLabel", "_itemNormalSprite", "Image_Setting_Tab_Costume_Normal", "2. 의상 & 악세서리" },
    };

    [MenuItem("VELO/Localization/스튜디오 탭 글자를 TMP로 돌리기")]
    public static void Apply()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);

        for (int i = 0; i < PANELS.GetLength(0); i++)
        {
            ApplyTo(PANELS[i, 0], PANELS[i, 1], font);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ApplyTo(string panelPath, string tabsNode, TMP_FontAsset font)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(panelPath);
        Transform tabs = root.transform.Find(tabsNode);

        if (tabs == null)
        {
            Debug.LogError($"[StudioTabLocalizeSetup] 탭 묶음을 찾지 못했습니다: {panelPath} / {tabsNode}");
            PrefabUtility.UnloadPrefabContents(root);
            return;
        }

        MonoBehaviour owner = FindTabsComponent(root);

        if (owner == null)
        {
            Debug.LogError($"[StudioTabLocalizeSetup] 탭 컴포넌트를 찾지 못했습니다: {panelPath}");
            PrefabUtility.UnloadPrefabContents(root);
            return;
        }

        SerializedObject serialized = new SerializedObject(owner);

        for (int i = 0; i < TABS.GetLength(0); i++)
        {
            Sprite erased = AssetDatabase.LoadAssetAtPath<Sprite>(IMAGE_FOLDER + TABS[i, 3] + "_NoKorean.png");

            if (erased == null)
            {
                Debug.LogError($"[StudioTabLocalizeSetup] 지운 판을 찾지 못했습니다: {TABS[i, 3]}_NoKorean.png");
                continue;
            }

            serialized.FindProperty(TABS[i, 2]).objectReferenceValue = erased;

            TMP_Text label = EnableLabel(tabs.Find(TABS[i, 0]), font, TABS[i, 4]);

            if (label != null)
            {
                serialized.FindProperty(TABS[i, 1]).objectReferenceValue = label;
            }
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(root, panelPath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log($"[StudioTabLocalizeSetup] 탭 글자를 TMP로 돌렸습니다: {System.IO.Path.GetFileName(panelPath)}");
    }

    /// <summary>
    /// 프리팹에 꺼진 채 놓여 있던 글자 오브젝트를 켜고 값을 맞춥니다.
    /// 새로 만들지 않는 것은 자리와 정렬이 이미 디자인대로 잡혀 있기 때문입니다.
    /// </summary>
    private static TMP_Text EnableLabel(Transform tab, TMP_FontAsset font, string korean)
    {
        if (tab == null)
        {
            return null;
        }

        Transform node = tab.Find("Text_Button");

        if (node == null)
        {
            Debug.LogWarning($"[StudioTabLocalizeSetup] Text_Button을 찾지 못했습니다: {tab.name}");
            return null;
        }

        node.gameObject.SetActive(true);

        TMP_Text label = node.GetComponent<TMP_Text>();

        if (label == null)
        {
            return null;
        }

        label.font = font;
        label.text = korean;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = false;
        label.raycastTarget = false;
        label.enableAutoSizing = true;
        label.fontSizeMax = LABEL_FONT_SIZE;
        label.fontSizeMin = LABEL_MIN_FONT_SIZE;
        label.fontSize = LABEL_FONT_SIZE;
        ((RectTransform)node).sizeDelta = new Vector2(LABEL_WIDTH, LABEL_HEIGHT);

        return label;
    }

    /// <summary>
    /// 탭을 맡은 컴포넌트를 찾습니다. 클래스 이름으로 찾지 않는 것은 스튜디오 화면과 라이브 준비
    /// 팝업이 이름만 다른 같은 모양의 컴포넌트를 각자 쓰고 있기 때문입니다. 칸 이름으로 알아봅니다.
    /// </summary>
    private static MonoBehaviour FindTabsComponent(GameObject root)
    {
        MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] == null)
            {
                continue;
            }

            SerializedObject probe = new SerializedObject(behaviours[i]);

            if (probe.FindProperty("_photocardSelectedSprite") != null)
            {
                return behaviours[i];
            }
        }

        return null;
    }
}
