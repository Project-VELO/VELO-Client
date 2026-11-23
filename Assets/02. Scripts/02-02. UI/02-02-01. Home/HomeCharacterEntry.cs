using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

/// <summary>
/// 홈 캐릭터 한 명의 표시 자료입니다(개발 기획서 3장 "캐릭터별: Spine, Idle, 웃는 모션, 초상, 기준점, 배율", 16장 홈 대사).
///
/// 기준점, 배율, 홈 대사는 기획자가 정할 값이라 값이 들어오기 전까지 비워 둡니다.
/// 기준점과 배율을 비우면(_hasLayout 꺼짐) 프리팹에 놓인 배치를 그대로 씁니다.
/// </summary>
[Serializable]
public class HomeCharacterEntry
{
    [SerializeField]
    private string _characterId = string.Empty;

    [SerializeField]
    private SkeletonDataAsset _skeletonDataAsset;

    /// <summary>
    /// H06을 눌렀을 때 한 번 재생할 웃는 모션입니다. 비어 있거나 Spine 자료에 없으면 그 인물은 클릭에 반응하지 않습니다.
    /// </summary>
    [SerializeField]
    private string _reactionAnimationName = string.Empty;

    /// <summary>
    /// 프로필 초상(H01, P10) 후보로 쓰는 그림입니다. 비어 있으면 그 인물은 초상 후보에 나오지 않습니다.
    /// </summary>
    [SerializeField]
    private Sprite _portrait;

    [SerializeField]
    private bool _hasLayout;

    [SerializeField]
    private Vector2 _anchoredPosition;

    [SerializeField]
    private float _scale = 1f;

    /// <summary>
    /// H07에서 클릭할 때마다 차례로 넘길 대사입니다. 한국어 원문을 적고 화면에서 번역합니다.
    /// </summary>
    [SerializeField]
    private List<string> _dialogLines = new List<string>();

    public string CharacterId => _characterId;
    public SkeletonDataAsset SkeletonDataAsset => _skeletonDataAsset;
    public string ReactionAnimationName => _reactionAnimationName;
    public Sprite Portrait => _portrait;
    public bool HasLayout => _hasLayout;
    public Vector2 AnchoredPosition => _anchoredPosition;
    public float Scale => _scale;
    public List<string> DialogLines => _dialogLines;

    /// <summary>
    /// 홈 화면과 프로필 팝업이 같은 표를 같은 방식으로 찾게 한곳에 둡니다. 없으면 null입니다.
    /// </summary>
    public static HomeCharacterEntry Find(List<HomeCharacterEntry> entries, string characterId)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].CharacterId == characterId)
            {
                return entries[i];
            }
        }

        return null;
    }

    /// <summary>
    /// 교체 전에 리소스를 확인합니다(개발 기획서 3장 "P08 확인 시 리소스 검증 후 저장").
    /// Spine 자료를 읽지 못하는 인물로 바꾸면 홈 가운데가 빈 채로 저장되기 때문입니다.
    /// </summary>
    public bool CanLoad()
    {
        return _skeletonDataAsset != null && !ReferenceEquals(_skeletonDataAsset.GetSkeletonData(true), null);
    }
}
