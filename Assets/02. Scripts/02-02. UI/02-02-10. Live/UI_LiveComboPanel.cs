using UnityEngine;
using TMPro;
using VInspector;

namespace VELO.UI
{
    public class UI_LiveComboPanel : MonoBehaviour
    {
        [Foldout("Components")]
        [SerializeField] private TMP_Text _comboTitleText;
        [SerializeField] private TMP_Text _comboCountText;

        public void SetCombo(int combo)
        {
            if (_comboCountText != null)
            {
                _comboCountText.text = combo > 0 ? combo.ToString() : "";
            }
        }
    }
}
