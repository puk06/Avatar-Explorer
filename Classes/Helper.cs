using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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

        public static async Task<Item> GetBoothItemInfoAsync(string id)
        {
            var url = $"https://booth.pm/ja/items/{id}.json";
            var response = await HttpClient.GetStringAsync(url);
            var json = JObject.Parse(response);

            var title = json["name"]?.ToString() ?? "";
            var author = json["shop"]?["name"]?.ToString() ?? "";
            var authorUrl = json["shop"]?["url"]?.ToString() ?? "";
            var imageUrl = json["images"]?[0]?["original"]?.ToString() ?? "";
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
            var match = Regex.Match(url, @"https://(.*).booth.pm/");
            return match.Success ? match.Groups[1].Value : "";
        }

        public static string GetCategoryName(ItemType itemType, string lang)
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
                _ => Translate("不明", lang)
            };
        }

        public static ItemFolderInfo GetItemFolderInfo(string path, string materialPath)
        {
            var itemFolderInfo = new ItemFolderInfo();
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

        public static string RemoveFormat(string str) => str.Replace(' ', '_').Replace('/', '-');

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

        public static void SaveItemsData(Item[] items)
        {
            using var sw = new StreamWriter("./Datas/ItemsData.json");
            sw.Write(JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true }));
        }

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

        public static void SaveCommonAvatarData(CommonAvatar[] commonAvatars)
        {
            using var sw = new StreamWriter("./Datas/CommonAvatar.json");
            sw.Write(JsonSerializer.Serialize(commonAvatars, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static void DragEnter(object _, DragEventArgs e) => e.Effect = DragDropEffects.All;

        private static Image ResizeImage(string imagePath, int width, int height)
        {
            if (!File.Exists(imagePath)) return new Bitmap(width, height);
            using var originalImage = Image.FromFile(imagePath);
            var resizedImage = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(resizedImage);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(originalImage, 0, 0, width, height);
            return resizedImage;
        }

        public static string Translate(string str, string to)
        {
            if (to == "ja-JP") return str;
            if (!File.Exists($"./Translate/{to}.json")) return str;
            var data = GetTranslateData(to);
            return data.TryGetValue(str, out var translated) ? translated : str;
        }

        private static Dictionary<string, string> GetTranslateData(string lang)
        {
            if (TranslateData.TryGetValue(lang, out var data)) return data;
            var json = File.ReadAllText(($"./Translate/{lang}.json"));
            var translateData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (translateData == null) return new Dictionary<string, string>();
            TranslateData.Add(lang, translateData);
            return translateData;
        }

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

        public static string? GetAvatarName(Item[] items, string? path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            items = items.Where(x => x.Type == ItemType.Avatar).ToArray();
            var item = items.FirstOrDefault(x => x.ItemPath == path);
            return item?.Title;
        }

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

        public class SupportedOrCommonAvatar
        {
            public bool IsSupported { get; set; }
            public bool IsCommon { get; set; }
            public bool IsSupportedOrCommon => IsSupported || IsCommon;
            public bool OnlyCommon => IsCommon && !IsSupported;
            public string CommonAvatarName { get; set; } = "";
        }

        public static SearchFilter GetSearchFilter(string searchWord)
        {
            var searchFilter = new SearchFilter();
            var regex = new Regex(@"(?<key>Author|Title|Booth)=(?:""(?<value>.*?)""|(?<value>[^\s]+))|(?<word>[^\s]+)");
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
                    }
                }
                else if (match.Groups["word"].Success)
                {
                    searchFilter.SearchWords = searchFilter.SearchWords.Append(match.Groups["word"].Value).ToArray();
                }
            }

            return searchFilter;
        }

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

        public static string CheckFilePath(string s)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Concat(s.Where(c => !invalidChars.Contains(c)));
        }

        public static async Task ModifyUnityPackageFilePathAsync(FileData file, CurrentPath currentPath,
            string currentLanguage)
        {
            ProgressForm progressForm = new ProgressForm(currentLanguage);
            progressForm.Show();

            try
            {
                progressForm.UpdateProgress(0, Translate("準備中", currentLanguage));
                var authorName = CheckFilePath(currentPath.CurrentSelectedItem?.AuthorName ?? "Unknown");
                var itemTitle = CheckFilePath(currentPath.CurrentSelectedItem?.Title ?? "Unknown");

                string saveFolder = Path.Combine("./Datas", "Temp", authorName, itemTitle);
                string saveFilePath = Path.Combine(saveFolder, $"{Path.GetFileNameWithoutExtension(file.FileName)}_export");
                if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);

                progressForm.UpdateProgress(10, Translate("ファイルの展開中", currentLanguage));
                await using var fileStream = File.OpenRead(file.FilePath);
                await using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                await using var tarReader = new TarReader(gzipStream);

                while (await tarReader.GetNextEntryAsync() is { } entry)
                {
                    if (Path.GetFileName(entry.Name) == "pathname" && entry.DataStream != null)
                    {
                        using StreamReader reader = new StreamReader(entry.DataStream);
                        string assetPath = await reader.ReadToEndAsync();

                        assetPath = assetPath.Insert(7,
                            $"{GetCategoryName(currentPath.CurrentSelectedCategory, currentLanguage)}/");

                        entry.DataStream = new MemoryStream(Encoding.UTF8.GetBytes(assetPath));
                    }

                    var entryPath = Path.Combine(saveFilePath, entry.Name);
                    if (entryPath.EndsWith("/"))
                    {
                        Directory.CreateDirectory(entryPath);
                    }
                    else
                    {
                        if (entry.DataStream == null) continue;
                        await using var entryStream = File.Create(entryPath);
                        await entry.DataStream.CopyToAsync(entryStream);
                    }
                }

                var unityPackagePath = saveFilePath + ".unitypackage";
                if (File.Exists(unityPackagePath)) File.Delete(unityPackagePath);

                progressForm.UpdateProgress(50, Translate("UnityPackageの作成中", currentLanguage));
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

        private sealed class ProgressForm : Form
        {
            private readonly ProgressBar _progressBar;
            private readonly Label _progressLabel;

            public ProgressForm(string currentLanguage)
            {
                Text = Helper.Translate("Unitypackageのインポート先の変更中", currentLanguage);
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
            }
        }

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
    }
}