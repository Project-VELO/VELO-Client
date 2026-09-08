using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 대본 위에 번역문을 덮습니다.
///
/// 대본을 읽는 일(StoryScriptLoader)과 나눈 이유는 없어도 되는 파일이기 때문입니다.
/// 번역문이 아직 없는 회차, 아직 없는 언어에서도 대본은 그대로 읽혀야 합니다.
/// </summary>
public class StoryTextOverlayLoader
{
    /// <summary>
    /// 지금 언어의 번역문을 대본에 적용합니다. 한국어이거나 파일이 없으면 아무것도 하지 않습니다.
    ///
    /// 빈 문장을 건너뛰는 것은 의도한 것입니다. 아직 옮기지 못한 줄이 빈 화면으로 나오는 것보다
    /// 원문이 그대로 나오는 편이 읽을 수 있습니다.
    /// </summary>
    public void Apply(StoryScriptData script, ELanguage language)
    {
        string folder = LanguageCode.GetFolderName(language);

        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        StoryTextOverlayData overlay = Load(script.StoryId, folder);

        if (overlay == null)
        {
            return;
        }

        Dictionary<int, StoryTextOverlayLine> byLineId = new Dictionary<int, StoryTextOverlayLine>(overlay.Lines.Count);

        foreach (StoryTextOverlayLine line in overlay.Lines)
        {
            byLineId[line.LineId] = line;
        }

        foreach (StoryLineData line in script.Lines)
        {
            if (!byLineId.TryGetValue(line.LineId, out StoryTextOverlayLine translated))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(translated.Text))
            {
                line.Text = translated.Text;
            }

            if (!string.IsNullOrEmpty(translated.SpeakerName))
            {
                line.SpeakerName = translated.SpeakerName;
            }
        }
    }

    private StoryTextOverlayData Load(string storyId, string folder)
    {
        string path = MasterDataPaths.GetStoryTextOverlayPath(storyId, folder);

        // 아직 옮기지 않은 회차입니다. 원문으로 진행하면 되므로 경고하지 않습니다.
        if (!File.Exists(path))
        {
            return null;
        }

        StoryTextOverlayData overlay = JsonUtility.FromJson<StoryTextOverlayData>(File.ReadAllText(path));

        if (ReferenceEquals(overlay, null) || ReferenceEquals(overlay.Lines, null))
        {
            Debug.LogWarning($"[StoryTextOverlayLoader] 번역문 파싱에 실패했습니다: {path}");
            return null;
        }

        return overlay;
    }
}
