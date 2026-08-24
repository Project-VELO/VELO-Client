using UnityEngine;
using VInspector;

/// <summary>
/// 라이브 화면의 배경 캔버스를 그 씬의 메인 카메라에 붙입니다.
///
/// 트랙은 월드 공간 캔버스라 Screen Space - Overlay 캔버스보다 항상 아래에 그려집니다.
/// 배경과 트랙 플레이트는 노트보다 뒤에 깔려야 하므로 Overlay가 아니라 Screen Space - Camera로 두고,
/// sortingOrder를 트랙보다 낮춰 순서를 맞춥니다.
///
/// 메인 카메라는 PersistentScene의 프리팹 인스턴스라 씬 에셋에 참조를 담아 둘 수 없어 실행 시점에 찾습니다.
/// LiveTrackRig가 같은 카메라를 Camera.main으로 찾아 쓰므로 방식을 맞췄습니다.
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
