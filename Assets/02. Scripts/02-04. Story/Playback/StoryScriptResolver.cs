using UnityEngine;

/// <summary>
/// 감상 화면에 들어올 때 "무엇을 재생할지"를 확정합니다.
///
/// 목록에서 넘겨준 StoryId로 메타데이터와 대본을 함께 얻어내고, 어느 쪽이 없으면 이유를 남깁니다.
/// 재생 컨트롤러에서 떼어낸 이유는 실패 경로가 둘(미등록 스토리 / 대본 없음)이고
/// 둘 다 화면을 띄우기 전에 판가름 나기 때문입니다. 재생 로직과 섞어 두면 진입 실패와
/// 재생 중 오류가 같은 자리에서 다뤄집니다.
/// </summary>
public static class StoryScriptResolver
{
    /// <summary>
    /// 재생 대상을 확정합니다. 실패하면 false이며, 호출부는 기획서 3-L에 따라 목록으로 되돌립니다.
    /// </summary>
    public static bool TryResolve(string storyId, out StoryData story, out StoryScriptData script)
    {
        script = null;

        if (!MasterDataProvider.Instance.TryGetStory(storyId, out story))
        {
            Debug.LogWarning($"[StoryScriptResolver] stories.json에 없는 스토리입니다: {storyId}");
            return false;
        }

        script = new StoryScriptLoader().Load(storyId);

        if (ReferenceEquals(script, null))
        {
            Debug.LogWarning($"[StoryScriptResolver] 대본을 읽지 못했습니다: {storyId}");
            return false;
        }

        return true;
    }
}
