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

    public LiveNoteVisualPool(RectTransform noteLayer)
    {
        _noteLayer = noteLayer;
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
            PoolManager.Instance.Push(handle.PoolType, handle.RectTransform.gameObject);
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
        _noteVisuals[note.NoteId] = new LiveNoteVisualHandle(rectTransform, poolType);
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
            LiveNoteVisualHandle handle = _noteVisuals[noteId];
            PoolManager.Instance.Push(handle.PoolType, handle.RectTransform.gameObject);
            _noteVisuals.Remove(noteId);
        }
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
