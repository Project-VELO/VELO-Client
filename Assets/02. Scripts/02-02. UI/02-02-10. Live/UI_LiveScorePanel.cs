using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VInspector;

public class UI_LiveScorePanel : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("Components")]
    [SerializeField]
    private Button _pauseButton;

    [SerializeField]
    private TMP_Text _scoreTitleText;

    [SerializeField]
    private TMP_Text _scoreValueText;

    [SerializeField]
    private Image _progressBarFill;

    [Foldout("Hierarchy")]
    [Header("Rank Labels")]
    [SerializeField]
    private TMP_Text _rankCText;

    [SerializeField]
    private TMP_Text _rankBText;

    [SerializeField]
    private TMP_Text _rankAText;

    [SerializeField]
    private TMP_Text _rankSText;

    [SerializeField]
    private TMP_Text _rankPerfectText;

    public Button PauseButton => _pauseButton;

    public void SetScore(int score)
    {
        _scoreValueText.text = score.ToString("N0");
    }

    public void SetRankProgress(float progress01)
    {
        _progressBarFill.fillAmount = Mathf.Clamp01(progress01);
    }
}
