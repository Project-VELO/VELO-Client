using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 감상 화면 배경 이미지를 StreamingAssets에서 읽어 스프라이트로 만듭니다.
///
/// StreamingAssets는 유니티가 임포트하지 않는 폴더라 스프라이트 에셋이 존재하지 않습니다.
/// 그래서 파일 바이트를 직접 읽어 텍스처를 만들고, 그 텍스처로 스프라이트를 한 장 씁니다.
///
/// 한 회차가 쓰는 배경은 많아야 서너 장이므로 처음 쓰일 때 읽고 캐시에 남깁니다.
/// 줄이 바뀔 때마다 다시 읽으면 같은 파일을 수십 번 디코딩하게 됩니다.
/// </summary>
public class StoryBackgroundLoader
{
    private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

    /// <summary>
    /// 없는 파일을 매 줄 경고하지 않도록, 한 번 실패한 ID는 기억해 둡니다.
    /// </summary>
    private readonly HashSet<string> _missingIds = new HashSet<string>();

    /// <summary>
    /// 배경 스프라이트를 돌려줍니다. 파일이 없거나 디코딩에 실패하면 null이며,
    /// 호출부는 기획서 3-L에 따라 단색 배경으로 대체합니다.
    /// </summary>
    public Sprite Load(string backgroundId)
    {
        if (string.IsNullOrEmpty(backgroundId) || _missingIds.Contains(backgroundId))
        {
            return null;
        }

        if (_sprites.TryGetValue(backgroundId, out Sprite cached))
        {
            return cached;
        }

        Sprite sprite = CreateSprite(backgroundId);

        if (sprite == null)
        {
            _missingIds.Add(backgroundId);
            return null;
        }

        _sprites[backgroundId] = sprite;
        return sprite;
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
        _missingIds.Clear();
    }

    private Sprite CreateSprite(string backgroundId)
    {
        string path = MasterDataPaths.GetStoryBackgroundPath(backgroundId);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[StoryBackgroundLoader] 배경 이미지가 없습니다: {path}");
            return null;
        }

        // 크기는 LoadImage가 파일에서 읽어 다시 잡으므로 여기서는 아무 값이나 둡니다.
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        if (!texture.LoadImage(File.ReadAllBytes(path)))
        {
            Debug.LogWarning($"[StoryBackgroundLoader] 배경 이미지를 읽지 못했습니다: {path}");
            Object.Destroy(texture);
            return null;
        }

        var rect = new Rect(0f, 0f, texture.width, texture.height);
        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
    }
}
