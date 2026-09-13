using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 격자선 오브젝트 한 벌을 풀에서 미리 확보해 두고, 이후에는 활성/비활성만 토글합니다.
/// 마디선과 분박선은 확보·반환·정리 방식이 완전히 같아 이 클래스를 하나씩 나눠 씁니다.
///
/// 유니티 생명주기를 쓰지 않으므로 컴포넌트가 아니며, LiveEditorGridRenderer가 생성해 소유합니다.
/// </summary>
public class LiveEditorGridLinePool
{
    private readonly List<RectTransform> _lines = new List<RectTransform>();
    private readonly EPoolable _poolType;

    /// <summary>
    /// 프리팹에 설정된 원본 두께입니다. 매 프레임 깊이 배율을 곱하므로 배율이 누적되지 않도록 따로 기억해 둡니다.
    /// </summary>
    public float BaseThickness { get; private set; }

    public int Count => _lines.Count;

    public LiveEditorGridLinePool(EPoolable poolType)
    {
        _poolType = poolType;
    }

    public RectTransform GetLine(int index)
    {
        return _lines[index];
    }

    public void Fill(RectTransform layer, int capacity)
    {
        while (_lines.Count < capacity)
        {
            GameObject go = PoolManager.Instance.Pop(_poolType);

            if (go == null)
            {
                return;
            }

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            BaseThickness = rectTransform.sizeDelta.y;
            rectTransform.SetParent(layer, false);
            rectTransform.gameObject.SetActive(false);
            _lines.Add(rectTransform);
        }
    }

    /// <summary>
    /// 확보해 둔 선을 풀로 되돌립니다.
    /// 풀은 꺼내 간 오브젝트를 추적하므로 반환하지 않고 사라지면 사용량 집계가 실제와 어긋나고,
    /// 풀 정리 시점에 주인 없는 오브젝트가 남습니다.
    /// 풀이 먼저 파괴된 뒤라면 되돌릴 곳이 없으므로 목록만 비웁니다.
    /// </summary>
    public void ReturnAll()
    {
        if (PoolManager.HasInstance)
        {
            foreach (RectTransform line in _lines)
            {
                if (line != null)
                {
                    PoolManager.Instance.Push(_poolType, line.gameObject);
                }
            }
        }

        _lines.Clear();
    }

    public void DeactivateFrom(int startIndex)
    {
        for (int i = startIndex; i < _lines.Count; i++)
        {
            _lines[i].gameObject.SetActive(false);
        }
    }
}
