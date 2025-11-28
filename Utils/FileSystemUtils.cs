using Avatar_Explorer.Forms;
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
        if (!Directory.Exists(sourceFolder)) throw new DirectoryNotFoundException($"指定されたフォルダーが見つかりません: {sourceFolder}");

        using var archive = TarArchive.Create();

        foreach (string filePath in FastEnumerateFiles(sourceFolder))
        {
            string relativePath = Path.GetRelativePath(sourceFolder, filePath);
            archive.AddEntry(relativePath, filePath);
        }

        using var fileStream = new FileStream(
            outputTarFile,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 1024 * 1024,
            FileOptions.SequentialScan
        );

        archive.SaveTo(fileStream, new WriterOptions(CompressionType.None));
    }

    /// <summary>
    /// 高速でファイルを列挙します。
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    internal static IEnumerable<string> FastEnumerateFiles(string root)
    {
        var dirs = new Stack<string>();
        dirs.Push(root);

        while (dirs.Count > 0)
        {
            var dir = dirs.Pop();

            string[] subDirs;
            try { subDirs = Directory.GetDirectories(dir); }
            catch { continue; }

            foreach (var d in subDirs)
            {
                dirs.Push(d);
            }

            string[] files;
            try { files = Directory.GetFiles(dir); }
            catch { continue; }

            foreach (var f in files)
            {
                yield return f;
            }
        }
    }

    /// <summary>
    /// zipファイルを指定されたフォルダに展開します。
    /// </summary>
    /// <param name="zipPath"></param>
    /// <param name="extractPath"></param>
    /// <param name="removeOriginal"></param>
    /// <returns></returns>
    internal static string ExtractZip(string zipPath, string extractPath, bool removeOriginal)
    {
        var extractFolder = Path.Combine(extractPath, Path.GetFileNameWithoutExtension(zipPath));

        if (Directory.Exists(extractFolder))
        {
            int i = 1;
            while (Directory.Exists(extractFolder + " - " + i)) i++;
            extractFolder += " - " + i;
        }
        Directory.CreateDirectory(extractFolder);

        const int BufferSize = 1024 * 1024;
        byte[] buffer = new byte[BufferSize];

        using var archive = SharpCompress.Archives.Zip.ZipArchive.Open(zipPath);

        foreach (var entry in archive.Entries)
        {
            if (!entry.IsDirectory)
            {
                string fullPath = Path.Combine(extractFolder, entry.Key!);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                using var inStream = entry.OpenEntryStream();
                using var outStream = File.Create(fullPath);

                int read;
                while ((read = inStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outStream.Write(buffer, 0, read);
                }
            }
            else
            {
                if (entry.Key != null)
                {
                    Directory.CreateDirectory(Path.Combine(extractFolder, entry.Key));
                }
            }
        }

        if (removeOriginal)
        {
            try { File.Delete(zipPath); } catch { }
        }

        return extractFolder;
    }

    /// <summary>
    /// フォルダを指定されたパスにコピーします。
    /// </summary>
    /// <param name="sourceDirName"></param>
    /// <param name="destDirName"></param>
    internal static async Task CopyDirectoryWithProgress(
        string sourceDirName,
        string destDirName,
        string currentLanguage = "",
        string progressFormTitle = "",
        bool showProgress = false,
        int maxDegreeOfParallelism = 4)
    {
        var cts = new CancellationTokenSource();
        ProgressForm? progressForm = null;

        if (showProgress)
        {
            progressForm = new(progressFormTitle);
            progressForm.FormClosing += (s, e) => cts.Cancel();
            progressForm.Show();
        }

        try
        {
            progressForm?.UpdateProgress(0, LanguageUtils.Translate("準備中", currentLanguage));

            var allFiles = FastEnumerateFiles(sourceDirName).ToList();
            int totalFiles = allFiles.Count;
            int copiedFiles = 0;
            int lastPercent = -1;

            object progressLock = new();

            await Task.Run(() =>
            {
                Parallel.ForEach(
                    allFiles, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism, CancellationToken = cts.Token },
                    file =>
                    {
                        try
                        {
                            string relativePath = Path.GetRelativePath(sourceDirName, file);
                            string destPath = Path.Combine(destDirName, relativePath);
                            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);

                            using var sourceStream = File.OpenRead(file);
                            using var destStream = File.Create(destPath);
                            sourceStream.CopyTo(destStream, 1024 * 1024);

                            lock (progressLock)
                            {
                                copiedFiles++;
                                int percent = (int)(copiedFiles / (double)totalFiles * 100);
                                if (percent != lastPercent)
                                {
                                    lastPercent = percent;
                                    progressForm?.UpdateProgress(percent, $"{copiedFiles}/{totalFiles} {LanguageUtils.Translate("コピー中", currentLanguage)}");
                                }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            LogUtils.ErrorLogger("ファイルコピー失敗: " + file, ex);
                        }
                    }
                );
            }, cts.Token);

            progressForm?.UpdateProgress(100, LanguageUtils.Translate("完了", currentLanguage));
        }
        finally
        {
            progressForm?.Close();
            cts.Dispose();
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
