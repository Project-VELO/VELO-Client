using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JsonUtility가 Dictionary를 직렬화하지 못하므로, 키와 값을 각각 List로 펼쳐 저장하고
/// 역직렬화 시 Dictionary로 되돌리는 컨테이너입니다.
///
/// 제네릭 필드 직렬화는 Unity 2020.1부터 지원되므로, 타입 조합마다 구체 서브클래스를 만들 필요가 없습니다.
/// 마스터 데이터와 세이브 데이터 양쪽이 이 클래스 하나를 공유합니다.
///
/// TKey와 TValue는 반드시 JsonUtility가 직렬화할 수 있는 타입이어야 합니다.
/// (기본형, string, enum, [Serializable]이 붙은 클래스·구조체)
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> _keys = new List<TKey>();

    [SerializeField]
    private List<TValue> _values = new List<TValue>();

    private Dictionary<TKey, TValue> _pairs = new Dictionary<TKey, TValue>();

    /// <summary>
    /// 런타임 조회에 사용하는 실제 딕셔너리입니다. 이 참조를 통해 수정한 내용은 다음 직렬화 시 그대로 반영됩니다.
    /// </summary>
    public Dictionary<TKey, TValue> Pairs => _pairs;

    public int Count => _pairs.Count;

    public TValue this[TKey key]
    {
        get => _pairs[key];
        set => _pairs[key] = value;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return _pairs.TryGetValue(key, out value);
    }

    public bool ContainsKey(TKey key)
    {
        return _pairs.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        return _pairs.Remove(key);
    }

    public void Clear()
    {
        _pairs.Clear();
        _keys.Clear();
        _values.Clear();
    }

    public void OnBeforeSerialize()
    {
        _keys.Clear();
        _values.Clear();

        foreach (KeyValuePair<TKey, TValue> pair in _pairs)
        {
            _keys.Add(pair.Key);
            _values.Add(pair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        _pairs.Clear();

        // 키와 값의 개수가 어긋나는 것은 파일이 손상되었다는 뜻입니다.
        // 통째로 버리면 세이브 전체를 잃으므로, 짝이 맞는 데까지만 복원하고 손실을 알립니다.
        if (_keys.Count != _values.Count)
        {
            Debug.LogWarning($"[SerializableDictionary] 키({_keys.Count})와 값({_values.Count})의 개수가 다릅니다. 짝이 맞는 항목까지만 복원합니다.");
        }

        int restorableCount = Mathf.Min(_keys.Count, _values.Count);
        for (int i = 0; i < restorableCount; i++)
        {
            _pairs[_keys[i]] = _values[i];
        }
    }
}
