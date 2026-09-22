using System;
using System.IO;

/// <summary>
/// 파일을 원자적으로 교체해 씁니다. 임시 파일에 먼저 쓰고 기존 파일을 백업으로 돌린 뒤 교체하므로,
/// 쓰기 도중 강제 종료되어도 정식 파일이 반쪽으로 남지 않습니다.
///
/// 플레이어 세이브(SaveService)와 플레이테스트 판정 통계가 같은 방식으로 저장하므로 한곳에 둡니다.
/// 재시도와 실패 통지는 호출하는 쪽마다 정책이 달라 여기서 다루지 않습니다.
/// </summary>
public static class AtomicFileWriter
{
    /// <summary>
    /// 예외를 밖으로 던지지 않고 실패 사유를 돌려줍니다. 폴더 생성까지 같은 try 안에 두어,
    /// 폴더를 만들지 못한 경우도 호출하는 쪽의 재시도 대상이 되게 합니다.
    /// </summary>
    public static bool TryWrite(string tempPath, string targetPath, string backupPath, string contents, out string error)
    {
        error = null;

        try
        {
            string directory = Path.GetDirectoryName(targetPath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(tempPath, contents);

            // File.Replace는 대상 파일이 있어야 동작하므로, 첫 저장은 단순 이동으로 처리합니다.
            if (File.Exists(targetPath))
            {
                File.Replace(tempPath, targetPath, backupPath);
            }
            else
            {
                File.Move(tempPath, targetPath);
            }

            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }
}
