using UnityEngine;
using VInspector;

/// <summary>
/// 저장된 하이스피드를 트랙에 물려 주는 것만 담당합니다.
///
/// 조절 UI는 일시정지 팝업 안에 있고, 팝업은 꺼진 채로 씬에 올라와 한 번 열기 전까지 Awake가 돌지 않습니다.
/// 팝업을 열지 않은 판에서도 저장값이 적용되려면 항상 켜져 있는 쪽에 적용 주체가 따로 있어야 합니다.
/// </summary>
public class LiveHiSpeedController : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveTrackScroller _trackScroller;

    private void Awake()
    {
        SetHiSpeed(LiveHiSpeedSetting.Current);

        LiveHiSpeedSetting.OnChanged += SetHiSpeed;
    }

    private void OnDestroy()
    {
        LiveHiSpeedSetting.OnChanged -= SetHiSpeed;
    }

    private void SetHiSpeed(float hiSpeed)
    {
        _trackScroller.HiSpeed = hiSpeed;
    }
}
