using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 노트 하나당 시각 오브젝트 하나를 풀에서 빌려 두고, 채보에서 사라진 노트의 것만 돌려주는 대장입니다.
/// 노트 ID로 관리하므로 편집 중 노트가 늘거나 줄어도 살아 있는 오브젝트는 그대로 유지됩니다.
/// 동기화 과정에서 GC 할당이 생기지 않도록 임시 컬렉션은 모두 필드로 재사용합니다.
/// </summary>
public class LiveNoteVisualPool
{
    private readonly Dictionary<string, LiveNoteVisualHandle> _noteVisuals = new Dictionary<string, LiveNoteVisualHandle>();
    private readonly HashSet<string> _livingNoteIds = new HashSet<string>();
    private readonly List<string> _staleNoteIds = new List<string>();

    private readonly RectTransform _noteLayer;
    private readonly LiveNoteSpriteTable _spriteTable;

    public LiveNoteVisualPool(RectTransform noteLayer, LiveNoteSpriteTable spriteTable)
    {
        _noteLayer = noteLayer;
        _spriteTable = spriteTable;
    }

    public bool TryGetHandle(string noteId, out LiveNoteVisualHandle handle)
    {
        return _noteVisuals.TryGetValue(noteId, out handle);
    }

    /// <summary>
    /// 현재 노트 목록과 빌려 둔 오브젝트를 다시 일치시킵니다. 커맨드 실행 및 Undo/Redo 직후에 호출됩니다.
    /// </summary>
    public void RefreshVisuals(List<NoteData> notes)
    {
        _livingNoteIds.Clear();

        foreach (NoteData note in notes)
        {
            _livingNoteIds.Add(note.NoteId);

            if (_noteVisuals.ContainsKey(note.NoteId))
            {
                continue;
            }

            AcquireVisual(note);
        }

        ReleaseStaleVisuals();
    }

    public void ReleaseAll()
    {
        foreach (LiveNoteVisualHandle handle in _noteVisuals.Values)
        {
            ReleaseVisual(handle);
        }

        _noteVisuals.Clear();
    }

    private void AcquireVisual(NoteData note)
    {
        EPoolable poolType = GetPoolTypeForNoteType(note.NoteType);
        GameObject go = PoolManager.Instance.Pop(poolType);

        if (go == null)
        {
            return;
        }

        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.SetParent(_noteLayer, false);

        // 노트 하나가 태어날 때 한 번만 실행되는 지점입니다. 매 프레임 도는 위치 갱신에는 절대 넣지 않습니다.
        UI_LiveNoteVisual noteVisual = go.GetComponent<UI_LiveNoteVisual>();

        if (noteVisual != null)
        {
            noteVisual.SetLaneSprite(_spriteTable.GetSprite(note.Lane));
        }

        _noteVisuals[note.NoteId] = new LiveNoteVisualHandle(rectTransform, poolType, noteVisual);
    }

    private void ReleaseStaleVisuals()
    {
        _staleNoteIds.Clear();

        foreach (string noteId in _noteVisuals.Keys)
        {
            if (!_livingNoteIds.Contains(noteId))
            {
                _staleNoteIds.Add(noteId);
            }
        }

        foreach (string noteId in _staleNoteIds)
        {
            ReleaseVisual(_noteVisuals[noteId]);
            _noteVisuals.Remove(noteId);
        }
    }

    /// <summary>
    /// 반환 직전에 레인 색을 지웁니다. 노트는 화면 밖으로 나갈 때마다 SetActive로 껐다 켜지므로
    /// OnEnable에서 지우면 화면에 되돌아온 노트까지 함께 지워집니다.
    /// </summary>
    private static void ReleaseVisual(LiveNoteVisualHandle handle)
    {
        if (handle.NoteVisual != null)
        {
            handle.NoteVisual.ClearLaneSprite();
        }

        PoolManager.Instance.Push(handle.PoolType, handle.RectTransform.gameObject);
    }

    private static EPoolable GetPoolTypeForNoteType(ENoteType noteType)
    {
        switch (noteType)
        {
            case ENoteType.GHOST:
                return EPoolable.LiveNoteGhost;

            case ENoteType.LONG:
                return EPoolable.LiveNoteLong;

            default:
                return EPoolable.LiveNoteNormal;
        }
    }
}
