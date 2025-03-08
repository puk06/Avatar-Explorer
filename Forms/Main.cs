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

        #region フォームのテキスト関連の変数
        /// <summary>
        /// ソフトの現在のバージョン
        /// </summary>
        private const string CurrentVersion = "v1.0.6";

        /// <summary>
        /// デフォルトのフォームテキスト
        /// </summary>
        private const string CurrentVersionFormText = $"VRChat Avatar Explorer {CurrentVersion} by ぷこるふ";
        #endregion

        #region ソフトのデータベース関連の変数
        /// <summary>
        /// アイテムデータベース
        /// </summary>
        public Item[] Items;

        /// <summary>
        /// 共通素体データベース
        /// </summary>
        public CommonAvatar[] CommonAvatars;

        /// <summary>
        /// カスタムカテゴリーデータベース
        /// </summary>
        public string[] CustomCategories;
        #endregion

        #region フォント関連の変数

        /// <summary>
        /// フォントコレクション
        /// </summary>
        private readonly PrivateFontCollection _fontCollection = new();

        /// <summary>
        /// フォントファミリー
        /// </summary>
        private readonly Dictionary<string, FontFamily> _fontFamilies = new();

        /// <summary>
        /// フォームのGUIフォント
        /// </summary>
        public FontFamily? GuiFont;
        #endregion

        #region 現在のウィンドウの種類に関する変数

        /// <summary>
        /// 現在開かれているウィンドウが作者モードかどうかを取得または設定します。
        /// </summary>
        private bool _authorMode;

        /// <summary>
        /// 現在開かれているウィンドウがカテゴリーモードかどうかを取得または設定します。
        /// </summary>
        private bool _categoryMode;

        /// <summary>
        /// 現在開いているメイン画面ウィンドウタイプ
        /// </summary>
        private Window _openingWindow = Window.Nothing;
        #endregion

        #region フォームリサイズ関連の変数

        /// <summary>
        /// フォントを変更する際の最小フォントサイズ
        /// </summary>
        private const float MinFontSize = 8f;

        /// <summary>
        /// フォームリサイズ時に使用されるコントロール名のディクショナリー
        /// </summary>
        private readonly Dictionary<string, string> _controlNames = new();

        /// <summary>
        /// フォームリサイズ時に使用されるコントロールのデフォルトサイズ
        /// </summary>
        private readonly Dictionary<string, SizeF> _defaultControlSize = new();

        /// <summary>
        /// フォームリサイズ時に使用されるコントロールのデフォルト位置
        /// </summary>
        private readonly Dictionary<string, PointF> _defaultControlLocation = new();

        /// <summary>
        /// フォームリサイズ時に使用されるコントロールのデフォルトフォントサイズ
        /// </summary>
        private readonly Dictionary<string, float> _defaultFontSize = new();

        /// <summary>
        /// フォームの初期サイズ
        /// </summary>
        private readonly Size _initialFormSize;

        /// <summary>
        ///　メイン画面左のアバター欄の初期幅
        /// </summary>
        private readonly int _baseAvatarSearchFilterListWidth;

        /// <summary>
        /// メイン画面右のアイテム欄の初期幅
        /// </summary>
        private readonly int _baseAvatarItemExplorerListWidth;

        /// <summary>
        /// Get AvatarList Width
        /// </summary>
        /// <returns>AvatarList Width</returns>
        private int GetAvatarListWidth() => AvatarSearchFilterList.Width - _baseAvatarSearchFilterListWidth;

        /// <summary>
        /// Get ItemExplorerList Width
        /// </summary>
        /// <returns>ItemExplorerList Width</returns>
        private int GetItemExplorerListWidth() => AvatarItemExplorer.Width - _baseAvatarItemExplorerListWidth;
        #endregion

        #region バックアップ関連の変数

        /// <summary>
        /// バックアップする間隔(ms)
        /// </summary>
        private const int BackupInterval = 300000; // 5 Minutes

        /// <summary>
        /// 最後のバックアップ時刻を取得または設定します。
        /// </summary>
        private DateTime _lastBackupTime;

        /// <summary>
        /// 最後のバックアップ時にエラーが発生したかどうかを取得または設定します。
        /// </summary>
        private bool _lastBackupError;
        #endregion

        #region ソフトのステータスに関する変数
        /// <summary>
        /// 現在のソフトの言語
        /// </summary>
        public string CurrentLanguage = "ja-JP";

        /// <summary>
        /// 現在のパス
        /// </summary>
        public CurrentPath CurrentPath = new();

        /// <summary>
        /// 検索中かどうかを取得または設定します。
        /// </summary>
        private bool _isSearching;

        /// <summary>
        /// フォームが初期化されたかどうかを取得します。
        /// </summary>
        private readonly bool _initialized;
        #endregion

        #region フォームの初期化

        /// <summary>
        /// メインフォームを初期化します。
        /// </summary>
        public Main(LaunchInfo launchInfo)
        {
            try
            {
                Items = Helper.LoadItemsData();
                CommonAvatars = Helper.LoadCommonAvatarData();

                // Fix Supported Avatar Path (Title => Path)
                Items = Helper.FixSupportedAvatarPath(Items);

                // Update Empty Dates
                Items = Helper.UpdateEmptyDates(Items);

                AddFontFile();
                CustomCategories = Helper.LoadCustomCategoriesData();
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

                // Check if the software is launched with a URL
                if (launchInfo.launchedWithUrl)
                {
                    if (!string.IsNullOrEmpty(launchInfo.assetDir) && !string.IsNullOrEmpty(launchInfo.assetId))
                    {
                        AddItem addItem = new(this, ItemType.Avatar, null, false, null, launchInfo.assetDir, launchInfo.assetId);
                        addItem.ShowDialog();

                        Helper.SaveItemsData(Items);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ソフトの起動中にエラーが発生しました。\n\n" + ex,
                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// フォントファイルをソフトに追加します。
        /// </summary>
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
        #endregion

        #region 左のリスト関連の処理

        /// <summary>
        /// メイン画面左のアバター欄を作成します。
        /// </summary>
        private void GenerateAvatarList()
        {
            ResetAvatarPage(AvatarPage);

            var items = Items.Where(item => item.Type == ItemType.Avatar).ToArray();
            if (items.Length == 0) return;

            items = SortingBox.SelectedIndex switch
            {
                0 => items.OrderBy(item => item.Title).ToArray(),
                1 => items.OrderBy(item => item.AuthorName).ToArray(),
                2 => items.OrderByDescending(item => item.CreatedDate).ToArray(),
                3 => items.OrderByDescending(item => item.UpdatedDate).ToArray(),
                _ => items.OrderBy(item => item.Title).ToArray(),
            };

            var index = 0;
            foreach (Item item in items)
            {
                var description = item.Title;

                if (!string.IsNullOrEmpty(item.CreatedDate))
                {
                    description += "\n" + Helper.Translate("登録日時", CurrentLanguage) + ": " + item.CreatedDate;
                }

                if (!string.IsNullOrEmpty(item.UpdatedDate))
                {
                    description += "\n" + Helper.Translate("更新日時", CurrentLanguage) + ": " + item.UpdatedDate;
                }

                if (!string.IsNullOrEmpty(item.ItemMemo))
                {
                    description += "\n\n" + Helper.Translate("メモ: ", CurrentLanguage) + item.ItemMemo;
                }

                Button button = Helper.CreateButton(item.ImagePath, item.Title,
                    Helper.Translate("作者: ", CurrentLanguage) + item.AuthorName, true,
                    description, GetAvatarListWidth());
                button.Location = new Point(0, (70 * index) + 2);

                EventHandler clickEvent = (_, _) =>
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

                button.Click += clickEvent;
                button.Disposed += (_, _) =>
                {
                    button.Click -= clickEvent;
                    button.ContextMenuStrip?.Dispose();
                };

                ContextMenuStrip contextMenuStrip = new();

                if (item.BoothId != -1)
                {
                    ToolStripMenuItem toolStripMenuItem =
                        new(Helper.Translate("Boothリンクのコピー", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    EventHandler clickEvent2 = (_, _) =>
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

                    toolStripMenuItem.Click += clickEvent2;
                    toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent2;

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Boothリンクを開く", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    EventHandler clickEvent3 = (_, _) =>
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

                    toolStripMenuItem1.Click += clickEvent3;
                    toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent3;

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("この作者の他のアイテムを表示", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                EventHandler? clickEvent4 = (_, _) =>
                {
                    SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                    SearchItems();
                };

                toolStripMenuItem2.Click += clickEvent4;
                toolStripMenuItem2.Disposed += (_, _) => toolStripMenuItem2.Click -= clickEvent4;

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent5 = (_, _) =>
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

                toolStripMenuItem3.Click += clickEvent5;
                toolStripMenuItem3.Disposed += (_, _) => toolStripMenuItem3.Click -= clickEvent5;

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("編集", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent6 = (_, _) =>
                {
                    var prePath = item.ItemPath;

                    AddItem addItem = new(this, item.Type, item.CustomCategory, true, item, null);
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

                toolStripMenuItem4.Click += clickEvent6;
                toolStripMenuItem4.Disposed += (_, _) => toolStripMenuItem4.Click -= clickEvent6;

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("メモの追加", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent7 = (_, _) =>
                {
                    var previouseMemo = item.ItemMemo;
                    AddNote addMemo = new(this, item);
                    addMemo.ShowDialog();

                    var memo = addMemo.Memo;
                    if (string.IsNullOrEmpty(memo) || memo == previouseMemo) return;

                    item.ItemMemo = memo;

                    RefleshWindow();
                    Helper.SaveItemsData(Items);
                };

                toolStripMenuItem5.Click += clickEvent7;
                toolStripMenuItem5.Disposed += (_, _) => toolStripMenuItem5.Click -= clickEvent7;

                ToolStripMenuItem toolStripMenuItem6 = new(Helper.Translate("削除", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                EventHandler clickEvent8 = (_, _) =>
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
                                ResetAvatarExplorer(true);
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
                            ResetAvatarExplorer(true);
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

                toolStripMenuItem6.Click += clickEvent8;
                toolStripMenuItem6.Disposed += (_, _) => toolStripMenuItem6.Click -= clickEvent8;

                contextMenuStrip.Items.Add(toolStripMenuItem2);
                contextMenuStrip.Items.Add(toolStripMenuItem3);
                contextMenuStrip.Items.Add(toolStripMenuItem4);
                contextMenuStrip.Items.Add(toolStripMenuItem5);
                contextMenuStrip.Items.Add(toolStripMenuItem6);
                button.ContextMenuStrip = contextMenuStrip;

                AvatarPage.Controls.Add(button);
                index++;
            }
        }

        /// <summary>
        /// メイン画面左の作者欄を作成します。
        /// </summary>
        private void GenerateAuthorList()
        {
            ResetAvatarPage(AvatarAuthorPage);

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
                EventHandler clickEvent = (_, _) =>
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

                button.Click += clickEvent;
                button.Disposed += (_, _) =>
                {
                    button.Click -= clickEvent;
                    button.ContextMenuStrip?.Dispose();
                };

                ContextMenuStrip contextMenuStrip = new();

                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent1 = (_, _) =>
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

                toolStripMenuItem.Click += clickEvent1;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent1;

                contextMenuStrip.Items.Add(toolStripMenuItem);
                button.ContextMenuStrip = contextMenuStrip;
                AvatarAuthorPage.Controls.Add(button);
                index++;
            }
        }

        /// <summary>
        /// メイン画面左のカテゴリー欄を作成します。
        /// </summary>
        private void GenerateCategoryListLeft()
        {
            ResetAvatarPage(CategoryPage);

            var index = 0;
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                if (itemType is ItemType.Unknown or ItemType.Custom) continue;

                var items = Items.Where(item => item.Type == itemType);
                var itemCount = items.Count();
                Button button = Helper.CreateButton(null,
                    Helper.GetCategoryName(itemType, CurrentLanguage),
                    itemCount + Helper.Translate("個の項目", CurrentLanguage), true, "", GetAvatarListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                EventHandler clickEvent = (_, _) =>
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

                button.Click += clickEvent;
                button.Disposed += (_, _) => button.Click -= clickEvent;

                CategoryPage.Controls.Add(button);
                index++;
            }

            if (CustomCategories.Length == 0) return;

            foreach (var customCategory in CustomCategories)
            {
                var items = Items.Where(item => item.CustomCategory == customCategory);
                var itemCount = items.Count();

                Button button = Helper.CreateButton(null, customCategory,
                    itemCount + Helper.Translate("個の項目", CurrentLanguage), true, "", GetAvatarListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                EventHandler clickEvent = (_, _) =>
                {
                    CurrentPath = new CurrentPath
                    {
                        CurrentSelectedCategory = ItemType.Custom,
                        CurrentSelectedCustomCategory = customCategory
                    };
                    _authorMode = false;
                    _categoryMode = true;
                    SearchBox.Text = "";
                    SearchResultLabel.Text = "";
                    _isSearching = false;
                    GenerateItems();
                    PathTextBox.Text = GeneratePath();
                };

                button.Click += clickEvent;
                button.Disposed += (_, _) => button.Click -= clickEvent;

                CategoryPage.Controls.Add(button);
                index++;
            }
        }
        #endregion

        #region 右のリスト関連の処理
        /// <summary>
        /// メイン画面右のカテゴリ欄を作成します。
        /// </summary>
        private void GenerateCategoryList()
        {
            _openingWindow = Window.ItemCategoryList;
            ResetAvatarExplorer();

            var index = 0;
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                if (itemType is ItemType.Unknown or ItemType.Custom) continue;

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

                EventHandler clickEvent = (_, _) =>
                {
                    CurrentPath.CurrentSelectedCategory = itemType;
                    GenerateItems();
                    PathTextBox.Text = GeneratePath();
                };

                button.Click += clickEvent;
                button.Disposed += (_, _) => button.Click -= clickEvent;

                AvatarItemExplorer.Controls.Add(button);
                index++;
            }

            if (CustomCategories.Length == 0) return;
            foreach (var customCategory in CustomCategories)
            {
                var itemCount = 0;
                if (_authorMode)
                {
                    itemCount = Items.Count(item =>
                        item.CustomCategory == customCategory &&
                        item.AuthorName == CurrentPath.CurrentSelectedAuthor?.AuthorName
                    );
                }
                else
                {
                    itemCount = Items.Count(item =>
                        item.CustomCategory == customCategory &&
                        (
                            Helper.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath)
                                .IsSupportedOrCommon ||
                            item.SupportedAvatar.Length == 0 || CurrentPath.CurrentSelectedAvatar == "*"
                        )
                    );
                }

                if (itemCount == 0) continue;

                Button button = Helper.CreateButton(null, customCategory,
                    itemCount + Helper.Translate("個の項目", CurrentLanguage), false, "", GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                EventHandler clickEvent = (_, _) =>
                {
                    CurrentPath.CurrentSelectedCategory = ItemType.Custom;
                    CurrentPath.CurrentSelectedCustomCategory = customCategory;
                    GenerateItems();
                    PathTextBox.Text = GeneratePath();
                };

                button.Click += clickEvent;
                button.Disposed += (_, _) => button.Click -= clickEvent;

                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        /// <summary>
        /// メイン画面右のアイテム欄を作成します。
        /// </summary>
        private void GenerateItems()
        {
            _openingWindow = Window.ItemList;
            ResetAvatarExplorer();

            var filteredItems = Items.AsEnumerable();

            if (_authorMode)
            {
                filteredItems = Items.Where(item =>
                    item.Type == CurrentPath.CurrentSelectedCategory && (item.Type != ItemType.Custom || item.CustomCategory == CurrentPath.CurrentSelectedCustomCategory) &&
                    item.AuthorName == CurrentPath.CurrentSelectedAuthor?.AuthorName
                );
            }
            else if (_categoryMode)
            {
                filteredItems = Items.Where(item =>
                    item.Type == CurrentPath.CurrentSelectedCategory && (item.Type != ItemType.Custom || item.CustomCategory == CurrentPath.CurrentSelectedCustomCategory)
                );
            }
            else
            {
                filteredItems = Items.Where(item =>
                    item.Type == CurrentPath.CurrentSelectedCategory && (item.Type != ItemType.Custom || item.CustomCategory == CurrentPath.CurrentSelectedCustomCategory) &&
                    (
                        Helper.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath)
                            .IsSupportedOrCommon ||
                        item.SupportedAvatar.Length == 0 || CurrentPath.CurrentSelectedAvatar == "*"
                    )
                );
            }

            filteredItems = SortingBox.SelectedIndex switch
            {
                0 => filteredItems.OrderBy(item => item.Title).ToArray(),
                1 => filteredItems.OrderBy(item => item.AuthorName).ToArray(),
                2 => filteredItems.OrderByDescending(item => item.CreatedDate).ToArray(),
                3 => filteredItems.OrderByDescending(item => item.UpdatedDate).ToArray(),
                _ => filteredItems.OrderBy(item => item.Title).ToArray(),
            };

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

                var description = item.Title;

                if (!string.IsNullOrEmpty(item.CreatedDate))
                {
                    description += "\n" + Helper.Translate("登録日時", CurrentLanguage) + ": " + item.CreatedDate;
                }

                if (!string.IsNullOrEmpty(item.UpdatedDate))
                {
                    description += "\n" + Helper.Translate("更新日時", CurrentLanguage) + ": " + item.UpdatedDate;
                }

                if (!string.IsNullOrEmpty(item.ItemMemo))
                {
                    description += "\n\n" + Helper.Translate("メモ: ", CurrentLanguage) + item.ItemMemo;
                }

                Button button = Helper.CreateButton(item.ImagePath, item.Title, authorText, false, description,
                    GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                EventHandler clickEvent = (_, _) =>
                {
                    if (!Directory.Exists(item.ItemPath))
                    {
                        var prePath = item.ItemPath;
                        DialogResult result =
                            MessageBox.Show(Helper.Translate("フォルダが見つかりませんでした。編集しますか？", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Error);
                        if (result != DialogResult.Yes) return;

                        AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, true, item, null);
                        addItem.ShowDialog();

                        if (!Directory.Exists(item.ItemPath))
                        {
                            MessageBox.Show(Helper.Translate("フォルダが見つかりませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

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

                button.Click += clickEvent;
                button.Disposed += (_, _) => button.Click -= clickEvent;

                ContextMenuStrip contextMenuStrip = new();

                if (Directory.Exists(item.ItemPath))
                {
                    ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("フォルダを開く", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.OpenIcon));
                    EventHandler clickEvent1 = (_, _) =>
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

                    toolStripMenuItem.Click += clickEvent1;
                    toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent1;

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                }

                if (item.BoothId != -1)
                {
                    ToolStripMenuItem toolStripMenuItem =
                        new(Helper.Translate("Boothリンクのコピー", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    EventHandler clickEvent2 = (_, _) =>
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

                    toolStripMenuItem.Click += clickEvent2;
                    toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent2;

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Boothリンクを開く", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    EventHandler clickEvent3 = (_, _) =>
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

                    toolStripMenuItem1.Click += clickEvent3;
                    toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent3;

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("この作者の他のアイテムを表示", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                EventHandler clickEvent4 = (_, _) =>
                {
                    SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                    SearchItems();
                };

                toolStripMenuItem2.Click += clickEvent4;
                toolStripMenuItem2.Disposed += (_, _) => toolStripMenuItem2.Click -= clickEvent4;

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent5 = (_, _) =>
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

                toolStripMenuItem3.Click += clickEvent5;
                toolStripMenuItem3.Disposed += (_, _) => toolStripMenuItem3.Click -= clickEvent5;

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("編集", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent6 = (_, _) =>
                {
                    var prePath = item.ItemPath;

                    AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, true, item, null);
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

                toolStripMenuItem4.Click += clickEvent6;
                toolStripMenuItem4.Disposed += (_, _) => toolStripMenuItem4.Click -= clickEvent6;

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("メモの追加", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent7 = (_, _) =>
                {
                    var previouseMemo = item.ItemMemo;
                    AddNote addMemo = new(this, item);
                    addMemo.ShowDialog();

                    var memo = addMemo.Memo;
                    if (string.IsNullOrEmpty(memo) || memo == previouseMemo) return;

                    item.ItemMemo = memo;

                    GenerateAuthorList();
                    GenerateItems();
                    Helper.SaveItemsData(Items);
                };

                toolStripMenuItem5.Click += clickEvent7;
                toolStripMenuItem5.Disposed += (_, _) => toolStripMenuItem5.Click -= clickEvent7;

                ToolStripMenuItem toolStripMenuItem6 = new(Helper.Translate("削除", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                EventHandler clickEvent8 = (_, _) =>
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
                        ResetAvatarExplorer(true);
                    }
                    else
                    {
                        RefleshWindow();
                    }

                    Helper.SaveItemsData(Items);
                };

                toolStripMenuItem6.Click += clickEvent8;
                toolStripMenuItem6.Disposed += (_, _) => toolStripMenuItem6.Click -= clickEvent8;

                contextMenuStrip.Items.Add(toolStripMenuItem2);
                contextMenuStrip.Items.Add(toolStripMenuItem3);
                contextMenuStrip.Items.Add(toolStripMenuItem4);
                contextMenuStrip.Items.Add(toolStripMenuItem5);
                contextMenuStrip.Items.Add(toolStripMenuItem6);
                button.ContextMenuStrip = contextMenuStrip;
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        /// <summary>
        /// メイン画面右のアイテム内のフォルダーカテゴリ欄を作成します。
        /// </summary>
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

            ResetAvatarExplorer();

            var index = 0;
            foreach (var itemType in types)
            {
                var itemCount = itemFolderInfo.GetItemCount(itemType);
                if (itemCount == 0) continue;

                Button button = Helper.CreateButton(null,
                    Helper.Translate(itemType, CurrentLanguage), itemCount + Helper.Translate("個の項目", CurrentLanguage),
                    false, "", GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                EventHandler clickEvent = (_, _) =>
                {
                    CurrentPath.CurrentSelectedItemCategory = itemType;
                    GenerateItemFiles();
                    PathTextBox.Text = GeneratePath();
                };

                button.Click += clickEvent;
                button.Disposed += (_, _) => button.Click -= clickEvent;

                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        /// <summary>
        /// メイン画面右のアイテム内のファイル欄を作成します。
        /// </summary>
        private void GenerateItemFiles()
        {
            _openingWindow = Window.ItemFolderItemsList;
            ResetAvatarExplorer();

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

                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("開く", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent = (_, _) =>
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

                toolStripMenuItem.Click += clickEvent;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent;

                ToolStripMenuItem toolStripMenuItem1 = new(Helper.Translate("ファイルのパスを開く", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent1 = (_, _) =>
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
                };

                toolStripMenuItem1.Click += clickEvent1;
                toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent1;

                contextMenuStrip.Items.Add(toolStripMenuItem);
                contextMenuStrip.Items.Add(toolStripMenuItem1);
                button.ContextMenuStrip = contextMenuStrip;

                EventHandler clickEvent2 = (_, _) =>
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

                button.Click += clickEvent2;
                button.Disposed += (_, _) => button.Click -= clickEvent2;

                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }
        #endregion

        #region 検索関連の処理
        /// <summary>
        /// 検索ボックスに入力された文字列を元にアイテムを検索します。
        /// </summary>
        /// <param name="searchFilter"></param>
        private void GenerateFilteredItem(SearchFilter searchFilter)
        {
            ResetAvatarExplorer();

            var filteredItems = Items.Where(item =>
            {
                if (searchFilter.Author.Length != 0 && !searchFilter.Author.Contains(item.AuthorName))
                    return false;

                if (searchFilter.Title.Length != 0 && !searchFilter.Title.Contains(item.Title))
                    return false;

                if (searchFilter.BoothId.Length != 0 && !searchFilter.BoothId.Contains(item.BoothId.ToString()))
                    return false;

                if (searchFilter.Avatar.Length != 0 && !searchFilter.Avatar.Any(avatar =>
                {
                    return item.SupportedAvatar.Any(supportedAvatar =>
                    {
                        var supportedAvatarName = Helper.GetAvatarNameFromPath(Items, supportedAvatar);
                        if (supportedAvatarName == "") return false;
                        return supportedAvatarName.ToLower().Contains(avatar.ToLower());
                    });
                }))
                {
                    return false;
                }

                if (searchFilter.Category.Length != 0 && !searchFilter.Category.Any(category =>
                {
                    var translatedCategory = Helper.GetCategoryName(item.Type, CurrentLanguage);
                    return translatedCategory.Contains(category) || item.CustomCategory.Contains(category);

                }))
                {
                    return false;
                }

                if (searchFilter.ItemMemo.Length != 0 && !searchFilter.ItemMemo.Any(memo =>
                    {
                        return item.ItemMemo.ToLower().Contains(memo.ToLower());
                    }))
                {
                    return false;
                }

                return true;
            });

            filteredItems = filteredItems
                .Where(item =>
                    searchFilter.SearchWords.All(word =>
                        item.Title.ToLower().Contains(word.ToLower()) ||
                        item.AuthorName.ToLower().Contains(word.ToLower()) ||
                        item.SupportedAvatar.Any(avatar =>
                        {
                            var supportedAvatarName = Helper.GetAvatarNameFromPath(Items, avatar);
                            if (supportedAvatarName == "") return false;
                            return supportedAvatarName.ToLower().Contains(word.ToLower());
                        }) ||
                        item.BoothId.ToString().Contains(word.ToLower()) ||
                        item.ItemMemo.ToLower().Contains(word.ToLower())
                    )
                )
                .OrderByDescending(item =>
                {
                    var matchCount = 0;
                    foreach (var word in searchFilter.SearchWords)
                    {
                        if (item.Title.ToLower().Contains(word.ToLower())) matchCount++;
                        if (item.AuthorName.ToLower().Contains(word.ToLower())) matchCount++;
                        if (item.SupportedAvatar.Any(avatar =>
                        {
                            var supportedAvatarName = Helper.GetAvatarNameFromPath(Items, avatar);
                            if (supportedAvatarName == "") return false;
                            return supportedAvatarName.ToLower().Contains(word.ToLower());
                        })) matchCount++;
                        if (item.BoothId.ToString().Contains(word.ToLower())) matchCount++;
                        if (item.ItemMemo.ToLower().Contains(word.ToLower())) matchCount++;
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
                var description = item.Title;

                if (!string.IsNullOrEmpty(item.CreatedDate))
                {
                    description += "\n" + Helper.Translate("登録日時", CurrentLanguage) + ": " + item.CreatedDate;
                }

                if (!string.IsNullOrEmpty(item.UpdatedDate))
                {
                    description += "\n" + Helper.Translate("更新日時", CurrentLanguage) + ": " + item.UpdatedDate;
                }

                if (!string.IsNullOrEmpty(item.ItemMemo))
                {
                    description += "\n\n" + Helper.Translate("メモ: ", CurrentLanguage) + item.ItemMemo;
                }

                Button button = Helper.CreateButton(item.ImagePath, item.Title,
                    Helper.Translate("作者: ", CurrentLanguage) + item.AuthorName, false,
                    description, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                EventHandler clickEvent = (_, _) =>
                {
                    if (!Directory.Exists(item.ItemPath))
                    {
                        var prePath = item.ItemPath;
                        DialogResult result =
                            MessageBox.Show(Helper.Translate("フォルダが見つかりませんでした。編集しますか？", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Error);
                        if (result != DialogResult.Yes) return;

                        AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, true, item, null);
                        addItem.ShowDialog();

                        if (!Directory.Exists(item.ItemPath))
                        {
                            MessageBox.Show(Helper.Translate("フォルダが見つかりませんでした。", CurrentLanguage),
                                Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

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

                button.Click += clickEvent;
                button.Disposed += (_, _) =>
                {
                    button.Click -= clickEvent;
                    button.ContextMenuStrip?.Dispose();
                };

                ContextMenuStrip contextMenuStrip = new();

                if (Directory.Exists(item.ItemPath))
                {
                    ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("フォルダを開く", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.OpenIcon));
                    EventHandler clickEvent1 = (_, _) =>
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

                    toolStripMenuItem.Click += clickEvent1;
                    toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent1;

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                }

                if (item.BoothId != -1)
                {
                    ToolStripMenuItem toolStripMenuItem =
                        new(Helper.Translate("Boothリンクのコピー", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    EventHandler clickEvent2 = (_, _) =>
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

                    toolStripMenuItem.Click += clickEvent2;
                    toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent2;

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Boothリンクを開く", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    EventHandler clickEvent3 = (_, _) =>
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

                    toolStripMenuItem1.Click += clickEvent3;
                    toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent3;

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("この作者の他のアイテムを表示", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                EventHandler clickEvent4 = (_, _) =>
                {
                    SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                    SearchItems();
                };

                toolStripMenuItem2.Click += clickEvent4;
                toolStripMenuItem2.Disposed += (_, _) => toolStripMenuItem2.Click -= clickEvent4;

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent5 = (_, _) =>
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

                toolStripMenuItem3.Click += clickEvent5;
                toolStripMenuItem3.Disposed += (_, _) => toolStripMenuItem3.Click -= clickEvent5;

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("編集", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent6 = (_, _) =>
                {
                    var prePath = item.ItemPath;
                    AddItem addItem = new(this, item.Type, item.CustomCategory, true, item, null);
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

                toolStripMenuItem4.Click += clickEvent6;
                toolStripMenuItem4.Disposed += (_, _) => toolStripMenuItem4.Click -= clickEvent6;

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("メモの追加", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent7 = (_, _) =>
                {
                    var previouseMemo = item.ItemMemo;
                    AddNote addMemo = new(this, item);
                    addMemo.ShowDialog();

                    var memo = addMemo.Memo;
                    if (string.IsNullOrEmpty(memo) || memo == previouseMemo) return;

                    item.ItemMemo = memo;

                    GenerateFilteredItem(searchFilter);
                    GenerateAvatarList();
                    Helper.SaveItemsData(Items);
                };

                ToolStripMenuItem toolStripMenuItem6 = new(Helper.Translate("削除", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                EventHandler clickEvent8 = (_, _) =>
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

                toolStripMenuItem6.Click += clickEvent8;
                toolStripMenuItem6.Disposed += (_, _) => toolStripMenuItem6.Click -= clickEvent8;

                contextMenuStrip.Items.Add(toolStripMenuItem2);
                contextMenuStrip.Items.Add(toolStripMenuItem3);
                contextMenuStrip.Items.Add(toolStripMenuItem4);
                contextMenuStrip.Items.Add(toolStripMenuItem5);
                contextMenuStrip.Items.Add(toolStripMenuItem6);
                button.ContextMenuStrip = contextMenuStrip;
                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }

        /// <summary>
        /// 検索ボックスに入力された文字列を元にアイテムフォルダー内を検索します。
        /// </summary>
        /// <param name="searchWords"></param>
        private void GenerateFilteredFolderItems(SearchFilter searchWords)
        {
            ResetAvatarExplorer();

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

                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("開く", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent = (_, _) =>
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

                toolStripMenuItem.Click += clickEvent;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent;

                ToolStripMenuItem toolStripMenuItem1 = new(Helper.Translate("ファイルのパスを開く", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent1 = (_, _) =>
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
                };

                toolStripMenuItem1.Click += clickEvent1;
                toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent1;

                contextMenuStrip.Items.Add(toolStripMenuItem);
                contextMenuStrip.Items.Add(toolStripMenuItem1);
                button.ContextMenuStrip = contextMenuStrip;

                EventHandler clickEvent2 = (_, _) =>
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

                button.Click += clickEvent2;
                button.Disposed += (_, _) => button.Click -= clickEvent2;

                AvatarItemExplorer.Controls.Add(button);
                index++;
            }
        }
        #endregion

        #region アイテム追加関連の処理

        /// <summary>
        /// アイテム追加ボタンが押された際の処理を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddItemButton_Click(object sender, EventArgs e)
        {
            AddItem addItem = new AddItem(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, false, null, null);
            addItem.ShowDialog();
            RefleshWindow();
            Helper.SaveItemsData(Items);
        }
        #endregion

        #region パス関連の処理

        /// <summary>
        /// 現在のパスを生成します。
        /// </summary>
        /// <returns></returns>
        private string GeneratePath()
        {
            var categoryName = Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage, CurrentPath.CurrentSelectedCustomCategory);

            if (_authorMode)
            {
                if (CurrentPath.CurrentSelectedAuthor == null)
                    return Helper.Translate("ここには現在のパスが表示されます", CurrentLanguage);
                if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
                    return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName);
                if (CurrentPath.CurrentSelectedItem == null)
                    return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + " / " +
                           categoryName;
                if (CurrentPath.CurrentSelectedItemCategory == null)
                    return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + " / " +
                           categoryName + " / " +
                           Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAuthor.AuthorName) + " / " +
                       categoryName + " / " +
                       Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + " / " +
                       Helper.Translate(CurrentPath.CurrentSelectedItemCategory, CurrentLanguage);
            }

            if (_categoryMode)
            {
                if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
                    return Helper.Translate("ここには現在のパスが表示されます", CurrentLanguage);
                if (CurrentPath.CurrentSelectedItem == null)
                    return categoryName;
                if (CurrentPath.CurrentSelectedItemCategory == null)
                    return categoryName + " / " +
                           Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

                return categoryName + " / " +
                       Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + " / " +
                       Helper.Translate(CurrentPath.CurrentSelectedItemCategory, CurrentLanguage);
            }

            if (CurrentPath.CurrentSelectedAvatar == null) return Helper.Translate("ここには現在のパスが表示されます", CurrentLanguage);
            if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar);
            if (CurrentPath.CurrentSelectedItem == null)
                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar) + " / " +
                       categoryName;
            if (CurrentPath.CurrentSelectedItemCategory == null)
                return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar) + " / " +
                       categoryName + " / " +
                       Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

            return Helper.RemoveFormat(CurrentPath.CurrentSelectedAvatar) + " / " +
                   categoryName + " / " +
                   Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + " / " +
                   Helper.Translate(CurrentPath.CurrentSelectedItemCategory, CurrentLanguage);
        }

        /// <summary>
        /// 選択されたアイテムからパスを生成します。
        /// </summary>
        /// <param name="item"></param>
        private void GeneratePathFromItem(Item item)
        {
            var avatarPath = item.SupportedAvatar.FirstOrDefault();
            var avatarName = Helper.GetAvatarName(Items, avatarPath);
            CurrentPath.CurrentSelectedAvatar = avatarName ?? "*";
            CurrentPath.CurrentSelectedAvatarPath = avatarPath;
            CurrentPath.CurrentSelectedCategory = item.Type;
            if (item.Type == ItemType.Custom)
                CurrentPath.CurrentSelectedCustomCategory = item.CustomCategory;
            CurrentPath.CurrentSelectedItem = item;
        }
        #endregion

        #region 戻るボタンの処理

        /// <summary>
        /// 戻るボタンが押された際の処理を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                ResetAvatarExplorer(true);
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
                    CurrentPath.CurrentSelectedCustomCategory = string.Empty;
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
                    CurrentPath.CurrentSelectedCustomCategory = string.Empty;
                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                    return;
                }

                if (CurrentPath.CurrentSelectedAvatar == "*")
                {
                    CurrentPath.CurrentSelectedAvatar = null;
                    CurrentPath.CurrentSelectedAvatarPath = null;
                    ResetAvatarExplorer(true);
                    PathTextBox.Text = GeneratePath();
                    return;
                }
            }

            ResetAvatarExplorer(true);
            PathTextBox.Text = GeneratePath();
        }
        #endregion

        #region 検索ボックスの処理

        /// <summary>
        /// 検索ボックスで検索対象キーが押された際の処理を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode is Keys.Enter or Keys.Space)
            {
                SearchItems();
            }
        }

        /// <summary>
        /// 検索ボックス内のテキストを元に検索処理を行います。
        /// </summary>
        private void SearchItems()
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                SearchResultLabel.Text = "";
                PathTextBox.Text = GeneratePath();
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

                ResetAvatarExplorer(true);
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

            if (searchFilter.Avatar.Length != 0)
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("アバター", CurrentLanguage) + ": " +
                                                 string.Join(", ", searchFilter.Avatar))
                    .ToArray();
            }

            if (searchFilter.Category.Length != 0)
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("カテゴリ", CurrentLanguage) + ": " +
                                                 string.Join(", ", searchFilter.Category))
                    .ToArray();
            }

            if (searchFilter.ItemMemo.Length != 0)
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("メモ", CurrentLanguage) + ": " +
                                                 string.Join(", ", searchFilter.ItemMemo))
                    .ToArray();
            }

            pathTextArr = pathTextArr.Append(string.Join(", ", searchFilter.SearchWords)).ToArray();

            PathTextBox.Text = Helper.Translate("検索中... - ", CurrentLanguage) + string.Join(" / ", pathTextArr);
        }
        #endregion

        #region リセット関連の処理

        /// <summary>
        /// メイン画面右の画面をリセットします。
        /// </summary>
        /// <param name="startLabelVisible"></param>
        private void ResetAvatarExplorer(bool startLabelVisible = false)
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

            _openingWindow = Window.Nothing;
        }

        /// <summary>
        /// メイン画面左の画面をリセットします。
        /// </summary>
        /// <param name="page"></param>
        private static void ResetAvatarPage(Control page)
        {
            for (int i = page.Controls.Count - 1; i >= 0; i--)
            {
                var control = page.Controls[i];
                page.Controls.RemoveAt(i);
                control.Dispose();
            }
        }

        /// <summary>
        /// メイン画面の全ての画面を読み込み直します。
        /// </summary>
        /// <param name="reloadLeft"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
        #endregion

        #region ドラッグアンドドロップ関連の処理

        /// <summary>
        /// メイン画面右の欄にドラッグされた際の処理を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AvatarItemExplorer_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? dragFilePathArr = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (dragFilePathArr == null) return;
            var folderPath = dragFilePathArr[0];

            if (!(File.Exists(folderPath) && folderPath.EndsWith(".zip")) && !Directory.Exists(folderPath))
            {
                MessageBox.Show(Helper.Translate("フォルダを選択してください", CurrentLanguage),
                    Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, false, null, folderPath);
            EventHandler itemAdded = (_, _) =>
            {
                RefleshWindow();
                Helper.SaveItemsData(Items);
                Enabled = true;
            };

            addItem.ItemAdded += itemAdded;
            addItem.FormClosed += (_, _) => addItem.ItemAdded -= itemAdded;
            addItem.Show();
            Enabled = false;
        }

        /// <summary>
        /// メイン画面左の欄にドラッグされた際の処理を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AvatarPage_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? dragFilePathArr = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (dragFilePathArr == null) return;
            var folderPath = dragFilePathArr[0];

            if (!(File.Exists(folderPath) && folderPath.EndsWith(".zip")) && !Directory.Exists(folderPath))
            {
                MessageBox.Show(Helper.Translate("フォルダを選択してください", CurrentLanguage),
                    Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AddItem addItem = new(this, ItemType.Avatar, null, false, null, folderPath);
            addItem.ItemAdded += (_, _) =>
            {
                RefleshWindow();
                Helper.SaveItemsData(Items);
                Enabled = true;
            };
            addItem.Show();
            Enabled = false;
        }
        #endregion

        #region 画面下部のボタン関連の処理

        /// <summary>
        /// CSVエクスポートボタンが押された際の処理を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                sw.WriteLine("Title,AuthorName,AuthorImageFilePath,ImagePath,Type,Memo,SupportedAvatar,BoothId,ItemPath");
                foreach (var item in Items)
                {
                    string[] avatarNames = Array.Empty<string>();
                    foreach (var avatar in item.SupportedAvatar)
                    {
                        var avatarName = Helper.GetAvatarName(Items, avatar);
                        if (avatarName == null) continue;
                        avatarNames = avatarNames.Append(avatarName).ToArray();
                    }

                    var itemTitle = Helper.EscapeCsv(item.Title);
                    var authorName = Helper.EscapeCsv(item.AuthorName);
                    var authorImageFilePath = Helper.EscapeCsv(item.AuthorImageFilePath);
                    var imagePath = Helper.EscapeCsv(item.ImagePath);
                    var type = Helper.EscapeCsv(Helper.GetCategoryName(item.Type, CurrentLanguage, item.CustomCategory));
                    var memo = Helper.EscapeCsv(item.ItemMemo);
                    var supportedAvatar = Helper.EscapeCsv(string.Join(";", avatarNames));
                    var boothId = Helper.EscapeCsv(item.BoothId.ToString());
                    var itemPath = Helper.EscapeCsv(item.ItemPath);

                    sw.WriteLine($"{itemTitle},{authorName},{authorImageFilePath},{imagePath},{type},{memo},{supportedAvatar},{boothId},{itemPath}");
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

        /// <summary>
        /// バックアップボタンが押された際の処理を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 言語変更ボックスの選択状況が更新された際の処理を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            string[] sortingItems = { "タイトル", "作者", "登録日時", "更新日時" };
            var selected = SortingBox.SelectedIndex;
            SortingBox.Items.Clear();
            SortingBox.Items.AddRange(sortingItems.Select(item => Helper.Translate(item, CurrentLanguage)).ToArray());
            SortingBox.SelectedIndex = selected;

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

            PathTextBox.Text = GeneratePath();
            RefleshWindow();
        }

        /// <summary>
        /// 並び替え順の選択状況が更新された際の処理を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortingBox_SelectedIndexChanged(object sender, EventArgs e) => RefleshWindow();

        /// <summary>
        /// データを読み込むボタンが押された際の処理を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadData_Click(object sender, EventArgs e) => LoadDataFromFolder();

        /// <summary>
        /// フォルダ選択ダイアログを表示し、選択されたフォルダからデータを読み込みます。
        /// </summary>
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
                        Items = Helper.UpdateEmptyDates(Items);
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

                    var customCategoryPath = fbd.SelectedPath + "/CustomCategory.txt";
                    if (!File.Exists(customCategoryPath))
                    {
                        MessageBox.Show(Helper.Translate("カスタムカテゴリーファイルが見つかりませんでした。", CurrentLanguage),
                            Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        CustomCategories = Helper.LoadCustomCategoriesData(customCategoryPath);
                        Helper.SaveCustomCategoriesData(CustomCategories);
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
                        Items = Helper.UpdateEmptyDates(Items);
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

                    var customCategoryPath = fbd.SelectedPath + "/CustomCategory.txt";
                    if (!File.Exists(customCategoryPath))
                    {
                        MessageBox.Show(Helper.Translate("カスタムカテゴリーファイルが見つかりませんでした。", CurrentLanguage),
                            Helper.Translate("エラー", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        CustomCategories = Helper.LoadCustomCategoriesData(customCategoryPath);
                        Helper.SaveCustomCategoriesData(CustomCategories);
                    }

                    var result2 = MessageBox.Show(
                        Helper.Translate("Thumbnailフォルダ、AuthorImageフォルダ、Itemsフォルダもコピーしますか？", CurrentLanguage),
                        Helper.Translate("確認", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result2 != DialogResult.Yes)
                    {
                        SearchBox.Text = "";
                        SearchResultLabel.Text = "";
                        _isSearching = false;
                        GenerateAvatarList();
                        GenerateAuthorList();
                        GenerateCategoryListLeft();
                        ResetAvatarExplorer(true);
                        PathTextBox.Text = GeneratePath();
                        MessageBox.Show(Helper.Translate("コピーが完了しました。", CurrentLanguage),
                            Helper.Translate("完了", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    var thumbnailPath = fbd.SelectedPath + "/Thumbnail";
                    var authorImagePath = fbd.SelectedPath + "/AuthorImage";
                    var itemsPath = fbd.SelectedPath + "/Items";

                    var thumbnailResult = true;
                    var authorImageResult = true;
                    var itemsResult = true;
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
                                thumbnailResult = false;
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
                                authorImageResult = false;
                            }
                        }
                    }

                    if (Directory.Exists(itemsPath))
                    {
                        try
                        {
                            Helper.CopyDirectory(itemsPath, "./Datas/Items");
                        }
                        catch (Exception ex)
                        {
                            Helper.ErrorLogger("Itemsのコピーに失敗しました。", ex);
                            itemsResult = false;
                        }
                    }

                    MessageBox.Show(Helper.Translate("コピーが完了しました。", CurrentLanguage) + "\n\n" + Helper.Translate("コピー失敗一覧: ", CurrentLanguage) +
                                    (thumbnailResult ? "" : "\n" + Helper.Translate("サムネイルのコピーに一部失敗しています。", CurrentLanguage)) +
                                    (authorImageResult ? "" : "\n" + Helper.Translate("作者画像のコピーに一部失敗しています。", CurrentLanguage)) +
                                    (itemsResult ? "" : "\n" + Helper.Translate("Itemsのコピーに一部失敗しています。", CurrentLanguage)),
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
            ResetAvatarExplorer(true);
            PathTextBox.Text = GeneratePath();
        }

        /// <summary>
        /// 共通素体管理ボタンが押された際の処理を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManageCommonAvatarButton_Click(object sender, EventArgs e)
        {
            ManageCommonAvatars manageCommonAvatar = new(this);
            manageCommonAvatar.ShowDialog();
            RefleshWindow();
            PathTextBox.Text = GeneratePath();
            Helper.SaveCommonAvatarData(CommonAvatars);
        }
        #endregion

        #region リサイズ関連の処理

        /// <summary>
        /// フォームのコントロールのフォントを変更します。
        /// </summary>
        /// <param name="control"></param>
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

        /// <summary>
        /// フォームのリサイズ時にコントロールのサイズや位置を変更します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_Resize(object sender, EventArgs e) => ResizeControl();

        /// <summary>
        /// コントロールのサイズや位置を変更します。
        /// </summary>
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

        /// <summary>
        /// メインフォーム内のボタンサイズを変更します。
        /// </summary>
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
        #endregion

        #region バックアップ関連の処理

        /// <summary>
        /// ファイルの自動バックアップを行います。
        /// </summary>
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

        /// <summary>
        /// 最終バックアップ時刻をフォームタイトルに表示します。
        /// </summary>
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

        /// <summary>
        /// ファイルのバックアップを行います。
        /// </summary>
        private void BackupFile()
        {
            try
            {
                var backupFilesArray = new[]
                {
                    "./Datas/ItemsData.json",
                    "./Datas/CommonAvatar.json",
                    "./Datas/CustomCategory.txt"
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
        #endregion

        #region フォルダーが閉じられる際の処理

        /// <summary>
        /// フォームが閉じられる際に一時フォルダを削除します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        #endregion
    }
}