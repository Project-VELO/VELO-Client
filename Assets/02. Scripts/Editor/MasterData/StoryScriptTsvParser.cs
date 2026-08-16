using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기획이 Google Sheets에서 내보낸 TSV를 회차별 대본으로 변환합니다.
///
/// 구분자로 탭을 쓰는 이유는 한국어 대사에 쉼표가 매우 많아 CSV의 따옴표 이스케이프가 쉽게 깨지기 때문입니다.
/// 탭은 시트 셀에 입력하는 것 자체가 불가능해 구분자 충돌이 원천적으로 일어나지 않습니다.
///
/// 컬럼 순서가 바뀌어도 동작하도록 첫 줄의 헤더 이름으로 위치를 찾습니다.
/// </summary>
public class StoryScriptTsvParser
{
    private static readonly char[] LINE_SEPARATORS = { '\n' };

    /// <summary>
    /// TSV 전체를 읽어 StoryId별 대본으로 묶습니다.
    /// 한 시트에 여러 회차를 함께 적을 수 있으므로 결과가 여러 편이 될 수 있습니다.
    /// </summary>
    public List<StoryScriptData> Parse(string tsv, List<string> errors)
    {
        var scripts = new List<StoryScriptData>();
        var scriptsByStoryId = new Dictionary<string, StoryScriptData>();

        string[] rows = tsv.Split(LINE_SEPARATORS, StringSplitOptions.None);

        if (rows.Length < 2)
        {
            errors.Add("헤더와 데이터 행이 모두 필요합니다.");
            return scripts;
        }

        Dictionary<string, int> columns = ParseHeader(rows[0], errors);

        if (columns == null)
        {
            return scripts;
        }

        for (int i = 1; i < rows.Length; i++)
        {
            string row = rows[i].TrimEnd('\r');

            if (string.IsNullOrWhiteSpace(row))
            {
                continue;
            }

            AddLine(row, i + 1, columns, scriptsByStoryId, scripts, errors);
        }

        return scripts;
    }

    private Dictionary<string, int> ParseHeader(string headerRow, List<string> errors)
    {
        var columns = new Dictionary<string, int>();
        string[] headers = headerRow.TrimEnd('\r').Split('\t');

        for (int i = 0; i < headers.Length; i++)
        {
            string header = headers[i].Trim();

            if (!string.IsNullOrEmpty(header))
            {
                columns[header] = i;
            }
        }

        // 이 셋이 없으면 어느 회차의 몇 번째 줄에 무슨 대사인지 알 수 없어 변환 자체가 불가능합니다.
        string[] requiredColumns = { StoryScriptTsvColumns.STORY_ID, StoryScriptTsvColumns.LINE_ID, StoryScriptTsvColumns.TEXT };

        foreach (string required in requiredColumns)
        {
            if (!columns.ContainsKey(required))
            {
                errors.Add($"필수 컬럼 '{required}'이 헤더에 없습니다.");
                return null;
            }
        }

        return columns;
    }

    private void AddLine(string row, int rowNumber, Dictionary<string, int> columns,
        Dictionary<string, StoryScriptData> scriptsByStoryId, List<StoryScriptData> scripts, List<string> errors)
    {
        string[] cells = row.Split('\t');

        string storyId = GetCell(cells, columns, StoryScriptTsvColumns.STORY_ID);
        string text = GetCell(cells, columns, StoryScriptTsvColumns.TEXT);

        if (string.IsNullOrEmpty(storyId))
        {
            errors.Add($"{rowNumber}행: storyId가 비어 있습니다.");
            return;
        }

        // 대사 없이 배경만 넘기는 컷을 허용합니다(연출표 없이 이미지만 있는 회차).
        // 다만 아무 변화도 없는 줄은 시트의 실수이므로, 배경이나 일러스트 중 하나는 있어야 합니다.
        if (string.IsNullOrWhiteSpace(text)
            && string.IsNullOrEmpty(GetCell(cells, columns, StoryScriptTsvColumns.BACKGROUND_ID))
            && string.IsNullOrEmpty(GetCell(cells, columns, StoryScriptTsvColumns.ILLUSTRATION_ID)))
        {
            errors.Add($"{rowNumber}행: text가 비어 있는데 배경·일러스트 지정도 없습니다.");
            return;
        }

        if (!scriptsByStoryId.TryGetValue(storyId, out StoryScriptData script))
        {
            script = new StoryScriptData { StoryId = storyId };
            scriptsByStoryId[storyId] = script;
            scripts.Add(script);
        }

        script.Lines.Add(CreateLine(cells, columns, rowNumber, storyId));
    }

    private StoryLineData CreateLine(string[] cells, Dictionary<string, int> columns, int rowNumber, string storyId)
    {
        string rawLineId = GetCell(cells, columns, StoryScriptTsvColumns.LINE_ID);

        if (!int.TryParse(rawLineId, out int lineId))
        {
            Debug.LogWarning($"[StoryScriptTsvParser] {rowNumber}행({storyId}): lineId '{rawLineId}'를 숫자로 읽지 못해 행 순서를 사용합니다.");
            lineId = rowNumber - 1;
        }

        string rawLineType = GetCell(cells, columns, StoryScriptTsvColumns.LINE_TYPE);
        string rawTextSpeed = GetCell(cells, columns, StoryScriptTsvColumns.TEXT_SPEED);
        string rawTextPlacement = GetCell(cells, columns, StoryScriptTsvColumns.TEXT_PLACEMENT);

        return new StoryLineData
        {
            LineId = lineId,
            LineType = MasterDataEnum.Parse(rawLineType, ELineType.NARRATION, $"{storyId} {rowNumber}행"),
            SpeakerId = GetCell(cells, columns, StoryScriptTsvColumns.SPEAKER_ID),
            SpeakerName = GetCell(cells, columns, StoryScriptTsvColumns.SPEAKER_RAW),
            Text = GetCell(cells, columns, StoryScriptTsvColumns.TEXT),
            BackgroundId = GetCell(cells, columns, StoryScriptTsvColumns.BACKGROUND_ID),
            CharacterId = GetCell(cells, columns, StoryScriptTsvColumns.CHARACTER_ID),
            ExpressionId = GetCell(cells, columns, StoryScriptTsvColumns.EXPRESSION_ID),
            CenterCharacterId = GetCell(cells, columns, StoryScriptTsvColumns.CENTER_CHARACTER_ID),
            CenterExpressionId = GetCell(cells, columns, StoryScriptTsvColumns.CENTER_EXPRESSION_ID),
            RightCharacterId = GetCell(cells, columns, StoryScriptTsvColumns.RIGHT_CHARACTER_ID),
            RightExpressionId = GetCell(cells, columns, StoryScriptTsvColumns.RIGHT_EXPRESSION_ID),
            UpperCharacterId = GetCell(cells, columns, StoryScriptTsvColumns.UPPER_CHARACTER_ID),
            UpperExpressionId = GetCell(cells, columns, StoryScriptTsvColumns.UPPER_EXPRESSION_ID),
            IllustrationId = GetCell(cells, columns, StoryScriptTsvColumns.ILLUSTRATION_ID),
            TextSpeed = MasterDataEnum.Parse(rawTextSpeed, ETextSpeed.NORMAL, $"{storyId} {rowNumber}행"),
            TextPlacement = MasterDataEnum.Parse(rawTextPlacement, EStoryTextPlacement.DIALOG_BOX, $"{storyId} {rowNumber}행"),
            TextStyleId = GetCell(cells, columns, StoryScriptTsvColumns.TEXT_STYLE_ID),
            EffectId = GetCell(cells, columns, StoryScriptTsvColumns.EFFECT_ID),
            BgmId = GetCell(cells, columns, StoryScriptTsvColumns.BGM_ID),
            SfxId = GetCell(cells, columns, StoryScriptTsvColumns.SFX_ID),
        };
    }

    /// <summary>
    /// 컬럼이 없거나 행이 짧으면 빈 문자열을 돌려줍니다.
    /// 배경·표정처럼 "바뀌는 줄에만 적는" 컬럼은 비어 있는 것이 정상이므로 오류로 보지 않습니다.
    /// </summary>
    private string GetCell(string[] cells, Dictionary<string, int> columns, string columnName)
    {
        if (!columns.TryGetValue(columnName, out int index) || cells.Length <= index)
        {
            return string.Empty;
        }

        return Unquote(cells[index].Trim());
    }

    /// <summary>
    /// 시트에서 내보낼 때 값 전체가 따옴표로 감싸이는 경우가 있어 벗겨 냅니다.
    /// </summary>
    private string Unquote(string value)
    {
        if (2 <= value.Length && value[0] == '"' && value[value.Length - 1] == '"')
        {
            return value.Substring(1, value.Length - 2).Replace("\"\"", "\"");
        }

        return value;
    }
}
