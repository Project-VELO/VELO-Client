using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 바꿀 오브젝트를 찾고, 같은 파일에 든 것끼리 묶습니다.
///
/// 바꾸는 일(UiTextImageConverter)과 나눈 것은 찾는 방식이 프리팹과 씬에서 다르기 때문입니다.
/// 프리팹은 루트가 하나라 경로를 그대로 따라가면 되지만, 씬은 루트가 여럿이라 먼저 골라야 합니다.
/// </summary>
public static class UiTextImageLookup
{
    /// <summary>
    /// 씬에서 경로로 오브젝트를 찾습니다. 경로의 첫 칸이 루트 이름입니다.
    /// </summary>
    public static Transform FindInScene(Scene scene, string nodePath)
    {
        int split = nodePath.IndexOf('/');
        string rootName = 0 <= split ? nodePath.Substring(0, split) : nodePath;
        string rest = 0 <= split ? nodePath.Substring(split + 1) : string.Empty;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != rootName)
            {
                continue;
            }

            return string.IsNullOrEmpty(rest) ? root.transform : root.transform.Find(rest);
        }

        return null;
    }

    /// <summary>
    /// 파일별로 묶습니다. 한 파일을 여러 번 열고 저장하면 느리고, 뒤에 연 쪽이 앞의 수정을 덮습니다.
    /// </summary>
    public static Dictionary<string, List<UiTextImageTarget>> GroupByAsset(UiTextImageTarget[] targets)
    {
        Dictionary<string, List<UiTextImageTarget>> byAsset = new Dictionary<string, List<UiTextImageTarget>>();

        for (int i = 0; i < targets.Length; i++)
        {
            if (!byAsset.TryGetValue(targets[i].AssetPath, out List<UiTextImageTarget> list))
            {
                list = new List<UiTextImageTarget>();
                byAsset[targets[i].AssetPath] = list;
            }

            list.Add(targets[i]);
        }

        return byAsset;
    }
}
