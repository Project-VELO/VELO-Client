using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 그림을 글자로 바꾼 뒤 끊어진 참조를 다시 물립니다.
///
/// 컴포넌트가 그 Image를 들고 있었다면 Image를 지우는 순간 참조가 빕니다. 필드 타입까지
/// 글자로 바꿔 두었어도 인스펙터에 다시 끌어다 놓기 전에는 비어 있어, 화면에서는 라벨이
/// 통째로 사라진 것처럼 보입니다. 바꾸는 김에 여기서 채웁니다.
///
/// 아는 컴포넌트만 다룹니다. 모든 컴포넌트를 뒤져 타입이 맞는 빈 칸을 채우는 방식은
/// 엉뚱한 칸을 채울 수 있고, 무엇이 어떻게 채워졌는지 읽어 낼 수 없습니다.
/// </summary>
public static class UiTextImageRewire
{
    private const string SCHEDULE_LABEL_FIELD = "_label";

    public static void Apply(Transform node, TMP_Text text)
    {
        UI_ScheduleShortcutButton shortcut = node.GetComponentInParent<UI_ScheduleShortcutButton>();

        if (shortcut == null)
        {
            return;
        }

        SerializedObject serialized = new SerializedObject(shortcut);
        SerializedProperty label = serialized.FindProperty(SCHEDULE_LABEL_FIELD);

        if (label == null)
        {
            Debug.LogWarning($"[UiTextImageRewire] {SCHEDULE_LABEL_FIELD} 칸을 찾지 못했습니다: {shortcut.name}");
            return;
        }

        label.objectReferenceValue = text;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
