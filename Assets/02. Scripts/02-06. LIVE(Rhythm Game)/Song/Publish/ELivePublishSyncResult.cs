/// <summary>
/// 채보를 저장할 때 수록본(LiveSongs)까지 함께 갱신했는지를 알리는 결과입니다.
/// </summary>
public enum ELivePublishSyncResult
{
    /// <summary>
    /// 갱신 대상이 아닙니다. 곡이 아직 수록되지 않았거나, 수록본이 이 난이도를 아직 담고 있지 않습니다.
    /// </summary>
    NotPublished,

    /// <summary>
    /// 수록본의 채보와 노트 수를 작업본에 맞췄습니다.
    /// </summary>
    Updated,

    /// <summary>
    /// 갱신에 실패했습니다. 작업 공간에 저장하는 것 자체는 이미 끝난 상태입니다.
    /// </summary>
    Failed,
}
