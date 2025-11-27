using Avatar_Explorer.Forms;
using Avatar_Explorer.Models;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Timer = System.Windows.Forms.Timer;

namespace Avatar_Explorer.Utils;

internal static partial class AEUtils
{
    [GeneratedRegex(@"(?<key>Author|Title|Booth|Avatar|Category|Memo|Folder|File|Implemented|NotImplemented|Tag|OR|BrokenItems)=(?:""(?<value>.*?)""|(?<value>[^\s]+))|(?<word>[^\s]+)")]
    private static partial Regex SearchFilterRegex();

    [GeneratedRegex(@"\u3010[^\u3011]+\u3011")]
    private static partial Regex BracketsRegex();

    internal static readonly Timer ThumbnailUpdateTimer = new()
    {
        Interval = 200
    };
    private static readonly HashSet<object> _pendingScrollSenders = new();

    static AEUtils()
    {
        ThumbnailUpdateTimer.Tick += (s, e) =>
        {
            ThumbnailUpdateTimer.Stop();
            if (_pendingScrollSenders.Count == 0) return;

            var toProcess = _pendingScrollSenders.ToList();
            _pendingScrollSenders.Clear();

            foreach (var sender in toProcess)
            {
                UpdateExplorerThumbnails(sender);
            }
        };
    }

    /// <summary>
    /// スクロールイベントを処理します。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal static void OnScroll(object? sender, EventArgs e)
    {
        if (sender == null) return;

        lock (_pendingScrollSenders)
        {
            _pendingScrollSenders.Add(sender);
        }
        ThumbnailUpdateTimer.Stop();
        ThumbnailUpdateTimer.Start();
    }

    /// <summary>
    /// パスを変更したUnityPackageファイルを作成します。
    /// </summary>
    /// <param name="file"></param>
    /// <param name="currentPath"></param>
    /// <param name="currentLanguage"></param>
    /// <returns></returns>
    internal static async Task ModifyUnityPackageFilePathAsync(FileData file, CurrentPath currentPath, string currentLanguage)
    {
        ProgressForm progressForm = new(LanguageUtils.Translate("Unitypackageのインポート先の変更中", currentLanguage));
        progressForm.Show();

        try
        {
            progressForm.UpdateProgress(0, LanguageUtils.Translate("準備中", currentLanguage));

            var (saveFolder, saveFilePath, unityPackagePath) = PrepareSavePaths(file, currentPath);
            PrepareSaveDirectory(saveFolder, saveFilePath);

            var extractingStatus = LanguageUtils.Translate("ファイルの展開中", currentLanguage);
            progressForm.UpdateProgress(10, extractingStatus);

            int totalEntries = await CountTarEntriesAsync(file.FilePath);
            var category = ItemUtils.GetCategoryName(currentPath.CurrentSelectedCategory, currentLanguage, currentPath.CurrentSelectedCustomCategory);
            await ExtractTarToFolderAsync(file.FilePath, saveFilePath, category, totalEntries, extractingStatus, progressForm);

            progressForm.UpdateProgress(90, LanguageUtils.Translate("UnityPackageの作成中", currentLanguage));
            FileSystemUtils.CreateTarArchive(saveFilePath, unityPackagePath);

            Directory.Delete(saveFilePath, true);
            progressForm.UpdateProgress(100, LanguageUtils.Translate("完了", currentLanguage));

            Process.Start(new ProcessStartInfo
            {
                FileName = unityPackagePath,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            LogUtils.ErrorLogger("UnityPackageの展開に失敗しました。", ex);
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("UnityPackageの展開に失敗しました。詳細はErrorLog.txtをご覧ください。", currentLanguage),
                LanguageUtils.Translate("エラー", currentLanguage),
                true
            );

            FileSystemUtils.OpenItemFile(file, true, currentLanguage);
        }
        finally
        {
            progressForm.Close();
        }
    }

    private static (string saveFolder, string saveFilePath, string unityPackagePath) PrepareSavePaths(FileData file, CurrentPath currentPath)
    {
        string authorName = FileSystemUtils.CheckFilePath(currentPath.CurrentSelectedItem?.AuthorName ?? "Unknown");
        string itemTitle = FileSystemUtils.CheckFilePath(currentPath.CurrentSelectedItem?.Title ?? "Unknown");

        string saveFolder = Path.Combine("./Datas", "Temp", authorName, itemTitle);
        string saveFilePath = Path.Combine(saveFolder, $"{Path.GetFileNameWithoutExtension(file.FileName)}_export");
        string unityPackagePath = saveFilePath + ".unitypackage";
        return (saveFolder, saveFilePath, unityPackagePath);
    }

    private static void PrepareSaveDirectory(string saveFolder, string saveFilePath)
    {
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        else if (Directory.Exists(saveFilePath))
        {
            Directory.Delete(saveFilePath, true);
        }
    }

    private static async Task<int> CountTarEntriesAsync(string filePath)
    {
        int count = 0;
        await using var fileStream = File.OpenRead(filePath);
        await using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        await using var tarReader = new TarReader(gzipStream);
        while (await tarReader.GetNextEntryAsync() is { })
            count++;
        return count;
    }

    private static async Task ExtractTarToFolderAsync(
        string tarGzFilePath,
        string saveFilePath,
        string category,
        int totalEntries,
        string extractingStatus,
        ProgressForm progressForm
    )
    {
        int processedEntries = 0;

        await using var fileStream = File.OpenRead(tarGzFilePath);
        await using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        await using var tarReader = new TarReader(gzipStream);

        while (await tarReader.GetNextEntryAsync() is { } entry)
        {
            if (Path.GetFileName(entry.Name) == "pathname" && entry.DataStream != null)
            {
                using var reader = new StreamReader(entry.DataStream);
                string assetPath = await reader.ReadToEndAsync();

                if (assetPath.StartsWith("Assets"))
                    assetPath = assetPath.Insert(7, $"{category}/");

                entry.DataStream = new MemoryStream(Encoding.UTF8.GetBytes(assetPath));
            }

            string entryPath = Path.Combine(saveFilePath, entry.Name);
            if (entryPath.EndsWith('/'))
            {
                Directory.CreateDirectory(entryPath);
            }
            else
            {
                entry.DataStream ??= new MemoryStream();
                Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);
                await using var entryStream = File.Create(entryPath);
                await entry.DataStream.CopyToAsync(entryStream);
            }

            processedEntries++;
            int progress = 10 + (int)(80.0 * processedEntries / totalEntries);
            progressForm.UpdateProgress(progress, $"{extractingStatus}: {processedEntries}/{totalEntries}");
        }
    }

    /// <summary>
    /// 指定されたTabPageのサムネイルを更新します。
    /// </summary>
    /// <param name="senderObject"></param>
    internal static void UpdateExplorerThumbnails(object senderObject)
    {
        try
        {
            if (senderObject is not Panel panel) return;

            var visibleArea = new Rectangle(0, panel.VerticalScroll.Value, panel.ClientSize.Width, panel.ClientSize.Height);

            foreach (Control control in panel.Controls)
            {
                if (control is not CustomItemButton button) continue;

                var buttonAbsoluteLocation = button.Location with { Y = button.Location.Y + panel.VerticalScroll.Value };
                button.CheckThmbnail(buttonAbsoluteLocation, button.Size, visibleArea);
            }
        }
        catch (Exception ex)
        {
            LogUtils.ErrorLogger("サムネイルの更新に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 渡された情報からアイテム用のボタンを生成します。
    /// </summary>
    /// <param name="darkmode"></param>
    /// <param name="buttonHeight"></param>
    /// <param name="previewScale"></param>
    /// <param name="imagePath"></param>
    /// <param name="labelTitle"></param>
    /// <param name="description"></param>
    /// <param name="short"></param>
    /// <param name="tooltip"></param>
    /// <param name="listWidthDiff"></param>
    /// <returns></returns>
    internal static CustomItemButton CreateButton(bool darkmode, int buttonHeight, float previewScale, string? imagePath, string labelTitle, string? description, bool @short = false, string tooltip = "", int listWidthDiff = 0)
    {
        var buttonWidth = @short ? 303 : 874;
        if (listWidthDiff != 0) buttonWidth += listWidthDiff;

        CustomItemButton button = new(buttonWidth, buttonHeight, darkmode)
        {
            PreviewScale = previewScale,
            ImagePath = imagePath,
            TitleText = labelTitle
        };

        if (!string.IsNullOrEmpty(description)) button.AuthorName = description;
        if (!string.IsNullOrEmpty(tooltip)) button.ToolTipText = tooltip;

        return button;
    }

    /// <summary>
    /// 起動時の引数から起動情報を取得します。
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    internal static LaunchInfo GetLaunchInfo(string url)
    {
        var uri = new Uri(url);
        var query = HttpUtility.ParseQueryString(uri.Query);

        var dir = query.GetValues("dir") ?? [];
        var id = query.Get("id") ?? "";

        return new LaunchInfo
        {
            LaunchedWithUrl = true,
            AssetDirs = dir,
            AssetId = id
        };
    }

    /// <summary>
    /// 検索用フィルターを取得します。
    /// </summary>
    /// <param name="searchWord"></param>
    /// <returns></returns>
    internal static SearchFilter GetSearchFilter(string searchWord)
    {
        var searchFilter = new SearchFilter();
        var regex = SearchFilterRegex();
        var matches = regex.Matches(searchWord);

        foreach (Match match in matches)
        {
            if (match.Groups["key"].Success)
            {
                var key = match.Groups["key"].Value;
                var value = match.Groups["value"].Value;

                switch (key)
                {
                    case "Author":
                        searchFilter.Authors.Add(value);
                        break;
                    case "Title":
                        searchFilter.Titles.Add(value);
                        break;
                    case "Booth":
                        searchFilter.BoothIds.Add(value);
                        break;
                    case "Avatar":
                        searchFilter.SupportedAvatars.Add(value);
                        break;
                    case "Category":
                        searchFilter.Categories.Add(value);
                        break;
                    case "Memo":
                        searchFilter.ItemMemos.Add(value);
                        break;
                    case "Folder":
                        searchFilter.FolderNames.Add(value);
                        break;
                    case "File":
                        searchFilter.FileNames.Add(value);
                        break;
                    case "Implemented":
                        searchFilter.ImplementedAvatars.Add(value);
                        break;
                    case "NotImplemented":
                        searchFilter.NotImplementedAvatars.Add(value);
                        break;
                    case "Tag":
                        searchFilter.Tags.Add(value);
                        break;
                    case "OR":
                        searchFilter.IsOrSearch = value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                        break;
                    case "BrokenItems":
                        searchFilter.BrokenItems = value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                        break;
                }
            }
            else if (match.Groups["word"].Success)
            {
                searchFilter.SearchWords.Add(match.Groups["word"].Value);
            }
        }

        return searchFilter;
    }

    /// <summary>
    /// 引数からスラッシュで区切られたパスを生成します。
    /// </summary>
    internal static string GenerateSeparatedPath(params string[] paths)
        => string.Join(" > ", paths);

    /// <summary>
    /// 与えられたタイトルから括弧を削除します。
    /// </summary>
    /// <param name="itemTitle"></param>
    /// <returns></returns>
    internal static string RemoveBrackets(string itemTitle)
        => BracketsRegex().Replace(itemTitle, "");

    /// <summary>
    /// DragEventで取得したパスの配列を返します。
    /// </summary>
    /// <param name="dragEventArgs"></param>
    /// <returns></returns>
    internal static string[] GetFileDropPaths(DragEventArgs dragEventArgs)
    {
        if (dragEventArgs.Data == null) return [];
        if (!dragEventArgs.Data.GetDataPresent(DataFormats.FileDrop)) return [];

        return (string[]?)dragEventArgs.Data.GetData(DataFormats.FileDrop, false) ?? [];
    }

    /// <summary>
    /// サムネイルを変更します。
    /// </summary>
    /// <param name="item"></param>
    /// <param name="currentLanguage"></param>
    /// <returns></returns>
    internal static bool ChangeThumbnail(Item item, string currentLanguage)
    {
        var previousPath = item.ImagePath;
        OpenFileDialog ofd = new()
        {
            Filter = LanguageUtils.Translate("画像ファイル|*.png;*.jpg", currentLanguage),
            Title = LanguageUtils.Translate("サムネイル変更", currentLanguage),
            Multiselect = false
        };
        if (ofd.ShowDialog() != DialogResult.OK) return false;

        item.ImagePath = ofd.FileName;

        FormUtils.ShowMessageBox(
            LanguageUtils.Translate("サムネイルを変更しました！", currentLanguage) + "\n\n" +
            LanguageUtils.Translate("変更前: ", currentLanguage) + "\n" + previousPath + "\n\n" +
            LanguageUtils.Translate("変更後: ", currentLanguage) + "\n" + ofd.FileName,
            LanguageUtils.Translate("完了", currentLanguage)
        );

        return true;
    }

    /// <summary>
    /// サムネイルを変更します。
    /// </summary>
    /// <param name="items"></param>
    /// <param name="author"></param>
    /// <param name="currentLanguage"></param>
    /// <returns></returns>
    internal static bool ChangeThumbnail(List<Item> items, Author author, string currentLanguage)
    {
        var previousPath = author.AuthorImagePath;
        OpenFileDialog ofd = new()
        {
            Filter = LanguageUtils.Translate("画像ファイル|*.png;*.jpg", currentLanguage),
            Title = LanguageUtils.Translate("サムネイル変更", currentLanguage),
            Multiselect = false
        };
        if (ofd.ShowDialog() != DialogResult.OK) return false;

        foreach (var item in items.Where(item => item.AuthorImageFilePath == previousPath))
        {
            item.AuthorImageFilePath = ofd.FileName;
        }

        FormUtils.ShowMessageBox(
            LanguageUtils.Translate("サムネイルを変更しました！", currentLanguage) + "\n\n" +
            LanguageUtils.Translate("変更前: ", currentLanguage) + "\n" + previousPath + "\n\n" +
            LanguageUtils.Translate("変更後: ", currentLanguage) + "\n" + ofd.FileName,
            "完了"
        );

        return true;
    }
}
