using System.IO;
using UnityEngine;

/// <summary>
/// 세이브 파일의 경로를 한곳에서 만들어 주는 정적 헬퍼입니다(MasterDataPaths와 같은 역할).
///
/// 세이브는 반드시 Application.persistentDataPath 아래에 둡니다.
/// 마스터 데이터가 있는 StreamingAssets는 빌드 산출물 안에 들어가 런타임 쓰기가 불가능하며,
/// 그곳에 저장하면 에디터에서만 동작하고 빌드에서 실패해 발견이 늦어집니다.
/// </summary>
public static class SavePaths
{
    private const string SAVE_FOLDER = "Save";
    private const string SAVE_FILE_NAME = "player.json";
    private const string BACKUP_FILE_NAME = "player.bak.json";
    private const string TEMP_FILE_NAME = "player.tmp.json";

    public static string SaveRoot => Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

    public static string SaveFilePath => Path.Combine(SaveRoot, SAVE_FILE_NAME);

    /// <summary>
    /// 직전 저장본입니다. 정식 파일이 손상되었을 때 여기서 복구를 시도합니다(기획서 3-K-4).
    /// </summary>
    public static string BackupFilePath => Path.Combine(SaveRoot, BACKUP_FILE_NAME);

    /// <summary>
    /// 쓰기가 끝나기 전의 중간 파일입니다. 저장 도중 종료되어도 정식 파일이 깨지지 않도록 여기에 먼저 씁니다.
    /// </summary>
    public static string TempFilePath => Path.Combine(SaveRoot, TEMP_FILE_NAME);

    public static void EnsureSaveRoot()
    {
        Directory.CreateDirectory(SaveRoot);
    }
}
