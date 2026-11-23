using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using VInspector;

/// <summary>
/// 홈 가운데 캐릭터(H06)입니다. 저장된 홈 캐릭터의 Spine 자료로 바꾸고 Idle을 반복합니다(개발 기획서 2장, 3장).
///
/// 같은 인물로 다시 정하면 아무것도 하지 않습니다. 같은 값으로 확인했을 때 Idle을 처음부터 다시 틀지 말라는
/// 원문(3장 "같은 값으로 확인하면 기존 Idle을 재시작하지 않음")을 지키기 위해서입니다.
/// </summary>
public class UI_HomeCharacter : MonoBehaviour
{
    public const string IDLE_ANIMATION_NAME = "idle";

    [Foldout("Hierarchy")]
    [SerializeField]
    private SkeletonGraphic _skeletonGraphic;

    [Foldout("Project")]
    [Header("Characters")]
    [SerializeField]
    private List<HomeCharacterEntry> _entries = new List<HomeCharacterEntry>();

    /// <summary>
    /// 기준점, 배율이 비어 있는 인물이 쓸 배치입니다. 프리팹에 놓인 값을 처음에 한 번 기억해 둡니다.
    /// </summary>
    private Vector2 _defaultAnchoredPosition;
    private Vector3 _defaultLocalScale;

    public List<HomeCharacterEntry> Entries => _entries;

    /// <summary>
    /// 지금 홈에 서 있는 인물입니다. 처음에는 프리팹에 넣어 둔 Spine 자료의 인물입니다.
    /// </summary>
    public HomeCharacterEntry CurrentEntry { get; private set; }

    private void Awake()
    {
        _defaultAnchoredPosition = _skeletonGraphic.rectTransform.anchoredPosition;
        _defaultLocalScale = _skeletonGraphic.rectTransform.localScale;
        CurrentEntry = FindEntryByAsset(_skeletonGraphic.skeletonDataAsset);

        // 처음 인물로 다시 정하면 SetCharacter가 같은 ID라 건너뛰므로, 그 인물의 기준점, 배율은 여기서 적용합니다.
        if (!ReferenceEquals(CurrentEntry, null))
        {
            RefreshLayout(CurrentEntry);
        }
    }

    /// <summary>
    /// 리소스를 읽지 못하는 인물이면 지금 표시를 그대로 둡니다(개발 기획서 3장 "리소스 로딩 실패: 기존 홈 표시 유지").
    /// </summary>
    public void SetCharacter(string characterId)
    {
        if (!ReferenceEquals(CurrentEntry, null) && CurrentEntry.CharacterId == characterId)
        {
            return;
        }

        HomeCharacterEntry entry = FindEntry(characterId);

        if (ReferenceEquals(entry, null) || !entry.CanLoad())
        {
            Debug.LogWarning($"[UI_HomeCharacter] 홈 캐릭터 {characterId}의 Spine 자료를 읽지 못해 지금 표시를 유지합니다.");
            return;
        }

        RefreshSkeleton(entry);
        RefreshLayout(entry);
        CurrentEntry = entry;
    }

    public HomeCharacterEntry FindEntry(string characterId)
    {
        return HomeCharacterEntry.Find(_entries, characterId);
    }

    private HomeCharacterEntry FindEntryByAsset(SkeletonDataAsset asset)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].SkeletonDataAsset == asset)
            {
                return _entries[i];
            }
        }

        return null;
    }

    private void RefreshSkeleton(HomeCharacterEntry entry)
    {
        if (_skeletonGraphic.skeletonDataAsset == entry.SkeletonDataAsset)
        {
            return;
        }

        _skeletonGraphic.skeletonDataAsset = entry.SkeletonDataAsset;
        _skeletonGraphic.startingAnimation = IDLE_ANIMATION_NAME;
        _skeletonGraphic.startingLoop = true;
        _skeletonGraphic.Initialize(true);
    }

    private void RefreshLayout(HomeCharacterEntry entry)
    {
        RectTransform rect = _skeletonGraphic.rectTransform;

        rect.anchoredPosition = entry.HasLayout ? entry.AnchoredPosition : _defaultAnchoredPosition;
        rect.localScale = entry.HasLayout ? Vector3.one * entry.Scale : _defaultLocalScale;
    }
}
