using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 노트 하나의 겉모습입니다. 디자인상 노트 색은 노트 종류가 아니라 레인으로 정해지므로,
/// 풀에서 꺼낼 때 그 레인의 스프라이트로 갈아 끼웁니다.
/// </summary>
public class UI_LiveNoteVisual : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _noteImage;

    private Sprite _defaultSprite;

    private void Awake()
    {
        _defaultSprite = _noteImage.sprite;
    }

    /// <summary>
    /// 스프라이트를 넣지 않은 레인은 프리팹의 기본 표시를 그대로 둡니다.
    /// </summary>
    public void SetLaneSprite(Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        _noteImage.sprite = sprite;
    }

    /// <summary>
    /// 풀에 돌려주기 직전에 이전 레인의 색을 지웁니다.
    /// 노트는 화면 밖으로 나갈 때마다 SetActive로 껐다 켜지므로, OnEnable에서 지우면 되살아난 노트가 사라집니다.
    /// </summary>
    public void ClearLaneSprite()
    {
        _noteImage.sprite = _defaultSprite;
    }
}
