using UnityEngine;

/// <summary>
/// 노트 하나에 대응하는 시각 오브젝트와 그 오브젝트가 속한 풀 타입을 함께 담는 불변 DTO 구조체입니다.
/// 반환 시 어떤 풀로 돌려보내야 하는지 알아야 하므로 풀 타입을 함께 보관합니다.
/// </summary>
public readonly struct LiveEditorNoteVisualHandle
{
    public readonly RectTransform RectTransform;
    public readonly EPoolable PoolType;

    public LiveEditorNoteVisualHandle(RectTransform rectTransform, EPoolable poolType)
    {
        RectTransform = rectTransform;
        PoolType = poolType;
    }
}
