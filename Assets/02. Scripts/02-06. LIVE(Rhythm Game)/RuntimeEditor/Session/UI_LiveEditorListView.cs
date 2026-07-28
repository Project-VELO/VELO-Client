using System.Collections.Generic;
using UnityEngine;
using VInspector;

/// <summary>
/// 목록 팝업에서 항목 버튼을 풀에서 꺼내 배치하고 반환하는 것을 전담합니다.
/// 곡 목록과 난이도 목록이 같은 항목 프리팹을 공유하므로, 두 팝업이 이 클래스를 함께 사용합니다.
/// </summary>
public class UI_LiveEditorListView : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private RectTransform _itemRoot;

    private readonly List<UI_LiveEditorListItem> _items = new List<UI_LiveEditorListItem>();

    public int ItemCount => _items.Count;

    public UI_LiveEditorListItem AcquireItem()
    {
        GameObject go = PoolManager.Instance.Pop(EPoolable.EditorListItem);
        if (go == null)
        {
            return null;
        }

        var item = go.GetComponent<UI_LiveEditorListItem>();
        item.transform.SetParent(_itemRoot, false);
        item.transform.SetAsLastSibling();
        _items.Add(item);

        return item;
    }

    public void ReleaseItems()
    {
        foreach (UI_LiveEditorListItem item in _items)
        {
            item.ResetItem();
            PoolManager.Instance.Push(EPoolable.EditorListItem, item.gameObject);
        }

        _items.Clear();
    }

    public void SetSelectedIndex(int selectedIndex)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            _items[i].SetSelected(i == selectedIndex);
        }
    }
}
