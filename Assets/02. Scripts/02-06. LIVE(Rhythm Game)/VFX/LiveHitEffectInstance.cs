using UnityEngine;

/// <summary>
/// 화면에 떠 있는 타격 이펙트 하나입니다. 돌려줄 풀과 돌려줄 시각을 함께 들고 있어야 꺼낸 순서대로 정리할 수 있습니다.
/// </summary>
public readonly struct LiveHitEffectInstance
{
    public readonly GameObject Effect;
    public readonly EPoolable PoolType;
    public readonly float ReleaseTime;

    public LiveHitEffectInstance(GameObject effect, EPoolable poolType, float releaseTime)
    {
        Effect = effect;
        PoolType = poolType;
        ReleaseTime = releaseTime;
    }
}
