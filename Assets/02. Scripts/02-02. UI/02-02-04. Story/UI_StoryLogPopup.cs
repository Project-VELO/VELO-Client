using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스크립트 확인 팝업입니다(기획서 SCREEN-004).
///
/// 아직 보지 않은 대사까지 포함해 회차의 전체 대사를 보여 줍니다(기획서 6.7 "전체 대사").
/// 그래서 현재 줄로 자동 스크롤하지 않고 항상 첫 대사부터 시작합니다.
///
/// 진행을 멈추고 NEXT 입력을 막는 것은 팝업이 아니라 재생 컨트롤러의 몫입니다.
/// 팝업이 재생 상태를 직접 만지면 열림·닫힘 경로마다 상태 전이가 흩어집니다.
/// </summary>
public class UI_StoryLogPopup : UI_Popup
{
    /// <summary>
    /// 팝업이 닫혔음을 알립니다. 이 팝업은 UI_Popup의 닫기 버튼으로 닫히므로,
    /// 통지가 없으면 재생 컨트롤러가 멈춘 상태에서 영영 깨어나지 못합니다.
    /// </summary>
    public Action OnClosed;

    [Foldout("Hierarchy")]
    [SerializeField]
    private ScrollRect _scrollRect;

    /// <summary>
    /// 대사 행이 들어갈 자리입니다.
    /// </summary>
    [SerializeField]
    private RectTransform _itemRoot;

    private readonly List<UI_StoryLogItem> _items = new List<UI_StoryLogItem>();

    private StoryVisualBinder _visualBinder;

    public void Init(StoryVisualBinder visualBinder)
    {
        _visualBinder = visualBinder;
    }

    /// <summary>
    /// 열기 직전에 호출해 대사를 채웁니다.
    /// 여는 시점에 한 번만 도는 구간이라 목록 할당은 허용 범위입니다.
    /// </summary>
    public void SetLines(IReadOnlyList<StoryLineData> lines)
    {
        ReleaseItems();

        for (int i = 0; i < lines.Count; i++)
        {
            UI_StoryLogItem item = AcquireItem();
            if (item == null)
            {
                break;
            }

            item.SetItem(lines[i], _visualBinder);
        }

        _scrollRect.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// 닫힐 때 행을 전부 돌려보냅니다. 남겨 두면 다음에 열 때 이전 대사가 그대로 이어 붙습니다.
    /// </summary>
    public override async Cysharp.Threading.Tasks.UniTask CloseAsync()
    {
        await base.CloseAsync();
        ReleaseItems();

        OnClosed?.Invoke();
    }

    private UI_StoryLogItem AcquireItem()
    {
        GameObject go = PoolManager.Instance.Pop(EPoolable.StoryLogItem);
        if (go == null)
        {
            return null;
        }

        var item = go.GetComponent<UI_StoryLogItem>();
        item.transform.SetParent(_itemRoot, false);
        item.transform.SetAsLastSibling();
        _items.Add(item);

        return item;
    }

    private void ReleaseItems()
    {
        foreach (UI_StoryLogItem item in _items)
        {
            item.ResetItem();
            PoolManager.Instance.Push(EPoolable.StoryLogItem, item.gameObject);
        }

        _items.Clear();
    }
}
