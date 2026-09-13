using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 홈 내비게이션 버튼에 언어 전환 장치를 달아 둡니다.
///
/// 버튼마다 글자 오브젝트를 만들고 참조를 물리는 일을 손으로 하면 세 번 반복해야 하고,
/// 자리와 크기를 눈대중으로 맞추게 됩니다. 원본 그림에서 잰 값을 여기 적어 두고 한 번에 답니다.
/// </summary>
public static class NavButtonLocalizeSetup
{
    private const string PANEL = "Assets/03. Prefabs/03-02. UI/03-02-01. Home/P_UI_Panel_Navigation.prefab";
    private const string IMAGE_FOLDER = "Assets/04. Images/04-02. UI/04-02-01. Home/";
    private const string FONT_PATH = "Assets/10. Fonts/NotoSansKR-Regular SDF.asset";
    private const string LABEL_NAME = "Text_NavLabel";

    /// <summary>
    /// 원본 그림(357x129)에서 잰 한글 자리입니다. 글자는 왼쪽 끝을 맞춰 아래 영문과 같은 선에 섭니다.
    /// </summary>
    private const float LABEL_ANCHOR_X = 0.294f;

    private const float LABEL_ANCHOR_Y = 0.597f;
    private const float LABEL_WIDTH = 180f;
    private const float LABEL_HEIGHT = 34f;
    private const int LABEL_FONT_SIZE = 40;
    private const int LABEL_MIN_FONT_SIZE = 14;

    private static readonly string[,] BUTTONS =
    {
        { "P_UI_Button_Space", "Image_Home_NavButton_Space", "스페이스" },
        { "P_UI_Button_Shop", "Image_Home_NavButton_Shop", "상점" },
        { "P_UI_Button_Collection", "Image_Home_NavButton_Collection", "컬렉션 (도감)" },
    };

    [MenuItem("VELO/Localization/홈 내비게이션 버튼 언어 전환 달기")]
    public static void Apply()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
        GameObject root = PrefabUtility.LoadPrefabContents(PANEL);
        int done = 0;

        for (int i = 0; i < BUTTONS.GetLength(0); i++)
        {
            Transform node = root.transform.Find(BUTTONS[i, 0]);

            if (node == null)
            {
                Debug.LogWarning($"[NavButtonLocalizeSetup] 버튼을 찾지 못했습니다: {BUTTONS[i, 0]}");
                continue;
            }

            Sprite korean = LoadSprite(BUTTONS[i, 1] + ".png");
            Sprite localized = LoadSprite(BUTTONS[i, 1] + "_NoKorean.png");

            if (korean == null || localized == null)
            {
                continue;
            }

            Bind(node, font, korean, localized, BUTTONS[i, 2]);
            done++;
        }

        if (0 < done)
        {
            PrefabUtility.SaveAsPrefabAsset(root, PANEL);
        }

        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[NavButtonLocalizeSetup] 버튼 {done}개에 달았습니다.");
    }

    private static Sprite LoadSprite(string fileName)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(IMAGE_FOLDER + fileName);

        if (sprite == null)
        {
            Debug.LogError($"[NavButtonLocalizeSetup] 그림을 찾지 못했습니다: {IMAGE_FOLDER}{fileName}");
        }

        return sprite;
    }

    private static void Bind(Transform node, TMP_FontAsset font, Sprite korean, Sprite localized, string text)
    {
        TMP_Text label = FindOrCreateLabel(node, font);
        UI_LocalizedNavButton owner = node.GetComponent<UI_LocalizedNavButton>();

        if (owner == null)
        {
            owner = node.gameObject.AddComponent<UI_LocalizedNavButton>();
        }

        SerializedObject serialized = new SerializedObject(owner);
        serialized.FindProperty("_background").objectReferenceValue = node.GetComponent<Image>();
        serialized.FindProperty("_label").objectReferenceValue = label;
        serialized.FindProperty("_koreanSprite").objectReferenceValue = korean;
        serialized.FindProperty("_localizedSprite").objectReferenceValue = localized;
        serialized.FindProperty("_korean").stringValue = text;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static TMP_Text FindOrCreateLabel(Transform node, TMP_FontAsset font)
    {
        Transform found = node.Find(LABEL_NAME);

        if (found != null)
        {
            return found.GetComponent<TMP_Text>();
        }

        GameObject go = new GameObject(LABEL_NAME, typeof(RectTransform));
        go.transform.SetParent(node, false);

        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = new Vector2(LABEL_ANCHOR_X, LABEL_ANCHOR_Y);
        rect.anchorMax = rect.anchorMin;
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(LABEL_WIDTH, LABEL_HEIGHT);

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.color = new Color(0.16f, 0.13f, 0.42f, 1f);
        text.alignment = TextAlignmentOptions.Left;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        text.enableAutoSizing = true;
        text.fontSizeMax = LABEL_FONT_SIZE;
        text.fontSizeMin = LABEL_MIN_FONT_SIZE;
        text.fontSize = LABEL_FONT_SIZE;

        return text;
    }
}
