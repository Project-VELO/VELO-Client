using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 화면 연출이 만지는 부품만 들고 있는 판입니다. 스스로 판단하는 것은 없습니다.
///
/// 무대(배경과 인물)를 한 컨테이너에 묶어 두는 이유는, 흔들림과 줌을 한 번에 걸기 위해서입니다.
/// 오브젝트마다 따로 흔들면 배경과 인물이 어긋나 보입니다.
///
/// 대사 상자는 이 컨테이너 밖에 있습니다. 흔들리고 확대되는 글자는 읽을 수 없고,
/// 연출표가 "화면 흔들림"이라고 적은 줄에도 대사는 계속 읽혀야 합니다.
///
/// 덮개도 무대와 대사 상자 사이에 있습니다. 암전이 대사까지 덮으면 진행할 수 없게 됩니다.
/// </summary>
public class UI_StoryEffectLayer : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("흔들림·줌·이동이 걸리는 컨테이너")]
    [SerializeField]
    private RectTransform _stage;

    [Header("전체화면 덮개")]
    [SerializeField]
    private Image _overlay;

    [Header("가장자리만 어두운 덮개")]
    [SerializeField]
    private Image _vignette;

    /// <summary>
    /// 컷씬의 위아래 검은 여백입니다. 컷씬으로 흐르는 줄에서만 켭니다.
    ///
    /// 덮개가 아니라 별도 오브젝트인 이유는 연출이 아니라 화면의 틀이기 때문입니다.
    /// 컷 하나하나가 켜고 끄는 것이 아니라 컷씬이 흐르는 동안 고정입니다.
    /// </summary>
    [Header("컷씬 위아래 여백")]
    [SerializeField]
    private GameObject _letterbox;

    [Foldout("Settings")]
    /// <summary>
    /// 무대를 평소에 얼마나 확대해 둘지입니다.
    ///
    /// 1배로 두면 화면을 흔들거나 밀 때 배경 바깥의 빈 자리가 드러납니다.
    /// 미리 조금 다가가 있으면 그 확대분이 여유분이 되어, 움직여도 화면이 배경으로 계속 채워집니다.
    /// 값을 올리면 빈 화면 걱정은 줄지만 배경이 그만큼 잘려 나갑니다.
    /// </summary>
    [SerializeField]
    private float _baseScale = 1.3f;

    /// <summary>
    /// 무대의 원래 자리입니다. 연출이 끝나면 여기로 돌립니다.
    /// 프리팹 값을 그대로 쓰는 것은, 무대가 화면 중앙에 있다는 가정을 코드에 박아 넣지 않기 위해서입니다.
    /// </summary>
    private Vector2 _homePosition;

    private void Awake()
    {
        _homePosition = _stage.anchoredPosition;
        ResetStage();
        SetOverlayColor(EStoryEffectTarget.OVERLAY, GetTransparent(_overlay.color));
        SetOverlayColor(EStoryEffectTarget.VIGNETTE, GetTransparent(_vignette.color));
    }

    /// <summary>
    /// 컷씬의 위아래 여백을 켜고 끕니다. 여백이 붙어 있지 않은 화면에서도 부를 수 있게 null을 허용합니다.
    /// </summary>
    public void SetLetterbox(bool isOn)
    {
        if (_letterbox == null)
        {
            return;
        }

        _letterbox.SetActive(isOn);
    }

    /// <summary>
    /// 무대를 원래 자리와 평소 배율로 돌립니다. 덮개는 건드리지 않습니다.
    /// 암전이나 비네팅은 "걷으라"는 별도 지시가 있어야 걷히는 상태이기 때문입니다.
    /// </summary>
    public void ResetStage()
    {
        SetStageScale(1f);
        SetStageOffset(Vector2.zero);
    }

    /// <summary>
    /// 무대를 원래 자리에서 얼마나 밀지 정합니다. 흔들림과 시점 이동이 함께 씁니다.
    ///
    /// 확대로 생긴 여유분을 넘지 못하게 잘라 냅니다. 데이터에 큰 값이 들어와도
    /// 화면 가장자리가 비는 일은 없어야 하므로, 판단을 데이터에 맡기지 않고 여기서 막습니다.
    /// </summary>
    public void SetStageOffset(Vector2 offset)
    {
        Vector2 slack = GetSlack();

        offset.x = Mathf.Clamp(offset.x, -slack.x, slack.x);
        offset.y = Mathf.Clamp(offset.y, -slack.y, slack.y);

        _stage.anchoredPosition = _homePosition + offset;
    }

    public Vector2 GetStageOffset()
    {
        return _stage.anchoredPosition - _homePosition;
    }

    /// <summary>
    /// 평소 배율에 곱할 배수를 받습니다. 1이면 평소 구도입니다.
    /// 화면보다 작아지지 않도록 아래를 막습니다. 작아지면 어차피 빈 자리가 드러납니다.
    /// </summary>
    public void SetStageScale(float multiplier)
    {
        float scale = Mathf.Max(1f, _baseScale * multiplier);

        _stage.localScale = new Vector3(scale, scale, 1f);

        // 배율이 줄면 여유분도 줄어듭니다. 이미 밀려 있던 무대가 여유분을 넘고 있으면 다시 잘라 줍니다.
        SetStageOffset(GetStageOffset());
    }

    public float GetStageScale()
    {
        return _stage.localScale.x / _baseScale;
    }

    public Color GetOverlayColor(EStoryEffectTarget target)
    {
        return Resolve(target).color;
    }

    public void SetOverlayColor(EStoryEffectTarget target, Color color)
    {
        Image image = Resolve(target);
        image.color = color;

        // 완전히 투명한 덮개가 켜져 있으면 화면 전체를 덮는 레이캐스트 대상이 하나 남습니다.
        // 대사 상자보다 아래에 있어 클릭을 막지는 않지만, 켜 둘 이유도 없습니다.
        image.enabled = 0f < color.a;
    }

    /// <summary>
    /// 지금 배율에서 무대를 좌우·상하로 밀 수 있는 최대 거리입니다.
    /// 확대로 화면 밖에 나가 있는 절반이 그대로 여유분이 됩니다.
    /// </summary>
    private Vector2 GetSlack()
    {
        Rect rect = _stage.rect;
        float scale = _stage.localScale.x;

        return new Vector2(
            Mathf.Max(0f, rect.width * (scale - 1f) * 0.5f),
            Mathf.Max(0f, rect.height * (scale - 1f) * 0.5f));
    }

    private Image Resolve(EStoryEffectTarget target)
    {
        return target == EStoryEffectTarget.VIGNETTE ? _vignette : _overlay;
    }

    private Color GetTransparent(Color color)
    {
        color.a = 0f;
        return color;
    }
}
