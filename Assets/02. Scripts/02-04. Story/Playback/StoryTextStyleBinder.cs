using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 대본의 TextStyleId를 실제 글꼴·크기·색으로 바꿉니다(StoryVisualBinder와 같은 역할).
///
/// 표를 ScriptableObject가 아니라 씬 컴포넌트에 둔 것은 배경·초상 표(StoryVisualBinder)와 맞추기 위해서입니다.
/// 감상 화면 밖에서 쓰이지 않는 표라 에셋으로 따로 빼면 참조만 한 단계 늘어납니다.
/// </summary>
public class StoryTextStyleBinder : MonoBehaviour
{
    [Foldout("Project")]
    [Header("대본의 TextStyleId를 키로 채웁니다")]
    [SerializeField]
    private SerializableDictionary<string, StoryTextStyle> _styles = new SerializableDictionary<string, StoryTextStyle>();

    /// <summary>
    /// 표에 없는 ID와 빈 칸이 떨어지는 기본값입니다.
    /// 여기로 떨어져도 글이 사라지지 않도록, 표가 비어 있어도 읽을 수 있는 값을 넣어 둡니다.
    /// </summary>
    [Foldout("Settings")]
    [SerializeField]
    private StoryTextStyle _defaultStyle = new StoryTextStyle();

    /// <summary>
    /// 이미 알린 미등록 ID입니다. 같은 ID가 여러 줄에 쓰이므로, 알리지 않으면 로그가 줄 수만큼 쌓입니다.
    /// </summary>
    private readonly HashSet<string> _reportedMissingIds = new HashSet<string>();

    /// <summary>
    /// 스타일을 돌려줍니다. 빈 칸은 기본값이며 경고하지 않습니다.
    /// 아직 아트가 정해지지 않은 회차의 대본이 빈 칸으로 들어오는 것은 정상입니다.
    /// </summary>
    public StoryTextStyle Get(string styleId)
    {
        if (string.IsNullOrEmpty(styleId))
        {
            return _defaultStyle;
        }

        if (_styles.TryGetValue(styleId, out StoryTextStyle style))
        {
            return style;
        }

        // 대본에는 있는데 표에 없다는 뜻이므로, 표를 채워야 한다는 신호로 한 번만 남깁니다.
        if (_reportedMissingIds.Add(styleId))
        {
            Debug.LogWarning($"[StoryTextStyleBinder] 표에 없는 스타일이라 기본값으로 출력합니다: {styleId}");
        }

        return _defaultStyle;
    }
}
