using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace Avatar_Explorer.Classes
{
    public class Helper
    {
        private static readonly HttpClient HttpClient = new();
        private static readonly Dictionary<string, Dictionary<string, string>> TranslateData = new();
        private static readonly string REG_PROTCOL = "VRCAE";
        private static readonly string SCHEME_FILE_PATH = "./Datas/VRCAESCHEME.txt";

        /// <summary>
        ///　Boothのアイテム情報を取得します。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<Item> GetBoothItemInfoAsync(string id)
        {
            var url = $"https://booth.pm/ja/items/{id}.json";
            var response = await HttpClient.GetStringAsync(url);
            var json = JObject.Parse(response);

            var title = json["name"]?.ToString() ?? "";
            var author = json["shop"]?["name"]?.ToString() ?? "";
            var authorUrl = json["shop"]?["url"]?.ToString() ?? "";
            var imageUrl = json["images"]?.Count() > 0 ? json["images"]?[0]?["original"]?.ToString() ?? "" : "";
            var authorIcon = json["shop"]?["thumbnail_url"]?.ToString() ?? "";
            var authorId = GetAuthorId(authorUrl);
            var category = json["category"]?["name"]?.ToString() ?? "";
            var estimatedCategory = GetItemType(title, category);

            return new Item
            {
                Title = title,
                AuthorName = author,
                ThumbnailUrl = imageUrl,
                AuthorImageUrl = authorIcon,
                AuthorId = authorId,
                Type = estimatedCategory
            };
        }

        private static string GetAuthorId(string url)
        {
            var match = Regex.Match(url, @"https://(.*)\.booth\.pm/");
            return match.Success ? match.Groups[1].Value : "";
        }

        /// <summary>
        /// 翻訳されたカテゴリー名を取得します。
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string GetCategoryName(ItemType itemType, string lang, string customCategory = "")
        {
            return itemType switch
            {
                ItemType.Avatar => Translate("アバター", lang),
                ItemType.Clothing => Translate("衣装", lang),
                ItemType.Texture => Translate("テクスチャ", lang),
                ItemType.Gimmick => Translate("ギミック", lang),
                ItemType.Accessory => Translate("アクセサリー", lang),
                ItemType.HairStyle => Translate("髪型", lang),
                ItemType.Animation => Translate("アニメーション", lang),
                ItemType.Tool => Translate("ツール", lang),
                ItemType.Shader => Translate("シェーダー", lang),
                ItemType.Custom => customCategory,
                _ => Translate("不明", lang)
            };
        }

        /// <summary>
        /// 指定されたパスからアイテムフォルダー情報を取得します。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="materialPath"></param>
        /// <returns></returns>
        public static ItemFolderInfo GetItemFolderInfo(string path, string materialPath)
        {
            var itemFolderInfo = new ItemFolderInfo();
            if (!Directory.Exists(path)) return itemFolderInfo;
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                var item = new FileData
                {
                    FileName = Path.GetFileName(file),
                    FilePath = file
                };

                switch (extension)
                {
                    case ".psd":
                    case ".clip":
                    case ".blend":
                    case ".fbx":
                        itemFolderInfo.ModifyFiles = itemFolderInfo.ModifyFiles.Append(item).ToArray();
                        break;
                    case ".png":
                    case ".jpg":
                        itemFolderInfo.TextureFiles = itemFolderInfo.TextureFiles.Append(item).ToArray();
                        break;
                    case ".txt":
                    case ".md":
                    case ".pdf":
                        itemFolderInfo.DocumentFiles = itemFolderInfo.DocumentFiles.Append(item).ToArray();
                        break;
                    case ".unitypackage":
                        itemFolderInfo.UnityPackageFiles = itemFolderInfo.UnityPackageFiles.Append(item).ToArray();
                        break;
                    default:
                        itemFolderInfo.UnkownFiles = itemFolderInfo.UnkownFiles.Append(item).ToArray();
                        break;
                }
            }

            if (string.IsNullOrEmpty(materialPath)) return itemFolderInfo;

            var materialFiles = Directory.GetFiles(materialPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in materialFiles)
            {
                var item = new FileData
                {
                    FileName = Path.GetFileName(file),
                    FilePath = file
                };

                itemFolderInfo.MaterialFiles = itemFolderInfo.MaterialFiles.Append(item).ToArray();
            }

            return itemFolderInfo;
        }

        /// <summary>
        ///　渡された情報からアイテム用のボタンを生成します。
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="labelTitle"></param>
        /// <param name="description"></param>
        /// <param name="short"></param>
        /// <param name="tooltip"></param>
        /// <param name="listWidthDiff"></param>
        /// <returns></returns>
        public static Button CreateButton(string? imagePath, string labelTitle, string? description,
            bool @short = false, string tooltip = "", int listWidthDiff = 0)
        {
            var buttonWidth = @short ? 303 : 874;
            if (listWidthDiff != 0)
                buttonWidth += listWidthDiff;
            CustomItemButton button = new CustomItemButton(buttonWidth);

            if (imagePath == null)
            {
                button.Picture = SharedImages.GetImage(SharedImages.Images.FolderIcon);
            }
            else
            {
                button.Picture = File.Exists(imagePath)
                    ? ResizeImage(imagePath, 100, 100)
                    : SharedImages.GetImage(SharedImages.Images.FileIcon);
            }

            button.ImagePath = imagePath;
            button.TitleText = labelTitle;
            if (description != null)
                button.AuthorName = description;
            if (!string.IsNullOrEmpty(tooltip))
                button.ToolTipText = tooltip;

            return button;
        }

        /// <summary>
        /// アイテムのデフォルトタイプを推測、取得します。
        /// </summary>
        /// <param name="title"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ItemType GetItemType(string title, string type)
        {
            var titleMappings = new Dictionary<string[], ItemType>
            {
                { new[] { "オリジナル3Dモデル", "オリジナル", "Avatar", "Original" }, ItemType.Avatar },
                { new[] { "アニメーション", "Animation" }, ItemType.Animation },
                { new[] { "衣装", "Clothing" }, ItemType.Clothing },
                { new[] { "ギミック", "Gimmick" }, ItemType.Gimmick },
                { new[] { "アクセサリ", "Accessory" }, ItemType.Accessory },
                { new[] { "髪", "Hair" }, ItemType.HairStyle },
                { new[] { "テクスチャ", "Eye", "Texture" }, ItemType.Texture },
                { new[] { "ツール", "システム", "Tool", "System" }, ItemType.Tool },
                { new[] { "シェーダー", "Shader" }, ItemType.Shader }
            };

            var suggestType = type switch
            {
                "3Dキャラクター" => ItemType.Avatar,
                "3Dモデル（その他）" => ItemType.Avatar,
                "3Dモーション・アニメーション" => ItemType.Animation,
                "3D衣装" => ItemType.Clothing,
                "3D小道具" => ItemType.Gimmick,
                "3D装飾品" => ItemType.Accessory,
                "3Dテクスチャ" => ItemType.Texture,
                "3Dツール・システム" => ItemType.Tool,
                _ => ItemType.Unknown
            };

            foreach (var mapping in titleMappings)
            {
                if (mapping.Key.Any(title.Contains))
                {
                    return mapping.Value;
                }
            }

            return suggestType;
        }

        /// <summary>
        /// 文字列からスペースを削除し、アンダースコアに変換します。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveFormat(string str) => str.Replace(' ', '_').Replace('/', '-');

        /// <summary>
        /// 指定されたパスからアイテムデータを取得します。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Item[] LoadItemsData(string path = "./Datas/ItemsData.json")
        {
            try
            {
                if (!File.Exists(path)) return Array.Empty<Item>();
                using var sr = new StreamReader(path);
                var data = JsonSerializer.Deserialize<Item[]>(sr.ReadToEnd());
                return data ?? Array.Empty<Item>();
            }
            catch
            {
                return Array.Empty<Item>();
            }
        }

        /// <summary>
        /// アイテムデータを保存します。
        /// </summary>
        /// <param name="items"></param>
        public static void SaveItemsData(Item[] items)
        {
            using var sw = new StreamWriter("./Datas/ItemsData.json");
            sw.Write(JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>
        /// 指定されたパスから共通素体データを取得します。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static CommonAvatar[] LoadCommonAvatarData(string path = "./Datas/CommonAvatar.json")
        {
            try
            {
                if (!File.Exists(path)) return Array.Empty<CommonAvatar>();
                using var sr = new StreamReader(path);
                var data = JsonSerializer.Deserialize<CommonAvatar[]>(sr.ReadToEnd());
                return data ?? Array.Empty<CommonAvatar>();
            }
            catch
            {
                return Array.Empty<CommonAvatar>();
            }
        }

        /// <summary>
        /// 共通素体データを保存します。
        /// </summary>
        /// <param name="commonAvatars"></param>
        public static void SaveCommonAvatarData(CommonAvatar[] commonAvatars)
        {
            using var sw = new StreamWriter("./Datas/CommonAvatar.json");
            sw.Write(JsonSerializer.Serialize(commonAvatars, new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>
        /// DragEnterイベントを処理します。
        /// </summary>
        /// <param name="_"></param>
        /// <param name="e"></param>
        public static void DragEnter(object _, DragEventArgs e) => e.Effect = DragDropEffects.All;

        /// <summary>
        /// 画像を指定されたサイズにリサイズします。
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private static Bitmap ResizeImage(string imagePath, int width, int height)
        {
            if (!File.Exists(imagePath)) return new Bitmap(width, height);
            using var originalImage = Image.FromFile(imagePath);
            var resizedImage = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(resizedImage);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(originalImage, 0, 0, width, height);
            return resizedImage;
        }

        /// <summary>
        /// 文字列を指定された言語に翻訳します。なければそのまま返します。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static string Translate(string str, string to)
        {
            if (to == "ja-JP") return str;
            if (!File.Exists($"./Translate/{to}.json")) return str;
            var data = GetTranslateData(to);
            return data.TryGetValue(str, out var translated) ? translated : str;
        }

        /// <summary>
        /// 翻訳データを取得します。
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetTranslateData(string lang)
        {
            if (TranslateData.TryGetValue(lang, out var data)) return data;
            var json = File.ReadAllText(($"./Translate/{lang}.json"));
            var translateData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (translateData == null) return new Dictionary<string, string>();
            TranslateData.Add(lang, translateData);
            return translateData;
        }

        /// <summary>
        /// 対応アバターのパスを修正します(前のバージョンからの移行用)。
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static Item[] FixSupportedAvatarPath(Item[] items)
        {
            var avatars = items.Where(x => x.Type == ItemType.Avatar).ToArray();
            foreach (var item in items)
            {
                if (item.SupportedAvatar.Length == 0) continue;
                foreach (var supportedAvatar in item.SupportedAvatar)
                {
                    var avatar = avatars.FirstOrDefault(x => x.Title == supportedAvatar);
                    if (avatar == null) continue;
                    item.SupportedAvatar = item.SupportedAvatar.Where(x => x != supportedAvatar).Append(avatar.ItemPath)
                        .ToArray();
                }
            }

            return items;
        }

        /// <summary>
        ///　指定されたパスからアバター名を取得します。なければnullを返します。
        /// </summary>
        /// <param name="items"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string? GetAvatarName(Item[] items, string? path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            items = items.Where(x => x.Type == ItemType.Avatar).ToArray();
            var item = items.FirstOrDefault(x => x.ItemPath == path);
            return item?.Title;
        }

        /// <summary>
        ///　指定されたパスからアバター名を取得します。なければ空文字を返します。
        /// </summary>
        /// <param name="items"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetAvatarNameFromPath(Item[] items, string? path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            var item = items.FirstOrDefault(x => x.ItemPath == path);
            return item?.Title ?? "";
        }

        /// <summary>
        /// 指定されたアイテムがタウ王アバターかどうか、共通素体グループに入っているかどうかを取得します。
        /// </summary>
        /// <param name="item"></param>
        /// <param name="commonAvatars"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static SupportedOrCommonAvatar IsSupportedAvatarOrCommon(Item item, CommonAvatar[] commonAvatars,
            string? path)
        {
            if (string.IsNullOrEmpty(path)) return new SupportedOrCommonAvatar();
            if (item.SupportedAvatar.Contains(path)) return new SupportedOrCommonAvatar { IsSupported = true };

            if (item.Type != ItemType.Clothing) return new SupportedOrCommonAvatar();
            var commonAvatarsArray = commonAvatars.Where(x => x.Avatars.Contains(path)).ToArray();
            var commonAvatarBool = item.SupportedAvatar.Any(supportedAvatar =>
                commonAvatarsArray.Any(x => x.Avatars.Contains(supportedAvatar)));

            if (!commonAvatarBool) return new SupportedOrCommonAvatar();
            {
                var commonAvatar = item.SupportedAvatar.Select(supportedAvatar =>
                        commonAvatarsArray.FirstOrDefault(x => x.Avatars.Contains(supportedAvatar)))
                    .FirstOrDefault(x => x != null);
                return new SupportedOrCommonAvatar
                {
                    IsCommon = true,
                    CommonAvatarName = commonAvatar?.Name ?? ""
                };
            }
        }

        /// <summary>
        /// 対応アバターかどうか、共通素体に含まれているか、どちらかを取得します。
        /// </summary>
        public class SupportedOrCommonAvatar
        {
            /// <summary>
            /// 対応アバターかどうかを取得または設定します。
            /// </summary>
            public bool IsSupported { get; set; }

            /// <summary>
            /// 共通素体グループに含まれているかどうかを取得または設定します。
            /// </summary>
            public bool IsCommon { get; set; }

            /// <summary>
            /// 対応アバターか共通素体に含まれているかどうかを取得します。
            /// </summary>
            public bool IsSupportedOrCommon => IsSupported || IsCommon;

            /// <summary>
            /// 共通素体に含まれているが対応アバターではないかどうかを取得します。
            /// </summary>
            public bool OnlyCommon => IsCommon && !IsSupported;

            /// <summary>
            /// もし共通素体グループに入っていれば、そのグループの名前を取得または設定します。
            /// </summary>
            public string CommonAvatarName { get; set; } = "";
        }

        /// <summary>
        /// 検索用フィルターを取得します。
        /// </summary>
        /// <param name="searchWord"></param>
        /// <returns></returns>
        public static SearchFilter GetSearchFilter(string searchWord)
        {
            var searchFilter = new SearchFilter();
            var regex = new Regex(@"(?<key>Author|Title|Booth|Avatar|Category|Memo)=(?:""(?<value>.*?)""|(?<value>[^\s]+))|(?<word>[^\s]+)");
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
        ///　指定されたファイルをバックアップします。
        /// </summary>
        /// <param name="path"></param>
        public static void Backup(string[] path)
        {
            var folderPath = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

            foreach (var p in path)
            {
                if (!File.Exists(p)) continue;
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var backupPath = Path.Combine(appDataPath, "Avatar Explorer", "Backup");
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                var backupFolderPath = Path.Combine(backupPath, folderPath);
                if (!Directory.Exists(backupFolderPath))
                {
                    Directory.CreateDirectory(backupFolderPath);
                }

                File.WriteAllText(Path.Combine(backupFolderPath, Path.GetFileName(p)), File.ReadAllText(p));
            }
        }

        /// <summary>
        /// 言語名からBoothのリンクの言語コードを取得します。
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string GetCurrentLanguageCode(string language = "")
        {
            return language switch
            {
                "ja-JP" => "ja",
                "ko-KR" => "ko",
                "en-US" => "en",
                _ => "ja"
            };
        }

        /// <summary>
        /// 渡されたエラーを記録します。
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void ErrorLogger(string message, Exception exception)
        {
            try
            {
                var currentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                File.AppendAllText("./ErrorLog.txt",
                    currentTime + " - " + message + "\n" + exception + "\n\n");
            }
            catch
            {
                Console.WriteLine("Failed to write error log.");
            }
        }

        /// <summary>
        /// ファイル名が正常かどうかをチェックします。あれば正常なファイル名を返します。
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string CheckFilePath(string filePath)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Concat(filePath.Where(c => !invalidChars.Contains(c)));
        }

        /// <summary>
        /// パスを変更したUnityPackageファイルを作成します。
        /// </summary>
        /// <param name="file"></param>
        /// <param name="currentPath"></param>
        /// <param name="currentLanguage"></param>
        /// <returns></returns>
        public static async Task ModifyUnityPackageFilePathAsync(FileData file, CurrentPath currentPath, string currentLanguage)
        {
            ProgressForm progressForm = new ProgressForm(currentLanguage);
            progressForm.Show();

            try
            {
                progressForm.UpdateProgress(0, Translate("準備中", currentLanguage));
                var authorName = CheckFilePath(currentPath.CurrentSelectedItem?.AuthorName ?? "Unknown");
                var itemTitle = CheckFilePath(currentPath.CurrentSelectedItem?.Title ?? "Unknown");
                var category = GetCategoryName(currentPath.CurrentSelectedCategory, currentLanguage, currentPath.CurrentSelectedCustomCategory);

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

                var extractingStatus = Translate("ファイルの展開中", currentLanguage);
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
                        using StreamReader reader = new StreamReader(entry.DataStream);
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

                progressForm.UpdateProgress(90, Translate("UnityPackageの作成中", currentLanguage));
                CreateTarArchive(saveFilePath, unityPackagePath);

                Directory.Delete(saveFilePath, true);
                progressForm.UpdateProgress(100, Translate("完了", currentLanguage));

                Process.Start(new ProcessStartInfo()
                {
                    FileName = unityPackagePath,
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                ErrorLogger("UnityPackageの展開に失敗しました。", ex);
                MessageBox.Show(
                    Translate("UnityPackageの展開に失敗しました。詳細はErrorLog.txtをご覧ください。", currentLanguage),
                    Translate("エラー", currentLanguage),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
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
        /// 進捗状況を表示するフォーム
        /// </summary>
        private sealed class ProgressForm : Form
        {
            private readonly ProgressBar _progressBar;
            private readonly Label _progressLabel;
            private readonly string _formTitle;

            public ProgressForm(string currentLanguage)
            {
                _formTitle = Translate("Unitypackageのインポート先の変更中", currentLanguage);
                Text = _formTitle;
                Size = new Size(400, 90);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterScreen;
                MaximizeBox = false;
                MinimizeBox = false;

                _progressBar = new ProgressBar
                {
                    Dock = DockStyle.Top,
                    Style = ProgressBarStyle.Continuous,
                    Minimum = 0,
                    Maximum = 100
                };

                _progressLabel = new Label
                {
                    Dock = DockStyle.Top,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = "0%",
                    AutoSize = false,
                    Height = 20
                };

                Controls.Add(_progressBar);
                Controls.Add(_progressLabel);
            }

            public void UpdateProgress(int percentage, string message = "")
            {
                if (InvokeRequired)
                {
                    Invoke(() => UpdateProgress(percentage, message));
                    return;
                }

                _progressBar.Value = percentage;
                _progressLabel.Text = $"{percentage}% {message}";
                Text = $"{_formTitle} - {percentage}%";
            }
        }

        /// <summary>
        /// Tarファイルを作成します。
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="outputTarFile"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        private static void CreateTarArchive(string sourceFolder, string outputTarFile)
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
        /// 指定されたパスからカスタムカテゴリーデータを取得します。
        /// </summary>
        /// <returns></returns>
        public static string[] LoadCustomCategoriesData(string path = "./Datas/CustomCategory.txt")
        {
            if (!File.Exists(path))
            {
                File.Create(path).Close();
                return Array.Empty<string>();
            }

            var categories = File.ReadAllLines(path, Encoding.UTF8);
            categories = categories.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            return categories;
        }

        /// <summary>
        /// カスタムカテゴリーデータを保存します。
        /// </summary>
        /// <param name="customCategories"></param>
        public static void SaveCustomCategoriesData(string[] customCategories)
        {
            using var sw = new StreamWriter("./Datas/CustomCategory.txt", false, Encoding.UTF8);
            foreach (var category in customCategories)
            {
                sw.WriteLine(category);
            }
        }

        /// <summary>
        /// zipファイルを指定されたフォルダに展開します。
        /// </summary>
        /// <param name="zipPath"></param>
        /// <param name="extractPath"></param>
        /// <returns></returns>
        public static string ExtractZip(string zipPath, string extractPath, string folderName)
        {
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                var invalidChars = Path.GetInvalidFileNameChars();
                folderName = string.Concat(folderName.Where(c => !invalidChars.Contains(c)));
                extractPath = Path.Combine(extractPath, folderName);
            }

            var extractFolder = Path.Combine(extractPath, Path.GetFileNameWithoutExtension(zipPath));
            if (!Directory.Exists(extractFolder))
            {
                Directory.CreateDirectory(extractFolder);
            }
            else
            {
                int i = 2;
                while (Directory.Exists(extractFolder + i))
                {
                    i++;
                }
                extractFolder += i;
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
        public static void CopyDirectory(string sourceDirName, string destDirName)
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
                file.CopyTo(temppath, false);
            }

            var dirs = dir.GetDirectories();
            foreach (var subdir in dirs)
            {
                var temppath = Path.Combine(destDirName, subdir.Name);
                CopyDirectory(subdir.FullName, temppath);
            }
        }

        /// <summary>
        /// 文字列をCSV形式にエスケープします。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EscapeCsv(string value)
        {
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }

        /// <summary>
        /// 空の登録日時と更新日時を現在の日時で埋めます。
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static Item[] UpdateEmptyDates(Item[] items)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.CreatedDate))
                {
                    item.CreatedDate = now;
                }

                if (string.IsNullOrEmpty(item.UpdatedDate))
                {
                    item.UpdatedDate = now;
                }
            }

            return items;
        }

        public static Item[] FixCurrentPathEscape(Item[] items)
        {
            foreach (var item in items)
            {
                item.ItemPath = item.ItemPath.Replace("./Datas", "Datas").Replace("/", "\\");
                item.MaterialPath = item.MaterialPath.Replace("./Datas", "Datas").Replace("/", "\\");
                item.ImagePath = item.ImagePath.Replace("./Datas", "Datas").Replace("/", "\\");
                item.AuthorImageFilePath = item.AuthorImageFilePath.Replace("./Datas", "Datas").Replace("/", "\\");
            }

            return items;
        }

        /// <summary>
        /// 起動時の引数から起動情報を取得します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static LaunchInfo GetLaunchInfo(string url)
        {
            var uri = new Uri(url);
            var query = HttpUtility.ParseQueryString(uri.Query);
            var dir = query.Get("dir") ?? "";
            var id = query.Get("id") ?? "";

            return new LaunchInfo
            {
                launchedWithUrl = true,
                assetDir = dir,
                assetId = id
            };
        }

        /// <summary>
        /// カスタムURLスキームの登録用のヘルパー関数です。
        /// </summary>
        public static void CheckScheme()
        {
            var isSchemeRegistered = IsSchemeRegistered(REG_PROTCOL);
            if (!File.Exists(SCHEME_FILE_PATH) && !isSchemeRegistered)
            {
                var result = MessageBox.Show("カスタムURLスキームを登録しますか？\n\n" +
                                             "登録すると、ブラウザから「" + REG_PROTCOL + "://」でこのソフトを起動できます。\n" +
                                             "登録しない場合は、URLスキームでの起動はできませんが、通常の起動は可能です。",
                    "カスタムURLスキーム登録", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    string? exePath = Process.GetCurrentProcess()?.MainModule?.FileName;

                    if (exePath != null)
                    {
                        try
                        {
                            if (!IsRunAsAdmin())
                            {
                                var result2 = MessageBox.Show("カスタムURLスキームの登録には管理者権限が必要です。\n" +
                                                              "再起動して管理者権限で起動しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (result2 == DialogResult.Yes)
                                    RestartAsAdmin();
                                return;
                            }

                            RegisterCustomScheme(REG_PROTCOL, exePath);
                            File.WriteAllText(SCHEME_FILE_PATH, exePath);

                            var result3 = MessageBox.Show("カスタムURLスキームの登録に成功しました。\n" +
                                                           "ソフトを終了して、通常のユーザーとして起動することをおすすめします！\n\n" +
                                                              "終了しないと、ソフト内のD&Dなどが正常に動作しない場合があります。\n" +
                                                              "終了しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            if (result3 == DialogResult.Yes)
                                Environment.Exit(0);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("カスタムURLスキームの登録に失敗しました。\n\n" + ex,
                                "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("カスタムURLスキームの登録をスキップしました。\nもし登録したければ、Datasフォルダ内のVRCAESCHEME.txtを削除してもう一度起動してください！", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    File.WriteAllText(SCHEME_FILE_PATH, "false");
                }
            }
            else if (!File.Exists(SCHEME_FILE_PATH) && isSchemeRegistered)
            {
                var result = MessageBox.Show("カスタムURLスキームは既に登録されていますが、ソフト内の登録先が不明です。再登録しますか？\n" +
                                             "再登録を行うことで、誤って前のバージョンが参照されたりすることを防げます。\n\n" +
                                             "登録すると、ブラウザから「" + REG_PROTCOL + "://」でこのソフトを起動できます。\n" +
                                             "登録しない場合は、URLスキームでの起動はできませんが、通常の起動は可能です。",
                    "カスタムURLスキーム登録", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    string? exePath = Process.GetCurrentProcess()?.MainModule?.FileName;
                    if (exePath != null)
                    {
                        try
                        {
                            if (!IsRunAsAdmin())
                            {
                                var result2 = MessageBox.Show("カスタムURLスキームの登録には管理者権限が必要です。\n" +
                                                              "再起動して管理者権限で起動しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (result2 == DialogResult.Yes)
                                    RestartAsAdmin();
                                return;
                            }

                            RegisterCustomScheme(REG_PROTCOL, exePath);
                            File.WriteAllText(SCHEME_FILE_PATH, exePath);

                            var result3 = MessageBox.Show("カスタムURLスキームの登録に成功しました。\n" +
                                                           "ソフトを終了して、通常のユーザーとして起動することをおすすめします！\n\n" +
                                                              "終了しないと、ソフト内のD&Dなどが正常に動作しない場合があります。\n" +
                                                              "終了しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            if (result3 == DialogResult.Yes)
                                Environment.Exit(0);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("カスタムURLスキームの登録に失敗しました。\n\n" + ex,
                                "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("カスタムURLスキームの再登録をスキップしました。\nもしもう一度登録したければ、Datasフォルダ内のVRCAESCHEME.txtを削除してもう一度起動してください！", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    File.WriteAllText(SCHEME_FILE_PATH, "false");
                }
            }
            else
            {
                string path = File.ReadAllText(SCHEME_FILE_PATH);

                string? exePath = Process.GetCurrentProcess()?.MainModule?.FileName;

                if (path != "false" && exePath != null && path != exePath)
                {
                    var result = MessageBox.Show("カスタムURLスキームの登録先が変更されているため、再登録しますか？\n\n" +
                                                 "登録すると、ブラウザから「" + REG_PROTCOL + "://」でこのソフトを起動できます。\n" +
                                                 "登録しない場合は、URLスキームでの起動はできませんが、通常の起動は可能です。",
                        "カスタムURLスキーム登録", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            if (!IsRunAsAdmin())
                            {
                                var result2 = MessageBox.Show("カスタムURLスキームの登録には管理者権限が必要です。\n" +
                                                              "再起動して管理者権限で起動しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (result2 == DialogResult.Yes)
                                    RestartAsAdmin();
                                return;
                            }

                            RegisterCustomScheme(REG_PROTCOL, exePath);
                            File.WriteAllText(SCHEME_FILE_PATH, exePath);

                            var result3 = MessageBox.Show("カスタムURLスキームの登録に成功しました。\n" +
                                                           "ソフトを終了して、通常のユーザーとして起動することをおすすめします！\n\n" +
                                                              "終了しないと、ソフト内のD&Dなどが正常に動作しない場合があります。\n" +
                                                              "終了しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            if (result3 == DialogResult.Yes)
                                Environment.Exit(0);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("カスタムURLスキームの登録に失敗しました。\n\n" + ex,
                                "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else if (result == DialogResult.No)
                    {
                        MessageBox.Show("カスタムURLスキームの再登録をスキップしました。\nもしもう一度登録したければ、Datasフォルダ内のVRCAESCHEME.txtを削除してもう一度起動してください！", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        File.WriteAllText(SCHEME_FILE_PATH, "false");
                    }
                }
            }
        }

        /// <summary>
        /// カスタムスキームを登録します。
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="exePath"></param>
        private static void RegisterCustomScheme(string protocol, string exePath)
        {
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(protocol))
            {
                key.SetValue("", "URL:" + protocol + " Protocol");
                key.SetValue("URL Protocol", "");
            }

            string commandKey = $@"{protocol}\shell\open\command";
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(commandKey))
            {
                key.SetValue("", $"\"{exePath}\" \"%1\"");
            }
        }

        /// <summary>
        /// 既にカスタムスキームが登録されているかどうかを取得します。
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        private static bool IsSchemeRegistered(string protocol)
        {
            using RegistryKey? key = Registry.ClassesRoot.OpenSubKey(protocol);
            return key != null;
        }

        /// <summary>
        /// ソフトを管理者権限で起動しているかどうかを取得します。
        /// </summary>
        /// <returns></returns>
        private static bool IsRunAsAdmin()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// 管理者権限で再起動します。
        /// </summary>
        private static void RestartAsAdmin()
        {
            var exePath = Process.GetCurrentProcess()?.MainModule?.FileName;
            if (string.IsNullOrEmpty(exePath))
            {
                MessageBox.Show("再起動に失敗しました。手動で管理者としてソフトを実行してください！", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProcessStartInfo proc = new()
            {
                FileName = exePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(proc);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("再起動に失敗しました。手動で管理者としてソフトを実行してください。\n" + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
    }
}