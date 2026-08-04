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

            if (string.IsNullOrWhiteSpace(line.Text))
            {
                report.AddError($"[대본] {location}의 대사가 비어 있습니다.");
            }

            ValidateSpeaker(provider, line, location, report);
            ValidateExpression(provider, line, location, report);
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
            if (string.IsNullOrEmpty(line.SpeakerName))
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

    private void ValidateExpression(MasterDataProvider provider, StoryLineData line, string location, MasterDataValidationReport report)
    {
        if (string.IsNullOrEmpty(line.ExpressionId) || string.IsNullOrEmpty(line.CharacterId))
        {
            return;
        }

        if (!provider.TryGetCharacter(line.CharacterId, out CharacterData character))
        {
            report.AddError($"[대본] {location}의 등장 캐릭터 '{line.CharacterId}'를 characters.json에서 찾을 수 없습니다.");
            return;
        }

        // 표정이 목록에 없으면 기본 이미지로 대체되므로(기획서 3-L) 진행은 막히지 않습니다. 경고로 둡니다.
        if (!character.ExpressionIds.Contains(line.ExpressionId))
        {
            report.AddWarning($"[대본] {location}의 표정 '{line.ExpressionId}'이 {line.CharacterId}의 표정 목록에 없습니다.");
        }
    }
}
