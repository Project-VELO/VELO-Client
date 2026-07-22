using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VInspector;

namespace VELO.UI
{
    public class UI_LiveScorePanel : MonoBehaviour
    {
        [Foldout("Components")]
        [SerializeField] private Button _pauseButton;
        [SerializeField] private TMP_Text _scoreTitleText;
        [SerializeField] private TMP_Text _scoreValueText;
        [SerializeField] private Image _progressBarFill;

        [Foldout("Rank Labels")]
        [SerializeField] private TMP_Text _rankCText;
        [SerializeField] private TMP_Text _rankBText;
        [SerializeField] private TMP_Text _rankAText;
        [SerializeField] private TMP_Text _rankSText;
        [SerializeField] private TMP_Text _rankPerfectText;

        public Button PauseButton => _pauseButton;

        public void SetScore(int score)
        {
            if (_scoreValueText != null)
            {
                _scoreValueText.text = score.ToString("N0");
            }
        }

        public void SetRankProgress(float progress01)
        {
            if (_progressBarFill != null)
            {
                _progressBarFill.fillAmount = Mathf.Clamp01(progress01);
            }
        }
    }
}
