using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 롱노트의 몸통과 꼬리를 붙잡는 겉모습입니다.
/// 머리는 일반 노트와 같은 마커라 UI_LiveNoteVisual이 그대로 맡습니다.
/// </summary>
public class UI_LiveHoldNoteVisual : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_LiveHoldNoteBody _body;

    [SerializeField]
    private Image _tailImage;

    public void SetHoldSprites(Sprite bodySprite, Sprite tailSprite)
    {
        _body.SetBodySprite(bodySprite);
        _tailImage.sprite = tailSprite;
    }

    /// <summary>
    /// 몸통은 노트 원점에서 위로 자랍니다. 원점은 판정선에 먹히고 남은 시작점입니다.
    /// </summary>
    public void RefreshBody(float length, float uvStart, float uvEnd)
    {
        _body.RefreshBody(length, uvStart, uvEnd);
        _body.rectTransform.anchoredPosition = new Vector2(0f, length * 0.5f);
    }

    public void RefreshTail(float offsetY, float thickness, bool isVisible)
    {
        _tailImage.enabled = isVisible;

        if (!isVisible)
        {
            return;
        }

        RectTransform tailTransform = _tailImage.rectTransform;
        tailTransform.anchoredPosition = new Vector2(0f, offsetY);
        tailTransform.sizeDelta = new Vector2(tailTransform.sizeDelta.x, thickness);
    }

    /// <summary>
    /// 풀에 돌려주기 직전에 이전 노트의 길이와 스프라이트를 지웁니다.
    /// </summary>
    public void ResetHold()
    {
        RefreshBody(0f, 0f, 0f);
        RefreshTail(0f, 0f, false);
        SetHoldSprites(null, null);
    }
}
