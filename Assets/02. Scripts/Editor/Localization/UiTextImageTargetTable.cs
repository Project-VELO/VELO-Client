/// <summary>
/// TMP로 바꿀 글자 그림의 목록입니다. 크기와 색은 바꾸기 전에 원본 그림에서 뽑아 적어 둔 값입니다.
///
/// 바꾸는 일(UiTextImageConverter)과 나눈 것은 목록이 앞으로 늘기 때문입니다.
/// 아이콘이 함께 구워진 그림에 일본어판이 들어오면 여기에 줄을 더하게 됩니다.
/// </summary>
public static class UiTextImageTargetTable
{
    private const string HOME = "Assets/03. Prefabs/03-02. UI/03-02-01. Home/";
    private const string STORY = "Assets/03. Prefabs/03-02. UI/03-02-04. Story/";
    private const string MUSIC_SELECT = "Assets/03. Prefabs/03-02. UI/03-02-09. MusicSelect/";
    private const string LIVE_SCENE = "Assets/01. Scenes/01-02. Sub/10_LiveScene.unity";

    /// <summary>일시정지 팝업이 씬에 직접 놓여 있어 프리팹과 찾는 방식이 다릅니다.</summary>
    private const string PAUSE_ROOT = "======= [ Canvas ] =======/P_UI_Live_PausePopup/Panel_Pause/";

    public static readonly UiTextImageTarget[] Prefabs =
    {
        new UiTextImageTarget(HOME + "P_UI_Panel_StoryBox.prefab",
            "Button_Continue/Image_Label", "이야기 보러가기", 21, "#F8F8F8"),
        new UiTextImageTarget(HOME + "P_UI_Panel_StoryBox.prefab",
            "Image_Text_Title", "스토리", 78, "#F8F8F8", 1, true),
        new UiTextImageTarget(STORY + "P_UI_Button_Log.prefab",
            "Image_Text_ScriptCheck", "스크립트 확인", 31, "#584878"),
        new UiTextImageTarget(STORY + "P_UI_Popup_StoryReward.prefab",
            "Panel/TextBlock_Reward", "보상 획득", 60, "#6848B0", 2, true),
        new UiTextImageTarget(STORY + "P_UI_Popup_StoryReward.prefab",
            "Panel/TextBlock_Ment", "스토리 감상을 완료하여 보상을 획득했습니다.", 26, "#606060"),
        new UiTextImageTarget(STORY + "P_UI_Popup_StoryReward.prefab",
            "Panel/Button/Image_Text_Confirm", "확인", 42, "#6848B0", 2, true),
        new UiTextImageTarget(MUSIC_SELECT + "P_UI_Popup_PhotocardSelect.prefab",
            "P_UI_Panel_Studio/P_UI_Panel_ItemSetting/P_UI_Button_Reset/Image_ButtonText",
            "초기화", 26, "#5038E8", 2, true),
        new UiTextImageTarget(MUSIC_SELECT + "P_UI_Popup_PhotocardSelect.prefab",
            "P_UI_Panel_Studio/P_UI_Panel_PhotocardSetting/P_UI_Button_Reset/Image_ButtonText",
            "초기화", 26, "#5038E8", 2, true),
    };

    public static readonly UiTextImageTarget[] Scenes =
    {
        new UiTextImageTarget(LIVE_SCENE, PAUSE_ROOT + "Image_Title", "일시정지", 60, "#302850", 2, true),
        new UiTextImageTarget(LIVE_SCENE, PAUSE_ROOT + "Button_Quit/Image_Label", "나가기", 43, "#282048", 2, true),
        new UiTextImageTarget(LIVE_SCENE, PAUSE_ROOT + "Button_Restart/Image_Label", "처음부터", 44, "#000000", 2, true),
        new UiTextImageTarget(LIVE_SCENE, PAUSE_ROOT + "Button_Resume/Image_Label", "계속하기", 43, "#282048", 2, true),
    };
}
