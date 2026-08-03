using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스페이스 화면입니다(기획서 SCREEN-005).
///
/// 장소 허브지만 1차 MVP에서 실제로 갈 수 있는 곳은 사무실 하나뿐입니다.
/// 스튜디오·숙소·연습실은 클릭 불가입니다(기획서.md:1373-1379, 1388 "사무실만 활성화").
///
/// 연습실은 기획 갱신으로 LIVE 진입 경로에서 빠졌습니다. 이전 기획에는 연습실 LIVE가 있었고
/// EEntryType.PRACTICE_LIVE와 그 처리 규칙이 코드에 남아 있지만, 이제 어디서도 설정하지 않습니다.
/// 여기에 연습실 배선을 되살리면 보상 5%·스케줄 미반영이 함께 살아나므로 그렇게 하지 마십시오.
/// </summary>
public class UI_Space : MonoBehaviour
{
    /// <summary>
    /// 1차 MVP에서 스페이스의 유일한 활성 장소입니다(기획서.md:1367, 1375).
    /// </summary>
    [Foldout("Hierarchy")]
    [Header("활성 버튼")]
    [SerializeField]
    private Button _officeButton;

    /// <summary>
    /// 우하단 LIVE 버튼입니다. 홈의 일반 LIVE와 같은 진입으로 다룹니다.
    /// </summary>
    [SerializeField]
    private Button _liveButton;

    [Foldout("Hierarchy")]
    [Header("Panels")]
    [SerializeField]
    private UI_CurrencyHud _currencyHud;

    private void Start()
    {
        RefreshScreen();
        InitButtons();
    }

    private void RefreshScreen()
    {
        // LIVE 보상을 받고 돌아온 직후에도 최신 값이 보이도록 진입할 때마다 다시 읽습니다.
        // UI_CurrencyHud는 스스로 갱신하지 않고 화면이 불러 주기를 기다립니다.
        _currencyHud.Refresh();
    }

    private void InitButtons()
    {
        _officeButton.onClick.AddListener(MoveToOffice);
        _liveButton.onClick.AddListener(StartLive);

        // 스튜디오·숙소·연습실은 여기서 다루지 않습니다.
        // 클릭 동작을 등록하지 않는 것으로 기능을 막고, 홈 화면과 마찬가지로 시각 처리는 하지 않습니다.
        // 되살릴 때는 UI_DisabledButton을 붙이면 됩니다(기획서.md:1834 "비활성 상태로 표시").
    }

    private void MoveToOffice()
    {
        LoadScene(ESceneNames.OfficeScene);
    }

    /// <summary>
    /// 일반 LIVE 버튼입니다. 지정곡 없이 진입하므로 곡 선택 화면에서 첫 곡이 자동 선택되고
    /// 다른 곡으로 바꿀 수 있습니다(기획서.md:358-365).
    ///
    /// 진입 위치를 홈과 사무실 둘로만 구분하므로 스페이스도 HOME_LIVE를 씁니다.
    /// 그래서 곡 선택 화면에서 뒤로가기를 누르면 스페이스가 아니라 홈으로 나갑니다.
    /// </summary>
    private void StartLive()
    {
        LiveEntryStarter.StartLive(EEntryType.HOME_LIVE, cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    private void LoadScene(ESceneNames sceneName)
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneAsync(sceneName, this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}
