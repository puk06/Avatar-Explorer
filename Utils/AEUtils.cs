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

internal static class AEUtils
{
    private static readonly Timer _thumbnailUpdateTimer = new()
    {
        Interval = 200
    };
    private static object? _lastScrollSender;

    static AEUtils()
    {
        _thumbnailUpdateTimer.Tick += (s, e) =>
        {
            _thumbnailUpdateTimer.Stop();
            if (_lastScrollSender == null) return;
            UpdateExplorerThumbnails(_lastScrollSender);
        };
    }

    /// <summary>
    /// スクロールイベントを処理します。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal static void OnScroll(object sender, EventArgs e)
    {
        _lastScrollSender = sender;
        _thumbnailUpdateTimer.Stop();
        _thumbnailUpdateTimer.Start();
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
        ProgressForm progressForm = new(currentLanguage);
        progressForm.Show();

        try
        {
            progressForm.UpdateProgress(0, LanguageUtils.Translate("準備中", currentLanguage));
            var authorName = FileSystemUtils.CheckFilePath(currentPath.CurrentSelectedItem?.AuthorName ?? "Unknown");
            var itemTitle = FileSystemUtils.CheckFilePath(currentPath.CurrentSelectedItem?.Title ?? "Unknown");
            var category = ItemUtils.GetCategoryName(currentPath.CurrentSelectedCategory, currentLanguage, currentPath.CurrentSelectedCustomCategory);

            string saveFolder = Path.Combine("./Datas", "Temp", authorName, itemTitle);
            string saveFilePath = Path.Combine(saveFolder, $"{Path.GetFileNameWithoutExtension(file.FileName)}_export");
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }
            else if (Directory.Exists(saveFilePath))
            {
                Directory.Delete(saveFilePath, true);
            }

            var extractingStatus = LanguageUtils.Translate("ファイルの展開中", currentLanguage);
            progressForm.UpdateProgress(10, extractingStatus);

            // ファイル数を取得するための一時リスト
            var entries = new List<TarEntry>();
            await using (var fileStream = File.OpenRead(file.FilePath))
            await using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
            await using (var tarReader = new TarReader(gzipStream))
            {
                while (await tarReader.GetNextEntryAsync() is { } entry)
                {
                    entries.Add(entry);
                }
            }

            // 合計ファイル数
            int totalEntries = entries.Count;
            int processedEntries = 0;

            // 再度読み込みして処理
            await using var fileStream2 = File.OpenRead(file.FilePath);
            await using var gzipStream2 = new GZipStream(fileStream2, CompressionMode.Decompress);
            await using var tarReader2 = new TarReader(gzipStream2);

            while (await tarReader2.GetNextEntryAsync() is { } entry)
            {
                if (Path.GetFileName(entry.Name) == "pathname" && entry.DataStream != null)
                {
                    using StreamReader reader = new(entry.DataStream);
                    string assetPath = await reader.ReadToEndAsync();

                    assetPath = assetPath.Insert(7, $"{category}/");

                    entry.DataStream = new MemoryStream(Encoding.UTF8.GetBytes(assetPath));
                }

                var entryPath = Path.Combine(saveFilePath, entry.Name);
                if (entryPath.EndsWith('/'))
                {
                    Directory.CreateDirectory(entryPath);
                }
                else
                {
                    entry.DataStream ??= new MemoryStream();
                    await using var entryStream = File.Create(entryPath);
                    await entry.DataStream.CopyToAsync(entryStream);
                }

                // 進捗更新
                processedEntries++;
                int progress = 10 + (int)(80.0 * processedEntries / totalEntries);
                progressForm.UpdateProgress(progress, extractingStatus + ": " + processedEntries + "/" + totalEntries);
            }

            var unityPackagePath = saveFilePath + ".unitypackage";
            if (File.Exists(unityPackagePath)) File.Delete(unityPackagePath);

            progressForm.UpdateProgress(90, LanguageUtils.Translate("UnityPackageの作成中", currentLanguage));
            FileSystemUtils.CreateTarArchive(saveFilePath, unityPackagePath);

            Directory.Delete(saveFilePath, true);
            progressForm.UpdateProgress(100, LanguageUtils.Translate("完了", currentLanguage));

            Process.Start(new ProcessStartInfo()
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

            Process.Start(new ProcessStartInfo()
            {
                FileName = file.FilePath,
                UseShellExecute = true
            });
        }
        finally
        {
            progressForm.Close();
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
            if (senderObject is TabPage tabPage)
            {
                var visibleArea = new Rectangle(0, tabPage.VerticalScroll.Value,
                    tabPage.ClientSize.Width, tabPage.ClientSize.Height);

                foreach (Control control in tabPage.Controls)
                {
                    if (control is CustomItemButton button)
                    {
                        var buttonAbsoluteLocation = button.Location;
                        buttonAbsoluteLocation.Y += tabPage.VerticalScroll.Value;
                        button.CheckThmbnail(buttonAbsoluteLocation, button.Size, visibleArea);
                    }
                }
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
    /// <param name="imagePath"></param>
    /// <param name="labelTitle"></param>
    /// <param name="description"></param>
    /// <param name="short"></param>
    /// <param name="tooltip"></param>
    /// <param name="listWidthDiff"></param>
    /// <returns></returns>
    internal static Button CreateButton(string? imagePath, string labelTitle, string? description, bool @short = false, string tooltip = "", int listWidthDiff = 0)
    {
        var buttonWidth = @short ? 303 : 874;
        if (listWidthDiff != 0) buttonWidth += listWidthDiff;
        CustomItemButton button = new(buttonWidth)
        {
            ImagePath = imagePath,
            TitleText = labelTitle
        };

        if (description != null) button.AuthorName = description;
        if (!string.IsNullOrEmpty(tooltip)) button.ToolTipText = tooltip;

        return button;
    }

    /// <summary>
    /// 文字列からスペースを削除し、アンダースコアに変換します。
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    internal static string RemoveFormat(string str) => str.Replace(' ', '_').Replace('/', '-');


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
            launchedWithUrl = true,
            assetDirs = dir,
            assetId = id
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
        var regex = new Regex(@"(?<key>Author|Title|Booth|Avatar|Category|Memo|Folder|File|Implemented)=(?:""(?<value>.*?)""|(?<value>[^\s]+))|(?<word>[^\s]+)");
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
                        searchFilter.Author = searchFilter.Author.Append(value).ToArray();
                        break;
                    case "Title":
                        searchFilter.Title = searchFilter.Title.Append(value).ToArray();
                        break;
                    case "Booth":
                        searchFilter.BoothId = searchFilter.BoothId.Append(value).ToArray();
                        break;
                    case "Avatar":
                        searchFilter.Avatar = searchFilter.Avatar.Append(value).ToArray();
                        break;
                    case "Category":
                        searchFilter.Category = searchFilter.Category.Append(value).ToArray();
                        break;
                    case "Memo":
                        searchFilter.ItemMemo = searchFilter.ItemMemo.Append(value).ToArray();
                        break;
                    case "Folder":
                        searchFilter.FolderName = searchFilter.FolderName.Append(value).ToArray();
                        break;
                    case "File":
                        searchFilter.FileName = searchFilter.FileName.Append(value).ToArray();
                        break;
                    case "Implemented":
                        searchFilter.ImplementedAvatars = searchFilter.ImplementedAvatars.Append(value).ToArray();
                        break;

                }
            }
            else if (match.Groups["word"].Success)
            {
                searchFilter.SearchWords = searchFilter.SearchWords.Append(match.Groups["word"].Value).ToArray();
            }
        }

        return searchFilter;
    }

    /// <summary>
    /// 引数からスラッシュで区切られたパスを生成します。
    /// </summary>
    internal static string GenerateSeparatedPath(params string[] paths)
        => string.Join(" / ", paths);
}
