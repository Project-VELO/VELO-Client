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
    /// 표정 키를 잇는 구분자입니다("CHAR_RIA/EXP_SAD").
    /// ID에 쓰이지 않는 문자여야 인물 키와 표정 키가 섞이지 않습니다.
    /// </summary>
    private const string EXPRESSION_KEY_SEPARATOR = "/";

    [Foldout("Project")]
    [Header("실제 아트가 들어오면 여기부터 채웁니다")]
    [SerializeField]
    private SerializableDictionary<string, Sprite> _backgroundSprites = new SerializableDictionary<string, Sprite>();

    [SerializeField]
    private SerializableDictionary<string, Sprite> _characterSprites = new SerializableDictionary<string, Sprite>();

    /// <summary>
    /// 얼굴만 담은 정사각 초상입니다. 대화 로그처럼 작은 원형 칸에 넣는 자리에서 씁니다.
    ///
    /// 전신 표와 나눈 이유는 그림의 구도가 다르기 때문입니다.
    /// 전신을 작은 칸에 넣으면 인물이 아주 작게 들어가거나 몸통만 잘려 누구인지 알 수 없습니다.
    /// </summary>
    [SerializeField]
    private SerializableDictionary<string, Sprite> _characterFaceSprites = new SerializableDictionary<string, Sprite>();

    [Foldout("Settings")]
    /// <summary>
    /// 표정을 무시하고 인물의 기본 그림만 씁니다.
    ///
    /// 표정별 원본의 구도가 서로 다릅니다. 기본과 EXP_NORMAL은 허리 위까지지만 나머지 넷은
    /// 전신이라, 같은 인물인데도 줄이 넘어갈 때마다 인물 크기와 구도가 튑니다.
    /// 표정 그림이 같은 구도로 다시 들어오면 이 값을 꺼 주세요.
    /// </summary>
    [SerializeField]
    private bool _useDefaultExpressionOnly = true;

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
    /// 캐릭터 스프라이트를 돌려줍니다.
    ///
    /// "CHAR_RIA/EXP_SAD"처럼 표정을 붙인 키를 먼저 찾고, 없으면 인물 키만으로 되짚습니다.
    /// 인물마다 그려진 표정의 수가 달라, 없는 표정이 빈 화면으로 떨어지지 않게 하기 위한 순서입니다.
    ///
    /// 표를 인물용과 표정용으로 나누지 않은 것은, 표정 그림이 없는 인물까지 빈 표를 들고 다니게 되기 때문입니다.
    /// </summary>
    public Sprite GetCharacter(string characterId, string expressionId)
    {
        if (!_useDefaultExpressionOnly && !string.IsNullOrEmpty(expressionId))
        {
            Sprite withExpression = TryGet(_characterSprites, BuildExpressionKey(characterId, expressionId));

            if (withExpression != null)
            {
                return withExpression;
            }
        }

        return TryGet(_characterSprites, characterId);
    }

    /// <summary>
    /// 표에 넣는 표정 키를 만듭니다. 대사 한 줄이 넘어갈 때만 부르므로 문자열 결합이 문제가 되지 않습니다.
    /// </summary>
    private string BuildExpressionKey(string characterId, string expressionId)
    {
        return characterId + EXPRESSION_KEY_SEPARATOR + expressionId;
    }

    /// <summary>
    /// 얼굴 초상을 돌려줍니다. 없으면 null이며, 호출부는 임시 색으로 대체합니다.
    ///
    /// 전신으로 되짚지 않는 것은 의도한 것입니다. 작은 원형 칸에 전신이 들어가면
    /// 인물의 다리나 배경만 보여, 없는 것보다 알아보기 어렵습니다.
    /// </summary>
    public Sprite GetCharacterFace(string characterId)
    {
        return TryGet(_characterFaceSprites, characterId);
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
