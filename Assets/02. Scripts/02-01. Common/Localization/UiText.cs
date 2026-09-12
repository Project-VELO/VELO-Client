using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// UI 글자를 지금 언어로 바꿔 줍니다.
///
/// 표는 언어 폴더 아래 ui_texts.json 하나이고 한국어 원문이 키입니다.
/// 한국어일 때는 파일을 읽지도 않습니다. 원문이 곧 결과이기 때문입니다.
///
/// 정적 클래스인 것은 프리팹에 박힌 글자를 훑는 쪽(UiTextLocalizer)과 코드에 적힌 글자를
/// 감싸는 쪽이 둘 다 화면 밖에서 부르기 때문입니다.
/// </summary>
public static class UiText
{
    public const string FILE_NAME = "ui_texts.json";

    private static Dictionary<string, string> _table;

    /// <summary>
    /// 옮긴 글자에서 한국어 원문으로 되돌아가는 표입니다. 언어를 바꿔도 비우지 않습니다.
    ///
    /// 한국어는 읽을 파일이 없어(원문이 곧 결과입니다) 일본어에서 한국어로 돌아온 순간
    /// 앞의 표가 사라집니다. 그러면 화면에 떠 있는 일본어를 되돌릴 길이 없어집니다.
    /// 한 번 읽은 짝은 그대로 들고 있습니다.
    /// </summary>
    private static readonly Dictionary<string, string> _sources = new Dictionary<string, string>();

    private static ELanguage _loadedLanguage;
    private static bool _isLoaded;

    /// <summary>
    /// 표에 있으면 옮긴 글자를, 없으면 원문을 그대로 돌려줍니다.
    ///
    /// 이미 옮겨진 글자를 다시 넣어도 됩니다. 한국어 원문으로 되돌린 뒤 옮기므로 몇 번을 불러도
    /// 결과가 같고, 언어를 바꾼 자리에서 화면에 떠 있는 글자를 그대로 다시 넣어 맞출 수 있습니다.
    /// 되돌릴 짝이 없으면 들어온 글자가 곧 원문입니다.
    ///
    /// 없는 글자에 경고를 남기지 않습니다. 아직 옮기지 않은 문구가 있는 것이 정상이고,
    /// 화면을 열 때마다 훑으므로 경고를 남기면 같은 줄이 수백 번 쌓입니다.
    /// </summary>
    public static string Localize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        EnsureLoaded();

        string korean = _sources.TryGetValue(text, out string found) ? found : text;

        if (_table != null && _table.TryGetValue(korean, out string translated))
        {
            return translated;
        }

        return korean;
    }

    /// <summary>
    /// 언어가 바뀌면 다음 조회 때 다시 읽습니다.
    /// 언어를 두 번 바꿔 되돌아왔을 때도 표가 어긋나지 않도록 언어를 함께 기억해 둡니다.
    /// </summary>
    private static void EnsureLoaded()
    {
        ELanguage current = LanguageSetting.Current;

        if (_isLoaded && _loadedLanguage == current)
        {
            return;
        }

        _isLoaded = true;
        _loadedLanguage = current;
        _table = Load(current);
    }

    private static Dictionary<string, string> Load(ELanguage language)
    {
        string folder = LanguageCode.GetFolderName(language);

        if (string.IsNullOrEmpty(folder))
        {
            return null;
        }

        string path = MasterDataPaths.GetTableOverlayPath(FILE_NAME, folder);

        if (!File.Exists(path))
        {
            return null;
        }

        MasterDataTable<UiTextData> table = JsonUtility.FromJson<MasterDataTable<UiTextData>>(File.ReadAllText(path));

        if (ReferenceEquals(table, null) || ReferenceEquals(table.Items, null))
        {
            Debug.LogWarning($"[UiText] 번역 표 파싱에 실패했습니다: {path}");
            return null;
        }

        Dictionary<string, string> map = new Dictionary<string, string>(table.Items.Count);

        for (int i = 0; i < table.Items.Count; i++)
        {
            UiTextData item = table.Items[i];

            if (item == null || string.IsNullOrEmpty(item.Source) || string.IsNullOrEmpty(item.Text))
            {
                continue;
            }

            map[item.Source] = item.Text;
            KeepSource(item);
        }

        return map;
    }

    /// <summary>
    /// 되돌아갈 원문을 기억해 둡니다.
    ///
    /// 한 번역에 원문이 둘 이상이면 짧은 쪽을 남깁니다. 줄바꿈만 덧붙은 사본이 원문 자리를
    /// 차지하면, 되돌린 글자에 빈 줄이 딸려 와 한 줄짜리 글상자가 두 줄로 잡힙니다.
    /// </summary>
    private static void KeepSource(UiTextData item)
    {
        if (_sources.TryGetValue(item.Text, out string kept) && kept.Length <= item.Source.Length)
        {
            return;
        }

        _sources[item.Text] = item.Source;
    }
}
