using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 주차 완료 안내 팝업입니다(기획서 9.5 "주차 마무리 연출", SCREEN-011).
///
/// 해금 편수만 알려주고 닫힙니다. 스토리 목록으로의 강제 이동은 기획서 3-F-2가 금지합니다.
/// 확인 버튼은 UI_Popup의 닫기 버튼 자리에 그대로 배선합니다 — 결과 분기가 없어 구분이 필요 없습니다.
///
/// 닫힘 통지를 CloseAsync에서 내는 것은 ESC(UIManager.HandleEscapeInput)로 닫혀도
/// 마무리 시퀀스가 이어져야 하기 때문입니다. 버튼 콜백에서만 통지하면 그 경로가 새어 나갑니다.
/// </summary>
public class UI_WeekCompletePopup : UI_Popup
{
    public Action OnClosed;

    private const string UNLOCK_MESSAGE_FORMAT = "이번 주차를 완료했습니다!\n새 스토리 {0}편이 해금되었습니다.";
    private const string NO_UNLOCK_MESSAGE = "이번 주차의 모든 스케줄을 마쳤습니다.";

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _messageText;

    /// <summary>
    /// 해금 편수를 안내문에 채웁니다. 이미 완료된 주차를 다시 마무리한 경우 등 해금이 없으면 0이 옵니다.
    /// </summary>
    public void SetResult(int unlockedStoryCount)
    {
        _messageText.text = 0 < unlockedStoryCount
            ? string.Format(UNLOCK_MESSAGE_FORMAT, unlockedStoryCount)
            : NO_UNLOCK_MESSAGE;
    }

    public override async UniTask CloseAsync()
    {
        await base.CloseAsync();
        OnClosed?.Invoke();
    }
}
