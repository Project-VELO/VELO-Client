
/// <summary>
/// 풀에서 꺼내 쓰는 오브젝트의 종류입니다.
///
/// 값은 반드시 맨 뒤에만 추가합니다. 중간에 끼워 넣으면 인스펙터에 이미 직렬화된 PoolInfo.Type이
/// 조용히 다른 항목을 가리키게 되어, 컴파일은 통과하지만 엉뚱한 프리팹이 나옵니다.
/// </summary>
public enum EPoolable
{
    Empty,
    EditorGridLine,
    LiveNoteNormal,
    LiveNoteGhost,
    LiveNoteLong,
    EditorBarLine,
    EditorBarLabel,
    EditorListItem,
    MusicSelectSongItem,

    // 아래는 화면 구현(T4~T8)에서 쓸 항목을 미리 모아 둔 것입니다.
    // 각 브랜치가 따로 추가하면 같은 파일을 동시에 고쳐 머지 때마다 충돌합니다.
    SelectStoryChapterSection,
    SelectStoryEpisodeItem,
    StoryLogItem,
}
