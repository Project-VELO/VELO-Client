using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 수록 공간의 곡 커버 이미지(cover.png)를 런타임에 불러옵니다.
/// StreamingAssets는 임포트된 에셋이 아니라 파일이므로, 음원과 동일하게 UnityWebRequest로 읽습니다.
/// 한 번 만든 Sprite는 챕터를 오가며 반복 로드되지 않도록 곡 단위로 캐시합니다.
/// </summary>
public class SongCoverLoader
{
    private readonly Dictionary<string, Sprite> _covers = new Dictionary<string, Sprite>();

    /// <summary>
    /// 커버가 없는 곡도 정상이므로, 실패 시 예외 대신 null을 돌려주고 호출부가 대체 이미지를 쓰게 합니다.
    /// </summary>
    public async UniTask<Sprite> LoadCoverAsync(SongData song, CancellationToken cancellationToken)
    {
        if (ReferenceEquals(song, null) || string.IsNullOrEmpty(song.FolderPath))
        {
            return null;
        }

        if (_covers.TryGetValue(song.SongId, out Sprite cached))
        {
            return cached;
        }

        string coverPath = GetCoverPath(song);
        if (!File.Exists(coverPath))
        {
            _covers[song.SongId] = null;
            return null;
        }

        Sprite cover = await RequestCoverAsync(coverPath, cancellationToken);
        _covers[song.SongId] = cover;

        return cover;
    }

    /// <summary>
    /// 캐시를 비우면서 만들어 둔 리소스도 함께 파괴합니다.
    /// 여기서 만든 Texture2D와 Sprite는 씬에 매여 있지 않아, 참조만 버리면 화면을 드나들 때마다 쌓입니다.
    /// </summary>
    public void Clear()
    {
        foreach (Sprite cover in _covers.Values)
        {
            DestroyCover(cover);
        }

        _covers.Clear();
    }

    private void DestroyCover(Sprite cover)
    {
        if (cover == null)
        {
            return;
        }

        // 스프라이트를 먼저 없앤 뒤 원본 텍스처를 정리해야 파괴된 텍스처를 참조하는 순간이 생기지 않습니다.
        Texture2D texture = cover.texture;
        UnityEngine.Object.Destroy(cover);

        if (texture != null)
        {
            UnityEngine.Object.Destroy(texture);
        }
    }

    /// <summary>
    /// song_info.json에 커버 파일명이 적혀 있으면 그것을 우선하고, 없으면 약속된 기본 파일명을 사용합니다.
    /// </summary>
    private string GetCoverPath(SongData song)
    {
        if (string.IsNullOrEmpty(song.CoverImagePath))
        {
            return LiveSongPaths.GetPublishedCoverPath(song.FolderPath);
        }

        return Path.Combine(song.FolderPath, song.CoverImagePath);
    }

    private async UniTask<Sprite> RequestCoverAsync(string coverPath, CancellationToken cancellationToken)
    {
        string url = "file://" + coverPath;
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[SongCoverLoader] 커버 이미지 로드 실패: {request.error} ({coverPath})");
            return null;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        if (texture == null)
        {
            return null;
        }

        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}
