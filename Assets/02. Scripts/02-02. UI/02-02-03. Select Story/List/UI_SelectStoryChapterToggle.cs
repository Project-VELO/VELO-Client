using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 챕터 헤더의 펼침·접힘을 담당합니다(기획서 5.2 "챕터 구분 영역").
///
/// 회차 카드를 채우는 UI_SelectStoryChapterSection과 분리한 이유는 책임이 다르기 때문입니다.
/// 섹션은 어떤 데이터를 어느 카드에 넣을지를 알고, 이 클래스는 그 카드 묶음을 보일지 말지만 압니다.
/// 한 클래스에 두면 목록을 다시 채울 때마다 펼침 상태 처리까지 얽힙니다.
/// </summary>
public class UI_SelectStoryChapterToggle : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _toggleButton;

    /// <summary>
    /// 화살표 그림입니다. 펼침 여부에 따라 스프라이트만 갈아 끼웁니다.
    /// </summary>
    [SerializeField]
    private Image _arrowImage;

    /// <summary>
    /// 접었을 때 감출 대상입니다. 회차 카드가 담긴 묶음을 가리킵니다.
    /// </summary>
    [SerializeField]
    private GameObject _episodeRoot;

    [Foldout("Project")]
    [Header("펼쳤을 때(접기 유도) / 접었을 때(펼치기 유도)")]
    [SerializeField]
    private Sprite _expandedArrow;

    [SerializeField]
    private Sprite _collapsedArrow;

    [Foldout("Settings")]
    [SerializeField]
    private bool _isExpandedByDefault = true;

    private bool _isExpanded;

    public bool IsExpanded => _isExpanded;

    private void Awake()
    {
        _toggleButton.onClick.AddListener(Toggle);
    }

    /// <summary>
    /// 섹션은 풀에서 재사용되므로 켜질 때마다 기본 상태로 되돌립니다.
    /// 남겨 두면 앞서 접어 둔 챕터 자리에 다른 챕터가 접힌 채로 들어옵니다.
    /// </summary>
    private void OnEnable()
    {
        SetExpanded(_isExpandedByDefault);
    }

    private void OnDestroy()
    {
        if (_toggleButton != null)
        {
            _toggleButton.onClick.RemoveListener(Toggle);
        }
    }

    /// <summary>
    /// 펼침 상태를 지정합니다. 목록을 다시 만드는 쪽에서 특정 챕터만 펼쳐 두고 싶을 때 씁니다.
    /// </summary>
    public void SetExpanded(bool isExpanded)
    {
        _isExpanded = isExpanded;

        _episodeRoot.SetActive(isExpanded);
        RefreshArrow();
    }

    private void Toggle()
    {
        SetExpanded(!_isExpanded);
    }

    /// <summary>
    /// 스프라이트가 비어 있으면 건드리지 않습니다. 한쪽만 채워 둔 상태에서 덮어쓰면
    /// 반대 상태로 갈 때 화살표가 사라진 것처럼 보입니다.
    /// </summary>
    private void RefreshArrow()
    {
        Sprite next = _isExpanded ? _expandedArrow : _collapsedArrow;

        if (next != null)
        {
            _arrowImage.sprite = next;
        }
    }
}
