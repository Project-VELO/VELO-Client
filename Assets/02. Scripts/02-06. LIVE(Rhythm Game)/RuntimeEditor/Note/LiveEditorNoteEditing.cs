using UnityEngine;
using VInspector;

/// <summary>
/// 노트 편집에 필요한 순수 로직 객체들을 생성해 소유하는 합성 루트입니다.
///
/// 편집 판정·선택 상태·커맨드 생성·좌표 변환은 모두 유니티 생명주기가 필요 없는 로직이라 컴포넌트가 아니지만,
/// 선택 상태만큼은 마우스 편집과 단축키가 같은 것을 봐야 하므로 누군가 하나가 만들어 공유해야 합니다.
/// 그 소유권을 이 클래스가 맡고, 입력 처리기들은 여기에서 필요한 것만 꺼내 씁니다.
/// 덕분에 입력 처리기마다 참조를 서너 개씩 배선하던 것이 이 하나로 줄어듭니다.
/// </summary>
public class LiveEditorNoteEditing : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorController _controller;

    [SerializeField]
    private LiveEditorTimeline _timeline;

    [Header("Track Pointer")]
    [Tooltip("클릭 좌표를 레인으로 환산할 기준이 되는 트랙 영역입니다.")]
    [SerializeField]
    private RectTransform _laneAreaRect;

    [Tooltip("트랙이 올라가 있는 캔버스입니다. 렌더 모드에 따라 좌표 변환에 넘길 카메라가 달라집니다.")]
    [SerializeField]
    private Canvas _canvas;

    private LiveEditorEditContext _editContext;
    private LiveEditorNoteSelection _selection;
    private LiveEditorNoteWriter _noteWriter;
    private LiveEditorTrackPointer _trackPointer;

    public LiveEditorEditContext EditContext => _editContext;
    public LiveEditorNoteSelection Selection => _selection;
    public LiveEditorNoteWriter NoteWriter => _noteWriter;
    public LiveEditorTrackPointer TrackPointer => _trackPointer;

    private void Awake()
    {
        _editContext = new LiveEditorEditContext(_controller, _timeline);
        _selection = new LiveEditorNoteSelection(_controller);
        _noteWriter = new LiveEditorNoteWriter(_controller, _controller.UndoRedo);
        _trackPointer = new LiveEditorTrackPointer(_timeline, _laneAreaRect, _canvas);
    }
}
