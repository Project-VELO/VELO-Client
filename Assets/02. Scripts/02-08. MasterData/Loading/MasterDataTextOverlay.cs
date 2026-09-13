using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 마스터 데이터 테이블 위에 번역문을 덮습니다. 대본에 쓰는 StoryTextOverlayLoader와 같은 방식입니다.
///
/// 번역 파일은 원본과 같은 모양으로 두고 옮길 칸만 채웁니다. 기획이 원본을 복사해
/// 글자만 바꾸면 되고, 새 항목이 원본에 늘어도 번역 파일은 그대로 둘 수 있습니다.
/// 채우지 않은 칸은 원문이 그대로 남습니다. 옮기지 못한 이름이 빈칸으로 나오는 것보다 낫습니다.
///
/// 어느 칸을 옮길지 타입마다 적어 두지 않는 것은, 그 목록이 DTO와 함께 낡기 때문입니다.
/// 대신 번역 파일에 적힌 글자만 덮습니다. 경로나 분류 이름처럼 옮기면 안 되는 칸은
/// 번역 파일에 적지 않으면 그만입니다.
/// </summary>
public class MasterDataTextOverlay
{
    private const string ID_FIELD_SUFFIX = "Id";

    private const string PATH_FIELD_SUFFIX = "Path";

    /// <summary>
    /// 지금 언어의 번역문을 테이블에 적용합니다. 한국어이거나 파일이 없으면 아무것도 하지 않습니다.
    ///
    /// 파일이 없을 때 경고를 남기지 않는 것은 의도한 것입니다. 아직 옮기지 않은 테이블이
    /// 있는 것이 정상이고, 켤 때마다 경고가 쌓이면 진짜 문제를 가립니다.
    /// </summary>
    public void Apply<T>(List<T> items, string fileName, ELanguage language)
    {
        if (items == null || items.Count == 0)
        {
            return;
        }

        string folder = LanguageCode.GetFolderName(language);

        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        string path = MasterDataPaths.GetTableOverlayPath(fileName, folder);

        if (!File.Exists(path))
        {
            return;
        }

        List<T> translated = Load<T>(path);

        if (translated == null)
        {
            return;
        }

        FieldInfo idField = FindIdField(typeof(T));

        if (idField == null)
        {
            Debug.LogWarning($"[MasterDataTextOverlay] {typeof(T).Name}에서 식별자 칸을 찾지 못해 번역을 덮지 못했습니다: {path}");
            return;
        }

        FieldInfo[] textFields = FindTextFields(typeof(T), idField);
        Dictionary<string, T> byId = BuildIndex(translated, idField);

        for (int i = 0; i < items.Count; i++)
        {
            string id = idField.GetValue(items[i]) as string;

            if (string.IsNullOrEmpty(id) || !byId.TryGetValue(id, out T source))
            {
                continue;
            }

            Overwrite(items[i], source, textFields);
        }
    }

    private static List<T> Load<T>(string path)
    {
        MasterDataTable<T> table = JsonUtility.FromJson<MasterDataTable<T>>(File.ReadAllText(path));

        if (ReferenceEquals(table, null) || ReferenceEquals(table.Items, null))
        {
            Debug.LogWarning($"[MasterDataTextOverlay] 번역 파일 파싱에 실패했습니다: {path}");
            return null;
        }

        return table.Items;
    }

    /// <summary>
    /// 식별자 칸입니다. 이 프로젝트의 테이블은 첫 문자열 칸이 언제나 자기 자신의 Id입니다
    /// (CharacterId, CardId, ScheduleId …). CardData의 CharacterId처럼 남을 가리키는 Id는
    /// 뒤에 오므로 먼저 만나는 것을 쓰면 맞습니다.
    /// </summary>
    private static FieldInfo FindIdField(System.Type type)
    {
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i].FieldType == typeof(string) && fields[i].Name.EndsWith(ID_FIELD_SUFFIX))
            {
                return fields[i];
            }
        }

        return null;
    }

    /// <summary>
    /// 덮어도 되는 칸입니다. 이름이 Id나 Path로 끝나는 칸은 제외합니다.
    ///
    /// 번역 파일에 적지 않으면 어차피 덮이지 않지만, 기획이 원본을 통째로 복사해 쓰다가
    /// 참조 Id나 리소스 경로를 함께 건드리면 그림이 사라지거나 해금 조건이 어긋납니다.
    /// 사람이 실수할 수 있는 자리는 코드에서 막아 둡니다.
    /// </summary>
    private static FieldInfo[] FindTextFields(System.Type type, FieldInfo idField)
    {
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        List<FieldInfo> texts = new List<FieldInfo>(fields.Length);

        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i].FieldType != typeof(string) || fields[i] == idField)
            {
                continue;
            }

            if (fields[i].Name.EndsWith(ID_FIELD_SUFFIX) || fields[i].Name.EndsWith(PATH_FIELD_SUFFIX))
            {
                continue;
            }

            texts.Add(fields[i]);
        }

        return texts.ToArray();
    }

    private static Dictionary<string, T> BuildIndex<T>(List<T> items, FieldInfo idField)
    {
        Dictionary<string, T> byId = new Dictionary<string, T>(items.Count);

        for (int i = 0; i < items.Count; i++)
        {
            string id = idField.GetValue(items[i]) as string;

            if (!string.IsNullOrEmpty(id))
            {
                byId[id] = items[i];
            }
        }

        return byId;
    }

    /// <summary>
    /// 번역 파일에 글자가 적힌 칸만 덮습니다. 적지 않은 칸은 JsonUtility가 null로 두므로
    /// 여기서 걸러지고, 원문이 그대로 살아남습니다.
    /// </summary>
    private static void Overwrite<T>(T target, T source, FieldInfo[] textFields)
    {
        for (int i = 0; i < textFields.Length; i++)
        {
            string value = textFields[i].GetValue(source) as string;

            if (!string.IsNullOrEmpty(value))
            {
                textFields[i].SetValue(target, value);
            }
        }
    }
}
