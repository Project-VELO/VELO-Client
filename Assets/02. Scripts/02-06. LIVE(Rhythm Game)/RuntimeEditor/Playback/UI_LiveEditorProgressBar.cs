using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VInspector;

/// <summary>
/// 곡 전체에서 현재 재생 위치가 어디쯤인지 보여 주고, 막대의 특정 지점을 눌러 그 위치로 이동시키는 UGUI 진행바입니다.
/// 누른 채로 끌면 이어서 이동하므로 긴 곡에서도 원하는 구간을 빠르게 찾을 수 있습니다.
/// </summary>
public class UI_LiveEditorProgressBar : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private const int UNBOUND_SECONDS = -1;

    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveAudioPlayer _audioPlayer;

    [SerializeField]
    private LiveEditorPlaybackControl _playbackControl;

    [SerializeField]
    private RectTransform _barRect;

    [SerializeField]
    private RectTransform _fillRect;

    [SerializeField]
    private TMP_Text _timeText;

    private int _lastShownSeconds = UNBOUND_SECONDS;
    private int _lastShownLengthSeconds = UNBOUND_SECONDS;

    private void Update()
    {
        RefreshProgress();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SeekToPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        SeekToPointer(eventData);
    }

    private void SeekToPointer(PointerEventData eventData)
    {
        int clipLengthMs = _audioPlayer.ClipLengthMs;
        if (clipLengthMs <= 0)
        {
            return;
        }

        // Screen Space - Overlay 캔버스에서는 pressEventCamera가 null로 들어오며, 그대로 넘겨야 좌표가 맞습니다.
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_barRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            return;
        }

        Rect rect = _barRect.rect;
        float ratio = Mathf.Clamp01((localPoint.x - rect.xMin) / rect.width);

        _playbackControl.SetPlaybackTime(Mathf.RoundToInt(ratio * clipLengthMs));
    }

    private void RefreshProgress()
    {
        int clipLengthMs = _audioPlayer.ClipLengthMs;
        if (clipLengthMs <= 0)
        {
            _fillRect.anchorMax = new Vector2(0f, 1f);
            return;
        }

        int currentTimeMs = _audioPlayer.CurrentTimeMs;
        _fillRect.anchorMax = new Vector2(Mathf.Clamp01((float)currentTimeMs / clipLengthMs), 1f);

        RefreshTimeText(currentTimeMs / 1000, clipLengthMs / 1000);
    }

    /// <summary>
    /// 매 프레임 문자열을 새로 만들면 GC가 발생하므로, 표시 중인 초가 실제로 바뀌었을 때만 갱신합니다.
    /// </summary>
    private void RefreshTimeText(int currentSeconds, int lengthSeconds)
    {
        if (currentSeconds == _lastShownSeconds && lengthSeconds == _lastShownLengthSeconds)
        {
            return;
        }

        _lastShownSeconds = currentSeconds;
        _lastShownLengthSeconds = lengthSeconds;

        _timeText.text = $"{currentSeconds / 60:00}:{currentSeconds % 60:00} / {lengthSeconds / 60:00}:{lengthSeconds % 60:00}";
    }
}
