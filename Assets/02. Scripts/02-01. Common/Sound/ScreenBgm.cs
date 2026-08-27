using UnityEngine;
using VInspector;

/// <summary>
/// 이 화면이 쓸 배경음을 BgmManager에 알립니다. 곡 선택은 화면 로직이 아니라 배치라서
/// 스크립트가 아닌 프리팹에서 고릅니다. 자기 재생기를 갖는 스토리와 라이브는 NONE입니다.
/// </summary>
public class ScreenBgm : MonoBehaviour
{
    [Foldout("Settings")]
    [SerializeField]
    private EBgm _bgm = EBgm.MAIN;

    // Start가 아닌 OnEnable인 것은 팝업에 가렸다 돌아오는 화면도 자기 곡을 되찾게 하기 위해서입니다.
    private void OnEnable()
    {
        if (BgmManager.Instance != null)
        {
            BgmManager.Instance.Play(_bgm);
        }
    }
}
