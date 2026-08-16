/// <summary>
/// 기획 시트(TSV)의 컬럼 이름입니다.
///
/// 파서에서 떼어 낸 이유는, 시트에 열이 하나 늘 때마다 파싱 로직과 무관한 상수만 불어나
/// StoryScriptTsvParser가 단일 책임을 넘어서기 때문입니다. 여기는 "시트에 어떤 열이 있는가"만 압니다.
///
/// 파서는 헤더 이름으로 위치를 찾으므로 이 목록의 선언 순서는 시트의 열 순서와 무관합니다.
/// </summary>
public static class StoryScriptTsvColumns
{
    public const string STORY_ID = "storyId";
    public const string LINE_ID = "lineId";
    public const string LINE_TYPE = "lineType";
    public const string SPEAKER_ID = "speakerId";

    /// <summary>
    /// 원고에 적힌 화자 표기입니다. characters.json에 없는 단역의 이름이 여기에만 있습니다.
    /// </summary>
    public const string SPEAKER_RAW = "speakerRaw";
    public const string TEXT = "text";
    public const string BACKGROUND_ID = "backgroundId";

    /// <summary>
    /// 인물이 서는 네 자리입니다. 왼쪽만 접두사가 없는 것은 기존 시트와의 호환 때문입니다.
    /// 바뀌는 줄에만 적으므로 대부분의 행에서 비어 있습니다.
    /// </summary>
    public const string CHARACTER_ID = "characterId";
    public const string EXPRESSION_ID = "expressionId";
    public const string CENTER_CHARACTER_ID = "centerCharacterId";
    public const string CENTER_EXPRESSION_ID = "centerExpressionId";
    public const string RIGHT_CHARACTER_ID = "rightCharacterId";
    public const string RIGHT_EXPRESSION_ID = "rightExpressionId";

    /// <summary>
    /// 화면 위쪽 슬롯입니다. 공중에 걸리는 인물이 있는 줄에만 채웁니다.
    /// </summary>
    public const string UPPER_CHARACTER_ID = "upperCharacterId";
    public const string UPPER_EXPRESSION_ID = "upperExpressionId";
    public const string ILLUSTRATION_ID = "illustrationId";

    /// <summary>
    /// 연출표의 [속도]·[디자인]·[화면 이펙트]·[사운드] 열입니다.
    /// 기존 시트에는 없던 컬럼이라 비어 있어도 오류로 보지 않습니다(속도는 NORMAL로 떨어집니다).
    /// </summary>
    public const string TEXT_SPEED = "textSpeed";
    public const string TEXT_PLACEMENT = "textPlacement";
    public const string TEXT_STYLE_ID = "textStyleId";
    public const string EFFECT_ID = "effectId";
    public const string BGM_ID = "bgmId";
    public const string SFX_ID = "sfxId";
}
