using UnityEngine;
using VInspector;

/// <summary>
/// 라이브 화면의 배경 캔버스를 그 씬의 메인 카메라에 붙입니다.
///
/// 배경과 트랙 플레이트는 노트와 HUD보다 뒤에 깔려야 합니다. 트랙과 HUD 캔버스가 모두 Screen Space - Overlay이고
/// Overlay는 sortingOrder와 무관하게 카메라가 그린 것 위에 얹히므로, 배경만 Screen Space - Camera로 두면
/// 무엇을 더 얹어도 항상 아래에 남습니다. 배경을 Overlay로 바꾸면 이 보장이 사라집니다.
///
/// 그래서 이 캔버스에는 렌더 카메라가 필요한데, 메인 카메라는 PersistentScene의 프리팹 인스턴스라
/// 씬 에셋에 참조를 담아 둘 수 없어 실행 시점에 찾습니다.
/// </summary>
public class UI_LiveBackground : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Canvas _canvas;

    private void Awake()
    {
        InitRenderCamera();
    }

    private void InitRenderCamera()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("[UI_LiveBackground] 메인 카메라를 찾지 못해 배경을 표시하지 못했습니다. PersistentScene이 로드되어 있는지 확인해 주세요.");
            return;
        }

        _canvas.worldCamera = mainCamera;
    }
}
