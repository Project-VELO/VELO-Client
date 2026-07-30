using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

#region Pool
internal class Pool
{
    private GameObject _prefab;
    private Transform _parent;
    private IObjectPool<GameObject> _pool;

    // Pop으로 꺼내 간 오브젝트를 따로 기억합니다.
    // 사용처가 오브젝트를 비활성 상태로 두었다가 반환하는 경우가 있어 활성 여부로는 이중 반환을 걸러낼 수 없고,
    // 걸러내지 못한 이중 반환은 UnityEngine.Pool의 중복 검사에서 예외로 이어집니다.
    private readonly HashSet<GameObject> _livingObjects = new();

    private Transform _root;
    private Transform Root
    {
        get
        {
            if (_root == null)
            {
                // 부모를 지정하지 않으면 루트가 "그 순간의 활성 씬"에 생깁니다.
                // 씬 로드 직후에는 아직 활성 씬이 바뀌기 전이라 루트만 엉뚱한 씬에 남아 풀 오브젝트가 새어 나가므로,
                // 항상 풀 매니저 아래에 매답니다.
                GameObject go = new GameObject() { name = $"{_prefab.name}Pool" };
                go.transform.SetParent(_parent, false);
                _root = go.transform;
            }
            return _root;
        }
    }

    public int LivingCount => _livingObjects.Count;

    public Pool(GameObject prefab, Transform parent)
    {
        _prefab = prefab;
        _parent = parent;
        _pool = new ObjectPool<GameObject>(OnCreate, OnGet, OnRelease, OnDestroy);
    }

    public bool Push(GameObject go)
    {
        // 꺼내 간 기록이 없는 오브젝트는 이중 반환이거나 이 풀의 것이 아니므로 받지 않습니다.
        if (go == null || !_livingObjects.Remove(go))
        {
            return false;
        }

        _pool.Release(go);
        return true;
    }

    public GameObject Pop()
    {
        GameObject go = _pool.Get();
        _livingObjects.Add(go);
        return go;
    }

    /// <summary>
    /// 풀을 통째로 정리합니다. 아직 반환되지 않은 오브젝트까지 함께 파괴하므로,
    /// 호출 이후에는 사용처가 들고 있던 참조가 모두 무효가 됩니다.
    /// </summary>
    public void Clear()
    {
        // 꺼내 간 오브젝트는 풀 루트 밖에 있어 루트만 지워서는 정리되지 않습니다.
        foreach (GameObject go in _livingObjects)
        {
            if (go != null)
            {
                GameObject.Destroy(go);
            }
        }
        _livingObjects.Clear();

        _pool.Clear();

        if (_root != null)
        {
            GameObject.Destroy(_root.gameObject);
            _root = null;
        }
    }

    #region Funcs
    private GameObject OnCreate()
    {
        GameObject go = GameObject.Instantiate(_prefab);
        go.transform.SetParent(Root);
        go.name = _prefab.name;
        return go;
    }

    private void OnGet(GameObject go)
    {
        go.SetActive(true);
    }

    private void OnRelease(GameObject go)
    {
        go.SetActive(false);

        // 반환된 오브젝트를 풀 루트로 되돌립니다.
        // 사용처가 Pop 직후 부모를 옮기기 때문에, 되돌리지 않으면 계층에서 풀이 항상 비어 보여
        // 실제 대기 물량을 눈으로 확인할 수 없습니다.
        // 씬이 닫히는 중에 반환이 들어올 수 있어, 없는 루트를 새로 만들지는 않습니다.
        if (_root != null)
        {
            go.transform.SetParent(_root, false);
        }
    }

    private void OnDestroy(GameObject go)
    {
        GameObject.Destroy(go);
    }
    #endregion
}
#endregion

/// <summary>
/// 오브젝트 풀을 총괄합니다. PersistentScene에 상주하며, 어떤 프리팹을 어떤 타입으로 풀링할지는
/// 각 씬의 PoolRegistrar가 런타임에 등록하고 씬이 내려갈 때 해제합니다.
/// 이렇게 하면 씬 전용 프리팹이 퍼시스턴트 프리팹에 묶이지 않고, 풀 오브젝트의 수명도 매니저와 일치합니다.
/// </summary>
public class PoolManager : MonoBehaviourSingleton<PoolManager>
{
    [SerializeField]
    private List<PoolInfo> _poolInfos = new();

    private Dictionary<EPoolable, Pool> _pools = new();
    private Dictionary<EPoolable, GameObject> _registeredPrefabs = new();

    /// <summary>
    /// 씬이 자기 프리팹을 풀에 등록합니다. 같은 타입에 다른 프리팹이 들어오면 기존 풀을 비우고 교체합니다.
    /// </summary>
    public void RegisterPool(EPoolable type, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError($"[ObjectPool] '{type}'에 등록하려는 프리팹이 비어 있습니다!");
            return;
        }

        if (_registeredPrefabs.TryGetValue(type, out GameObject registered) && registered != prefab)
        {
            Debug.LogWarning($"[ObjectPool] '{type}'에 이미 다른 프리팹이 등록되어 있어 교체합니다. 등록 주체가 겹치는지 확인이 필요합니다.");
            ClearPool(type);
        }

        _registeredPrefabs[type] = prefab;
    }

    /// <summary>
    /// 등록을 해제하고 해당 풀을 비웁니다. 풀 오브젝트는 매니저 아래에 있어 씬 언로드로는 정리되지 않으므로,
    /// 등록을 건 쪽이 반드시 해제해 주어야 합니다.
    /// 씬 전환은 새 씬을 모두 띄운 뒤 이전 씬을 내리므로, 등록해 둔 프리팹이 그대로 남아 있을 때만 해제합니다.
    /// 그렇지 않으면 물러나는 씬이 새 씬의 등록을 지워 Pop이 실패합니다.
    /// </summary>
    public void UnregisterPool(EPoolable type, GameObject prefab)
    {
        if (!_registeredPrefabs.TryGetValue(type, out GameObject registered) || registered != prefab)
        {
            return;
        }

        ClearPool(type);
        _registeredPrefabs.Remove(type);
    }

    private void ClearPool(EPoolable type)
    {
        if (!_pools.TryGetValue(type, out Pool pool))
        {
            return;
        }

        pool.Clear();
        _pools.Remove(type);
    }

    /// <summary>
    /// 오브젝트를 풀로 반환합니다. 반환에 실패하면 그 오브젝트는 풀의 관리를 벗어나 주인 없이 남으므로,
    /// 실패를 조용히 넘기지 않고 경고로 알립니다.
    /// </summary>
    public bool Push(EPoolable type, GameObject go)
    {
        if (!_pools.TryGetValue(type, out Pool pool))
        {
            Debug.LogWarning($"[ObjectPool] '{type}' 타입의 풀이 아직 없어 '{(go == null ? "null" : go.name)}' 반환에 실패했습니다!");
            return false;
        }

        if (!pool.Push(go))
        {
            Debug.LogWarning($"[ObjectPool] '{type}' 풀이 꺼내 준 적 없는 '{(go == null ? "null" : go.name)}'을(를) 반환하려 했습니다! 이중 반환인지 확인이 필요합니다.");
            return false;
        }

        return true;
    }

    public GameObject Pop(EPoolable type)
    {
        if (_pools.TryGetValue(type, out Pool pool))
        {
            return pool.Pop();
        }

        GameObject prefab = GetPrefabOnType(type);
        if (prefab == null)
        {
            return null;
        }

        CreatePool(type, prefab);
        return _pools[type].Pop();
    }

    /// <summary>
    /// 해당 타입의 풀이 꺼내 주고 아직 돌려받지 못한 오브젝트 수입니다. 반환 누락을 확인할 때 사용합니다.
    /// </summary>
    public int GetLivingCount(EPoolable type)
    {
        return _pools.TryGetValue(type, out Pool pool) ? pool.LivingCount : 0;
    }

    private GameObject GetPrefabOnType(EPoolable type)
    {
        if (_registeredPrefabs.TryGetValue(type, out GameObject registered))
        {
            return registered;
        }

        // 인스펙터에 직접 채워 둔 항목은 씬 등록 없이도 쓸 수 있게 남겨 둡니다.
        foreach (var poolInfo in _poolInfos)
        {
            if (type == poolInfo.Type)
            {
                return poolInfo.Prefab;
            }
        }

        Debug.LogError($"[ObjectPool] '{type}' 타입에 대한 프리팹이 등록되지 않았습니다! 해당 씬에 PoolRegistrar가 있는지 확인해 주세요.");
        return null;
    }

    private void CreatePool(EPoolable type, GameObject go)
    {
        _pools.Add(type, new Pool(go, transform));
    }

    public void Clear()
    {
        foreach (var pool in _pools.Values)
        {
            pool.Clear();
        }
        _pools.Clear();
    }
}
