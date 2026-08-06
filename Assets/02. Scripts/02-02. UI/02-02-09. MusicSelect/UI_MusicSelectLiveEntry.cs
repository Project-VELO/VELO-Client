using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

/// <summary>
/// 곡 선택 화면의 LIVE 진입 게이트입니다. P_UI_MusicSelect 루트에 UI_MusicSelect와 나란히 붙습니다
/// (화면 총괄과 진입 시퀀스를 나누는 UI_Office / UI_OfficeDayFinishFlow와 같은 분할).
///
/// 편성이 5장 규칙을 채우지 못하면 안내 팝업으로 차단하되, 막는 것은 리듬게임 진입입니다
/// (기획서 16-15 "리듬게임 진입이 차단된다", 10.4 "카드 5장 미만이면 플레이가 막힌다").
/// 준비 화면은 편성을 고치는 화면이므로 막지 않습니다 — 막으면 모자란 편성을 되돌릴 방법이 없어집니다.
/// </summary>
public class UI_MusicSelectLiveEntry : MonoBehaviour
{
    [Foldout("Hierarchy")]
    // 두 팝업 모두 이 화면에서만 열고 닫으므로 PersistentScene이 아니라 09_MusicSelectScene에 두고 참조합니다.
    [SerializeField]
    private UI_PhotocardSelectPopup _photocardSelectPopup;

    [SerializeField]
    private UI_CardShortagePopup _cardShortagePopup;

    /// <summary>
    /// LIVE 준비 버튼입니다. 선택 결과를 컨텍스트에 확정하고 편성 팝업을 엽니다(기획서 10.3).
    /// 실제 LIVE 시작은 팝업 안의 라이브 스타트 버튼이 담당하며, 편성 검사도 그쪽에서 합니다.
    /// </summary>
    public void OpenPreparePopup(SongData song, EDifficulty difficulty)
    {
        // 팝업 스택은 PersistentScene의 UIManager가 들고 있으므로, 이 씬만 단독으로 열어 확인할 때는 존재하지 않을 수 있습니다.
        if (ReferenceEquals(song, null) || UIManager.Instance == null)
        {
            return;
        }

        LiveEntryContext.Instance.SetSelection(song.SongId, difficulty);
        UIManager.Instance.PopupHandler.OpenPopup(_photocardSelectPopup);
    }

    /// <summary>
    /// 준비 화면을 거치지 않는 즉시 시작입니다(기획서 10.3의 "플레이 버튼"). 편성은 기본 편성이 그대로 적용됩니다.
    /// </summary>
    public void StartLiveDirectly(SongData song, EDifficulty difficulty)
    {
        if (ReferenceEquals(song, null) || RefuseWhenCardShortage())
        {
            return;
        }

        LiveEntryContext.Instance.SetSelection(song.SongId, difficulty);

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogWarning($"[UI_MusicSelectLiveEntry] SceneTransitionManager가 없어 {ESceneNames.LiveScene}으로 이동하지 못했습니다. PersistentScene이 로드되어 있는지 확인해 주세요.");
            return;
        }

        SceneTransitionManager.Instance.LoadSceneAsync(ESceneNames.LiveScene, this.GetCancellationTokenOnDestroy()).Forget();
    }

    /// <summary>
    /// 편성이 5장 규칙을 채우지 못했으면 안내 팝업으로 리듬게임 진입을 차단합니다(기획서 16-15).
    /// 조용한 버튼 비활성 대신 사유를 안내하는 방식입니다.
    /// </summary>
    private bool RefuseWhenCardShortage()
    {
        if (LiveLoadoutRule.IsCurrentLoadoutPlayable(LiveLoadoutContext.Instance, PlayerDataProvider.Instance.Data))
        {
            return false;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.PopupHandler.OpenPopup(_cardShortagePopup);
        }

        return true;
    }
}
