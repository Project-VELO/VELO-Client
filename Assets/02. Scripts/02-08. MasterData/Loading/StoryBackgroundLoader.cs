using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 감상 화면 배경 이미지를 StreamingAssets에서 읽어 스프라이트로 만듭니다.
///
/// StreamingAssets는 유니티가 임포트하지 않는 폴더라 스프라이트 에셋이 존재하지 않습니다.
/// 그래서 파일을 직접 읽어 텍스처를 만들고, 그 텍스처로 스프라이트를 한 장 씁니다.
///
/// 읽기에 File API가 아니라 UnityWebRequest를 쓰는 이유는 안드로이드 때문입니다.
/// 그곳의 StreamingAssets는 APK 안에 압축되어 있어 파일 경로로 열리지 않습니다.
///
/// 회차에 들어갈 때 한 번에 미리 읽습니다. 줄이 바뀌는 시점에 디코딩하면 그 프레임이 끊기고,
/// 한 회차가 쓰는 배경은 많아야 서너 장이라 미리 읽어도 부담이 없습니다.
/// </summary>
public class StoryBackgroundLoader
{
    private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

    /// <summary>
    /// 미리 읽어 둔 배경을 돌려줍니다. 없으면 null이며 호출부는 단색으로 대체합니다(기획서 3-L).
    /// 여기서 새로 읽지 않는 것은, 줄을 넘기는 도중에 디스크를 만지지 않기 위해서입니다.
    /// </summary>
    public Sprite Get(string backgroundId)
    {
        if (string.IsNullOrEmpty(backgroundId))
        {
            return null;
        }

        return _sprites.TryGetValue(backgroundId, out Sprite sprite) ? sprite : null;
    }

    /// <summary>
    /// 대본이 쓰는 배경을 모두 읽어 둡니다. 같은 ID가 여러 줄에 나와도 한 번만 읽습니다.
    /// </summary>
    public async UniTask PreloadAsync(StoryScriptData script, CancellationToken cancellationToken)
    {
        foreach (string backgroundId in CollectBackgroundIds(script))
        {
            if (_sprites.ContainsKey(backgroundId))
            {
                continue;
            }

            Sprite sprite = await LoadSpriteAsync(backgroundId, cancellationToken);

            if (sprite != null)
            {
                _sprites[backgroundId] = sprite;
            }
        }
    }

    /// <summary>
    /// 만들어 둔 텍스처를 모두 해제합니다. 화면이 내려갈 때 반드시 불러야 합니다.
    ///
    /// 런타임에 만든 Texture2D는 어떤 게임 오브젝트에도 붙어 있지 않아 씬이 내려가도 함께 사라지지 않습니다.
    /// PoolManager는 게임 오브젝트를 다루는 도구라 이 텍스처를 맡길 수 없어, 여기서 직접 해제합니다.
    /// </summary>
    public void Release()
    {
        foreach (KeyValuePair<string, Sprite> pair in _sprites)
        {
            Texture2D texture = pair.Value.texture;
            Object.Destroy(pair.Value);
            Object.Destroy(texture);
        }

        _sprites.Clear();
    }

    private List<string> CollectBackgroundIds(StoryScriptData script)
    {
        var ids = new List<string>();

        foreach (StoryLineData line in script.Lines)
        {
            if (!string.IsNullOrEmpty(line.BackgroundId) && !ids.Contains(line.BackgroundId))
            {
                ids.Add(line.BackgroundId);
            }
        }

        return ids;
    }

    private async UniTask<Sprite> LoadSpriteAsync(string backgroundId, CancellationToken cancellationToken)
    {
        string url = ToUrl(MasterDataPaths.GetStoryBackgroundPath(backgroundId));

        if (string.IsNullOrEmpty(url))
        {
            return null;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            await request.SendWebRequest().WithCancellation(cancellationToken);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[StoryBackgroundLoader] 배경 이미지를 읽지 못했습니다({request.error}): {url}");
                return null;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            var rect = new Rect(0f, 0f, texture.width, texture.height);

            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        }
    }

    /// <summary>
    /// 안드로이드의 StreamingAssets 경로는 이미 URL 형태(jar:file://...)입니다.
    /// 그 외 플랫폼은 평범한 파일 경로라 UnityWebRequest가 알아볼 수 있게 스킴을 붙여 줍니다.
    /// </summary>
    private string ToUrl(string path)
    {
        if (string.IsNullOrEmpty(path) || path.Contains("://"))
        {
            return path;
        }

        return $"file://{path}";
    }
}
