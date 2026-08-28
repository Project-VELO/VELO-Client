using UnityEngine;

/// <summary>
/// 롱노트 몸통과 꼬리의 배치를 전담합니다. 머리 마커는 LiveNoteRenderer가 일반 노트와 같은 방식으로 처리합니다.
/// </summary>
public class LiveHoldNoteRenderer
{
    private readonly LiveNoteRenderSettings _settings;

    private UI_LiveTrackLanes _lanes;
    private LiveScrollMapper _scrollMapper;

    public LiveHoldNoteRenderer(LiveNoteRenderSettings settings)
    {
        _settings = settings;
    }

    public void Init(UI_LiveTrackLanes lanes, LiveScrollMapper scrollMapper)
    {
        _lanes = lanes;
        _scrollMapper = scrollMapper;
    }

    /// <summary>
    /// 몸통을 drawStartRatio부터 꼬리까지 채웁니다. drawStartRatio는 판정선에 먹히고 남은 시작점입니다.
    /// </summary>
    public void RefreshHold(LiveNoteVisualHandle handle, float headRatio, float tailRatio, float drawStartRatio)
    {
        float spawnRatio = _scrollMapper.SpawnRatio;
        float trackHeight = GetTrackHeight();
        float length = Mathf.Max(0f, (Mathf.Min(tailRatio, spawnRatio) - drawStartRatio) * trackHeight);

        // 타일 길이를 트랙 좌표로 재므로 하이스피드를 바꿔도 텍스처 밀도가 일정합니다.
        float tileLength = Mathf.Max(1f, _settings.BodyTileLength);

        // 트랙 좌표 변환은 비율을 0~1로 자르므로, 트랙 밖으로 내려간 머리까지의 거리는 비율에서 직접 잽니다.
        float uvStart = (drawStartRatio - headRatio) * trackHeight / tileLength;

        // 길이가 0이면 단타와 다를 것이 없으므로 꼬리를 감춥니다. 편집 중 길이를 0으로 줄인 롱노트도 여기로 걸립니다.
        bool isTailVisible = 0f < length && tailRatio <= spawnRatio;

        handle.HoldVisual.RefreshBody(length, uvStart, uvStart + length / tileLength);
        handle.HoldVisual.RefreshTail(length, _settings.GetNoteHeightAtRatio(_lanes, tailRatio), isTailVisible);
    }

    private float GetTrackHeight()
    {
        _lanes.GetTrackEdgesAtRatio(0f, out _, out _, out float nearY);
        _lanes.GetTrackEdgesAtRatio(1f, out _, out _, out float farY);

        return farY - nearY;
    }
}
