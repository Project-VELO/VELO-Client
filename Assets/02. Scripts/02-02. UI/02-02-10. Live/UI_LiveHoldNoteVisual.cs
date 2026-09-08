using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 롱노트의 몸통을 붙잡는 겉모습입니다.
/// 머리는 일반 노트와 같은 마커라 UI_LiveNoteVisual이 그대로 맡고, 끝은 몸통이 끊기는 자리로 보여 줍니다.
/// </summary>
public class UI_LiveHoldNoteVisual : MonoBehaviour
{
    private static readonly List<LiveHoldBodySample> EMPTY_SAMPLES = new List<LiveHoldBodySample>();

    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_LiveHoldNoteBody _body;

    public void SetBodySprite(Sprite bodySprite)
    {
        _body.SetBodySprite(bodySprite);
    }

    /// <summary>
    /// 몸통은 노트 원점에서 위로 자랍니다. 원점은 판정선에 먹히고 남은 시작점입니다.
    /// </summary>
    public void RefreshBody(List<LiveHoldBodySample> samples)
    {
        _body.RefreshBody(samples);
    }

    /// <summary>
    /// 풀에 돌려주기 직전에 이전 노트의 길이와 스프라이트를 지웁니다.
    /// </summary>
    public void ResetHold()
    {
        RefreshBody(EMPTY_SAMPLES);
        SetBodySprite(null);
    }
}
