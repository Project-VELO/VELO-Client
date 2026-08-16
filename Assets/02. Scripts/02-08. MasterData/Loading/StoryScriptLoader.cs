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
    ///
    /// 캐릭터 칸의 NONE은 "내려라"라는 지시입니다. 빈 칸이 유지를 뜻하는 탓에 이 값이 없으면
    /// 한 번 등장한 캐릭터를 회차가 끝날 때까지 세워 두게 됩니다.
    /// </summary>
    private void ApplyCarryOverState(StoryScriptData script)
    {
        string backgroundId = string.Empty;
        string leftCharacterId = string.Empty;
        string leftExpressionId = string.Empty;
        string centerCharacterId = string.Empty;
        string centerExpressionId = string.Empty;
        string rightCharacterId = string.Empty;
        string rightExpressionId = string.Empty;
        string upperCharacterId = string.Empty;
        string upperExpressionId = string.Empty;

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

            // 네 슬롯은 서로의 상태를 모릅니다. 왼쪽이 바뀌어도 나머지는 그대로 서 있어야 합니다.
            ApplySlotState(ref line.CharacterId, ref line.ExpressionId, ref leftCharacterId, ref leftExpressionId);
            ApplySlotState(ref line.CenterCharacterId, ref line.CenterExpressionId, ref centerCharacterId, ref centerExpressionId);
            ApplySlotState(ref line.RightCharacterId, ref line.RightExpressionId, ref rightCharacterId, ref rightExpressionId);
            ApplySlotState(ref line.UpperCharacterId, ref line.UpperExpressionId, ref upperCharacterId, ref upperExpressionId);
        }
    }

    /// <summary>
    /// 슬롯 한 쪽의 캐릭터와 표정을 펼칩니다.
    ///
    /// 표정을 캐릭터와 함께 다루는 이유는, 캐릭터가 내려갔는데 표정만 남으면
    /// 다음에 그 자리에 서는 다른 인물이 앞사람의 표정을 물려받기 때문입니다.
    /// </summary>
    private void ApplySlotState(ref string lineCharacterId, ref string lineExpressionId,
        ref string carriedCharacterId, ref string carriedExpressionId)
    {
        if (lineCharacterId == StoryScriptTokens.NONE)
        {
            carriedCharacterId = string.Empty;
            carriedExpressionId = string.Empty;
            lineCharacterId = string.Empty;
            lineExpressionId = string.Empty;
            return;
        }

        if (string.IsNullOrEmpty(lineCharacterId))
        {
            lineCharacterId = carriedCharacterId;
        }
        else
        {
            // 다른 인물로 교체되는 줄입니다. 앞사람의 표정을 지우지 않으면
            // 표정을 적지 않은 새 인물이 앞사람의 표정을 그대로 물려받습니다.
            if (lineCharacterId != carriedCharacterId)
            {
                carriedExpressionId = string.Empty;
            }

            carriedCharacterId = lineCharacterId;
        }

        if (string.IsNullOrEmpty(lineExpressionId))
        {
            lineExpressionId = carriedExpressionId;
        }
        else
        {
            carriedExpressionId = lineExpressionId;
        }
    }
}
