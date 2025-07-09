using Avatar_Explorer.Models;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Writers;
using System.Diagnostics;

namespace Avatar_Explorer.Utils;

internal static class FileSystemUtils
{
    /// <summary>
    /// ファイル名が正常かどうかをチェックします。もし使えない文字があれば正常なファイル名を返します。
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    internal static string CheckFilePath(string filePath)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(filePath.Where(c => !invalidChars.Contains(c)));
    }

    /// <summary>
    /// Tarファイルを作成します。
    /// </summary>
    /// <param name="sourceFolder"></param>
    /// <param name="outputTarFile"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    internal static void CreateTarArchive(string sourceFolder, string outputTarFile)
    {
        if (!Directory.Exists(sourceFolder))
        {
            throw new DirectoryNotFoundException($"指定されたフォルダーが見つかりません: {sourceFolder}");
        }

        using var archive = TarArchive.Create();

        foreach (string filePath in Directory.EnumerateFiles(sourceFolder, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(sourceFolder, filePath);
            archive.AddEntry(relativePath, filePath);
        }

        using var fileStream = File.OpenWrite(outputTarFile);
        archive.SaveTo(fileStream, new WriterOptions(CompressionType.None));
    }

    /// <summary>
    /// zipファイルを指定されたフォルダに展開します。
    /// </summary>
    /// <param name="zipPath"></param>
    /// <param name="extractPath"></param>
    /// <returns></returns>
    internal static string ExtractZip(string zipPath, string extractPath)
    {
        var extractFolder = Path.Combine(extractPath, Path.GetFileNameWithoutExtension(zipPath));
        if (!Directory.Exists(extractFolder))
        {
            Directory.CreateDirectory(extractFolder);
        }
        else
        {
            int i = 1;
            while (Directory.Exists(extractFolder + " - " + i))
            {
                i++;
            }
            extractFolder += " - " + i;
            Directory.CreateDirectory(extractFolder);
        }

        using var archive = SharpCompress.Archives.Zip.ZipArchive.Open(zipPath);
        foreach (var entry in archive.Entries)
        {
            if (entry.IsDirectory)
            {
                if (entry.Key == null) continue;
                Directory.CreateDirectory(Path.Combine(extractFolder, entry.Key));
            }
            else
            {
                entry.WriteToDirectory(extractFolder, new ExtractionOptions()
                {
                    ExtractFullPath = true,
                    Overwrite = true
                });
            }
        }

        return extractFolder;
    }

    /// <summary>
    /// フォルダを指定されたパスにコピーします。
    /// </summary>
    /// <param name="sourceDirName"></param>
    /// <param name="destDirName"></param>
    internal static void CopyDirectory(string sourceDirName, string destDirName)
    {
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        var dir = new DirectoryInfo(sourceDirName);
        var files = dir.GetFiles();

        foreach (var file in files)
        {
            var temppath = Path.Combine(destDirName, file.Name);
            file.CopyTo(temppath, true);
        }

        var dirs = dir.GetDirectories();
        foreach (var subdir in dirs)
        {
            var temppath = Path.Combine(destDirName, subdir.Name);
            CopyDirectory(subdir.FullName, temppath);
        }
    }

    /// <summary>
    /// アイテムのファイルを開きます。
    /// </summary>
    /// <param name="file"></param>
    /// <param name="openFile"></param>
    /// <param name="CurrentLanguage"></param>
    internal static void OpenItemFile(FileData file, bool openFile, string CurrentLanguage)
    {
        if (openFile)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = file.FilePath,
                    UseShellExecute = true
                });
            }
            catch
            {
                OpenItemFilePath(file, CurrentLanguage);
            }
        }
        else
        {
            OpenItemFilePath(file, CurrentLanguage);
        }
    }

    /// <summary>
    /// アイテムのファイルパスを開きます。
    /// </summary>
    /// <param name="file"></param>
    /// <param name="CurrentLanguage"></param>
    private static void OpenItemFilePath(FileData file, string CurrentLanguage)
    {
        try
        {
            var itemFullFolderPath = Path.GetFullPath(file.FilePath);
            Process.Start("explorer.exe", "/select," + itemFullFolderPath);
        }
        catch
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("ファイルを開けませんでした。", CurrentLanguage),
                LanguageUtils.Translate("エラー", CurrentLanguage),
                true
            );
        }
    }

    /// <summary>
    /// アイテムのフォルダを開きます。
    /// </summary>
    /// <param name="item"></param>
    /// <param name="CurrentLanguage"></param>
    internal static void OpenItemFolder(Item item, string CurrentLanguage)
    {
        if (!Directory.Exists(item.ItemPath))
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("フォルダが見つかりませんでした。", CurrentLanguage),
                LanguageUtils.Translate("エラー", CurrentLanguage),
                true
            );
            return;
        }

        try
        {
            var itemFullFolderPath = Path.GetFullPath(item.ItemPath);
            Process.Start("explorer.exe", itemFullFolderPath);
        }
        catch
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("フォルダを開けませんでした。", CurrentLanguage),
                LanguageUtils.Translate("エラー", CurrentLanguage),
                true
            );
        }
    }
}
