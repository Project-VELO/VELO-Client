using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스튜디오 화면 상단의 두 탭입니다. 포토카드 세팅과 의상·악세서리 세팅을 갈아 끼웁니다.
///
/// 두 화면을 따로 두지 않고 한 화면에서 바꾸는 것은, 어느 쪽을 보고 있든 LIVE START까지의
/// 거리가 같아야 하기 때문입니다. 탭을 오갈 때 편성이 초기화되지도 않습니다.
///
/// 탭 배경은 선택·비선택 두 장을 갈아 끼웁니다. 글자가 그림에 들어 있어 색만 바꾸는 것으로는
/// 선택 상태가 드러나지 않습니다.
/// </summary>
public class UI_StudioTabs : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("포토카드 세팅")]
    [SerializeField]
    private Button _photocardTabButton;

    [SerializeField]
    private Image _photocardTabImage;

    [SerializeField]
    private GameObject _photocardPanel;

    [Foldout("Hierarchy")]
    [Header("의상 & 악세서리")]
    [SerializeField]
    private Button _itemTabButton;

    [SerializeField]
    private Image _itemTabImage;

    [SerializeField]
    private GameObject _itemPanel;

    [Foldout("Project")]
    [Header("탭 배경")]
    [SerializeField]
    private Sprite _photocardSelectedSprite;

    [SerializeField]
    private Sprite _photocardNormalSprite;

    [SerializeField]
    private Sprite _itemSelectedSprite;

    [SerializeField]
    private Sprite _itemNormalSprite;

    private void Awake()
    {
        _photocardTabButton.onClick.AddListener(SelectPhotocard);
        _itemTabButton.onClick.AddListener(SelectItem);
    }

    private void OnDestroy()
    {
        if (_photocardTabButton != null)
        {
            _photocardTabButton.onClick.RemoveListener(SelectPhotocard);
        }

        if (_itemTabButton != null)
        {
            _itemTabButton.onClick.RemoveListener(SelectItem);
        }
    }

    /// <summary>
    /// 화면에 들어올 때는 항상 첫 탭부터 봅니다. 편성은 포토카드를 고른 뒤 의상을 맞추는 순서라
    /// 마지막으로 보던 탭을 기억하면 순서가 뒤집힌 채 다시 들어오게 됩니다.
    /// </summary>
    private void OnEnable()
    {
        SelectPhotocard();
    }

    private void SelectPhotocard()
    {
        Select(isPhotocard: true);
    }

    private void SelectItem()
    {
        Select(isPhotocard: false);
    }

    private void Select(bool isPhotocard)
    {
        _photocardPanel.SetActive(isPhotocard);
        _itemPanel.SetActive(!isPhotocard);

        SetSprite(_photocardTabImage, isPhotocard ? _photocardSelectedSprite : _photocardNormalSprite);
        SetSprite(_itemTabImage, isPhotocard ? _itemNormalSprite : _itemSelectedSprite);
    }

    /// <summary>
    /// 아트가 아직 안 들어온 탭은 비워 두어도 됩니다. 빈 스프라이트를 대입하면 흰 사각형이
    /// 남아, 배경이 없는 것보다 더 어색해집니다.
    /// </summary>
    private static void SetSprite(Image target, Sprite sprite)
    {
        if (target == null || sprite == null)
        {
            return;
        }

        target.sprite = sprite;
    }
}
