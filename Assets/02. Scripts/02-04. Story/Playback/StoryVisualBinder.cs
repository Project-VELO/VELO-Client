using UnityEngine;
using VInspector;

/// <summary>
/// 대본의 ID를 화면에 그릴 이미지로 바꿉니다.
///
/// 현재 프로젝트에는 배경·초상 스프라이트가 하나도 없습니다. 그렇다고 전부 같은 회색으로 떨어뜨리면
/// 2인 대화에서 화자가 바뀌었는지 화면만 보고는 알 수 없어, 기획서 16-5의
/// "캐릭터와 배경이 대사 데이터에 맞게 변경된다"를 확인할 방법이 없어집니다.
/// 그래서 스프라이트가 없을 때는 캐릭터마다 다른 색의 단색 실루엣으로 대체합니다.
///
/// 색은 마스터 데이터에 넣지 않고 여기 인스펙터에만 둡니다. 실제 아트가 들어오면 버릴 개발용 값이라
/// characters.json을 오염시킬 이유가 없습니다. 스프라이트가 채워지면 색 처리는 자연히 쓰이지 않습니다.
/// </summary>
public class StoryVisualBinder : MonoBehaviour
{
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
    /// 캐릭터 스프라이트를 돌려줍니다. 표정 ID는 아직 자산이 없어 무시하며,
    /// 표정별 이미지가 생기면 여기서 키를 조합하도록 바꾸면 됩니다.
    /// </summary>
    public Sprite GetCharacter(string characterId, string expressionId)
    {
        return TryGet(_characterSprites, characterId);
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
