using System.IO;
using UnityEngine;

/// <summary>
/// 회차별 대본 파일(StoryScripts/{storyId}.json)을 읽습니다.
///
/// 대본은 스토리 감상 화면에 들어갈 때만 필요하고 31화를 모두 들고 있을 이유가 없으므로,
/// 다른 마스터 데이터와 달리 시작 시 일괄 로드하지 않고 필요한 회차만 그때 읽습니다.
/// 채보를 곡 선택 시점이 아니라 플레이 시작 시점에 읽는 것(LiveChartLoader)과 같은 이유입니다.
/// </summary>
public class StoryScriptLoader
{
    /// <summary>
    /// 대본을 읽습니다. 파일이 없거나 손상되었으면 null을 돌려주며,
    /// 호출부는 기획서 3-L에 따라 오류 안내 후 스토리 목록으로 되돌립니다.
    /// </summary>
    public StoryScriptData Load(string storyId)
    {
        if (string.IsNullOrEmpty(storyId))
        {
            Debug.LogWarning("[StoryScriptLoader] StoryId가 비어 있어 대본을 읽을 수 없습니다.");
            return null;
        }

        string path = MasterDataPaths.GetStoryScriptPath(storyId);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[StoryScriptLoader] 대본 파일이 없습니다: {path}");
            return null;
        }

        StoryScriptData script = JsonUtility.FromJson<StoryScriptData>(File.ReadAllText(path));

        if (ReferenceEquals(script, null) || ReferenceEquals(script.Lines, null) || script.Lines.Count == 0)
        {
            Debug.LogWarning($"[StoryScriptLoader] 대본이 비어 있거나 파싱에 실패했습니다: {path}");
            return null;
        }

        // 파일명과 내용이 어긋나면 다른 회차의 대사가 출력되므로, 조용히 넘기지 않고 알립니다.
        if (script.StoryId != storyId)
        {
            Debug.LogWarning($"[StoryScriptLoader] 파일명과 StoryId가 다릅니다(파일 {storyId} / 내용 {script.StoryId}): {path}");
        }

        ApplyCarryOverState(script);

        return script;
    }

    /// <summary>
    /// 배경·캐릭터·표정이 비어 있는 줄에 직전 줄의 값을 채웁니다.
    /// 기획 입력을 줄이기 위해 "바뀌는 줄에만 적는다"는 규칙을 쓰므로, 읽는 쪽에서 한 번만 펼쳐 두면
    /// 화면 코드가 매 줄마다 이전 값을 되짚지 않아도 됩니다.
    /// </summary>
    private void ApplyCarryOverState(StoryScriptData script)
    {
        string backgroundId = string.Empty;
        string characterId = string.Empty;
        string expressionId = string.Empty;

        foreach (StoryLineData line in script.Lines)
        {
            if (string.IsNullOrEmpty(line.BackgroundId))
            {
                line.BackgroundId = backgroundId;
            }
            else
            {
                backgroundId = line.BackgroundId;
            }

            if (string.IsNullOrEmpty(line.CharacterId))
            {
                line.CharacterId = characterId;
            }
            else
            {
                characterId = line.CharacterId;
            }

            if (string.IsNullOrEmpty(line.ExpressionId))
            {
                line.ExpressionId = expressionId;
            }
            else
            {
                expressionId = line.ExpressionId;
            }
        }
    }
}
