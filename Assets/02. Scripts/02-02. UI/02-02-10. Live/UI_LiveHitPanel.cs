using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using VInspector;

public class UI_LiveHitPanel : MonoBehaviour
{
    [Header("Display")]
    [Tooltip("판정 문구가 화면에 머무는 시간(초)입니다.")]
    [SerializeField]
    private float _displaySeconds = 0.4f;

    [Foldout("Hierarchy")]
    [Header("Components")]
    [SerializeField]
    private TMP_Text _judgmentText;

    private CancellationTokenSource _hideCancellation;

    private void OnDestroy()
    {
        CancelHide();
    }

    public void SetJudgmentText(string text)
    {
        _judgmentText.text = text;
    }

    /// <summary>
    /// 판정 문구를 띄우고 잠시 뒤 지웁니다. 다음 노트가 곧바로 판정되면 타이머를 새로 걸어 문구만 갈아 끼웁니다.
    /// </summary>
    public void RefreshJudgement(EJudgement judgement)
    {
        SetJudgmentText(judgement.ToString());

        CancelHide();
        _hideCancellation = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        HideAfterDelayAsync(_hideCancellation.Token).Forget();
    }

    public void ClearJudgement()
    {
        CancelHide();
        SetJudgmentText(string.Empty);
    }

    private async UniTaskVoid HideAfterDelayAsync(CancellationToken cancellationToken)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(_displaySeconds), DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken);
        SetJudgmentText(string.Empty);
    }

    private void CancelHide()
    {
        if (_hideCancellation == null)
        {
            return;
        }

        _hideCancellation.Cancel();
        _hideCancellation.Dispose();
        _hideCancellation = null;
    }
}
