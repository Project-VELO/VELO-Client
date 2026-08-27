using UnityEngine;
using VInspector;

/// <summary>
/// 이 화면이 어떤 배경음을 쓰는지 알립니다.
///
/// 화면 스크립트마다 재생 코드를 넣지 않고 컴포넌트 하나로 둔 이유는, 곡을 바꾸는 일이
/// 화면의 로직이 아니라 배치의 문제이기 때문입니다. 프리팹에서 값을 고르면 끝나고,
/// 화면이 늘어도 스크립트를 고칠 일이 없습니다.
///
/// 스토리와 라이브는 EBgm.NONE을 골라 상주 곡을 멈춥니다. 두 화면은 자기 재생기를 갖습니다.
///
/// 화면 프리팹의 뿌리에 붙지만 UI_ 접두사를 달지 않습니다. 접두사는 UI 로직을 수행하는
/// 클래스의 것인데, 이 클래스는 Button도 Canvas도 건드리지 않고 쓸 곡만 알립니다.
/// 붙는 자리가 UI일 뿐 하는 일은 소리라서 BgmManager 옆에 둡니다.
/// </summary>
public class ScreenBgm : MonoBehaviour
{
    [Foldout("Settings")]
    [SerializeField]
    private EBgm _bgm = EBgm.MAIN;

    /// <summary>
    /// 화면이 켜질 때마다 알립니다. 같은 곡이면 재생기가 무시하므로 곡은 이어집니다.
    /// Start가 아니라 OnEnable인 것은 팝업처럼 껐다 켜지는 화면도 자기 곡을 되찾게 하기 위해서입니다.
    /// </summary>
    private void OnEnable()
    {
        if (BgmManager.Instance != null)
        {
            BgmManager.Instance.Play(_bgm);
        }
    }
}
