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
    private TMP_Text _scoreValueText;

    [SerializeField]
    private UI_LiveRankProgressBar _rankProgressBar;

    public Button PauseButton => _pauseButton;

    public void SetScore(int score)
    {
        _scoreValueText.text = score.ToString("N0");
    }

    public void RefreshRankProgress(float accuracy, ELiveRank rank)
    {
        _rankProgressBar.RefreshProgress(accuracy, rank);
    }
}
