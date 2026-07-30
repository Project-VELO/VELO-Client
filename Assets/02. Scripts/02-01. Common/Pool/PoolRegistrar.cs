using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 자기 씬에서 쓸 풀 프리팹을 PoolManager에 등록하고, 씬이 내려갈 때 해제합니다.
/// 풀 매니저는 PersistentScene에 상주하므로, 씬 전용 프리팹 참조를 퍼시스턴트 프리팹에 심지 않기 위해
/// 등록 책임만 각 씬으로 내려 둡니다.
/// </summary>
public class PoolRegistrar : MonoBehaviour
{
    [Foldout("Project")]
    [SerializeField]
    private List<PoolInfo> _poolInfos = new List<PoolInfo>();

    private void Awake()
    {
        InitPools();
    }

    private void OnDestroy()
    {
        ReleasePools();
    }

    private void InitPools()
    {
        if (PoolManager.Instance == null)
        {
            Debug.LogError("[PoolRegistrar] PoolManager를 찾지 못했습니다. PersistentScene이 로드되어 있는지 확인해 주세요.");
            return;
        }

        foreach (PoolInfo poolInfo in _poolInfos)
        {
            PoolManager.Instance.RegisterPool(poolInfo.Type, poolInfo.Prefab);
        }
    }

    /// <summary>
    /// 해체 시점에 싱글톤을 직접 만지지 않도록 별도 메서드로 분리했습니다.
    /// 애플리케이션 종료 중에는 매니저가 이미 사라졌을 수 있어 존재 여부를 먼저 확인합니다.
    /// </summary>
    private void ReleasePools()
    {
        if (!PoolManager.HasInstance)
        {
            return;
        }

        foreach (PoolInfo poolInfo in _poolInfos)
        {
            PoolManager.Instance.UnregisterPool(poolInfo.Type);
        }
    }
}
