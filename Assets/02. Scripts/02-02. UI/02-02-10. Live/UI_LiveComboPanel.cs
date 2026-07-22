using UnityEngine;
using TMPro;
using VInspector;

public class UI_LiveComboPanel : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("Components")]
    [SerializeField]
    private TMP_Text _comboCountText;

    public void SetCombo(int combo)
    {
        _comboCountText.text = combo > 0 ? combo.ToString() : "";
    }
}
