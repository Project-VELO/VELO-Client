using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스크립트 확인 팝업입니다(기획서 SCREEN-004).
///
/// 지금까지 지나온 대사만 보여 줍니다. 아직 넘기지 않은 줄을 함께 띄우면 앞으로의 전개가
/// 미리 새어 나가므로, 회차 전체가 아니라 커서가 지나온 구간까지만 채웁니다.
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
    /// 열기 직전에 호출해 대사를 채웁니다. readCount는 지금까지 지나온 줄 수입니다.
    ///
    /// 잘라낸 목록을 따로 만들지 않고 개수만 받는 이유는, 회차가 길어질수록 그 사본이
    /// 팝업을 열 때마다 통째로 새로 생기기 때문입니다.
    /// </summary>
    public void SetLines(IReadOnlyList<StoryLineData> lines, int readCount)
    {
        ReleaseItems();

        int count = Mathf.Clamp(readCount, 0, lines.Count);

        for (int i = 0; i < count; i++)
        {
            UI_StoryLogItem item = AcquireItem();
            if (item == null)
            {
                break;
            }

            item.SetItem(lines[i], _visualBinder);
        }
    }

    /// <summary>
    /// 열기 애니메이션이 시작되기 전에 스크롤 위치를 잡습니다.
    ///
    /// SetLines는 팝업이 꺼져 있을 때 호출되어 레이아웃이 계산되지 않으므로 그때는 위치를 잡을 수 없고,
    /// 그렇다고 base를 먼저 기다리면 이미 보이는 상태에서 목록이 아래로 밀려 내려가는 것이 눈에 띕니다.
    /// 그래서 오브젝트만 먼저 켜서 레이아웃을 확정한 뒤, 자리를 잡고 나서 애니메이션을 재생합니다.
    /// </summary>
    public override async Cysharp.Threading.Tasks.UniTask OpenAsync()
    {
        gameObject.SetActive(true);
        ScrollToLatest();

        await base.OpenAsync();
    }

    /// <summary>
    /// 가장 최근 대사가 보이도록 맨 아래에서 시작합니다.
    /// 방금 지나온 대사를 다시 확인하려고 여는 팝업이라, 첫 줄부터 보여 주면 매번 끝까지 내려야 합니다.
    ///
    /// 행을 붙인 직후에는 아직 레이아웃이 다시 계산되지 않아 스크롤 범위가 이전 크기 그대로입니다.
    /// 그 상태에서 위치를 지정하면 ScrollRect가 옛 범위로 값을 잘라내 엉뚱한 곳에 멈춥니다.
    /// 그래서 크기를 먼저 확정한 뒤 위치를 잡습니다.
    /// </summary>
    private void ScrollToLatest()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_itemRoot);
        Canvas.ForceUpdateCanvases();

        _scrollRect.verticalNormalizedPosition = 0f;
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
