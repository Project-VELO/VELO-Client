using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 화면 좌측 상단에서 음원 재생/일시정지를 토글하는 버튼입니다. 스페이스 단축키와 같은 동작을 수행합니다.
/// </summary>
public class UI_LiveEditorPlaybackButton : MonoBehaviour
{
    // 프로젝트 기본 글꼴(GowunBatang)에 일시정지 기호(U+275A, U+23F8)가 없어 □로 깨지므로,
    // 해당 글꼴이 지원하는 이중 세로선(U+2016)을 일시정지 아이콘으로 사용합니다.
    private const string PLAY_LABEL = "▶  재생";
    private const string PAUSE_LABEL = "‖  일시정지";

    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorPlaybackControl _playbackControl;

    [SerializeField]
    private LiveEditorAudioPlayer _audioPlayer;

    [SerializeField]
    private Button _playPauseButton;

    [SerializeField]
    private TMP_Text _label;

    private bool _isShowingPauseLabel;

    private void Awake()
    {
        _playPauseButton.onClick.AddListener(_playbackControl.TogglePlayPause);
        _label.text = PLAY_LABEL;
    }

    private void Update()
    {
        RefreshLabel();
    }

    /// <summary>
    /// 매 프레임 텍스트를 대입하면 TMP가 메시를 다시 만들므로, 재생 상태가 실제로 바뀐 프레임에만 갱신합니다.
    /// </summary>
    private void RefreshLabel()
    {
        bool isPlaying = _audioPlayer.IsPlaying;
        if (isPlaying == _isShowingPauseLabel)
        {
            return;
        }

        _isShowingPauseLabel = isPlaying;
        _label.text = isPlaying ? PAUSE_LABEL : PLAY_LABEL;
    }
}
