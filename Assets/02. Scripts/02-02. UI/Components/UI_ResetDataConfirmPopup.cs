using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 데이터 초기화를 한 번 더 묻는 팝업입니다.
///
/// 설정 팝업은 관람객도 열 수 있어, 버튼 한 번에 지우면 플레이 중인 사람의 진행이 사라집니다.
/// 취소는 UI_Popup의 닫기 버튼이 처리하므로 여기서는 초기화 버튼만 맡습니다.
///
/// 초기화 뒤에는 홈을 처음부터 다시 올립니다. 이 팝업은 화면 씬 안에 있어 전환 도중 함께 내려가므로,
/// 전환은 상주하는 SceneTransitionManager의 수명에 맡깁니다. 이 팝업의 파괴 토큰을 넘기면
/// 화면이 내려가는 순간 전환이 취소되어 로딩 화면에 멈춥니다.
/// </summary>
public class UI_ResetDataConfirmPopup : UI_Popup
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _confirmButton;

    protected override void Awake()
    {
        base.Awake();

        _confirmButton.onClick.AddListener(ResetData);
    }

    /// <summary>
    /// 닫는 연출을 기다리지 않습니다. 전환이 시작되면 로딩 패널이 화면을 덮고 UIManager가 열린 팝업을 모두 정리합니다.
    /// </summary>
    private void ResetData()
    {
        PlayerDataResetter.ResetAll();

        SceneTransitionManager transitionManager = SceneTransitionManager.Instance;

        if (transitionManager == null)
        {
            // PersistentScene 없이 화면만 열어 확인하는 경우입니다. 데이터는 이미 지웠으므로 닫기만 합니다.
            RequestClose();
            return;
        }

        transitionManager.ReloadSceneAsync(ESceneNames.HomeScene, transitionManager.GetCancellationTokenOnDestroy()).Forget();
    }
}
