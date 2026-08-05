using System.Collections.Generic;
using System.IO;

/// <summary>
/// 스토리 메타데이터의 보상·해금 참조를 검사합니다. 대사 내용 검사는 StoryScriptValidator가 맡습니다.
/// </summary>
public class StoryValidator
{
    public void Validate(MasterDataProvider provider, MasterDataValidationReport report)
    {
        foreach (KeyValuePair<string, StoryData> pair in provider.Stories)
        {
            StoryData story = pair.Value;

            if (!string.IsNullOrEmpty(story.RewardId) && !provider.TryGetReward(story.RewardId, out _))
            {
                report.AddError($"[스토리] {story.StoryId}의 RewardId '{story.RewardId}'를 rewards.json에서 찾을 수 없습니다.");
            }

            ValidateUnlock(provider, story, report);

            // 대본이 없으면 감상 화면에 들어갈 수 없지만, 메타만 먼저 등록하는 작업 순서를 막을 이유는 없어 경고로 둡니다.
            if (!File.Exists(MasterDataPaths.GetStoryScriptPath(story.StoryId)))
            {
                report.AddWarning($"[스토리] {story.StoryId}의 대본 파일이 아직 없습니다.");
            }
        }
    }

    private void ValidateUnlock(MasterDataProvider provider, StoryData story, MasterDataValidationReport report)
    {
        if (story.UnlockType == EUnlockType.NONE)
        {
            return;
        }

        if (string.IsNullOrEmpty(story.UnlockTargetId))
        {
            report.AddError($"[스토리] {story.StoryId}의 해금 조건이 {story.UnlockType}인데 UnlockTargetId가 비어 있습니다.");
            return;
        }

        if (story.UnlockType == EUnlockType.PREVIOUS_STORY_CLEAR && !provider.TryGetStory(story.UnlockTargetId, out _))
        {
            report.AddError($"[스토리] {story.StoryId}의 선행 스토리 '{story.UnlockTargetId}'를 찾을 수 없습니다.");
        }
    }
}
