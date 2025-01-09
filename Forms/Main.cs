using System.Diagnostics;
using System.Drawing.Text;
using System.IO.Compression;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using Avatar_Explorer.Classes;
using Timer = System.Windows.Forms.Timer;

namespace Avatar_Explorer.Forms
{
    public sealed partial class Main : Form
    {
        // Current Version
        private const string CurrentVersion = "v1.0.2";

        // Current Version Form Text
        private const string CurrentVersionFormText = $"VRChat Avatar Explorer {CurrentVersion} by ぷこるふ";

        // Min Resize Font Size
        private const float MinFontSize = 8f;

        // Backup Interval
        private const int BackupInterval = 300000; // 5 Minutes

        // Items Data
        public Item[] Items;

        // Common Avatars
        public CommonAvatar[] CommonAvatars;

        // Current Path
        public CurrentPath CurrentPath = new();

        // Font
        private readonly PrivateFontCollection _fontCollection = new();
        private readonly Dictionary<string, FontFamily> _fontFamilies = new();
        public FontFamily? GuiFont;

        // Language
        public string CurrentLanguage = "ja-JP";

        // Search Mode
        private bool _authorMode;
        private bool _categoryMode;

        private Window _openingWindow = Window.Nothing;

        // Dictionary For Resize Controls
        private readonly Dictionary<string, string> _controlNames = new();
        private readonly Dictionary<string, SizeF> _defaultControlSize = new();
        private readonly Dictionary<string, PointF> _defaultControlLocation = new();
        private readonly Dictionary<string, float> _defaultFontSize = new();

        // Initial Form, AvatarSearchFilterList, AvatarItemExplorer Size
        private readonly Size _initialFormSize;
        private readonly int _baseAvatarSearchFilterListWidth;
        private readonly int _baseAvatarItemExplorerListWidth;

        // For Resize Button
        private int GetAvatarListWidth() => AvatarSearchFilterList.Width - _baseAvatarSearchFilterListWidth;
        private int GetItemExplorerListWidth() => AvatarItemExplorer.Width - _baseAvatarItemExplorerListWidth;

        // Last Backup Time
        private DateTime _lastBackupTime;

        // Last Backup Error
        private bool _lastBackupError;

        // Searching Bool
        private bool _isSearching;

        // Initialized Bool
        private readonly bool _initialized;

        public Main()
        {
            try
            {
                Items = Helper.LoadItemsData();
                CommonAvatars = Helper.LoadCommonAvatarData();

                // Fix Supported Avatar Path (Title => Path)
                Items = Helper.FixSupportedAvatarPath(Items);

                AddFontFile();
                InitializeComponent();

                // Save the default Size
                _initialFormSize = ClientSize;
                _baseAvatarSearchFilterListWidth = AvatarSearchFilterList.Width;
                _baseAvatarItemExplorerListWidth = AvatarItemExplorer.Width;
                _initialized = true;

                // Render Window
                RefleshWindow();

                // Start AutoBackup
                AutoBackup();

                // Set Backup Title Loop
                BackupTimeTitle();

                Text = $"VRChat Avatar Explorer {CurrentVersion} by ぷこるふ";
            }
            catch (Exception ex)
            {
                MessageBox.Show("ソフトの起動中にエラーが発生しました。\n\n" + ex,
                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        private void AddFontFile()
        {
            string[] fontFiles = Directory.GetFiles("./Datas/Fonts", "*.ttf");
            foreach (var fontFile in fontFiles)
            {
                _fontCollection.AddFontFile(fontFile);
            }

            foreach (var fontFamily in _fontCollection.Families)
            {
                switch (fontFamily.Name)
                {
                    case "Noto Sans JP":
                        _fontFamilies.Add("ja-JP", fontFamily);
                        break;
                    case "Noto Sans":
                        _fontFamilies.Add("en-US", fontFamily);
                        break;
                    case "Noto Sans KR":
                        _fontFamilies.Add("ko-KR", fontFamily);
                        break;
                }
            }

            var newFont = _fontFamilies.TryGetValue(CurrentLanguage, out var family) ? family : _fontFamilies["ja-JP"];
            GuiFont = newFont;
        }

        // Generate List (LEFT)
        private void GenerateAvatarList()
        {
            AvatarPage.Controls.Clear();

            var items = Items.Where(item => item.Type == ItemType.Avatar).ToArray();
            if (items.Length == 0) return;
            items = items.OrderBy(item => item.Title).ToArray();

            var index = 0;
            foreach (Item item in items)
            {
                Button button = Helper.CreateButton(item.ImagePath, item.Title,
                    Helper.Translate("作者: ", CurrentLanguage) + item.AuthorName, true,
                    item.Title, GetAvatarListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath = new CurrentPath
                    {
                        CurrentSelectedAvatar = item.Title,
                        CurrentSelectedAvatarPath = item.ItemPath
                    };
                    _authorMode = false;
                    _categoryMode = false;
                    SearchBox.Text = "";
                    SearchResultLabel.Text = "";
                    _isSearching = false;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();

                if (item.BoothId != -1)
                {
                    ToolStripMenuItem toolStripMenuItem =
                        new(Helper.Translate("Boothリンクのコピー", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        try
                        {
                            Clipboard.SetText(
                                $"https://booth.pm/{Helper.GetCurrentLanguageCode(CurrentLanguage)}/items/" +
                                item.BoothId);
                        }
                        catch (Exception ex)
                        {
                            if (ex is ExternalException) return;
                            MessageBox.Show(Helper.Translate("クリップボードにコピーできませんでした", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Boothリンクを開く", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem1.Click += (_, _) =>
                    {
                        try
                        {
                            Process.Start($"https://booth.pm/{Helper.GetCurrentLanguageCode(CurrentLanguage)}/items/" +
                                          item.BoothId);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("リンクを開けませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("この作者の他のアイテムを表示", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                toolStripMenuItem2.Click += (_, _) =>
                {
                    SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                    SearchItems();
                };

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem3.Click += (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("画像ファイル|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("サムネイル変更", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("サムネイルを変更しました！", CurrentLanguage) + "\n\n" +
                        Helper.Translate("変更前: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                        Helper.Translate("変更後: ", CurrentLanguage) + ofd.FileName,
                        Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    item.ImagePath = ofd.FileName;

                    // もしアバターの欄を右で開いていたら、そのサムネイルも更新しないといけないため。
                    if (_openingWindow == Window.ItemList && !_isSearching) GenerateItems();

                    //検索中だと、検索画面を再読込してあげる
                    if (_isSearching) SearchItems();

                    GenerateAvatarList();
                    Helper.SaveItemsData(Items);
                };

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("編集", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem4.Click += (_, _) =>
                {
                    var prePath = item.ItemPath;
                    AddItem addItem = new(this, item.Type, true, item, null);
                    addItem.ShowDialog();

                    //対応アバターのパスを変えてあげる
                    foreach (var item2 in Items)
                    {
                        if (item2.SupportedAvatar.Contains(prePath))
                        {
                            item2.SupportedAvatar = item2.SupportedAvatar.Select(avatar =>
                                avatar == prePath ? item.ItemPath : avatar).ToArray();
                        }
                    }

                    // もしアイテムで編集されたアイテムを開いていたら、パスなどに使用される文字列も更新しないといけないため
                    if (CurrentPath.CurrentSelectedAvatarPath == prePath)
                    {
                        CurrentPath.CurrentSelectedAvatar = item.Title;
                        CurrentPath.CurrentSelectedAvatarPath = item.ItemPath;
                    }

                    // もしアバターの欄を右で開いていたら、そのアイテムの情報も更新しないといけないため
                    if (_openingWindow == Window.ItemList && !_isSearching) GenerateItems();

                    //検索中だと、検索画面を再読込してあげる
                    if (_isSearching) SearchItems();

                    // 検索時の文字列を消さないようにするために_isSearchingでチェックしている
                    if (!_isSearching) PathTextBox.Text = GeneratePath();

                    RefleshWindow();
                    Helper.SaveItemsData(Items);
                };

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("削除", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                toolStripMenuItem5.Click += (_, _) =>
                {
                    DialogResult result = MessageBox.Show(Helper.Translate("本当に削除しますか？", CurrentLanguage),
                        Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;

                    var undo = false; //もし削除されるアイテムが開かれていたら
                    if (CurrentPath.CurrentSelectedItem?.ItemPath == item.ItemPath)
                    {
                        CurrentPath.CurrentSelectedItemCategory = null;
                        CurrentPath.CurrentSelectedItem = null;
                        undo = true;
                    }

                    var undo2 = false; //アバターモードでもし削除されるアバターから今までのアイテムが開かれていたら
                    if (CurrentPath.CurrentSelectedAvatarPath == item.ItemPath && !_authorMode && !_categoryMode)
                    {
                        CurrentPath = new CurrentPath();
                        undo2 = true;
                    }

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();

                    // アバターのときは対応アバター削除、共通素体グループから削除用の処理を実行する
                    if (item.Type == ItemType.Avatar)
                    {
                        var result2 = MessageBox.Show(
                            Helper.Translate("このアバターを対応アバターとしているアイテムの対応アバターからこのアバターを削除しますか？", CurrentLanguage),
                            Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result2 == DialogResult.Yes)
                        {
                            foreach (var item2 in Items)
                            {
                                item2.SupportedAvatar = item2.SupportedAvatar.Where(avatar => avatar != item.ItemPath)
                                    .ToArray();
                            }
                        }

                        if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                        {
                            var result3 = MessageBox.Show(Helper.Translate("このアバターを共通素体グループから削除しますか？", CurrentLanguage),
                                Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (result3 == DialogResult.Yes)
                            {
                                foreach (var commonAvatar in CommonAvatars)
                                {
                                    commonAvatar.Avatars = commonAvatar.Avatars.Where(avatar => avatar != item.ItemPath)
                                        .ToArray();
                                }

                                Helper.SaveCommonAvatarData(CommonAvatars);
                            }
                        }

                    }

                    MessageBox.Show(Helper.Translate("削除が完了しました。", CurrentLanguage),
                        Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (_isSearching)
                    {
                        GenerateAvatarList();
                        GenerateAuthorList();
                        GenerateCategoryListLeft();

                        // フォルダー内検索の時
                        if (_openingWindow is Window.ItemFolderCategoryList or Window.ItemFolderItemsList)
                        {
                            // 選択されたアバターから現在の所まで来てる場合
                            if (undo2)
                            {
                                SearchBox.Text = "";
                                SearchResultLabel.Text = "";
                                _isSearching = false;
                                ResetAvatarList(true);
                                PathTextBox.Text = GeneratePath();
                                Helper.SaveItemsData(Items);
                                return;
                            }

                            // アイテムとして選択されている場合
                            if (undo)
                            {
                                SearchBox.Text = "";
                                SearchResultLabel.Text = "";
                                _isSearching = false;
                                GenerateItems();
                                PathTextBox.Text = GeneratePath();
                                Helper.SaveItemsData(Items);
                            }
                        }
                        else
                        {
                            SearchItems();
                            Helper.SaveItemsData(Items);
                        }
                    }
                    else
                    {
                        GenerateAvatarList();
                        GenerateAuthorList();
                        GenerateCategoryListLeft();

                        // アバターが選択された状態(CurrentSelectedAvatarPathとして設定されている時)
                        if (undo2)
                        {
                            ResetAvatarList(true);
                            PathTextBox.Text = GeneratePath();
                            Helper.SaveItemsData(Items);
                            return;
                        }

                        // フォルダーを開いていって、アイテムが選択された状態(CurrentSelectedItemとして設定されている時)
                        if (undo)
                        {
                            GenerateItems();
                            PathTextBox.Text = GeneratePath();
                            Helper.SaveItemsData(Items);
                            return;
                        }

                        // アイテム画面に既にいる
                        if (_openingWindow == Window.ItemList)
                        {
                            GenerateItems();
                            Helper.SaveItemsData(Items);
                            return;
                        }

                        // アイテム画面の前にいる
                        RefleshWindow();

                        Helper.SaveItemsData(Items);
                    }
                };

                contextMenuStrip.Items.Add(toolStripMenuItem2);
                contextMenuStrip.Items.Add(toolStripMenuItem3);
                contextMenuStrip.Items.Add(toolStripMenuItem4);
                contextMenuStrip.Items.Add(toolStripMenuItem5);
                button.ContextMenuStrip = contextMenuStrip;

                AvatarPage.Controls.Add(button);
                index++;
            }
        }

        private void GenerateAuthorList()
        {
            AvatarAuthorPage.Controls.Clear();
            var index = 0;

            var authors = Array.Empty<Author>();
            foreach (Item item in Items)
            {
                if (authors.Any(author => author.AuthorName == item.AuthorName)) continue;
                authors = authors.Append(new Author
                {
                    AuthorName = item.AuthorName,
                    AuthorImagePath = item.AuthorImageFilePath
                }).ToArray();
            }

            if (authors.Length == 0) return;
            authors = authors.OrderBy(author => author.AuthorName).ToArray();

            foreach (var author in authors)
            {
                Button button = Helper.CreateButton(author.AuthorImagePath, author.AuthorName,
                    Items.Count(item => item.AuthorName == author.AuthorName) +
                    Helper.Translate("個の項目", CurrentLanguage), true, author.AuthorName, GetAvatarListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath = new CurrentPath
                    {
                        CurrentSelectedAuthor = author
                    };

                    _authorMode = true;
                    _categoryMode = false;
                    SearchBox.Text = "";
                    SearchResultLabel.Text = "";
                    _isSearching = false;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();

                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem.Click += (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("画像ファイル|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("サムネイル変更", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("サムネイルを変更しました！", CurrentLanguage) + "\n\n" +
                        Helper.Translate("変更前: ", CurrentLanguage) + author.AuthorImagePath + "\n\n" +
                        Helper.Translate("変更後: ", CurrentLanguage) + ofd.FileName,
                        "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    foreach (var item in Items.Where(item => item.AuthorName == author.AuthorName))
                    {
                        item.AuthorImageFilePath = ofd.FileName;
                    }

                    GenerateAuthorList();
                    Helper.SaveItemsData(Items);
                };

                contextMenuStrip.Items.Add(toolStripMenuItem);
                button.ContextMenuStrip = contextMenuStrip;
                AvatarAuthorPage.Controls.Add(button);
                index++;
            }
        }

        private void GenerateCategoryListLeft()
        {
            CategoryPage.Controls.Clear();
            var index = 0;
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                if (itemType is ItemType.Unknown) continue;

                var items = Items.Where(item => item.Type == itemType);
                var itemCount = items.Count();
                Button button = Helper.CreateButton(null,
                    Helper.GetCategoryName(itemType, CurrentLanguage),
                    itemCount + Helper.Translate("個の項目", CurrentLanguage), true, "", GetAvatarListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    CurrentPath = new CurrentPath
                    {
                        CurrentSelectedCategory = itemType
                    };

                    _authorMode = false;
                    _categoryMode = true;
                    SearchBox.Text = "";
                    SearchResultLabel.Text = "";
                    _isSearching = false;
                    GenerateItems();
                    PathTextBox.Text = GeneratePath();
                };
                CategoryPage.Controls.Add(button);
                index++;
            }
        }

        // Generate List (RIGHT)
        private void GenerateCategoryList()
        {
            _openingWindow = Window.ItemCategoryList;
            ResetAvatarList();

            var index = 0;
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                if (itemType is ItemType.Unknown) continue;

                int itemCount = 0;
                if (_authorMode)
                {
                    itemCount = Items.Count(item =>
                        item.Type == itemType &&
                        item.AuthorName == CurrentPath.CurrentSelectedAuthor?.AuthorName
                    );
                }
                else
                {
                    itemCount = Items.Count(item =>
                        item.Type == itemType &&
                        (
                            Helper.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath)
                                .IsSupportedOrCommon ||
                            item.SupportedAvatar.Length == 0 || CurrentPath.CurrentSelectedAvatar == "*"
                        )
                    );
                }

                if (itemCount == 0) continue;

                Button button = Helper.CreateButton(null,
                    Helper.GetCategoryName(itemType, CurrentLanguage),
                    itemCount + Helper.Translate("個の項目", CurrentLanguage), false, "", GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);

                button.Click += (_, _) =>
                {
                    CurrentPath.CurrentSelectedCategory = itemType;
                    GenerateItems();
                    PathTextBox.Text = GeneratePath();
                };

                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateItems()
        {
            _openingWindow = Window.ItemList;
            ResetAvatarList();

            var filteredItems = Items.AsEnumerable();

            if (_authorMode)
            {
                filteredItems = Items.Where(item =>
                    item.Type == CurrentPath.CurrentSelectedCategory &&
                    item.AuthorName == CurrentPath.CurrentSelectedAuthor?.AuthorName
                );
            }
            else if (_categoryMode)
            {
                filteredItems = Items.Where(item =>
                    item.Type == CurrentPath.CurrentSelectedCategory
                );
            }
            else
            {
                filteredItems = Items.Where(item =>
                    item.Type == CurrentPath.CurrentSelectedCategory &&
                    (
                        Helper.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath)
                            .IsSupportedOrCommon ||
                        item.SupportedAvatar.Length == 0 || CurrentPath.CurrentSelectedAvatar == "*"
                    )
                );
            }

            filteredItems = filteredItems.OrderBy(item => item.Title).ToList();
            if (!filteredItems.Any()) return;

            var index = 0;
            foreach (Item item in filteredItems)
            {
                var authorText = Helper.Translate("作者: ", CurrentLanguage) + item.AuthorName;

                var isSupportedOrCommon =
                    Helper.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath);

                if (isSupportedOrCommon.OnlyCommon && item.SupportedAvatar.Length != 0 &&
                    !item.SupportedAvatar.Contains(CurrentPath.CurrentSelectedAvatarPath))
                {
                    var commonAvatarName = isSupportedOrCommon.CommonAvatarName;
                    if (!string.IsNullOrEmpty(commonAvatarName))
                    {
                        authorText += "\n" + Helper.Translate("共通素体: ", CurrentLanguage) + commonAvatarName;
                    }
                }

                Button button = Helper.CreateButton(item.ImagePath, item.Title, authorText, false, item.Title,
                    GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    if (!Directory.Exists(item.ItemPath))
                    {
                        var prePath = item.ItemPath;
                        DialogResult result =
                            MessageBox.Show(Helper.Translate("フォルダが見つかりませんでした。編集しますか？", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Error);
                        if (result != DialogResult.Yes) return;
                        AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, true, item, null);
                        addItem.ShowDialog();

                        //対応アバターのパスを変えてあげる
                        foreach (var item2 in Items)
                        {
                            if (item2.SupportedAvatar.Contains(prePath))
                            {
                                item2.SupportedAvatar = item2.SupportedAvatar.Select(avatar =>
                                    avatar == prePath ? item.ItemPath : avatar).ToArray();
                            }
                        }

                        if (CurrentPath.CurrentSelectedAvatarPath == prePath)
                        {
                            CurrentPath.CurrentSelectedAvatar = item.Title;
                            CurrentPath.CurrentSelectedAvatarPath = item.ItemPath;
                        }

                        RefleshWindow();
                        Helper.SaveItemsData(Items);
                    }

                    CurrentPath.CurrentSelectedItem = item;
                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();

                if (Directory.Exists(item.ItemPath))
                {
                    ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("フォルダを開く", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.OpenIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        if (!Directory.Exists(item.ItemPath))
                        {
                            MessageBox.Show(Helper.Translate("フォルダが見つかりませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        try
                        {
                            Process.Start("explorer.exe", item.ItemPath);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("フォルダを開けませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };
                    contextMenuStrip.Items.Add(toolStripMenuItem);
                }

                if (item.BoothId != -1)
                {
                    ToolStripMenuItem toolStripMenuItem =
                        new(Helper.Translate("Boothリンクのコピー", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        try
                        {
                            Clipboard.SetText(
                                $"https://booth.pm/{Helper.GetCurrentLanguageCode(CurrentLanguage)}/items/" +
                                item.BoothId);
                        }
                        catch (Exception ex)
                        {
                            if (ex is ExternalException) return;
                            MessageBox.Show(Helper.Translate("クリップボードにコピーできませんでした", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Boothリンクを開く", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem1.Click += (_, _) =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = $"https://booth.pm/{Helper.GetCurrentLanguageCode(CurrentLanguage)}/items/" +
                                           item.BoothId,
                                UseShellExecute = true
                            });
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("リンクを開けませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("この作者の他のアイテムを表示", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                toolStripMenuItem2.Click += (_, _) =>
                {
                    SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                    SearchItems();
                };

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem3.Click += (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("画像ファイル|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("サムネイル変更", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("サムネイルを変更しました！", CurrentLanguage) + "\n\n" +
                        Helper.Translate("変更前: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                        Helper.Translate("変更後: ", CurrentLanguage) + ofd.FileName,
                        Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    item.ImagePath = ofd.FileName;

                    if (_isSearching)
                    {
                        SearchItems();
                    }
                    else
                    {
                        GenerateItems();
                    }

                    GenerateAvatarList();
                    Helper.SaveItemsData(Items);
                };

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("編集", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem4.Click += (_, _) =>
                {
                    var prePath = item.ItemPath;
                    AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, true, item, null);
                    addItem.ShowDialog();

                    //対応アバターのパスを変えてあげる
                    foreach (var item2 in Items)
                    {
                        if (item2.SupportedAvatar.Contains(prePath))
                        {
                            item2.SupportedAvatar = item2.SupportedAvatar.Select(avatar =>
                                avatar == prePath ? item.ItemPath : avatar).ToArray();
                        }
                    }


                    if (CurrentPath.CurrentSelectedAvatarPath == prePath)
                    {
                        CurrentPath.CurrentSelectedAvatar = item.Title;
                        CurrentPath.CurrentSelectedAvatarPath = item.ItemPath;
                    }

                    if (!_isSearching) PathTextBox.Text = GeneratePath();
                    RefleshWindow();
                    Helper.SaveItemsData(Items);
                };

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("削除", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                toolStripMenuItem5.Click += (_, _) =>
                {
                    DialogResult result = MessageBox.Show(Helper.Translate("本当に削除しますか？", CurrentLanguage),
                        Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;

                    var undo = false;
                    if (CurrentPath.CurrentSelectedAvatarPath == item.ItemPath && !_authorMode && !_categoryMode)
                    {
                        CurrentPath = new CurrentPath();
                        undo = true;
                        PathTextBox.Text = GeneratePath();
                    }

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();

                    if (item.Type == ItemType.Avatar)
                    {
                        var result2 = MessageBox.Show(
                            Helper.Translate("このアバターを対応アバターとしているアイテムの対応アバターからこのアバターを削除しますか？", CurrentLanguage),
                            Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result2 == DialogResult.Yes)
                        {
                            foreach (var item2 in Items)
                            {
                                item2.SupportedAvatar = item2.SupportedAvatar.Where(avatar => avatar != item.ItemPath)
                                    .ToArray();
                            }
                        }

                        if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                        {
                            var result3 = MessageBox.Show(Helper.Translate("このアバターを共通素体グループから削除しますか？", CurrentLanguage),
                                Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (result3 == DialogResult.Yes)
                            {
                                foreach (var commonAvatar in CommonAvatars)
                                {
                                    commonAvatar.Avatars = commonAvatar.Avatars.Where(avatar => avatar != item.ItemPath)
                                        .ToArray();
                                }

                                Helper.SaveCommonAvatarData(CommonAvatars);
                            }
                        }
                    }

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();
                    MessageBox.Show(Helper.Translate("削除が完了しました。", CurrentLanguage),
                        Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (undo)
                    {
                        SearchBox.Text = "";
                        SearchResultLabel.Text = "";
                        _isSearching = false;
                        GenerateAvatarList();
                        GenerateAuthorList();
                        GenerateCategoryListLeft();
                        ResetAvatarList(true);
                    }
                    else
                    {
                        RefleshWindow();
                    }

                    Helper.SaveItemsData(Items);
                };

                contextMenuStrip.Items.Add(toolStripMenuItem2);
                contextMenuStrip.Items.Add(toolStripMenuItem3);
                contextMenuStrip.Items.Add(toolStripMenuItem4);
                contextMenuStrip.Items.Add(toolStripMenuItem5);
                button.ContextMenuStrip = contextMenuStrip;
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateItemCategoryList()
        {
            _openingWindow = Window.ItemFolderCategoryList;
            var types = new[]
            {
                "改変用データ",
                "テクスチャ",
                "ドキュメント",
                "Unityパッケージ",
                "マテリアル",
                "不明"
            };
            if (CurrentPath.CurrentSelectedItem == null) return;
            ItemFolderInfo itemFolderInfo = Helper.GetItemFolderInfo(CurrentPath.CurrentSelectedItem.ItemPath,
                CurrentPath.CurrentSelectedItem.MaterialPath);
            CurrentPath.CurrentSelectedItemFolderInfo = itemFolderInfo;

            ResetAvatarList();

            var index = 0;
            foreach (var itemType in types)
            {
                var itemCount = itemFolderInfo.GetItemCount(itemType);
                if (itemCount == 0) continue;

                Button button = Helper.CreateButton(null,
                    Helper.Translate(itemType, CurrentLanguage), itemCount + Helper.Translate("個の項目", CurrentLanguage),
                    false, "", GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);

                button.Click += (_, _) =>
                {
                    CurrentPath.CurrentSelectedItemCategory = itemType;
                    GenerateItemFiles();
                    PathTextBox.Text = GeneratePath();
                };

                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateItemFiles()
        {
            _openingWindow = Window.ItemFolderItemsList;
            ResetAvatarList();

            var files = CurrentPath.CurrentSelectedItemFolderInfo.GetItems(CurrentPath.CurrentSelectedItemCategory);
            if (files.Length == 0) return;

            files = files.OrderBy(file => file.FileName).ToArray();

            var index = 0;
            foreach (var file in files)
            {
                var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : "";
                Button button = Helper.CreateButton(imagePath, file.FileName,
                    file.FileExtension.Replace(".", "") + Helper.Translate("ファイル", CurrentLanguage), false,
                    Helper.Translate("開くファイルのパス: ", CurrentLanguage) + file.FilePath, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);

                ContextMenuStrip contextMenuStrip = new();
                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("ファイルのパスを開く", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon));
                toolStripMenuItem.Click += (_, _) => { Process.Start("explorer.exe", "/select," + file.FilePath); };
                contextMenuStrip.Items.Add(toolStripMenuItem);
                button.ContextMenuStrip = contextMenuStrip;

                button.Click += (_, _) =>
                {
                    try
                    {
                        if (file.FileExtension is ".unitypackage")
                        {
                            _ = Helper.ModifyUnityPackageFilePathAsync(file, CurrentPath, CurrentLanguage);
                        }
                        else
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = file.FilePath,
                                UseShellExecute = true
                            });
                        }
                    }
                    catch
                    {
                        try
                        {
                            Process.Start("explorer.exe", "/select," + file.FilePath);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("ファイルを開けませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                };

                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateFilteredItem(SearchFilter searchFilter)
        {
            ResetAvatarList();

            var filteredItems = Items.Where(item =>
            {
                if (searchFilter.Author.Length != 0 && !searchFilter.Author.Contains(item.AuthorName))
                    return false;
                if (searchFilter.Title.Length != 0 && !searchFilter.Title.Contains(item.Title))
                    return false;
                if (searchFilter.BoothId.Length != 0 && !searchFilter.BoothId.Contains(item.BoothId.ToString()))
                    return false;

                return true;
            });

            filteredItems = filteredItems
                .Where(item =>
                    searchFilter.SearchWords.All(word =>
                        item.Title.ToLower().Contains(word.ToLower()) ||
                        item.AuthorName.ToLower().Contains(word.ToLower()) ||
                        item.SupportedAvatar.Any(avatar => avatar.ToLower().Contains(word.ToLower())) ||
                        item.BoothId.ToString().Contains(word.ToLower())
                    )
                )
                .OrderByDescending(item =>
                {
                    var matchCount = 0;
                    foreach (var word in searchFilter.SearchWords)
                    {
                        if (item.Title.ToLower().Contains(word.ToLower())) matchCount++;
                        if (item.AuthorName.ToLower().Contains(word.ToLower())) matchCount++;
                    }

                    return matchCount;
                })
                .ToList();

            SearchResultLabel.Text = Helper.Translate("検索結果: ", CurrentLanguage) + filteredItems.Count() +
                                     Helper.Translate("件", CurrentLanguage) + Helper.Translate(" (全", CurrentLanguage) +
                                     Items.Length + Helper.Translate("件)", CurrentLanguage);
            if (!filteredItems.Any()) return;

            var index = 0;
            foreach (Item item in filteredItems)
            {
                Button button = Helper.CreateButton(item.ImagePath, item.Title,
                    Helper.Translate("作者: ", CurrentLanguage) + item.AuthorName, false,
                    item.Title, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                button.Click += (_, _) =>
                {
                    if (!Directory.Exists(item.ItemPath))
                    {
                        var prePath = item.ItemPath;
                        DialogResult result =
                            MessageBox.Show(Helper.Translate("フォルダが見つかりませんでした。編集しますか？", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Error);
                        if (result != DialogResult.Yes) return;
                        AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, true, item, null);
                        addItem.ShowDialog();

                        //対応アバターのパスを変えてあげる
                        foreach (var item2 in Items)
                        {
                            if (item2.SupportedAvatar.Contains(prePath))
                            {
                                item2.SupportedAvatar = item2.SupportedAvatar.Select(avatar =>
                                    avatar == prePath ? item.ItemPath : avatar).ToArray();
                            }
                        }

                        GenerateFilteredItem(searchFilter);
                        GenerateAvatarList();
                        GenerateAuthorList();
                        GenerateCategoryListLeft();
                        Helper.SaveItemsData(Items);
                    }

                    _authorMode = false;
                    _categoryMode = false;
                    GeneratePathFromItem(item);
                    SearchBox.Text = "";
                    SearchResultLabel.Text = "";
                    _isSearching = false;
                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                };

                ContextMenuStrip contextMenuStrip = new();

                if (Directory.Exists(item.ItemPath))
                {
                    ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("フォルダを開く", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.OpenIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        if (!Directory.Exists(item.ItemPath))
                        {
                            MessageBox.Show(Helper.Translate("フォルダが見つかりませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        try
                        {
                            Process.Start("explorer.exe", item.ItemPath);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("フォルダを開けませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };
                    contextMenuStrip.Items.Add(toolStripMenuItem);
                }

                if (item.BoothId != -1)
                {
                    ToolStripMenuItem toolStripMenuItem =
                        new(Helper.Translate("Boothリンクのコピー", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem.Click += (_, _) =>
                    {
                        try
                        {
                            Clipboard.SetText(
                                $"https://booth.pm/{Helper.GetCurrentLanguageCode(CurrentLanguage)}/items/" +
                                item.BoothId);
                        }
                        catch (Exception ex)
                        {
                            if (ex is ExternalException) return;
                            MessageBox.Show(Helper.Translate("クリップボードにコピーできませんでした", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Boothリンクを開く", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    toolStripMenuItem1.Click += (_, _) =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = $"https://booth.pm/{Helper.GetCurrentLanguageCode(CurrentLanguage)}/items/" +
                                           item.BoothId,
                                UseShellExecute = true
                            });
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("リンクを開けませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    };

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("この作者の他のアイテムを表示", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                toolStripMenuItem2.Click += (_, _) =>
                {
                    SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                    SearchItems();
                };

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem3.Click += (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("画像ファイル|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("サムネイル変更", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("サムネイルを変更しました！", CurrentLanguage) + "\n\n" +
                        Helper.Translate("変更前: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                        Helper.Translate("変更後: ", CurrentLanguage) + ofd.FileName,
                        Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    item.ImagePath = ofd.FileName;
                    GenerateFilteredItem(searchFilter);
                    GenerateAvatarList();
                    Helper.SaveItemsData(Items);
                };

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("編集", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                toolStripMenuItem4.Click += (_, _) =>
                {
                    var prePath = item.ItemPath;
                    AddItem addItem = new(this, item.Type, true, item, null);
                    addItem.ShowDialog();

                    //対応アバターのパスを変えてあげる
                    foreach (var item2 in Items)
                    {
                        if (item2.SupportedAvatar.Contains(prePath))
                        {
                            item2.SupportedAvatar = item2.SupportedAvatar.Select(avatar =>
                                avatar == prePath ? item.ItemPath : avatar).ToArray();
                        }
                    }

                    if (CurrentPath.CurrentSelectedAvatarPath == prePath)
                    {
                        CurrentPath.CurrentSelectedAvatar = item.Title;
                        CurrentPath.CurrentSelectedAvatarPath = item.ItemPath;
                    }

                    GenerateFilteredItem(searchFilter);
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                    Helper.SaveItemsData(Items);
                };

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("削除", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                toolStripMenuItem5.Click += (_, _) =>
                {
                    DialogResult result = MessageBox.Show(Helper.Translate("本当に削除しますか？", CurrentLanguage),
                        Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();

                    if (item.Type == ItemType.Avatar)
                    {
                        var result2 = MessageBox.Show(
                            Helper.Translate("このアバターを対応アバターとしているアイテムの対応アバターからこのアバターを削除しますか？", CurrentLanguage),
                            Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result2 == DialogResult.Yes)
                        {
                            foreach (var item2 in Items)
                            {
                                item2.SupportedAvatar = item2.SupportedAvatar.Where(avatar => avatar != item.ItemPath)
                                    .ToArray();
                            }
                        }

                        if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                        {
                            var result3 = MessageBox.Show(Helper.Translate("このアバターを共通素体グループから削除しますか？", CurrentLanguage),
                                Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (result3 == DialogResult.Yes)
                            {
                                foreach (var commonAvatar in CommonAvatars)
                                {
                                    commonAvatar.Avatars = commonAvatar.Avatars.Where(avatar => avatar != item.ItemPath)
                                        .ToArray();
                                }

                                Helper.SaveCommonAvatarData(CommonAvatars);
                            }
                        }
                    }

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();
                    MessageBox.Show(Helper.Translate("削除が完了しました。", CurrentLanguage),
                        Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);

                    GenerateFilteredItem(searchFilter);
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                    Helper.SaveItemsData(Items);
                };

                contextMenuStrip.Items.Add(toolStripMenuItem2);
                contextMenuStrip.Items.Add(toolStripMenuItem3);
                contextMenuStrip.Items.Add(toolStripMenuItem4);
                contextMenuStrip.Items.Add(toolStripMenuItem5);
                button.ContextMenuStrip = contextMenuStrip;
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        private void GenerateFilteredFolderItems(SearchFilter searchWords)
        {
            ResetAvatarList();

            var fileDatas = _openingWindow switch
            {
                Window.ItemFolderItemsList => CurrentPath.CurrentSelectedItemFolderInfo.GetItems(CurrentPath
                    .CurrentSelectedItemCategory),
                Window.ItemFolderCategoryList => CurrentPath.CurrentSelectedItemFolderInfo.GetAllItem(),
                _ => Array.Empty<FileData>()
            };

            var filteredItems = fileDatas
                .Where(file =>
                    searchWords.SearchWords.All(word =>
                        file.FileName.ToLower().Contains(word.ToLower())
                    )
                )
                .OrderByDescending(file =>
                {
                    return searchWords.SearchWords.Count(word => file.FileName.ToLower().Contains(word.ToLower()));
                })
                .ToList();

            SearchResultLabel.Text = Helper.Translate("フォルダー内検索結果: ", CurrentLanguage) + filteredItems.Count +
                                     Helper.Translate("件", CurrentLanguage) + Helper.Translate(" (全", CurrentLanguage) +
                                     fileDatas.Length + Helper.Translate("件)", CurrentLanguage);
            if (!filteredItems.Any()) return;

            var index = 0;
            foreach (var file in filteredItems)
            {
                var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : "";
                Button button = Helper.CreateButton(imagePath, file.FileName,
                    file.FileExtension.Replace(".", "") + Helper.Translate("ファイル", CurrentLanguage), false,
                    Helper.Translate("開くファイルのパス: ", CurrentLanguage) + file.FilePath, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);

                ContextMenuStrip contextMenuStrip = new();
                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("ファイルのパスを開く", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon));
                toolStripMenuItem.Click += (_, _) => { Process.Start("explorer.exe", "/select," + file.FilePath); };
                contextMenuStrip.Items.Add(toolStripMenuItem);
                button.ContextMenuStrip = contextMenuStrip;

                button.Click += (_, _) =>
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
                        try
                        {
                            Process.Start("explorer.exe", "/select," + file.FilePath);
                        }
                        catch
                        {
                            MessageBox.Show(Helper.Translate("ファイルを開けませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                };

                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        // Add Item Form
        private void AddItemButton_Click(object sender, EventArgs e)
        {
            AddItem addItem = new AddItem(this, CurrentPath.CurrentSelectedCategory, false, null, null);
            addItem.ShowDialog();
            RefleshWindow();
            Helper.SaveItemsData(Items);
        }

        // Generate Path
        private string GeneratePath()
        {
            if (_authorMode)
            {
                if (CurrentPath.CurrentSelectedAuthor == null)
                    return Helper.Translate("ここには現在のパスが表示されます", CurrentLanguage);
                if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
                    return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName);
                if (CurrentPath.CurrentSelectedItem == null)
                    return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + " / " +
                           Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage);
                if (CurrentPath.CurrentSelectedItemCategory == null)
                    return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + " / " +
                           Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage) + " / " +
                           Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + " / " +
                       Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage) + " / " +
                       Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + " / " +
                       Helper.Translate(CurrentPath.CurrentSelectedItemCategory, CurrentLanguage);
            }

            if (_categoryMode)
            {
                if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
                    return Helper.Translate("ここには現在のパスが表示されます", CurrentLanguage);
                if (CurrentPath.CurrentSelectedItem == null)
                    return Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage);
                if (CurrentPath.CurrentSelectedItemCategory == null)
                    return Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage) + " / " +
                           Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

                return Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage) + " / " +
                       Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + " / " +
                       Helper.Translate(CurrentPath.CurrentSelectedItemCategory, CurrentLanguage);
            }

            if (CurrentPath.CurrentSelectedAvatar == null) return Helper.Translate("ここには現在のパスが表示されます", CurrentLanguage);
            if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar);
            if (CurrentPath.CurrentSelectedItem == null)
                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar) + " / " +
                       Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage);
            if (CurrentPath.CurrentSelectedItemCategory == null)
                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar) + " / " +
                       Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage) + " / " +
                       Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

            return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar) + " / " +
                   Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage) + " / " +
                   Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + " / " +
                   Helper.Translate(CurrentPath.CurrentSelectedItemCategory, CurrentLanguage);
        }

        private void GeneratePathFromItem(Item item)
        {
            var avatarPath = item.SupportedAvatar.FirstOrDefault();
            var avatarName = Helper.GetAvatarName(Items, avatarPath);
            CurrentPath.CurrentSelectedAvatar = avatarName ?? "*";
            CurrentPath.CurrentSelectedAvatarPath = avatarPath;
            CurrentPath.CurrentSelectedCategory = item.Type;
            CurrentPath.CurrentSelectedItem = item;
        }

        // Undo Button
        private void UndoButton_Click(object sender, EventArgs e)
        {
            //検索中だった場合は前の画面までとりあえず戻してあげる
            if (_isSearching)
            {
                SearchBox.Text = "";
                SearchResultLabel.Text = "";
                _isSearching = false;
                if (CurrentPath.CurrentSelectedItemCategory != null)
                {
                    GenerateItemFiles();
                    PathTextBox.Text = GeneratePath();
                    return;
                }

                if (CurrentPath.CurrentSelectedItem != null)
                {
                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                    return;
                }

                if (CurrentPath.CurrentSelectedCategory != ItemType.Unknown)
                {
                    GenerateItems();
                    PathTextBox.Text = GeneratePath();
                    return;
                }

                if (CurrentPath.CurrentSelectedAvatar != null || CurrentPath.CurrentSelectedAuthor != null)
                {
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                    return;
                }

                ResetAvatarList(true);
                PathTextBox.Text = GeneratePath();
                return;
            }

            SearchBox.Text = "";
            SearchResultLabel.Text = "";
            _isSearching = false;

            if (CurrentPath.IsEmpty())
            {
                //エラー音を再生
                SystemSounds.Hand.Play();
                return;
            }

            if (CurrentPath.CurrentSelectedItemCategory != null)
            {
                CurrentPath.CurrentSelectedItemCategory = null;
                GenerateItemCategoryList();
                PathTextBox.Text = GeneratePath();
                return;
            }

            if (CurrentPath.CurrentSelectedItem != null)
            {
                CurrentPath.CurrentSelectedItem = null;
                GenerateItems();
                PathTextBox.Text = GeneratePath();
                return;
            }

            if (_authorMode)
            {
                if (CurrentPath.CurrentSelectedCategory != ItemType.Unknown)
                {
                    CurrentPath.CurrentSelectedCategory = ItemType.Unknown;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                    return;
                }
            }
            else if (!_categoryMode)
            {
                if (CurrentPath.CurrentSelectedCategory != ItemType.Unknown)
                {
                    CurrentPath.CurrentSelectedCategory = ItemType.Unknown;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                    return;
                }

                if (CurrentPath.CurrentSelectedAvatar == "*")
                {
                    CurrentPath.CurrentSelectedAvatar = null;
                    CurrentPath.CurrentSelectedAvatarPath = null;
                    ResetAvatarList(true);
                    PathTextBox.Text = GeneratePath();
                    return;
                }
            }

            ResetAvatarList(true);
            PathTextBox.Text = GeneratePath();
        }

        // Search Box
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode is Keys.Enter or Keys.Space)
            {
                SearchItems();
            }
        }

        private void SearchItems()
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                SearchResultLabel.Text = "";
                if (CurrentPath.CurrentSelectedItemCategory != null)
                {
                    GenerateItemFiles();
                    return;
                }

                if (CurrentPath.CurrentSelectedItem != null)
                {
                    GenerateItemCategoryList();
                    return;
                }

                if (CurrentPath.CurrentSelectedCategory != ItemType.Unknown)
                {
                    GenerateItems();
                    return;
                }

                if (CurrentPath.CurrentSelectedAvatar != null || CurrentPath.CurrentSelectedAuthor != null)
                {
                    GenerateCategoryList();
                    return;
                }

                ResetAvatarList(true);
                return;
            }

            _isSearching = true;
            SearchFilter searchFilter = Helper.GetSearchFilter(SearchBox.Text);

            if (_openingWindow is Window.ItemFolderCategoryList or Window.ItemFolderItemsList)
            {
                GenerateFilteredFolderItems(searchFilter);
            }
            else
            {
                GenerateFilteredItem(searchFilter);
            }

            string[] pathTextArr = Array.Empty<string>();
            if (searchFilter.Author.Length != 0)
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("作者", CurrentLanguage) + ": " +
                                                 string.Join(", ", searchFilter.Author))
                    .ToArray();
            }

            if (searchFilter.Title.Length != 0)
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("タイトル", CurrentLanguage) + ": " +
                                                 string.Join(", ", searchFilter.Title))
                    .ToArray();
            }

            if (searchFilter.BoothId.Length != 0)
            {
                pathTextArr = pathTextArr.Append("BoothID: " + string.Join(", ", searchFilter.BoothId)).ToArray();
            }

            pathTextArr = pathTextArr.Append(string.Join(", ", searchFilter.SearchWords)).ToArray();

            PathTextBox.Text = Helper.Translate("検索中... - ", CurrentLanguage) + string.Join(" / ", pathTextArr);
        }

        // ResetAvatarList
        private void ResetAvatarList(bool startLabelVisible = false)
        {
            if (startLabelVisible)
            {
                _authorMode = false;
                _categoryMode = false;
                CurrentPath = new CurrentPath();
            }

            for (int i = AvatarItemExplorer.Controls.Count - 1; i >= 0; i--)
            {
                if (AvatarItemExplorer.Controls[i].Name != "StartLabel")
                {
                    var control = AvatarItemExplorer.Controls[i];
                    AvatarItemExplorer.Controls.RemoveAt(i);
                    control.Dispose();
                }
                else
                {
                    AvatarItemExplorer.Controls[i].Visible = startLabelVisible;
                }
            }
        }

        // Drag and Drop Item Folder
        private void AvatarItemExplorer_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? dragFilePathArr = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (dragFilePathArr == null) return;
            var folderPath = dragFilePathArr[0];

            if (File.Exists(folderPath) || !Directory.Exists(folderPath))
            {
                MessageBox.Show(Helper.Translate("フォルダを選択してください", CurrentLanguage),
                    Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, false, null, folderPath);
            addItem.ItemAdded += (_, _) =>
            {
                RefleshWindow();
                Helper.SaveItemsData(Items);
                Enabled = true;
            };
            addItem.Show();
            Enabled = false;
        }

        private void AvatarPage_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? dragFilePathArr = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (dragFilePathArr == null) return;
            var folderPath = dragFilePathArr[0];

            if (File.Exists(folderPath) || !Directory.Exists(folderPath))
            {
                MessageBox.Show(Helper.Translate("フォルダを選択してください", CurrentLanguage),
                    Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AddItem addItem = new(this, ItemType.Avatar, false, null, folderPath);
            addItem.ItemAdded += (_, _) =>
            {
                RefleshWindow();
                Helper.SaveItemsData(Items);
                Enabled = true;
            };
            addItem.Show();
            Enabled = false;
        }

        // Export to CSV
        private void ExportButton_Click(object sender, EventArgs e)
        {
            try
            {
                ExportButton.Enabled = false;
                if (!Directory.Exists("./Output"))
                {
                    Directory.CreateDirectory("./Output");
                }

                var currentTimeStr = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                var fileName = currentTimeStr + ".csv";

                var index = 1;
                while (File.Exists("./Output/" + fileName))
                {
                    if (index > 60) throw new Exception("Too many exports.");
                    fileName = currentTimeStr + $"_{index}.csv";
                    index++;
                }

                using var sw = new StreamWriter("./Output/" + fileName, false, Encoding.UTF8);
                sw.WriteLine("Title,AuthorName,AuthorImageFilePath,ImagePath,Type,SupportedAvatar,BoothId,ItemPath");
                foreach (var item in Items)
                {
                    string[] avatarNames = Array.Empty<string>();
                    foreach (var avatar in item.SupportedAvatar)
                    {
                        var avatarName = Helper.GetAvatarName(Items, avatar);
                        if (avatarName == null) continue;
                        avatarNames = avatarNames.Append(avatarName).ToArray();
                    }

                    sw.WriteLine(
                        $"{item.Title},{item.AuthorName},{item.AuthorImageFilePath},{item.ImagePath},{item.Type},{string.Join(";", avatarNames)},{item.BoothId},{item.ItemPath}");
                }

                MessageBox.Show(Helper.Translate("Outputフォルダにエクスポートが完了しました！\nファイル名: ", CurrentLanguage) + fileName,
                    Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                ExportButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helper.Translate("エクスポートに失敗しました", CurrentLanguage),
                    Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Helper.ErrorLogger("エクスポートに失敗しました。", ex);
                ExportButton.Enabled = true;
            }
        }

        // Make Backup
        private void MakeBackupButton_Click(object sender, EventArgs e)
        {
            try
            {
                MakeBackupButton.Enabled = false;
                if (!Directory.Exists("./Backup"))
                {
                    Directory.CreateDirectory("./Backup");
                }

                var currentTimeStr = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                var fileName = currentTimeStr + ".zip";

                var index = 1;
                while (File.Exists("./Backup/" + fileName))
                {
                    if (index > 60) throw new Exception("Too many backups");
                    fileName = currentTimeStr + $"_{index}.zip";
                    index++;
                }

                ZipFile.CreateFromDirectory("./Datas", "./Backup/" + fileName);

                MessageBox.Show(
                    Helper.Translate(
                        "Backupフォルダにバックアップが完了しました！\n\n復元したい場合は、\"データを読み込む\"ボタンで現在作成されたファイルを展開したものを選択してください。\n\nファイル名: ",
                        CurrentLanguage) + fileName, Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                MakeBackupButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helper.Translate("バックアップに失敗しました", CurrentLanguage),
                    Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Helper.ErrorLogger("バックアップに失敗しました。", ex);
                MakeBackupButton.Enabled = true;
            }
        }

        // Change Language
        private void LanguageBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentLanguage = LanguageBox.SelectedIndex switch
            {
                0 => "ja-JP",
                1 => "ko-KR",
                2 => "en-US",
                _ => CurrentLanguage
            };

            var newFont = _fontFamilies.TryGetValue(CurrentLanguage, out var family) ? family : _fontFamilies["ja-JP"];
            GuiFont = newFont;

            foreach (Control control in Controls)
            {
                if (control.Name == "LanguageBox") continue;
                if (string.IsNullOrEmpty(control.Text)) continue;
                _controlNames.TryAdd(control.Name, control.Text);
                control.Text = Helper.Translate(_controlNames[control.Name], CurrentLanguage);
                ChangeControlFont(control);
            }

            foreach (Control control in AvatarSearchFilterList.Controls)
            {
                if (string.IsNullOrEmpty(control.Text)) continue;
                _controlNames.TryAdd(control.Name, control.Text);
                control.Text = Helper.Translate(_controlNames[control.Name], CurrentLanguage);
                ChangeControlFont(control);
            }

            foreach (Control control in ExplorerList.Controls)
            {
                if (string.IsNullOrEmpty(control.Text)) continue;
                _controlNames.TryAdd(control.Name, control.Text);
                control.Text = Helper.Translate(_controlNames[control.Name], CurrentLanguage);
                ChangeControlFont(control);
            }

            foreach (Control control in AvatarItemExplorer.Controls)
            {
                if (string.IsNullOrEmpty(control.Text)) continue;
                _controlNames.TryAdd(control.Name, control.Text);
                control.Text = Helper.Translate(_controlNames[control.Name], CurrentLanguage);
                ChangeControlFont(control);
            }

            RefleshWindow();
            PathTextBox.Text = GeneratePath();
        }

        private void ChangeControlFont(Control control)
        {
            if (GuiFont == null) return;
            var previousFont = control.Font;
            var familyName = previousFont.FontFamily.Name;
            if (familyName == "Yu Gothic UI") return;
            var previousSize = control.Font.Size;
            if (previousSize is < 0 or >= float.MaxValue) return;
            control.Font = new Font(GuiFont, previousSize);
        }

        private void RefleshWindow(bool reloadLeft = true)
        {
            if (_isSearching)
            {
                SearchItems();
                GenerateAvatarList();
                GenerateAuthorList();
                GenerateCategoryListLeft();
                return;
            }

            switch (_openingWindow)
            {
                case Window.ItemList:
                    GenerateItems();
                    break;
                case Window.ItemCategoryList:
                    GenerateCategoryList();
                    break;
                case Window.ItemFolderCategoryList:
                    GenerateItemCategoryList();
                    break;
                case Window.ItemFolderItemsList:
                    GenerateItemFiles();
                    break;
                case Window.Nothing:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!reloadLeft) return;
            GenerateAvatarList();
            GenerateAuthorList();
            GenerateCategoryListLeft();
        }

        private void ManageCommonAvatarButton_Click(object sender, EventArgs e)
        {
            ManageCommonAvatars manageCommonAvatar = new(this);
            manageCommonAvatar.ShowDialog();
            RefleshWindow();
            PathTextBox.Text = GeneratePath();
            Helper.SaveCommonAvatarData(CommonAvatars);
        }

        // Load Data Button
        private void LoadData_Click(object sender, EventArgs e) => LoadDataFromFolder();

        // Load Data From Folder
        private void LoadDataFromFolder()
        {
            //自動バックアップフォルダから復元するか聞く
            var result = MessageBox.Show(Helper.Translate("自動バックアップフォルダから復元しますか？", CurrentLanguage),
                Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                //バックアップ先のフォルダ
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var backupPath = Path.Combine(appDataPath, "Avatar Explorer", "Backup");

                //バックアップフォルダが存在しない場合
                if (!Directory.Exists(backupPath))
                {
                    MessageBox.Show(Helper.Translate("バックアップフォルダが見つかりませんでした。", CurrentLanguage),
                        Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //最初のフォルダ
                var firstFolder = Directory.GetDirectories(backupPath).MaxBy(d => new DirectoryInfo(d).CreationTime) ??
                                  backupPath;

                FolderBrowserDialog fbd = new()
                {
                    UseDescriptionForTitle = true,
                    Description = Helper.Translate("復元する時間のバックアップフォルダを選択してください", CurrentLanguage),
                    ShowNewFolderButton = false,
                    SelectedPath = firstFolder
                };

                if (fbd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var filePath = fbd.SelectedPath + "/ItemsData.json";
                    if (!File.Exists(filePath))
                    {
                        MessageBox.Show(Helper.Translate("アイテムファイルが見つかりませんでした。", CurrentLanguage),
                            Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        Items = Helper.LoadItemsData(filePath);
                        Items = Helper.FixSupportedAvatarPath(Items);
                        Helper.SaveItemsData(Items);
                    }

                    var filePath2 = fbd.SelectedPath + "/CommonAvatar.json";
                    if (!File.Exists(filePath2))
                    {
                        MessageBox.Show(Helper.Translate("共通素体ファイルが見つかりませんでした。", CurrentLanguage),
                            Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        CommonAvatars = Helper.LoadCommonAvatarData(filePath2);
                        Helper.SaveCommonAvatarData(CommonAvatars);
                    }

                    MessageBox.Show(Helper.Translate("復元が完了しました。", CurrentLanguage),
                        Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Helper.ErrorLogger("データの読み込みに失敗しました。", ex);
                    MessageBox.Show(Helper.Translate("データの読み込みに失敗しました。詳細はErrorLog.txtをご覧ください。", CurrentLanguage),
                        Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                FolderBrowserDialog fbd = new()
                {
                    UseDescriptionForTitle = true,
                    Description = Helper.Translate("以前のバージョンのDatasフォルダ、もしくは展開したバックアップフォルダを選択してください", CurrentLanguage),
                    ShowNewFolderButton = false
                };
                if (fbd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    if (Directory.Exists(Path.Combine(fbd.SelectedPath, "Datas")) &&
                        File.Exists(Path.Combine(fbd.SelectedPath, "Datas", "ItemsData.json")) &&
                        !File.Exists(Path.Combine(fbd.SelectedPath, "ItemsData.json")))
                    {
                        fbd.SelectedPath += "/Datas";
                    }

                    var filePath = fbd.SelectedPath + "/ItemsData.json";
                    if (!File.Exists(filePath))
                    {
                        MessageBox.Show(Helper.Translate("アイテムファイルが見つかりませんでした。", CurrentLanguage),
                            Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        Items = Helper.LoadItemsData(filePath);
                        Items = Helper.FixSupportedAvatarPath(Items);
                        Helper.SaveItemsData(Items);
                    }

                    var filePath2 = fbd.SelectedPath + "/CommonAvatar.json";
                    if (!File.Exists(filePath2))
                    {
                        MessageBox.Show(Helper.Translate("共通素体ファイルが見つかりませんでした。", CurrentLanguage),
                            Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        CommonAvatars = Helper.LoadCommonAvatarData(filePath2);
                        Helper.SaveCommonAvatarData(CommonAvatars);
                    }

                    var result2 = MessageBox.Show(
                        Helper.Translate("Thumbnailフォルダ、AuthorImageフォルダもコピーしますか？", CurrentLanguage),
                        Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result2 != DialogResult.Yes)
                    {
                        SearchBox.Text = "";
                        SearchResultLabel.Text = "";
                        _isSearching = false;
                        GenerateAvatarList();
                        GenerateAuthorList();
                        GenerateCategoryListLeft();
                        ResetAvatarList(true);
                        PathTextBox.Text = GeneratePath();
                        MessageBox.Show(Helper.Translate("コピーが完了しました。", CurrentLanguage),
                            Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    var thumbnailPath = fbd.SelectedPath + "/Thumbnail";
                    var authorImagePath = fbd.SelectedPath + "/AuthorImage";

                    if (Directory.Exists(thumbnailPath))
                    {
                        Directory.CreateDirectory("./Datas/Thumbnail");
                        foreach (var file in Directory.GetFiles(thumbnailPath))
                        {
                            try
                            {
                                File.Copy(file, "./Datas/Thumbnail/" + Path.GetFileName(file), true);
                            }
                            catch (Exception ex)
                            {
                                Helper.ErrorLogger("サムネイルのコピーに失敗しました。", ex);
                            }
                        }
                    }

                    if (Directory.Exists(authorImagePath))
                    {
                        Directory.CreateDirectory("./Datas/AuthorImage");
                        foreach (var file in Directory.GetFiles(authorImagePath))
                        {
                            try
                            {
                                File.Copy(file, "./Datas/AuthorImage/" + Path.GetFileName(file), true);
                            }
                            catch (Exception ex)
                            {
                                Helper.ErrorLogger("作者画像のコピーに失敗しました。", ex);
                            }
                        }
                    }

                    MessageBox.Show(Helper.Translate("コピーが完了しました。", CurrentLanguage),
                        Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Helper.ErrorLogger("データの読み込みに失敗しました。", ex);
                    MessageBox.Show(Helper.Translate("データの読み込みに失敗しました。詳細はErrorLog.txtをご覧ください。", CurrentLanguage),
                        Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            SearchBox.Text = "";
            SearchResultLabel.Text = "";
            _isSearching = false;
            GenerateAvatarList();
            GenerateAuthorList();
            GenerateCategoryListLeft();
            ResetAvatarList(true);
            PathTextBox.Text = GeneratePath();
        }

        // Resize Form
        private void Main_Resize(object sender, EventArgs e) => ResizeControl();

        private void ResizeControl()
        {
            if (!_initialized) return;
            var widthRatio = (float)ClientSize.Width / _initialFormSize.Width;
            var heightRatio = (float)ClientSize.Height / _initialFormSize.Height;

            foreach (Control control in Controls)
            {
                // サイズのスケーリング
                if (!_defaultControlSize.TryGetValue(control.Name, out var defaultSize))
                {
                    defaultSize = new SizeF(control.Size.Width, control.Size.Height);
                    _defaultControlSize.Add(control.Name, defaultSize);
                }

                var newWidth = (int)(defaultSize.Width * widthRatio);
                var newHeight = (int)(defaultSize.Height * heightRatio);

                // サイズがクライアント領域を超えないように制約
                newWidth = Math.Min(newWidth, ClientSize.Width);
                newHeight = Math.Min(newHeight, ClientSize.Height);

                control.Size = new Size(newWidth, newHeight);

                // 位置のスケーリング
                if (!_defaultControlLocation.TryGetValue(control.Name, out var defaultLocation))
                {
                    defaultLocation = new PointF(control.Location.X, control.Location.Y);
                    _defaultControlLocation.Add(control.Name, defaultLocation);
                }

                var newX = (int)(defaultLocation.X * widthRatio);
                var newY = (int)(defaultLocation.Y * heightRatio);

                // 位置がクライアント領域を超えないように制約
                newX = Math.Max(0, Math.Min(newX, ClientSize.Width - control.Width));
                newY = Math.Max(0, Math.Min(newY, ClientSize.Height - control.Height));

                control.Location = new Point(newX, newY);

                switch (control)
                {
                    // ラベル、テキストボックスのフォントサイズのスケーリング
                    case Label { Name: "SearchResultLabel" } label:
                        {
                            if (!_defaultFontSize.TryGetValue(label.Name, out var defaultFontSize))
                            {
                                defaultFontSize = label.Font.Size;
                                _defaultFontSize.Add(label.Name, defaultFontSize);
                            }

                            var scaleRatio = Math.Min(widthRatio, heightRatio);
                            var newFontSize = defaultFontSize * scaleRatio;

                            // 小さくなる場合のみフォントサイズを変更
                            if (!(newFontSize < defaultFontSize)) continue;
                            newFontSize = Math.Max(newFontSize, MinFontSize);
                            if (newFontSize is < 0 or >= float.MaxValue) continue;
                            label.Font = new Font(label.Font.FontFamily, newFontSize, label.Font.Style);
                            break;
                        }
                    // SearchResultLabel以外
                    case Label label:
                        {
                            if (!_defaultFontSize.TryGetValue(label.Name, out var defaultFontSize))
                            {
                                defaultFontSize = label.Font.Size;
                                _defaultFontSize.Add(label.Name, defaultFontSize);
                            }

                            var scaleRatio = Math.Min(widthRatio, heightRatio);
                            var newFontSize = defaultFontSize * scaleRatio;
                            newFontSize = Math.Max(newFontSize, MinFontSize);
                            if (newFontSize is < 0 or >= float.MaxValue) continue;
                            label.Font = new Font(label.Font.FontFamily, newFontSize, label.Font.Style);
                            break;
                        }
                    case TextBox:
                        {
                            if (!_defaultFontSize.TryGetValue(control.Name, out var defaultFontSize))
                            {
                                defaultFontSize = control.Font.Size;
                                _defaultFontSize.Add(control.Name, defaultFontSize);
                            }

                            var scaleRatio = Math.Min(widthRatio, heightRatio);
                            var newFontSize = defaultFontSize * scaleRatio;
                            newFontSize = Math.Max(newFontSize, MinFontSize);
                            if (newFontSize is < 0 or >= float.MaxValue) continue;
                            control.Font = new Font(control.Font.FontFamily, newFontSize, control.Font.Style);
                            break;
                        }
                }
            }

            for (int i = AvatarItemExplorer.Controls.Count - 1; i >= 0; i--)
            {
                if (AvatarItemExplorer.Controls[i].Name != "StartLabel") continue;
                var control = AvatarItemExplorer.Controls[i];
                control.Location = control.Location with
                {
                    X = (AvatarItemExplorer.Width - control.Width) / 2,
                    Y = (AvatarItemExplorer.Height - control.Height) / 2
                };
            }

            ScaleItemButtons();
        }

        private void ScaleItemButtons()
        {
            const int avatarItemExplorerBaseWidth = 874;
            const int avatarItemListBaseWidth = 303;

            var avatarItemExplorerWidth = avatarItemExplorerBaseWidth + GetItemExplorerListWidth();
            var avatarItemListWidth = avatarItemListBaseWidth + GetAvatarListWidth();

            foreach (Control control in AvatarItemExplorer.Controls)
            {
                if (control is Button button)
                {
                    button.Size = button.Size with { Width = avatarItemExplorerWidth };
                }
            }

            var controls = new Control[]
            {
                AvatarPage,
                AvatarAuthorPage,
                CategoryPage
            };

            foreach (var control in controls)
            {
                foreach (Control control1 in control.Controls)
                {
                    if (control1 is Button button)
                    {
                        button.Size = button.Size with { Width = avatarItemListWidth };
                    }
                }
            }
        }

        // Backup
        private void AutoBackup()
        {
            BackupFile();
            Timer timer = new()
            {
                Interval = BackupInterval
            };

            timer.Tick += (_, _) => BackupFile();
            timer.Start();
        }

        private void BackupTimeTitle()
        {
            Timer timer = new()
            {
                Interval = 1000
            };

            timer.Tick += (_, _) =>
            {
                if (_lastBackupTime == DateTime.MinValue)
                {
                    if (_lastBackupError)
                        Text = CurrentVersionFormText + " - " + Helper.Translate("バックアップエラー", CurrentLanguage);
                    return;
                }

                var timeSpan = DateTime.Now - _lastBackupTime;
                var minutes = timeSpan.Minutes;
                Text = CurrentVersionFormText +
                       $" - {Helper.Translate("最終自動バックアップ: ", CurrentLanguage) + minutes + Helper.Translate("分前", CurrentLanguage)}";

                if (_lastBackupError) Text += " - " + Helper.Translate("バックアップエラー", CurrentLanguage);
            };
            timer.Start();
        }

        private void BackupFile()
        {
            try
            {
                var backupFilesArray = new[]
                {
                    "./Datas/ItemsData.json",
                    "./Datas/CommonAvatar.json"
                };

                Helper.Backup(backupFilesArray);
                _lastBackupError = false;
                _lastBackupTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                _lastBackupError = true;
                Helper.ErrorLogger("自動バックアップに失敗しました。", ex);
            }
        }

        // Form Closing
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (!Directory.Exists("./Datas/Temp")) return;
                Directory.Delete("./Datas/Temp", true);
            }
            catch (Exception ex)
            {
                Helper.ErrorLogger("一時フォルダの削除に失敗しました。", ex);
            }
        }
    }
}