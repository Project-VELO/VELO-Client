using UnityEngine;

/// <summary>
/// 저장이 최종 실패했을 때 사용자에게 알립니다(기획서 3-L "저장 실패 | 재시도 후 실패 안내").
///
/// 재시도는 SaveService가 이미 수행하므로 여기서는 알리기만 합니다.
/// SaveService에 UI를 들이지 않으려고 이벤트로 갈라 둔 것이라, 구독은 화면 쪽인 이 클래스가 맡습니다.
///
/// PersistentScene에 두는 이유는 저장이 어느 화면에서든 일어나기 때문입니다.
/// 화면마다 구독자를 두면 전환 중에 실패한 저장은 아무도 알리지 못합니다.
/// </summary>
public class SaveFailureNotifier : MonoBehaviour
{
    private SaveService _saveService;

    private void OnEnable()
    {
        // 세이브를 아직 불러오지 않았다면 여기서 처음 만들어집니다. 구독은 그 인스턴스에 걸어야 하므로
        // 프로퍼티를 거쳐 받아 두고, 해제할 때 같은 것을 쓰도록 필드에 남깁니다.
        _saveService = PlayerDataProvider.Instance.SaveService;
        _saveService.OnSaveFailed += NotifySaveFailed;
    }

    private void OnDisable()
    {
        if (_saveService == null)
        {
            return;
        }

        _saveService.OnSaveFailed -= NotifySaveFailed;
        _saveService = null;
    }

    /// <summary>
    /// 진행을 막지는 않습니다. 저장에 실패해도 플레이 자체는 이어갈 수 있어야 하고,
    /// 기획서도 안내까지만 요구합니다.
    /// </summary>
    private void NotifySaveFailed()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.OpenErrorPopup(ErrorMessages.SAVE_FAILED);
    }
}
