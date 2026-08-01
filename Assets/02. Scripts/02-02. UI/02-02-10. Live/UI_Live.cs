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

    /// <summary>
    /// 플레이 중에만 필요한 표시(점수·콤보·판정)를 한꺼번에 켜고 끕니다.
    /// 채보 에디터는 같은 UI를 얹어 두고 테스트 플레이 동안에만 보여 주므로, 묶어서 다루는 창구가 필요합니다.
    /// 리듬게임 씬은 항상 켜져 있으면 되므로 이 메서드를 쓰지 않습니다.
    /// </summary>
    public void SetPlayHudVisible(bool isVisible)
    {
        SetActive(_scorePanel, isVisible);
        SetActive(_comboPanel, isVisible);
        SetActive(_judgementPanel, isVisible);
    }

    private static void SetActive(MonoBehaviour panel, bool isVisible)
    {
        if (panel == null)
        {
            return;
        }

        panel.gameObject.SetActive(isVisible);
    }
}
