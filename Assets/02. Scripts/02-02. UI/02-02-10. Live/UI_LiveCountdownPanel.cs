using UnityEngine;
using TMPro;
using VInspector;

/// <summary>
/// 곡 시작과 일시정지 재개 직전의 카운트다운 숫자를 표시합니다.
/// </summary>
public class UI_LiveCountdownPanel : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("Components")]
    [SerializeField]
    private GameObject _root;

    [SerializeField]
    private TMP_Text _countText;

    public void SetCount(int remainingSeconds)
    {
        SetVisible(true);
        _countText.text = remainingSeconds.ToString();
    }

    public void SetVisible(bool isVisible)
    {
        _root.SetActive(isVisible);
    }
}
