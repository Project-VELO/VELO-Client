using System.Collections.Generic;
using System.IO;

/// <summary>
/// 대본 파일의 참조와 형식을 검사합니다.
/// 대사는 물량이 많고 도구로 생성되므로, 사람이 눈으로 확인하기 어려운 것들을 기계가 걸러 줍니다.
/// </summary>
public class StoryScriptValidator
{
    private readonly StoryScriptLoader _loader = new StoryScriptLoader();

    public void Validate(MasterDataValidationReport report)
    {
        MasterDataProvider provider = MasterDataProvider.Instance;
        int checkedScriptCount = 0;

        foreach (KeyValuePair<string, StoryData> pair in provider.Stories)
        {
            string storyId = pair.Key;

            if (!File.Exists(MasterDataPaths.GetStoryScriptPath(storyId)))
            {
                continue;
            }

            StoryScriptData script = _loader.Load(storyId);

            if (ReferenceEquals(script, null))
            {
                report.AddError($"[대본] {storyId} 대본을 읽지 못했습니다.");
                continue;
            }

            checkedScriptCount++;
            ValidateLines(provider, script, report);
        }

        report.AddSummary($"검사한 대본 {checkedScriptCount}편");
    }

    private void ValidateLines(MasterDataProvider provider, StoryScriptData script, MasterDataValidationReport report)
    {
        for (int i = 0; i < script.Lines.Count; i++)
        {
            StoryLineData line = script.Lines[i];
            string location = $"{script.StoryId} {i + 1}번째 줄";

            // 빈 대사는 배경만 넘기는 컷이라 오류가 아닙니다.
            // 실수로 비운 줄은 가져오기 단계(StoryScriptTsvParser)에서 걸러집니다.
            // 여기서는 배경이 캐리오버로 이미 채워져 있어 "아무 일도 하지 않는 줄"을 가려낼 수 없습니다.
            if (string.IsNullOrWhiteSpace(line.Text) && line.LineType != ELineType.NARRATION)
            {
                report.AddError($"[대본] {location}이 {line.LineType}인데 대사가 비어 있습니다.");
            }

            ValidateSpeaker(provider, line, location, report);
            ValidateExpression(provider, line.CharacterId, line.ExpressionId, location, report);
            ValidateExpression(provider, line.CenterCharacterId, line.CenterExpressionId, $"{location}(가운데)", report);
            ValidateExpression(provider, line.RightCharacterId, line.RightExpressionId, $"{location}(오른쪽)", report);

            ValidateSlotDuplication(line, location, report);
        }
    }

    /// <summary>
    /// 같은 인물이 두 자리 이상에 동시에 서 있는지 봅니다.
    /// 한 명이 좌우로 복제돼 보이는 사고를 데이터 단계에서 잡습니다.
    /// </summary>
    private void ValidateSlotDuplication(StoryLineData line, string location, MasterDataValidationReport report)
    {
        CheckPair(line.CharacterId, line.CenterCharacterId, "왼쪽과 가운데", location, report);
        CheckPair(line.CharacterId, line.RightCharacterId, "왼쪽과 오른쪽", location, report);
        CheckPair(line.CenterCharacterId, line.RightCharacterId, "가운데와 오른쪽", location, report);
    }

    private void CheckPair(string first, string second, string slots, string location, MasterDataValidationReport report)
    {
        if (!string.IsNullOrEmpty(first) && first == second)
        {
            report.AddError($"[대본] {location}의 {slots} 슬롯에 같은 인물({first})이 배치되어 있습니다.");
        }
    }

    private void ValidateSpeaker(MasterDataProvider provider, StoryLineData line, string location, MasterDataValidationReport report)
    {
        // 지문은 화자가 없는 것이 정상입니다.
        if (line.LineType == ELineType.NARRATION)
        {
            return;
        }

        if (string.IsNullOrEmpty(line.SpeakerId))
        {
            // 단역은 characters.json에 없고 원고 표기(speakerRaw)만 있습니다. 화면은 그 값을 화자명으로 씁니다.
            // 공백만 든 칸은 시트에서 실수로 스페이스를 넣은 경우이며, 화면에는 빈 이름표로 나가므로 걸러 냅니다.
            if (string.IsNullOrWhiteSpace(line.SpeakerName))
            {
                report.AddError($"[대본] {location}이 {line.LineType}인데 화자가 비어 있습니다.");
            }

            return;
        }

        if (!provider.TryGetCharacter(line.SpeakerId, out _))
        {
            report.AddError($"[대본] {location}의 화자 '{line.SpeakerId}'를 characters.json에서 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 슬롯 한 쪽의 캐릭터와 표정을 검사합니다. 좌우가 같은 규칙을 쓰므로 슬롯을 인자로 받습니다.
    /// </summary>
    private void ValidateExpression(MasterDataProvider provider, string characterId, string expressionId,
        string location, MasterDataValidationReport report)
    {
        if (string.IsNullOrEmpty(expressionId) || string.IsNullOrEmpty(characterId))
        {
            return;
        }

        if (!provider.TryGetCharacter(characterId, out CharacterData character))
        {
            report.AddError($"[대본] {location}의 등장 캐릭터 '{characterId}'를 characters.json에서 찾을 수 없습니다.");
            return;
        }

        // 표정이 목록에 없으면 기본 이미지로 대체되므로(기획서 3-L) 진행은 막히지 않습니다. 경고로 둡니다.
        if (!character.ExpressionIds.Contains(expressionId))
        {
            report.AddWarning($"[대본] {location}의 표정 '{expressionId}'이 {characterId}의 표정 목록에 없습니다.");
        }
    }
}
