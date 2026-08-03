using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 세이브 파일의 저장·불러오기를 전담합니다. 디스크 접근은 이 클래스에만 둡니다.
///
/// 기획서 3-K-1은 저장 방식을 개발자에게 맡기되, 스토리·스케줄 진행과 재화, 최고 기록,
/// 보상 중복 방지 결과가 재실행 후에도 유지될 것을 요구합니다(16-14).
/// </summary>
public class SaveService
{
    /// <summary>
    /// 저장에 실패했을 때 다시 시도하는 횟수입니다.
    /// 다른 프로세스가 파일을 잠깐 잡고 있는 경우가 대부분이라 즉시 재시도로 대개 해결됩니다(기획서 3-L).
    /// </summary>
    private const int SAVE_RETRY_COUNT = 2;

    /// <summary>
    /// 저장이 최종 실패했을 때 알립니다. 사용자 안내 팝업은 공통 팝업 작업에서 이 이벤트에 연결합니다.
    /// </summary>
    public Action OnSaveFailed;

    /// <summary>
    /// 저장은 원자적으로 수행합니다. 임시 파일에 먼저 쓰고 기존 파일을 백업으로 돌린 뒤 교체하므로,
    /// 쓰기 도중 게임이 강제 종료되어도 정식 파일이 반쪽으로 남지 않습니다.
    /// </summary>
    public bool Save(PlayerData data)
    {
        if (ReferenceEquals(data, null))
        {
            Debug.LogError("[SaveService] 저장할 데이터가 없습니다.");
            return false;
        }

        string json = JsonUtility.ToJson(data, true);

        for (int attempt = 0; attempt <= SAVE_RETRY_COUNT; attempt++)
        {
            if (TryWrite(json, out string error))
            {
                return true;
            }

            Debug.LogWarning($"[SaveService] 저장 시도 {attempt + 1}회 실패: {error}");
        }

        Debug.LogError($"[SaveService] 저장에 최종 실패했습니다: {SavePaths.SaveFilePath}");
        OnSaveFailed?.Invoke();

        return false;
    }

    /// <summary>
    /// 세이브를 불러옵니다. 정식 파일이 손상되었으면 백업을 시도하고, 둘 다 실패하면 null을 돌려줍니다.
    /// null은 "신규 게임을 만들라"는 뜻이며, 호출부가 기본 데이터 복구를 수행합니다(기획서 3-K-4).
    /// </summary>
    public PlayerData Load()
    {
        if (TryRead(SavePaths.SaveFilePath, out PlayerData data))
        {
            return data;
        }

        if (TryRead(SavePaths.BackupFilePath, out PlayerData backup))
        {
            Debug.LogWarning("[SaveService] 정식 세이브를 읽지 못해 직전 저장본으로 복구했습니다.");
            return backup;
        }

        return null;
    }

    public bool HasSaveFile()
    {
        return File.Exists(SavePaths.SaveFilePath) || File.Exists(SavePaths.BackupFilePath);
    }

    /// <summary>
    /// 세이브를 모두 지웁니다. 신규 게임 리셋에 사용합니다.
    /// </summary>
    public void Delete()
    {
        DeleteIfExists(SavePaths.SaveFilePath);
        DeleteIfExists(SavePaths.BackupFilePath);
        DeleteIfExists(SavePaths.TempFilePath);
    }

    private bool TryWrite(string json, out string error)
    {
        error = null;

        try
        {
            SavePaths.EnsureSaveRoot();
            File.WriteAllText(SavePaths.TempFilePath, json);

            // File.Replace는 대상 파일이 있어야 동작하므로, 첫 저장은 단순 이동으로 처리합니다.
            if (File.Exists(SavePaths.SaveFilePath))
            {
                File.Replace(SavePaths.TempFilePath, SavePaths.SaveFilePath, SavePaths.BackupFilePath);
            }
            else
            {
                File.Move(SavePaths.TempFilePath, SavePaths.SaveFilePath);
            }

            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private bool TryRead(string path, out PlayerData data)
    {
        data = null;

        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            data = JsonUtility.FromJson<PlayerData>(File.ReadAllText(path));
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[SaveService] 세이브 파싱에 실패했습니다({path}): {exception.Message}");
            return false;
        }

        // 파싱은 통과했지만 내용이 비어 있는 경우입니다. 이 상태로 진행하면 카드도 편성도 없는 게임이 됩니다.
        //
        // 필드 기본값 때문에 "{}" 같은 빈 JSON도 파싱에 성공하고 주차 ID까지 채워집니다.
        // 그래서 기본값이 존재하지 않는 두 가지를 봅니다. 저장을 거쳐야만 붙는 스키마 버전과,
        // 신규 게임이라면 반드시 지급되어 있어야 하는 보유 카드입니다.
        if (ReferenceEquals(data, null) || data.SchemaVersion <= SaveSchema.UNSAVED_VERSION || data.OwnedCardIds.Count == 0)
        {
            Debug.LogWarning($"[SaveService] 세이브에 필수 데이터가 없습니다: {path}");
            data = null;
            return false;
        }

        if (data.SchemaVersion != SaveSchema.CURRENT_VERSION)
        {
            Debug.LogWarning($"[SaveService] 세이브 스키마 버전이 다릅니다(파일 {data.SchemaVersion} / 코드 {SaveSchema.CURRENT_VERSION}).");
        }

        return true;
    }

    private void DeleteIfExists(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[SaveService] 세이브 삭제에 실패했습니다({path}): {exception.Message}");
        }
    }
}
