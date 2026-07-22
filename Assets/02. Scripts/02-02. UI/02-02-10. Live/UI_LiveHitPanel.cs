using UnityEngine;
using TMPro;
using VInspector;

namespace VELO.UI
{
    public class UI_LiveHitPanel : MonoBehaviour
    {
        [Foldout("Components")]
        [SerializeField] private TMP_Text _judgmentText;

        public void SetJudgmentText(string text)
        {
            if (_judgmentText != null)
            {
                _judgmentText.text = text;
            }
        }
    }
}
