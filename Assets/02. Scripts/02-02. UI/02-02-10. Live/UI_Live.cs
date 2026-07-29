using UnityEngine;
using VInspector;

public class UI_Live : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("Sub UI Panels")]
    [SerializeField]
    private UI_LiveScorePanel _scorePanel;

    [SerializeField]
    private UI_LiveComboPanel _comboPanel;

    [SerializeField]
    private UI_LiveHitPanel _judgementPanel;

    [SerializeField]
    private UI_LiveTrackLanes _noteLanes;

    public UI_LiveScorePanel ScorePanel => _scorePanel;
    public UI_LiveComboPanel ComboPanel => _comboPanel;
    public UI_LiveHitPanel JudgementPanel => _judgementPanel;
    public UI_LiveTrackLanes NoteLanes => _noteLanes;
}
