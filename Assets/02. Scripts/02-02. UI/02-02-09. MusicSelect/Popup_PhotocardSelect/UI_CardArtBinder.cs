using UnityEngine;
using VInspector;

/// <summary>
/// 카드 ID를 카드 일러스트로 바꿉니다. 편성 슬롯과 보유 목록이 나눠 씁니다.
///
/// 표를 마스터 데이터 JSON이 아니라 인스펙터에 두는 이유는 일러스트가 에셋 참조이기 때문입니다.
/// JsonUtility로는 Sprite를 직렬화할 수 없어, JSON으로 가면 경로 문자열과 런타임 로더가 따라와야 합니다.
/// 대사 배경·초상을 StoryVisualBinder가, 음원을 StoryAudioBinder가 같은 방식으로 들고 있습니다.
///
/// cards.json의 ImagePath는 아직 비어 있습니다. 로더를 들이는 대신 이 표가 그 자리를 대신합니다.
/// </summary>
public class UI_CardArtBinder : MonoBehaviour
{
    [Foldout("Project")]
    [SerializeField]
    private SerializableDictionary<string, Sprite> _cardSprites = new SerializableDictionary<string, Sprite>();

    /// <summary>
    /// 아직 그림이 없는 카드는 null을 돌려줍니다. 없는 것을 빈 스프라이트로 채우면
    /// 흰 사각형이 남아, 그림이 아직 안 들어온 것인지 잘못 배선된 것인지 구분되지 않습니다.
    /// </summary>
    public Sprite GetSprite(string cardId)
    {
        if (string.IsNullOrEmpty(cardId))
        {
            return null;
        }

        return _cardSprites.TryGetValue(cardId, out Sprite sprite) ? sprite : null;
    }
}
