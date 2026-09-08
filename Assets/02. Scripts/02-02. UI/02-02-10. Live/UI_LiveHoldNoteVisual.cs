using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 롱노트의 몸통과 꼬리를 붙잡는 겉모습입니다.
/// 머리는 일반 노트와 같은 마커라 UI_LiveNoteVisual이 그대로 맡습니다.
/// </summary>
public class UI_LiveHoldNoteVisual : MonoBehaviour
{
    private static readonly List<LiveHoldBodySample> EMPTY_SAMPLES = new List<LiveHoldBodySample>();

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
    public void RefreshBody(List<LiveHoldBodySample> samples)
    {
        _body.RefreshBody(samples);
    }

    /// <summary>
    /// 꼬리를 끝 시각의 노트 자리에 그대로 올립니다. 그 높이의 레인 폭과 기울기를 따라야 몸통 끝과 어긋나지 않습니다.
    /// </summary>
    public void RefreshTail(Vector2 localCenter, Vector2 size, bool isVisible)
    {
        _tailImage.enabled = isVisible;

        if (!isVisible)
        {
            return;
        }

        RectTransform tailTransform = _tailImage.rectTransform;
        tailTransform.anchoredPosition = localCenter;
        tailTransform.sizeDelta = size;
    }

    /// <summary>
    /// 풀에 돌려주기 직전에 이전 노트의 길이와 스프라이트를 지웁니다.
    /// </summary>
    public void ResetHold()
    {
        RefreshBody(EMPTY_SAMPLES);
        RefreshTail(Vector2.zero, Vector2.zero, false);
        SetHoldSprites(null, null);
    }
}
