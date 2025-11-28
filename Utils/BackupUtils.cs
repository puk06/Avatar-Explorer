namespace Avatar_Explorer.Utils;

internal static class BackupUtils
{
    /// <summary>
    /// 指定されたファイルをバックアップします。
    /// </summary>
    /// <param name="paths"></param>
    internal static void Backup(string[] paths)
    {
        var folderPath = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var backupPath = Path.Combine(appDataPath, "Avatar Explorer", "Backup");

        foreach (var path in paths)
        {
            if (!File.Exists(path)) continue;
            if (!Directory.Exists(backupPath)) Directory.CreateDirectory(backupPath);

            var backupFolderPath = Path.Combine(backupPath, folderPath);
            if (!Directory.Exists(backupFolderPath)) Directory.CreateDirectory(backupFolderPath);

            File.WriteAllText(Path.Combine(backupFolderPath, Path.GetFileName(path)), File.ReadAllText(path));
        }
    }

    /// <summary>
    /// バックアップの時間を取得します。
    /// </summary>
    /// <param name="FileName"></param>
    /// <param name="CurrnentLanguage"></param>
    /// <returns></returns>
    internal static string GetBackupTime(string FileName)
    {
        try
        {
            var dateTime = DateTime.ParseExact(FileName, "yyyy-MM-dd-HH-mm-ss", null);
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            return FileName;
        }
    }
}
