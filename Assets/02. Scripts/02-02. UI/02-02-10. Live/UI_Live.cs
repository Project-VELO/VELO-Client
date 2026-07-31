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

    [SerializeField]
    private UI_LiveCountdownPanel _countdownPanel;

    [Tooltip("하단 리듬 버튼의 눌림 연출입니다. 판정에는 관여하지 않으므로 비워 두어도 플레이에 영향이 없습니다.")]
    [SerializeField]
    private UI_LiveLaneFeedback _laneFeedback;

    public UI_LiveScorePanel ScorePanel => _scorePanel;
    public UI_LiveComboPanel ComboPanel => _comboPanel;
    public UI_LiveHitPanel JudgementPanel => _judgementPanel;
    public UI_LiveTrackLanes NoteLanes => _noteLanes;
    public UI_LiveCountdownPanel CountdownPanel => _countdownPanel;
    public UI_LiveLaneFeedback LaneFeedback => _laneFeedback;
}
