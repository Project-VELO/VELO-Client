using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VInspector;

public abstract class UI_Popup : MonoBehaviour
{
    /// <summary>
    /// 닫기 요청을 받을 소유자(UI_PopupHandler)가 팝업을 열 때 주입합니다.
    /// 팝업이 UIManager를 직접 호출하면 팝업 계층 전체가 매니저와 양방향으로 얽히므로,
    /// 팝업은 이 콜백으로 요청만 하고 실제 닫기는 소유자가 결정합니다.
    /// </summary>
    public Action<UI_Popup> OnCloseRequested;

    [Foldout("Hierarchy")]
    [SerializeField]
    private UnityEngine.UI.Button _closeButton;

    public bool IsClosing { get; private set; }

    private UI_SpriteAnimator[] _spriteAnimators;

    protected virtual void Awake()
    {
        _spriteAnimators = GetComponentsInChildren<UI_SpriteAnimator>(true);
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    public virtual void InitPopup()
    {
    }

    public virtual async UniTask OpenAsync()
    {
        gameObject.SetActive(true);
        if (TryGetComponent<UI_ScaleAnimator>(out var animator))
        {
            await animator.PlayOpenAsync(this.GetCancellationTokenOnDestroy());
        }
    }

    public virtual async UniTask CloseAsync()
    {
        IsClosing = true;

        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        if (_spriteAnimators != null && 0 < _spriteAnimators.Length)
        {
            var tasks = new List<UniTask>();
            foreach (var spriteAnimator in _spriteAnimators)
            {
                if (spriteAnimator != null && spriteAnimator.isActiveAndEnabled)
                {
                    tasks.Add(spriteAnimator.PlayReverseAsync(this.GetCancellationTokenOnDestroy()));
                }
            }
            
            if (0 < tasks.Count)
            {
                await UniTask.WhenAll(tasks);
            }
        }

        if (TryGetComponent<UI_ScaleAnimator>(out var animator))
        {
            await animator.PlayCloseAsync(this.GetCancellationTokenOnDestroy());
        }

        gameObject.SetActive(false);
        IsClosing = false;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnCloseButtonClicked()
    {
        RequestClose();
    }

    /// <summary>
    /// 열리지 않은 팝업(콜백 미주입)에서는 아무 일도 하지 않습니다.
    /// 스택에 없는 팝업을 임의로 꺼 버리면 형제 순서 복원과 입력 모드 정리가 건너뛰어지기 때문입니다.
    /// </summary>
    protected void RequestClose()
    {
        OnCloseRequested?.Invoke(this);
    }
}
