using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

#region Pool
internal class Pool
{
    private GameObject _prefab;
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
                GameObject go = new GameObject() { name = $"{_prefab.name}Pool" };
                _root = go.transform;
            }
            return _root;
        }
    }

    public int LivingCount => _livingObjects.Count;

    public Pool(GameObject prefab)
    {
        _prefab = prefab;
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

public class PoolManager : MonoBehaviourSingleton<PoolManager>
{
    [SerializeField]
    private List<PoolInfo> _poolInfos = new();

    private Dictionary<EPoolable, Pool> _pools = new();

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
        foreach (var poolInfo in _poolInfos)
        {
            if (type == poolInfo.Type)
            {
                return poolInfo.Prefab;
            }
        }
        Debug.LogError($"[ObjectPool] '{type}' 타입에 대한 프리팹이 _poolInfos에 등록되지 않았습니다!");
        return null;
    }

    private void CreatePool(EPoolable type, GameObject go)
    {
        _pools.Add(type, new Pool(go));
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
