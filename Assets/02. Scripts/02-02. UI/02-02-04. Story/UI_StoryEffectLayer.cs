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
    /// 무대를 원래 자리와 크기로 돌립니다. 덮개는 건드리지 않습니다.
    /// 암전이나 비네팅은 "걷으라"는 별도 지시가 있어야 걷히는 상태이기 때문입니다.
    /// </summary>
    public void ResetStage()
    {
        _stage.anchoredPosition = _homePosition;
        _stage.localScale = Vector3.one;
    }

    /// <summary>
    /// 무대를 원래 자리에서 얼마나 밀지 정합니다. 흔들림과 시점 이동이 함께 씁니다.
    /// </summary>
    public void SetStageOffset(Vector2 offset)
    {
        _stage.anchoredPosition = _homePosition + offset;
    }

    public Vector2 GetStageOffset()
    {
        return _stage.anchoredPosition - _homePosition;
    }

    public void SetStageScale(float scale)
    {
        _stage.localScale = new Vector3(scale, scale, 1f);
    }

    public float GetStageScale()
    {
        return _stage.localScale.x;
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
