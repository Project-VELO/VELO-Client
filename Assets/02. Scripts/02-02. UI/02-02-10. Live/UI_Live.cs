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

    [Tooltip("레인 하단 판정 링의 연출입니다. 판정에는 관여하지 않지만 링은 상시 표시되는 아트이므로, 비워 두면 트랙 아래가 비어 보입니다.")]
    [SerializeField]
    private UI_LiveLaneFeedback _laneFeedback;

    private LiveJudgementProcessor _judgementProcessor;

    public UI_LiveScorePanel ScorePanel => _scorePanel;
    public UI_LiveComboPanel ComboPanel => _comboPanel;
    public UI_LiveHitPanel JudgementPanel => _judgementPanel;
    public UI_LiveTrackLanes NoteLanes => _noteLanes;
    public UI_LiveCountdownPanel CountdownPanel => _countdownPanel;
    public UI_LiveLaneFeedback LaneFeedback => _laneFeedback;

    private void OnDestroy()
    {
        if (_judgementProcessor == null)
        {
            return;
        }

        _judgementProcessor.OnNoteJudged -= RefreshJudgement;
        _judgementProcessor.OnScoreChanged -= RefreshScoreHud;
        _judgementProcessor.OnSessionReset -= ResetHud;
    }

    /// <summary>
    /// 판정기의 통지를 받아 점수·콤보·판정 표시를 갱신합니다. 판정기가 UI를 직접 참조하는 대신
    /// 화면이 도메인을 관찰하는 방향이라, 씬을 여는 컨트롤러가 이 메서드로 배선을 맡깁니다.
    /// </summary>
    public void BindJudgement(LiveJudgementProcessor judgementProcessor)
    {
        _judgementProcessor = judgementProcessor;

        judgementProcessor.OnNoteJudged += RefreshJudgement;
        judgementProcessor.OnScoreChanged += RefreshScoreHud;
        judgementProcessor.OnSessionReset += ResetHud;
    }

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

    private void RefreshJudgement(NoteData note, EJudgement judgement)
    {
        _judgementPanel.RefreshJudgement(judgement);
    }

    private void RefreshScoreHud()
    {
        LiveScoreTracker tracker = _judgementProcessor.ScoreTracker;

        _scorePanel.SetScore(tracker.Score);
        _comboPanel.SetCombo(tracker.Combo);

        // 진행도 막대는 현재까지의 정확도를 나타내므로, 전체 노트가 아니라 이미 판정된 노트를 분모로 씁니다.
        float accuracy = LiveRankEvaluator.GetAccuracy(tracker.Score, tracker.JudgedNoteCount);
        _scorePanel.SetRankProgress(accuracy * 0.01f);
    }

    private void ResetHud()
    {
        RefreshScoreHud();
        _judgementPanel.ClearJudgement();
    }
}
