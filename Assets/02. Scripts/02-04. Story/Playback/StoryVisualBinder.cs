using UnityEngine;
using VInspector;

/// <summary>
/// 대본의 ID를 화면에 그릴 이미지로 바꿉니다.
///
/// 아직 배경·초상 스프라이트가 없습니다. 전부 같은 회색으로 떨어뜨리면 2인 대화에서 화자가
/// 바뀌었는지 화면만 보고 알 수 없으므로, 스프라이트가 없을 때는 캐릭터마다 다른 색의 단색
/// 실루엣으로 대체합니다.
///
/// 색은 실제 아트가 들어오면 버릴 개발용 값이라 characters.json에 넣지 않고 여기 인스펙터에만 둡니다.
/// </summary>
public class StoryVisualBinder : MonoBehaviour
{
    /// <summary>
    /// 표정별 항목을 "CHAR_RIA/EXP_SMILE" 형태로 구분합니다.
    /// 캐릭터 ID와 표정 ID 어디에도 쓰이지 않는 글자여야 키가 겹치지 않습니다.
    /// </summary>
    private const string EXPRESSION_KEY_SEPARATOR = "/";

    [Foldout("Project")]
    [Header("실제 아트가 들어오면 여기부터 채웁니다")]
    [SerializeField]
    private SerializableDictionary<string, Sprite> _backgroundSprites = new SerializableDictionary<string, Sprite>();

    [SerializeField]
    private SerializableDictionary<string, Sprite> _characterSprites = new SerializableDictionary<string, Sprite>();

    [Foldout("Settings")]
    [Header("스프라이트가 없을 때 쓰는 임시 색")]
    [SerializeField]
    private SerializableDictionary<string, Color> _characterPlaceholderColors = new SerializableDictionary<string, Color>();

    /// <summary>
    /// 표에 없는 화자가 떨어지는 색입니다. 여기로 떨어졌다는 것 자체가 표를 채워야 한다는 신호입니다.
    /// </summary>
    [SerializeField]
    private Color _unknownCharacterColor = new Color(0.45f, 0.45f, 0.48f, 1f);

    [SerializeField]
    private Color _backgroundPlaceholderColor = new Color(0.12f, 0.11f, 0.16f, 1f);

    /// <summary>
    /// 배경 스프라이트를 돌려줍니다. 없으면 null이며 호출부는 색으로 대체합니다(기획서 3-L "기본 배경 표시").
    /// </summary>
    public Sprite GetBackground(string backgroundId)
    {
        return TryGet(_backgroundSprites, backgroundId);
    }

    public Color BackgroundPlaceholderColor => _backgroundPlaceholderColor;

    /// <summary>
    /// 캐릭터 스프라이트를 돌려줍니다. 표정별 이미지가 있으면 그것을 쓰고,
    /// 없으면 표정 구분 없는 한 장으로 돌아갑니다.
    ///
    /// 표정이 다 갖춰지지 않은 인물이 있어(단역, 표정 한두 개만 그려진 인물) 조합 키만 두면
    /// 그 인물의 나머지 대사가 통째로 비어 실루엣 색으로 떨어집니다. 그래서 캐릭터 단독 키를
    /// 기본 그림 자리로 남겨 둡니다.
    /// </summary>
    public Sprite GetCharacter(string characterId, string expressionId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return null;
        }

        if (!string.IsNullOrEmpty(expressionId))
        {
            Sprite expressionSprite = TryGet(_characterSprites, BuildExpressionKey(characterId, expressionId));
            if (expressionSprite != null)
            {
                return expressionSprite;
            }
        }

        return TryGet(_characterSprites, characterId);
    }

    /// <summary>
    /// 표정별 항목의 키입니다. 대사 한 줄마다 한 번만 만들어지므로 문자열 결합으로 충분합니다.
    /// </summary>
    private static string BuildExpressionKey(string characterId, string expressionId)
    {
        return characterId + EXPRESSION_KEY_SEPARATOR + expressionId;
    }

    public Color GetCharacterPlaceholderColor(string characterId)
    {
        if (!string.IsNullOrEmpty(characterId)
            && _characterPlaceholderColors.TryGetValue(characterId, out Color color))
        {
            return color;
        }

        return _unknownCharacterColor;
    }

    private Sprite TryGet(SerializableDictionary<string, Sprite> table, string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        return table.TryGetValue(id, out Sprite sprite) ? sprite : null;
    }
}
