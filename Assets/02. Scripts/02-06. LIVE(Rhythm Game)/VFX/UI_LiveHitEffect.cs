using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 판정이 확정된 레인의 판정선 자리에 타격 이펙트를 띄우고, 그 레인을 한 번 번쩍이게 합니다.
///
/// PERFECT만 가장 큰 이펙트를 쓰고 GREAT와 GOOD은 한 단계 작은 이펙트를 씁니다. BAD는 제대로 친 타격이 아니므로 아무것도 띄우지 않습니다.
/// 연타에서는 앞선 이펙트가 끝나기 전에 다음 이펙트가 겹쳐 나와야 하므로, 레인마다 하나를 두고 다시 재생하지 않고
/// 풀에서 매번 꺼내 씁니다. 이펙트 프리팹은 UI Particle(com.coffee.ui-particle)로 캔버스 안에서 그려지고
/// 켜질 때 파티클이 처음부터 재생되므로, 풀에서 꺼내는 것만으로 다시 재생됩니다.
/// </summary>
public class UI_LiveHitEffect : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Tooltip("풀에서 꺼낸 타격 이펙트를 붙이는 레이어입니다.")]
    [SerializeField]
    private RectTransform _effectLayer;

    [SerializeField]
    private UI_LiveLaneFlash _laneFlash;

    [Header("Placement")]
    [Tooltip("판정선에서 이펙트 중심까지의 높이입니다(1920x1080 시안 px). 판정선 위아래 두 줄 사이를 채우는 노트 높이 56의 절반이라 노트 한가운데에서 터집니다.")]
    [SerializeField]
    private float _hitLineOffsetY = 28f;

    [Header("Timing")]
    [Tooltip("이펙트를 풀에 돌려주기까지의 시간(초)입니다. 파티클이 모두 사라지는 시간보다 짧으면 잘려 보입니다.")]
    [SerializeField]
    private float _effectLifetime = 1f;

    // 모든 이펙트가 같은 수명으로 꺼낸 순서대로 끝나므로, 맨 앞만 확인해도 돌려줄 것을 빠짐없이 찾습니다.
    private readonly Queue<LiveHitEffectInstance> _activeEffects = new Queue<LiveHitEffectInstance>();

    private UI_LiveTrackLanes _lanes;

    private void Update()
    {
        ReleaseExpiredEffects();
    }

    /// <summary>
    /// 이펙트 자리는 레인 모양에서 구하는데 트랙은 다른 프리팹에 있으므로, 씬에서 이어 준 레인을 넘겨받습니다.
    /// </summary>
    public void Init(UI_LiveTrackLanes lanes)
    {
        _lanes = lanes;
        _laneFlash.Init(lanes);
    }

    public void RefreshHitEffect(int lane, EJudgement judgement)
    {
        if (judgement == EJudgement.BAD || !LiveLane.IsValid(lane))
        {
            return;
        }

        EPoolable effectType = judgement == EJudgement.PERFECT ? EPoolable.LiveHitEffectPerfect : EPoolable.LiveHitEffectGreat;

        SpawnEffect(effectType, lane);
        _laneFlash.RefreshFlash(lane);
    }

    /// <summary>
    /// 떠 있는 이펙트와 번쩍임을 모두 걷어 냅니다. 다시 시작할 때 이전 판의 잔상이 남지 않게 합니다.
    /// </summary>
    public void ClearHitEffects()
    {
        while (0 < _activeEffects.Count)
        {
            ReleaseEffect(_activeEffects.Dequeue());
        }

        _laneFlash.ClearFlashes();
    }

    private void SpawnEffect(EPoolable effectType, int lane)
    {
        GameObject effect = PoolManager.Instance.Pop(effectType);

        if (effect == null)
        {
            return;
        }

        Transform effectTransform = effect.transform;
        effectTransform.SetParent(_effectLayer, false);
        effectTransform.localPosition = GetEffectLocalPosition(lane);

        _activeEffects.Enqueue(new LiveHitEffectInstance(effect, effectType, Time.unscaledTime + _effectLifetime));
    }

    /// <summary>
    /// 노트가 판정선 위아래 두 줄 사이를 채우므로, 판정선에서 노트 반높이만큼 올린 높이의 레인 한가운데에 띄웁니다.
    /// 트랙과 이펙트 레이어는 서로 다른 캔버스에 있으므로 월드 좌표를 거쳐 옮깁니다.
    /// </summary>
    private Vector3 GetEffectLocalPosition(int lane)
    {
        LiveTrackShape shape = _lanes.Shape;
        float hitLineScreenY = shape.GetScreenYAtRatio(_lanes.GetHitLineVerticalRatio());
        float effectRatio = shape.GetRatioAtScreenY(hitLineScreenY + _hitLineOffsetY);

        Vector3 world = _lanes.rectTransform.TransformPoint(_lanes.GetLaneCenterPosition(lane, effectRatio));
        Vector3 local = _effectLayer.InverseTransformPoint(world);

        return new Vector3(local.x, local.y, 0f);
    }

    private void ReleaseExpiredEffects()
    {
        float now = Time.unscaledTime;

        while (0 < _activeEffects.Count && _activeEffects.Peek().ReleaseTime <= now)
        {
            ReleaseEffect(_activeEffects.Dequeue());
        }
    }

    private static void ReleaseEffect(LiveHitEffectInstance instance)
    {
        PoolManager.Instance.Push(instance.PoolType, instance.Effect);
    }
}
