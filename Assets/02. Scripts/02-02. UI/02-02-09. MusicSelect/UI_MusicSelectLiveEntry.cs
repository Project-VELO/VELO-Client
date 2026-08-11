using UnityEngine;
using VInspector;

/// <summary>
/// 곡 선택 화면의 LIVE 진입 게이트입니다. P_UI_MusicSelect 루트에 UI_MusicSelect와 나란히 붙습니다
/// (화면 총괄과 진입 시퀀스를 나누는 UI_Office / UI_OfficeDayFinishFlow와 같은 분할).
///
/// 곡 선택 화면에서 리듬게임으로 바로 들어가는 경로는 두지 않습니다. 편성을 고치는 준비 화면을 반드시 거치게 해야
/// 카드 5장 규칙(기획서 16-15, 10.4)을 안내와 함께 지킬 수 있기 때문입니다.
/// 편성 검사와 실제 LIVE 시작은 준비 팝업의 라이브 시작 버튼(UI_PhotocardSelectPopup)이 맡습니다.
/// </summary>
public class UI_MusicSelectLiveEntry : MonoBehaviour
{
    [Foldout("Hierarchy")]
    // 이 팝업은 이 화면에서만 열고 닫으므로 PersistentScene이 아니라 09_MusicSelectScene에 두고 참조합니다.
    [SerializeField]
    private UI_PhotocardSelectPopup _photocardSelectPopup;

    /// <summary>
    /// LIVE 준비 버튼입니다. 선택 결과를 컨텍스트에 확정하고 편성 팝업을 엽니다(기획서 10.3).
    /// </summary>
    public void OpenPreparePopup(SongData song, EDifficulty difficulty)
    {
        // 팝업 스택은 PersistentScene의 UIManager가 들고 있으므로, 이 씬만 단독으로 열어 확인할 때는 존재하지 않을 수 있습니다.
        if (ReferenceEquals(song, null) || UIManager.Instance == null)
        {
            return;
        }

        LiveEntryContext.Instance.SetSelection(song.SongId, difficulty);
        UIManager.Instance.OpenPopup(_photocardSelectPopup);
    }
}
