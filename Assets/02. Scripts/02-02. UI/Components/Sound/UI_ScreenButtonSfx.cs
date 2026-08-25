using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 이 화면의 버튼 전부에 조작음을 붙입니다.
///
/// 버튼이 117개라 하나씩 손으로 붙일 수 없습니다. 화면 뿌리에 이것 하나만 두면
/// 아래의 버튼이 모두 소리를 갖고, 버튼이 늘어도 다시 손댈 일이 없습니다.
///
/// 이미 UI_ButtonSfx를 갖고 있는 버튼은 건드리지 않습니다. 뒤로가기처럼 다른 소리를
/// 내야 하는 버튼이 프리팹에 값을 적어 두고, 여기서는 그 값을 덮어쓰지 않습니다.
///
/// 풀에서 꺼내 쓰는 행(곡 목록 등)은 화면이 세워질 때 아직 없으므로 여기에 걸리지 않습니다.
/// 그런 프리팹은 자기 프리팹에 UI_ButtonSfx를 직접 붙입니다.
/// </summary>
public class UI_ScreenButtonSfx : MonoBehaviour
{
    /// <summary>
    /// 꺼져 있는 버튼도 포함합니다. 팝업 안의 버튼처럼 나중에 켜지는 것이 많습니다.
    /// </summary>
    private void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].GetComponent<UI_ButtonSfx>() == null)
            {
                buttons[i].gameObject.AddComponent<UI_ButtonSfx>();
            }
        }
    }
}
