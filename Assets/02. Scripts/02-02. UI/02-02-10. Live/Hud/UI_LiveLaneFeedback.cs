using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 키 입력이 들어온 레인의 판정선 자리에서 타격 VFX를 재생합니다.
/// 판정은 키보드 입력으로만 이루어지므로 이 컴포넌트는 표시만 담당하며, 버튼 클릭을 받지 않습니다.
/// </summary>
public class UI_LiveLaneFeedback : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Tooltip("레인 1번부터 순서대로, 각 레인이 판정선과 만나는 자리에 놓아 둡니다. 비어 있는 레인은 연출을 건너뜁니다.")]
    [SerializeField]
    private List<ParticleSystem> _laneVfxs = new List<ParticleSystem>();

    private void Awake()
    {
        ClearLaneVfxs();
    }

    public void RefreshLanePress(int lane)
    {
        int index = lane - LiveLane.FIRST;

        if (index < 0 || LiveLane.COUNT <= index)
        {
            return;
        }

        PlayLaneVfx(index);
    }

    /// <summary>
    /// 재생 중인 타격 연출을 모두 걷어 냅니다. 곡을 되감을 때 이전 판정의 잔상이 남지 않게 합니다.
    /// </summary>
    public void ClearLaneVfxs()
    {
        for (int i = 0; i < _laneVfxs.Count; i++)
        {
            ParticleSystem vfx = _laneVfxs[i];

            if (vfx == null)
            {
                continue;
            }

            vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // VFX가 아직 준비되지 않아 인스펙터가 비어 있을 수 있으므로, 할당된 레인만 골라 재생합니다.
    private void PlayLaneVfx(int index)
    {
        if (_laneVfxs.Count <= index)
        {
            return;
        }

        ParticleSystem vfx = _laneVfxs[index];

        if (vfx == null)
        {
            return;
        }

        // 연타 시 이전 재생이 남아 있으면 새 타격이 묻히므로, 항상 처음부터 다시 재생합니다.
        vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        vfx.Play(true);
    }
}
