using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 노트 판정 통계를 정렬·필터해 표로 그립니다.
///
/// 채보 하나가 노트 수백 개라 모든 줄을 IMGUI로 그리면 다시 그릴 때마다 칸 수천 개를 만들게 됩니다.
/// 스크롤 위치에서 보이는 줄만 그리고, 정렬·필터 결과는 조건이 바뀔 때만 다시 만듭니다.
/// </summary>
public class LiveNoteHitStatTable
{
    private const float ROW_HEIGHT = 18f;
    private const int DEFAULT_MIN_JUDGED_COUNT = 3;

    // GUILayout.Toolbar가 배열을 받으므로 배열로 둡니다. 순서는 ELiveNoteHitStatSort와 같습니다.
    private static readonly string[] SORT_LABELS = { "문제 비율순", "편차 큰 순", "시각순" };

    // 순서는 LiveNoteHitStatRow.Cells와 같습니다.
    private static readonly List<string> COLUMN_TITLES = new List<string>
    {
        "시각", "마디:박", "레인", "종류", "P", "Gr", "Go", "B", "이른B", "MISS", "조기",
        "문제", "표본", "평균", "편차", "표준편차", "상태",
    };

    private static readonly List<float> COLUMN_WIDTHS = new List<float>
    {
        72f, 96f, 36f, 60f, 34f, 34f, 34f, 34f, 42f, 42f, 36f,
        44f, 40f, 58f, 58f, 58f, 56f,
    };

    private static readonly Color ALTERNATE_ROW_COLOR = new Color(0f, 0f, 0f, 0.08f);

    private readonly List<LiveNoteHitStatRow> _visibleRows = new List<LiveNoteHitStatRow>();

    private LiveNoteHitStatReport _report;
    private ELiveNoteHitStatSort _sort = ELiveNoteHitStatSort.PROBLEM_RATE;
    private int _minJudgedCount = DEFAULT_MIN_JUDGED_COUNT;
    private Vector2 _scrollPosition;
    private bool _isDirty = true;

    public void SetReport(LiveNoteHitStatReport report)
    {
        _report = report;
        _scrollPosition = Vector2.zero;
        _isDirty = true;
    }

    public void Draw()
    {
        DrawControls();

        if (_isDirty)
        {
            RefreshVisibleRows();
        }

        EditorGUILayout.LabelField($"표시 {_visibleRows.Count} / 전체 {_report.Rows.Count}", EditorStyles.miniLabel);

        float totalWidth = GetTotalWidth();
        DrawHeader(totalWidth);
        DrawRows(totalWidth);
    }

    private void DrawControls()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            ELiveNoteHitStatSort sort = (ELiveNoteHitStatSort)GUILayout.Toolbar((int)_sort, SORT_LABELS);
            int minJudgedCount = Mathf.Max(0, EditorGUILayout.IntField("최소 판정 수", _minJudgedCount));

            if (sort != _sort || minJudgedCount != _minJudgedCount)
            {
                _sort = sort;
                _minJudgedCount = minJudgedCount;
                _isDirty = true;
            }
        }
    }

    private void RefreshVisibleRows()
    {
        _visibleRows.Clear();

        foreach (LiveNoteHitStatRow row in _report.Rows)
        {
            if (_minJudgedCount <= row.Stat.JudgedCount)
            {
                _visibleRows.Add(row);
            }
        }

        _visibleRows.Sort(LiveNoteHitStatSorter.GetComparison(_sort));
        _isDirty = false;
    }

    // 헤더는 표와 같은 가로 스크롤 위치만큼 밀어, 좁은 창에서 옆으로 넘겨도 열 제목이 칸과 어긋나지 않게 합니다.
    private void DrawHeader(float totalWidth)
    {
        Rect headerRect = GUILayoutUtility.GetRect(0f, ROW_HEIGHT, GUILayout.ExpandWidth(true));

        GUI.BeginClip(headerRect);
        DrawCells(new Rect(-_scrollPosition.x, 0f, totalWidth, ROW_HEIGHT), COLUMN_TITLES, EditorStyles.boldLabel);
        GUI.EndClip();
    }

    private void DrawRows(float totalWidth)
    {
        Rect area = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        Rect content = new Rect(0f, 0f, totalWidth, _visibleRows.Count * ROW_HEIGHT);

        _scrollPosition = GUI.BeginScrollView(area, _scrollPosition, content);

        int firstIndex = Mathf.Max(0, Mathf.FloorToInt(_scrollPosition.y / ROW_HEIGHT));
        int lastIndex = Mathf.Min(_visibleRows.Count, firstIndex + Mathf.CeilToInt(area.height / ROW_HEIGHT) + 1);

        for (int i = firstIndex; i < lastIndex; i++)
        {
            Rect rowRect = new Rect(0f, i * ROW_HEIGHT, totalWidth, ROW_HEIGHT);

            if (i % 2 == 1)
            {
                EditorGUI.DrawRect(rowRect, ALTERNATE_ROW_COLOR);
            }

            DrawCells(rowRect, _visibleRows[i].Cells, EditorStyles.label);
        }

        GUI.EndScrollView();
    }

    private static void DrawCells(Rect rowRect, List<string> cells, GUIStyle style)
    {
        float x = rowRect.x;

        for (int i = 0; i < COLUMN_WIDTHS.Count && i < cells.Count; i++)
        {
            GUI.Label(new Rect(x, rowRect.y, COLUMN_WIDTHS[i], rowRect.height), cells[i], style);
            x += COLUMN_WIDTHS[i];
        }
    }

    private static float GetTotalWidth()
    {
        float totalWidth = 0f;

        foreach (float width in COLUMN_WIDTHS)
        {
            totalWidth += width;
        }

        return totalWidth;
    }
}
