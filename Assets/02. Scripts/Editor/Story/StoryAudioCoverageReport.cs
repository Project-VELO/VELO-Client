using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 대본이 요구하는 소리 중 어떤 것이 아직 음원 없이 남아 있는지 콘솔에 정리해 줍니다.
///
/// 음원이 한 번에 들어오지 않고 회차별로 조금씩 들어오는 상황을 위한 도구입니다.
/// 런타임에도 없는 소리를 경고하지만, 그건 그 줄을 실제로 지나가야 보입니다.
/// 무엇이 남았는지 미리 알아야 다음에 무슨 파일을 받아야 할지 정할 수 있습니다.
///
/// 창을 만들지 않고 로그로 내보내는 것은, 이 정보가 한 번 읽고 넘기는 목록이기 때문입니다.
/// </summary>
public static class StoryAudioCoverageReport
{
    private const string STORY_PREFAB_PATH = "Assets/03. Prefabs/03-02. UI/03-02-04. Story/P_UI_Story.prefab";

    [MenuItem("VELO/Story/음원 점검")]
    public static void Report()
    {
        StoryAudioBinder binder = LoadBinder();

        if (binder == null)
        {
            return;
        }

        MasterDataProvider.Instance.Rebuild();

        var bgmUses = new Dictionary<string, int>();
        var sfxUses = new Dictionary<string, int>();
        var loader = new StoryScriptLoader();
        int scriptCount = 0;

        foreach (KeyValuePair<string, StoryData> pair in MasterDataProvider.Instance.Stories)
        {
            StoryScriptData script = loader.Load(pair.Key);

            if (ReferenceEquals(script, null))
            {
                continue;
            }

            scriptCount++;

            foreach (StoryLineData line in script.Lines)
            {
                Count(bgmUses, line.BgmId);
                Count(sfxUses, line.SfxId);
            }
        }

        var text = new StringBuilder();
        text.AppendLine($"[스토리 음원 점검] 대본 {scriptCount}편");
        AppendSection(text, "BGM", bgmUses, binder.GetBgm);
        AppendSection(text, "효과음", sfxUses, binder.GetSfx);

        Debug.Log(text.ToString());
    }

    private static StoryAudioBinder LoadBinder()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(STORY_PREFAB_PATH);

        if (prefab == null)
        {
            Debug.LogError($"[스토리 음원 점검] 프리팹을 찾지 못했습니다: {STORY_PREFAB_PATH}");
            return null;
        }

        var binder = prefab.GetComponent<StoryAudioBinder>();

        if (binder == null)
        {
            Debug.LogError($"[스토리 음원 점검] {nameof(StoryAudioBinder)}가 프리팹에 붙어 있지 않습니다.");
        }

        return binder;
    }

    /// <summary>
    /// BGM_NONE은 "멈춰라"는 지시이므로 음원이 필요한 소리로 세지 않습니다.
    /// </summary>
    private static void Count(Dictionary<string, int> uses, string id)
    {
        if (string.IsNullOrEmpty(id) || id == StoryScriptTokens.BGM_NONE)
        {
            return;
        }

        uses.TryGetValue(id, out int count);
        uses[id] = count + 1;
    }

    private static void AppendSection(StringBuilder text, string title,
        Dictionary<string, int> uses, System.Func<string, AudioClip> resolve)
    {
        var missing = new List<string>();
        int readyKinds = 0;
        int readyUses = 0;
        int totalUses = 0;

        foreach (KeyValuePair<string, int> pair in uses)
        {
            totalUses += pair.Value;

            if (resolve(pair.Key) != null)
            {
                readyKinds++;
                readyUses += pair.Value;
            }
            else
            {
                missing.Add($"{pair.Key} ({pair.Value}회)");
            }
        }

        missing.Sort();

        text.AppendLine();
        text.AppendLine($"── {title}: {readyKinds}/{uses.Count}종 준비, {readyUses}/{totalUses}회 소리 남");

        if (missing.Count == 0)
        {
            text.AppendLine("   빠진 것 없음");
            return;
        }

        text.AppendLine($"   음원 없는 {missing.Count}종:");

        foreach (string entry in missing)
        {
            text.AppendLine($"     {entry}");
        }
    }
}
