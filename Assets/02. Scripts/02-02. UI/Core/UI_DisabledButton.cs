using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// "보이지만 누를 수 없음"을 고정하는 컴포넌트입니다.
///
/// 1차 MVP는 상점·도감·우편·공지·이벤트·가챠·숙소 강화를 클릭 불가로, 랭킹을 비활성으로 둡니다
/// (기획서.txt:52-81). 스페이스의 스튜디오·숙소도 같습니다(기획서.txt:1974-1975, 1992).
///
/// interactable만 끄면 라벨과 아이콘이 밝게 남습니다. 버튼 프리팹 대부분이 TargetGraphic으로
/// 배경 이미지 하나만 지정해 두어 색 전환이 그 하나에만 걸리기 때문입니다.
/// 어둡게 보이려면 CanvasGroup 알파를 함께 낮춰야 합니다(기획서.txt:1717).
/// </summary>
public class UI_DisabledButton : MonoBehaviour
{
    private const float DISABLED_ALPHA = 0.4f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private CanvasGroup _canvasGroup;

    /// <summary>
    /// 없어도 됩니다. 공지·이벤트·설정처럼 Button이 아예 붙어 있지 않은 대상은 알파만 낮춥니다.
    /// </summary>
    [SerializeField]
    private Button _button;

    private void Awake()
    {
        SetDisabled();
    }

    /// <summary>
    /// 비활성 표시를 적용합니다. Awake에서 한 번 부르며, 프리팹 상태에 관계없이 같은 결과가 되도록
    /// 값을 조건 없이 덮어씁니다.
    /// </summary>
    public void SetDisabled()
    {
        if (_button != null)
        {
            _button.interactable = false;
        }

        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        if (_canvasGroup == null)
        {
            Debug.LogWarning($"[UI_DisabledButton] CanvasGroup이 없어 흐리게 표시하지 못했습니다: {gameObject.name}");
            return;
        }

        _canvasGroup.alpha = DISABLED_ALPHA;

        // 레이캐스트는 살려 둡니다. 막으면 클릭이 뒤에 있는 요소로 새어 엉뚱한 화면이 열립니다.
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = false;
    }
}
