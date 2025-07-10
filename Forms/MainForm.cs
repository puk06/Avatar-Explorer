using Avatar_Explorer.Models;
using Avatar_Explorer.Utils;
using System.Drawing.Text;
using System.IO.Compression;
using System.Media;
using System.Text;
using Timer = System.Windows.Forms.Timer;

namespace Avatar_Explorer.Forms;

internal sealed partial class MainForm : Form
{
    #region フォームのテキスト関連の変数

    /// <summary>
    /// ソフトの現在のバージョン
    /// </summary>
    private const string CurrentVersion = "v1.1.1";

    /// <summary>
    /// デフォルトのフォームテキスト
    /// </summary>
    private const string CurrentVersionFormText = $"VRChat Avatar Explorer {CurrentVersion} by ぷこるふ";

    #endregion

    #region ソフトのデータベース関連の変数

    /// <summary>
    /// アイテムデータベース
    /// </summary>
    internal List<Item> Items;

    /// <summary>
    /// 共通素体データベース
    /// </summary>
    internal List<CommonAvatar> CommonAvatars;

    /// <summary>
    /// カスタムカテゴリーデータベース
    /// </summary>
    internal List<string> CustomCategories;

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
    internal FontFamily? GuiFont;
    #endregion

    #region 現在のウィンドウの種類に関する変数
    /// <summary>
    /// 現在開かれている左ウィンドウのタイプを取得または設定します。
    /// </summary>

    private LeftWindow _leftWindow = LeftWindow.Default;

    /// <summary>
    /// 現在開いているメイン画面ウィンドウタイプ
    /// </summary>
    private Window _openingWindow = Window.Nothing;
    #endregion

    #region フォームリサイズ関連の変数
    /// <summary>
    /// フォームリサイズ時に使用されるコントロール名のディクショナリー
    /// </summary>
    private readonly Dictionary<string, string> _controlNames = new();

    /// <summary>
    /// フォームリサイズ時に使用されるコントロールのデフォルトサイズ
    /// </summary>
    private readonly Dictionary<string, ControlScale> _defaultControlSize = new();

    /// <summary>
    /// フォームの初期サイズ
    /// </summary>
    private readonly Size _initialFormSize;

    /// <summary>
    /// メイン画面左のアバター欄の初期幅
    /// </summary>
    private readonly int _baseAvatarSearchFilterListWidth;

    /// <summary>
    /// メイン画面右のアイテム欄の初期幅
    /// </summary>
    private readonly int _baseAvatarItemExplorerListWidth;

    /// <summary>
    /// リサイズ用のタイマー
    /// </summary>
    private readonly Timer _resizeTimer = new()
    {
        Interval = 100
    };

    /// <summary>
    /// Get AvatarList Width
    /// </summary>
    /// <returns>AvatarList Width</returns>
    private int GetAvatarListWidth
        => AvatarSearchFilterList.Width - _baseAvatarSearchFilterListWidth;

    /// <summary>
    /// Get ItemExplorerList Width
    /// </summary>
    /// <returns>ItemExplorerList Width</returns>
    private int GetItemExplorerListWidth
        => AvatarItemExplorer.Width - _baseAvatarItemExplorerListWidth;
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
    internal string CurrentLanguage = "ja-JP";

    /// <summary>
    /// 現在のパス
    /// </summary>
    internal CurrentPath CurrentPath = new();

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
    internal MainForm(LaunchInfo launchInfo)
    {
        try
        {
            Items = DatabaseUtils.LoadItemsData();
            CommonAvatars = DatabaseUtils.LoadCommonAvatarData();
            CustomCategories = DatabaseUtils.LoadCustomCategoriesData();

            // Fix Supported Avatar Path (Title => Path)
            DatabaseUtils.FixSupportedAvatarPath(ref Items);

            // Update Empty Dates
            DatabaseUtils.UpdateEmptyDates(ref Items);

            // Fix Item Dates
            DatabaseUtils.FixItemDates(ref Items);

            // Fix Relative Path Escape
            DatabaseUtils.FixRelativePathEscape(ref Items);

            AddFontFile();
            InitializeComponent();

            // Save the default Size
            _initialFormSize = ClientSize;
            _baseAvatarSearchFilterListWidth = AvatarSearchFilterList.Width;
            _baseAvatarItemExplorerListWidth = AvatarItemExplorer.Width;
            _resizeTimer.Tick += (s, ev) =>
            {
                _resizeTimer.Stop();
                ResizeControl();
            };
            _initialized = true;

            // Render Window
            RefleshWindow();

            // Start AutoBackup
            AutoBackup();

            // Set Backup Title Loop
            BackupTimeTitle();

            Text = $"VRChat Avatar Explorer {CurrentVersion} by ぷこるふ";

            // Check if the software is launched with a URL
            if (launchInfo.launchedWithUrl && launchInfo.assetDirs.Length != 0 && !string.IsNullOrEmpty(launchInfo.assetId))
            {
                AddItemForm addItem = new(this, ItemType.Avatar, null, false, null, launchInfo.assetDirs, launchInfo.assetId);
                addItem.ShowDialog();

                RefleshWindow();
                DatabaseUtils.SaveItemsData(Items);
            }

            AdjustLabelPosition();
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox("ソフトの起動中にエラーが発生しました。\n\n" + ex, "エラー", true);
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

        var items = Items.Where(item => item.Type == ItemType.Avatar);
        if (!items.Any()) return;

        items = SortingBox.SelectedIndex switch
        {
            0 => items.OrderBy(item => item.Title),
            1 => items.OrderBy(item => item.AuthorName),
            2 => items.OrderByDescending(item => item.CreatedDate),
            3 => items.OrderByDescending(item => item.UpdatedDate),
            _ => items.OrderBy(item => item.Title),
        };

        AvatarPage.SuspendLayout();
        AvatarPage.AutoScroll = false;

        var index = 0;
        foreach (Item item in items)
        {
            var description = ItemUtils.GetItemDescription(item, CurrentLanguage);

            Button button = AEUtils.CreateButton(item.ImagePath, item.Title, LanguageUtils.Translate("作者: ", CurrentLanguage) + item.AuthorName, true, description, GetAvatarListWidth);
            button.Location = new Point(0, (70 * index) + 2);
            button.MouseClick += OnMouseClick;

            EventHandler clickEvent = (_, _) =>
            {
                CurrentPath = new CurrentPath
                {
                    CurrentSelectedAvatar = item.Title,
                    CurrentSelectedAvatarPath = item.ItemPath
                };

                _leftWindow = LeftWindow.Default;

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
                ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("Boothリンクのコピー", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent2 = (_, _) => BoothUtils.CopyItemBoothLink(item, CurrentLanguage);

                toolStripMenuItem.Click += clickEvent2;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent2;

                ToolStripMenuItem toolStripMenuItem1 = new(LanguageUtils.Translate("Boothリンクを開く", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent3 = (_, _) => BoothUtils.OpenItenBoothLink(item, CurrentLanguage);

                toolStripMenuItem1.Click += clickEvent3;
                toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent3;

                contextMenuStrip.Items.Add(toolStripMenuItem);
                contextMenuStrip.Items.Add(toolStripMenuItem1);
            }

            ToolStripMenuItem toolStripMenuItem2 = new(LanguageUtils.Translate("この作者の他のアイテムを表示", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.OpenIcon));
            EventHandler? clickEvent4 = (_, _) =>
            {
                SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                SearchItems();
            };

            toolStripMenuItem2.Click += clickEvent4;
            toolStripMenuItem2.Disposed += (_, _) => toolStripMenuItem2.Click -= clickEvent4;

            ToolStripMenuItem toolStripMenuItem3 = new(LanguageUtils.Translate("サムネイル変更", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent5 = (_, _) =>
            {
                OpenFileDialog ofd = new()
                {
                    Filter = LanguageUtils.Translate("画像ファイル|*.png;*.jpg", CurrentLanguage),
                    Title = LanguageUtils.Translate("サムネイル変更", CurrentLanguage),
                    Multiselect = false
                };
                if (ofd.ShowDialog() != DialogResult.OK) return;

                item.ImagePath = ofd.FileName;

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("サムネイルを変更しました！", CurrentLanguage) + "\n\n" +
                    LanguageUtils.Translate("変更前: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                    LanguageUtils.Translate("変更後: ", CurrentLanguage) + ofd.FileName,
                    LanguageUtils.Translate("完了", CurrentLanguage)
                );

                // もしアバターの欄を右で開いていたら、そのサムネイルも更新しないといけないため。
                if (_openingWindow == Window.ItemList && !_isSearching) GenerateItems();

                // 検索中だと、検索画面を再読込してあげる
                if (_isSearching) SearchItems();

                GenerateAvatarList();
                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem3.Click += clickEvent5;
            toolStripMenuItem3.Disposed += (_, _) => toolStripMenuItem3.Click -= clickEvent5;

            ToolStripMenuItem toolStripMenuItem4 = new(LanguageUtils.Translate("編集", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent6 = (_, _) =>
            {
                var prePath = item.ItemPath;

                AddItemForm addItem = new(this, item.Type, item.CustomCategory, true, item, null);
                addItem.ShowDialog();

                // 対応アバターのパスを変えてあげる
                DatabaseUtils.ChangeAllItemPath(ref Items, prePath);

                // もしアイテムで編集されたアイテムを開いていたら、パスなどに使用される文字列も更新しないといけないため
                if (CurrentPath.CurrentSelectedAvatarPath == prePath)
                {
                    CurrentPath.CurrentSelectedAvatar = item.Title;
                    CurrentPath.CurrentSelectedAvatarPath = item.ItemPath;
                }

                // もしアバターの欄を右で開いていたら、そのアイテムの情報も更新しないといけないため
                if (_openingWindow == Window.ItemList && !_isSearching) GenerateItems();

                // 検索中だと、検索画面を再読込してあげる
                if (_isSearching) SearchItems();

                // 検索時の文字列を消さないようにするために_isSearchingでチェックしている
                if (!_isSearching) PathTextBox.Text = GeneratePath();

                RefleshWindow();
                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem4.Click += clickEvent6;
            toolStripMenuItem4.Disposed += (_, _) => toolStripMenuItem4.Click -= clickEvent6;

            ToolStripMenuItem toolStripMenuItem5 = new(LanguageUtils.Translate("メモの追加", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent7 = (_, _) =>
            {
                var previousMemo = item.ItemMemo;
                AddNoteForm addMemo = new(this, item);
                addMemo.ShowDialog();

                var memo = addMemo.Memo;
                if (string.IsNullOrEmpty(memo) || memo == previousMemo) return;

                item.ItemMemo = memo;
                item.UpdatedDate = DateUtils.GetUnixTime();

                RefleshWindow();
                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem5.Click += clickEvent7;
            toolStripMenuItem5.Disposed += (_, _) => toolStripMenuItem5.Click -= clickEvent7;

            ToolStripMenuItem toolStripMenuItem6 = new(LanguageUtils.Translate("削除", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.TrashIcon));
            EventHandler clickEvent8 = (_, _) =>
            {
                var result = FormUtils.ShowConfirmDialog(
                    LanguageUtils.Translate("本当に削除しますか？", CurrentLanguage),
                    LanguageUtils.Translate("確認", CurrentLanguage)
                );
                if (!result) return;

                var undo = false; // もし削除されるアイテムが開かれていたら
                if (CurrentPath.CurrentSelectedItem?.ItemPath == item.ItemPath)
                {
                    CurrentPath.CurrentSelectedItemCategory = null;
                    CurrentPath.CurrentSelectedItem = null;
                    undo = true;
                }

                var undo2 = false; // アバターモードでもし削除されるアバターから今までのアイテムが開かれていたら
                if (CurrentPath.CurrentSelectedAvatarPath == item.ItemPath && _leftWindow == LeftWindow.Default)
                {
                    CurrentPath = new CurrentPath();
                    undo2 = true;
                }

                // アバターのときは対応アバター削除、共通素体グループから削除用の処理を実行する
                if (item.Type == ItemType.Avatar)
                {
                    var result2 = FormUtils.ShowConfirmDialog(
                        LanguageUtils.Translate("このアバターを対応アバターとしているアイテムの対応アバターからこのアバターを削除しますか？", CurrentLanguage),
                        LanguageUtils.Translate("確認", CurrentLanguage)
                    );

                    DatabaseUtils.DeleteAvatarFromItem(ref Items, item.ItemPath, result2);

                    if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                    {
                        var result3 = FormUtils.ShowConfirmDialog(
                            LanguageUtils.Translate("このアバターを共通素体グループから削除しますか？", CurrentLanguage),
                            LanguageUtils.Translate("確認", CurrentLanguage)
                        );

                        if (result3)
                        {
                            DatabaseUtils.DeleteAvatarFromCommonAvatars(ref CommonAvatars, item.ItemPath);
                            DatabaseUtils.SaveCommonAvatarData(CommonAvatars);
                        }
                    }
                }

                Items.RemoveAll(i => i.ItemPath == item.ItemPath);

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("削除が完了しました。", CurrentLanguage),
                    LanguageUtils.Translate("完了", CurrentLanguage)
                );

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
                            DatabaseUtils.SaveItemsData(Items);
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
                            DatabaseUtils.SaveItemsData(Items);
                        }
                    }
                    else
                    {
                        SearchItems();
                        DatabaseUtils.SaveItemsData(Items);
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
                        DatabaseUtils.SaveItemsData(Items);
                        return;
                    }

                    // フォルダーを開いていって、アイテムが選択された状態(CurrentSelectedItemとして設定されている時)
                    if (undo)
                    {
                        GenerateItems();
                        PathTextBox.Text = GeneratePath();
                        DatabaseUtils.SaveItemsData(Items);
                        return;
                    }

                    // アイテム画面に既にいる
                    if (_openingWindow == Window.ItemList)
                    {
                        GenerateItems();
                        DatabaseUtils.SaveItemsData(Items);
                        return;
                    }

                    // アイテム画面の前にいる
                    RefleshWindow();

                    DatabaseUtils.SaveItemsData(Items);
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

        AvatarPage.ResumeLayout();
        AvatarPage.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(AvatarPage);
    }

    /// <summary>
    /// メイン画面左の作者欄を作成します。
    /// </summary>
    private void GenerateAuthorList()
    {
        ResetAvatarPage(AvatarAuthorPage);

        var authors = ItemUtils.GetAuthors(Items);
        if (authors.Count == 0) return;

        authors.Sort((a, b) => string.Compare(a.AuthorName, b.AuthorName, StringComparison.OrdinalIgnoreCase));

        AvatarAuthorPage.SuspendLayout();
        AvatarAuthorPage.AutoScroll = false;

        var index = 0;
        foreach (var author in authors)
        {
            Button button = AEUtils.CreateButton(author.AuthorImagePath, author.AuthorName,Items.Count(item => item.AuthorName == author.AuthorName) + LanguageUtils.Translate("個の項目", CurrentLanguage), true, author.AuthorName, GetAvatarListWidth);
            button.Location = new Point(0, (70 * index) + 2);
            button.MouseClick += OnMouseClick;

            EventHandler clickEvent = (_, _) =>
            {
                CurrentPath = new CurrentPath
                {
                    CurrentSelectedAuthor = author
                };

                _leftWindow = LeftWindow.Author;

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

            ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("サムネイル変更", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent1 = (_, _) =>
            {
                OpenFileDialog ofd = new()
                {
                    Filter = LanguageUtils.Translate("画像ファイル|*.png;*.jpg", CurrentLanguage),
                    Title = LanguageUtils.Translate("サムネイル変更", CurrentLanguage),
                    Multiselect = false
                };
                if (ofd.ShowDialog() != DialogResult.OK) return;

                foreach (var item in Items.Where(item => item.AuthorName == author.AuthorName))
                {
                    item.AuthorImageFilePath = ofd.FileName;
                }

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("サムネイルを変更しました！", CurrentLanguage) + "\n\n" +
                    LanguageUtils.Translate("変更前: ", CurrentLanguage) + author.AuthorImagePath + "\n\n" +
                    LanguageUtils.Translate("変更後: ", CurrentLanguage) + ofd.FileName,
                    "完了"
                );

                GenerateAuthorList();
                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem.Click += clickEvent1;
            toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent1;

            contextMenuStrip.Items.Add(toolStripMenuItem);
            button.ContextMenuStrip = contextMenuStrip;
            AvatarAuthorPage.Controls.Add(button);
            index++;
        }

        AvatarAuthorPage.ResumeLayout();
        AvatarAuthorPage.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(AvatarAuthorPage);
    }

    /// <summary>
    /// メイン画面左のカテゴリー欄を作成します。
    /// </summary>
    private void GenerateCategoryListLeft()
    {
        ResetAvatarPage(CategoryPage);

        CategoryPage.SuspendLayout();
        CategoryPage.AutoScroll = false;

        var index = 0;
        foreach (ItemType itemType in Enum.GetValues<ItemType>())
        {
            if (itemType is ItemType.Unknown or ItemType.Custom) continue;

            var items = Items.Where(item => item.Type == itemType);
            var itemCount = items.Count();

            Button button = AEUtils.CreateButton(null, ItemUtils.GetCategoryName(itemType, CurrentLanguage), itemCount + LanguageUtils.Translate("個の項目", CurrentLanguage), true, "", GetAvatarListWidth);
            button.Location = new Point(0, (70 * index) + 2);
            button.MouseClick += OnMouseClick;

            EventHandler clickEvent = (_, _) =>
            {
                CurrentPath = new CurrentPath
                {
                    CurrentSelectedCategory = itemType
                };

                _leftWindow = LeftWindow.Category;

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

        if (CustomCategories.Count != 0)
        {
            foreach (var customCategory in CustomCategories)
            {
                var items = Items.Where(item => item.CustomCategory == customCategory);
                var itemCount = items.Count();

                Button button = AEUtils.CreateButton(null, customCategory, itemCount + LanguageUtils.Translate("個の項目", CurrentLanguage), true, "", GetAvatarListWidth);
                button.Location = new Point(0, (70 * index) + 2);
                button.MouseClick += OnMouseClick;

                EventHandler clickEvent = (_, _) =>
                {
                    CurrentPath = new CurrentPath
                    {
                        CurrentSelectedCategory = ItemType.Custom,
                        CurrentSelectedCustomCategory = customCategory
                    };

                    _leftWindow = LeftWindow.Category;

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

        CategoryPage.ResumeLayout();
        CategoryPage.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(CategoryPage);
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

        AvatarItemExplorer.SuspendLayout();
        AvatarItemExplorer.AutoScroll = false;

        var index = 0;
        foreach (ItemType itemType in Enum.GetValues<ItemType>())
        {
            if (itemType is ItemType.Unknown or ItemType.Custom) continue;

            int itemCount = 0;
            if (_leftWindow == LeftWindow.Author)
            {
                itemCount = Items.Count(item => item.Type == itemType && item.AuthorName == CurrentPath.CurrentSelectedAuthor?.AuthorName);
            }
            else
            {
                itemCount = Items.Count(item =>
                    item.Type == itemType &&
                    (ItemUtils.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath).IsSupportedOrCommon || item.SupportedAvatar.Count == 0 || CurrentPath.CurrentSelectedAvatar == "*")
                );
            }

            if (itemCount == 0) continue;

            Button button = AEUtils.CreateButton(null, ItemUtils.GetCategoryName(itemType, CurrentLanguage), itemCount + LanguageUtils.Translate("個の項目", CurrentLanguage), false, "", GetItemExplorerListWidth);
            button.Location = new Point(0, (70 * index) + 2);
            button.MouseClick += OnMouseClick;

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

        if (CustomCategories.Count != 0)
        {
            foreach (var customCategory in CustomCategories)
            {
                var itemCount = 0;
                if (_leftWindow == LeftWindow.Author)
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
                        (ItemUtils.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath).IsSupportedOrCommon || item.SupportedAvatar.Count == 0 || CurrentPath.CurrentSelectedAvatar == "*")
                    );
                }

                if (itemCount == 0) continue;

                Button button = AEUtils.CreateButton(null, customCategory, itemCount + LanguageUtils.Translate("個の項目", CurrentLanguage), false, "", GetItemExplorerListWidth);
                button.Location = new Point(0, (70 * index) + 2);
                button.MouseClick += OnMouseClick;

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

        AvatarItemExplorer.ResumeLayout();
        AvatarItemExplorer.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(AvatarItemExplorer);
    }

    /// <summary>
    /// メイン画面右のアイテム欄を作成します。
    /// </summary>
    private void GenerateItems()
    {
        _openingWindow = Window.ItemList;
        ResetAvatarExplorer();

        IEnumerable<Item> filteredItems;

        if (_leftWindow == LeftWindow.Author)
        {
            filteredItems = Items.Where(item =>
                item.Type == CurrentPath.CurrentSelectedCategory &&
                item.AuthorName == CurrentPath.CurrentSelectedAuthor?.AuthorName &&
                (item.Type != ItemType.Custom || item.CustomCategory == CurrentPath.CurrentSelectedCustomCategory)
            );
        }
        else if (_leftWindow == LeftWindow.Category)
        {
            filteredItems = Items.Where(item => item.Type == CurrentPath.CurrentSelectedCategory && (item.Type != ItemType.Custom || item.CustomCategory == CurrentPath.CurrentSelectedCustomCategory));
        }
        else
        {
            filteredItems = Items.Where(item =>
                item.Type == CurrentPath.CurrentSelectedCategory &&
                (item.Type != ItemType.Custom || item.CustomCategory == CurrentPath.CurrentSelectedCustomCategory) &&
                (ItemUtils.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath).IsSupportedOrCommon || item.SupportedAvatar.Count == 0 || CurrentPath.CurrentSelectedAvatar == "*")
            );
        }

        filteredItems = SortingBox.SelectedIndex switch
        {
            0 => filteredItems.OrderBy(item => item.Title).ToArray(),
            1 => filteredItems.OrderBy(item => item.AuthorName).ToArray(),
            2 => filteredItems.OrderByDescending(item => item.CreatedDate).ToArray(),
            3 => filteredItems.OrderByDescending(item => item.UpdatedDate).ToArray(),
            4 => filteredItems.OrderBy(item => ItemUtils.ContainsSelectedAvatar(item, CurrentPath.CurrentSelectedAvatarPath) ? 0 : 1),
            5 => filteredItems.OrderBy(item => ItemUtils.ContainsSelectedAvatar(item, CurrentPath.CurrentSelectedAvatarPath) ? 1 : 0),
            _ => filteredItems.OrderBy(item => item.Title).ToArray(),
        };

        if (!filteredItems.Any()) return;

        AvatarItemExplorer.SuspendLayout();
        AvatarItemExplorer.AutoScroll = false;

        var index = 0;
        foreach (Item item in filteredItems)
        {
            var authorText = LanguageUtils.Translate("作者: ", CurrentLanguage) + item.AuthorName;

            var isSupportedOrCommon = ItemUtils.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath);

            if (isSupportedOrCommon.OnlyCommon && item.SupportedAvatar.Count != 0 && CurrentPath.CurrentSelectedAvatarPath != null && !item.SupportedAvatar.Contains(CurrentPath.CurrentSelectedAvatarPath))
            {
                var commonAvatarName = isSupportedOrCommon.CommonAvatarName;
                if (!string.IsNullOrEmpty(commonAvatarName))
                {
                    authorText += "\n" + LanguageUtils.Translate("共通素体: ", CurrentLanguage) + commonAvatarName;
                }
            }

            var description = ItemUtils.GetItemDescription(item, CurrentLanguage);

            Button button = AEUtils.CreateButton(item.ImagePath, item.Title, authorText, false, description,GetItemExplorerListWidth);
            button.Location = new Point(0, (70 * index) + 2);
            button.MouseClick += OnMouseClick;

            if (SortingBox.SelectedIndex == 4 || SortingBox.SelectedIndex == 5)
            {
                var currentAvatar = CurrentPath.CurrentSelectedAvatarPath;
                if (!string.IsNullOrEmpty(currentAvatar))
                {
                    button.BackColor = item.ImplementedAvatars.Contains(currentAvatar) ? Color.LightGreen : Color.LightPink;
                }
            }

            EventHandler clickEvent = (_, _) =>
            {
                if (!Directory.Exists(item.ItemPath))
                {
                    var result = FormUtils.ShowConfirmDialog(
                        LanguageUtils.Translate("フォルダが見つかりませんでした。編集しますか？", CurrentLanguage),
                        LanguageUtils.Translate("エラー", CurrentLanguage)
                    );
                    if (!result) return;

                    var prePath = item.ItemPath;

                    AddItemForm addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, true, item, null);
                    addItem.ShowDialog();

                    if (!Directory.Exists(item.ItemPath))
                    {
                        FormUtils.ShowMessageBox(
                            LanguageUtils.Translate("フォルダが見つかりませんでした。", CurrentLanguage),
                            LanguageUtils.Translate("エラー", CurrentLanguage),
                            true
                        );
                        return;
                    }

                    // 対応アバターのパスを変えてあげる
                    DatabaseUtils.ChangeAllItemPath(ref Items, prePath);

                    if (CurrentPath.CurrentSelectedAvatarPath == prePath)
                    {
                        CurrentPath.CurrentSelectedAvatar = item.Title;
                        CurrentPath.CurrentSelectedAvatarPath = item.ItemPath;
                    }

                    RefleshWindow();
                    DatabaseUtils.SaveItemsData(Items);
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
                ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("フォルダを開く", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.OpenIcon));
                EventHandler clickEvent1 = (_, _) => FileSystemUtils.OpenItemFolder(item, CurrentLanguage);

                toolStripMenuItem.Click += clickEvent1;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent1;

                contextMenuStrip.Items.Add(toolStripMenuItem);
            }

            if (item.BoothId != -1)
            {
                ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("Boothリンクのコピー", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent2 = (_, _) => BoothUtils.CopyItemBoothLink(item, CurrentLanguage);

                toolStripMenuItem.Click += clickEvent2;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent2;

                ToolStripMenuItem toolStripMenuItem1 = new(LanguageUtils.Translate("Boothリンクを開く", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent3 = (_, _) => BoothUtils.OpenItenBoothLink(item, CurrentLanguage);

                toolStripMenuItem1.Click += clickEvent3;
                toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent3;

                contextMenuStrip.Items.Add(toolStripMenuItem);
                contextMenuStrip.Items.Add(toolStripMenuItem1);
            }

            ToolStripMenuItem toolStripMenuItem2 = new(LanguageUtils.Translate("この作者の他のアイテムを表示", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.OpenIcon));
            EventHandler clickEvent4 = (_, _) =>
            {
                SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                SearchItems();
            };

            toolStripMenuItem2.Click += clickEvent4;
            toolStripMenuItem2.Disposed += (_, _) => toolStripMenuItem2.Click -= clickEvent4;

            ToolStripMenuItem toolStripMenuItem3 = new(LanguageUtils.Translate("サムネイル変更", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent5 = (_, _) =>
            {
                OpenFileDialog ofd = new()
                {
                    Filter = LanguageUtils.Translate("画像ファイル|*.png;*.jpg", CurrentLanguage),
                    Title = LanguageUtils.Translate("サムネイル変更", CurrentLanguage),
                    Multiselect = false
                };
                if (ofd.ShowDialog() != DialogResult.OK) return;

                item.ImagePath = ofd.FileName;

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("サムネイルを変更しました！", CurrentLanguage) + "\n\n" +
                    LanguageUtils.Translate("変更前: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                    LanguageUtils.Translate("変更後: ", CurrentLanguage) + ofd.FileName,
                    LanguageUtils.Translate("完了", CurrentLanguage)
                );

                if (_isSearching)
                {
                    SearchItems();
                }
                else
                {
                    GenerateItems();
                }

                GenerateAvatarList();
                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem3.Click += clickEvent5;
            toolStripMenuItem3.Disposed += (_, _) => toolStripMenuItem3.Click -= clickEvent5;

            ToolStripMenuItem toolStripMenuItem4 = new(LanguageUtils.Translate("編集", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent6 = (_, _) =>
            {
                var prePath = item.ItemPath;

                AddItemForm addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, true, item, null);
                addItem.ShowDialog();

                // 対応アバターのパスを変えてあげる
                DatabaseUtils.ChangeAllItemPath(ref Items, prePath);

                if (CurrentPath.CurrentSelectedAvatarPath == prePath)
                {
                    CurrentPath.CurrentSelectedAvatar = item.Title;
                    CurrentPath.CurrentSelectedAvatarPath = item.ItemPath;
                }

                if (!_isSearching) PathTextBox.Text = GeneratePath();
                RefleshWindow();
                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem4.Click += clickEvent6;
            toolStripMenuItem4.Disposed += (_, _) => toolStripMenuItem4.Click -= clickEvent6;

            ToolStripMenuItem toolStripMenuItem5 = new(LanguageUtils.Translate("メモの追加", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent7 = (_, _) =>
            {
                var previousMemo = item.ItemMemo;
                AddNoteForm addMemo = new(this, item);
                addMemo.ShowDialog();

                var memo = addMemo.Memo;
                if (string.IsNullOrEmpty(memo) || memo == previousMemo) return;

                item.ItemMemo = memo;

                GenerateAuthorList();
                GenerateItems();
                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem5.Click += clickEvent7;
            toolStripMenuItem5.Disposed += (_, _) => toolStripMenuItem5.Click -= clickEvent7;

            ToolStripMenuItem toolStripMenuItem6 = new(LanguageUtils.Translate("実装/未実装", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));

            foreach (var avatar in Items.Where(i => i.Type == ItemType.Avatar))
            {
                ToolStripMenuItem toolStripMenuItemTemp = new(DatabaseUtils.GetAvatarNameFromPath(Items, avatar.ItemPath))
                {
                    Tag = avatar.ItemPath,
                    Checked = item.ImplementedAvatars.Contains(avatar.ItemPath)
                };

                EventHandler clickEvent8 = (_, _) =>
                {
                    if (toolStripMenuItemTemp.Checked)
                    {
                        item.ImplementedAvatars.RemoveAll(avatarPath => avatarPath == (string)toolStripMenuItemTemp.Tag);
                        toolStripMenuItemTemp.Checked = false;
                    }
                    else
                    {
                        item.ImplementedAvatars.Add((string)toolStripMenuItemTemp.Tag);
                        toolStripMenuItemTemp.Checked = true;
                    }

                    if (SortingBox.SelectedIndex == 4 || SortingBox.SelectedIndex == 5)
                    {
                        var currentAvatar = CurrentPath.CurrentSelectedAvatarPath;
                        if (!string.IsNullOrEmpty(currentAvatar))
                        {
                            button.BackColor = item.ImplementedAvatars.Contains(currentAvatar) ? Color.LightGreen : Color.LightPink;
                        }
                    }

                    DatabaseUtils.SaveItemsData(Items);
                };

                toolStripMenuItemTemp.Click += clickEvent8;
                toolStripMenuItemTemp.Click += FormUtils.ShowParentToolStrip;
                toolStripMenuItemTemp.Disposed += (_, _) =>
                {
                    toolStripMenuItemTemp.Click -= clickEvent8;
                    toolStripMenuItemTemp.Click -= FormUtils.ShowParentToolStrip;
                };
                toolStripMenuItem6.DropDownItems.Add(toolStripMenuItemTemp);
            }

            ToolStripMenuItem toolStripMenuItem7 = new(LanguageUtils.Translate("削除", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.TrashIcon));
            EventHandler clickEvent9 = (_, _) =>
            {
                var result = FormUtils.ShowConfirmDialog(
                    LanguageUtils.Translate("本当に削除しますか？", CurrentLanguage),
                    LanguageUtils.Translate("確認", CurrentLanguage)
                );
                if (!result) return;

                var undo = false;
                if (CurrentPath.CurrentSelectedAvatarPath == item.ItemPath && _leftWindow == LeftWindow.Default)
                {
                    CurrentPath = new CurrentPath();
                    undo = true;
                    PathTextBox.Text = GeneratePath();
                }

                if (item.Type == ItemType.Avatar)
                {
                    var result2 = FormUtils.ShowConfirmDialog(
                        LanguageUtils.Translate("このアバターを対応アバターとしているアイテムの対応アバターからこのアバターを削除しますか？", CurrentLanguage),
                        LanguageUtils.Translate("確認", CurrentLanguage)
                    );

                    DatabaseUtils.DeleteAvatarFromItem(ref Items, item.ItemPath, result2);

                    if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                    {
                        var result3 = FormUtils.ShowConfirmDialog(
                            LanguageUtils.Translate("このアバターを共通素体グループから削除しますか？", CurrentLanguage),
                            LanguageUtils.Translate("確認", CurrentLanguage)
                        );

                        if (result3)
                        {
                            DatabaseUtils.DeleteAvatarFromCommonAvatars(ref CommonAvatars, item.ItemPath);
                            DatabaseUtils.SaveCommonAvatarData(CommonAvatars);
                        }
                    }
                }

                Items.RemoveAll(i => i.ItemPath == item.ItemPath);

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("削除が完了しました。", CurrentLanguage),
                    LanguageUtils.Translate("完了", CurrentLanguage)
                );

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

                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem7.Click += clickEvent9;
            toolStripMenuItem7.Disposed += (_, _) => toolStripMenuItem7.Click -= clickEvent9;

            contextMenuStrip.Items.Add(toolStripMenuItem2);
            contextMenuStrip.Items.Add(toolStripMenuItem3);
            contextMenuStrip.Items.Add(toolStripMenuItem4);
            contextMenuStrip.Items.Add(toolStripMenuItem5);
            contextMenuStrip.Items.Add(toolStripMenuItem6);
            contextMenuStrip.Items.Add(toolStripMenuItem7);
            button.ContextMenuStrip = contextMenuStrip;
            AvatarItemExplorer.Controls.Add(button);
            index++;
        }

        AvatarItemExplorer.ResumeLayout();
        AvatarItemExplorer.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(AvatarItemExplorer);
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
        ItemFolderInfo itemFolderInfo = ItemUtils.GetItemFolderInfo(CurrentPath.CurrentSelectedItem.ItemPath, CurrentPath.CurrentSelectedItem.MaterialPath);
        CurrentPath.CurrentSelectedItemFolderInfo = itemFolderInfo;

        ResetAvatarExplorer();

        AvatarItemExplorer.SuspendLayout();
        AvatarItemExplorer.AutoScroll = false;

        var index = 0;
        foreach (var itemType in types)
        {
            var itemCount = itemFolderInfo.GetItemCount(itemType);
            if (itemCount == 0) continue;

            Button button = AEUtils.CreateButton(null, LanguageUtils.Translate(itemType, CurrentLanguage), itemCount + LanguageUtils.Translate("個の項目", CurrentLanguage), false, "", GetItemExplorerListWidth);
            button.Location = new Point(0, (70 * index) + 2);
            button.MouseClick += OnMouseClick;

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

        AvatarItemExplorer.ResumeLayout();
        AvatarItemExplorer.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(AvatarItemExplorer);
    }

    /// <summary>
    /// メイン画面右のアイテム内のファイル欄を作成します。
    /// </summary>
    private void GenerateItemFiles()
    {
        _openingWindow = Window.ItemFolderItemsList;
        ResetAvatarExplorer();

        var files = CurrentPath.CurrentSelectedItemFolderInfo.GetItems(CurrentPath.CurrentSelectedItemCategory);
        if (!files.Any()) return;

        files = files.OrderBy(file => file.FileName);

        AvatarItemExplorer.SuspendLayout();
        AvatarItemExplorer.AutoScroll = false;

        var index = 0;
        foreach (var file in files)
        {
            var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : "";
            Button button = AEUtils.CreateButton(imagePath, file.FileName, file.FileExtension.Replace(".", "") + LanguageUtils.Translate("ファイル", CurrentLanguage), false, LanguageUtils.Translate("開くファイルのパス: ", CurrentLanguage) + file.FilePath, GetItemExplorerListWidth);
            button.Location = new Point(0, (70 * index) + 2);
            button.MouseClick += OnMouseClick;

            ContextMenuStrip contextMenuStrip = new();

            ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("開く", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
            EventHandler clickEvent = (_, _) => FileSystemUtils.OpenItemFile(file, true, CurrentLanguage);

            toolStripMenuItem.Click += clickEvent;
            toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent;

            ToolStripMenuItem toolStripMenuItem1 = new(LanguageUtils.Translate("ファイルのパスを開く", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
            EventHandler clickEvent1 = (_, _) => FileSystemUtils.OpenItemFile(file, false, CurrentLanguage);

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
                        _ = AEUtils.ModifyUnityPackageFilePathAsync(file, CurrentPath, CurrentLanguage);
                    }
                    else
                    {
                        FileSystemUtils.OpenItemFile(file, true, CurrentLanguage);
                    }
                }
                catch
                {
                    FileSystemUtils.OpenItemFile(file, false, CurrentLanguage);
                }
            };

            button.Click += clickEvent2;
            button.Disposed += (_, _) => button.Click -= clickEvent2;

            AvatarItemExplorer.Controls.Add(button);
            index++;
        }

        AvatarItemExplorer.ResumeLayout();
        AvatarItemExplorer.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(AvatarItemExplorer);
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

        var filteredItems = Items
            .Where(item => DatabaseUtils.GetSearchResult(Items, item, searchFilter, CurrentLanguage))
            .Where(item =>
                searchFilter.SearchWords.All(word =>
                    item.Title.Contains(word, StringComparison.CurrentCultureIgnoreCase) ||
                    item.AuthorName.Contains(word, StringComparison.CurrentCultureIgnoreCase) ||
                    item.SupportedAvatar.Any(avatar =>
                    {
                        var supportedAvatarName = DatabaseUtils.GetAvatarNameFromPath(Items, avatar);
                        if (supportedAvatarName == "") return false;
                        return supportedAvatarName.Contains(word, StringComparison.CurrentCultureIgnoreCase);
                    }) ||
                    item.BoothId.ToString().Contains(word, StringComparison.CurrentCultureIgnoreCase) ||
                    item.ItemMemo.Contains(word, StringComparison.CurrentCultureIgnoreCase)
                )
            )
            .OrderByDescending(item =>
            {
                var matchCount = 0;
                foreach (var word in searchFilter.SearchWords)
                {
                    if (item.Title.Contains(word, StringComparison.CurrentCultureIgnoreCase)) matchCount++;
                    if (item.AuthorName.Contains(word, StringComparison.CurrentCultureIgnoreCase)) matchCount++;
                    if (item.SupportedAvatar.Any(avatar =>
                    {
                        var supportedAvatarName = DatabaseUtils.GetAvatarNameFromPath(Items, avatar);
                        if (supportedAvatarName == "") return false;
                        return supportedAvatarName.Contains(word, StringComparison.CurrentCultureIgnoreCase);
                    })) matchCount++;
                    if (item.BoothId.ToString().Contains(word, StringComparison.CurrentCultureIgnoreCase)) matchCount++;
                    if (item.ItemMemo.Contains(word, StringComparison.CurrentCultureIgnoreCase)) matchCount++;
                }

                return matchCount;
            });

        SearchResultLabel.Text =
            LanguageUtils.Translate("検索結果: ", CurrentLanguage) + filteredItems.Count() + LanguageUtils.Translate("件", CurrentLanguage) +
            LanguageUtils.Translate(" (全", CurrentLanguage) + Items.Count + LanguageUtils.Translate("件)", CurrentLanguage);

        if (!filteredItems.Any()) return;

        AvatarItemExplorer.SuspendLayout();
        AvatarItemExplorer.AutoScroll = false;

        var index = 0;
        foreach (Item item in filteredItems)
        {
            var description = ItemUtils.GetItemDescription(item, CurrentLanguage);

            Button button = AEUtils.CreateButton(item.ImagePath, item.Title, LanguageUtils.Translate("作者: ", CurrentLanguage) + item.AuthorName, false, description, GetItemExplorerListWidth);
            button.Location = new Point(0, (70 * index) + 2);
            button.MouseClick += OnMouseClick;

            EventHandler clickEvent = (_, _) =>
            {
                if (!Directory.Exists(item.ItemPath))
                {
                    bool result = FormUtils.ShowConfirmDialog(
                        LanguageUtils.Translate("フォルダが見つかりませんでした。編集しますか？", CurrentLanguage),
                        LanguageUtils.Translate("エラー", CurrentLanguage)
                    );
                    if (!result) return;

                    var prePath = item.ItemPath;

                    AddItemForm addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, true, item, null);
                    addItem.ShowDialog();

                    if (!Directory.Exists(item.ItemPath))
                    {
                        FormUtils.ShowMessageBox(
                            LanguageUtils.Translate("フォルダが見つかりませんでした。", CurrentLanguage),
                            LanguageUtils.Translate("エラー", CurrentLanguage),
                            true
                        );
                        return;
                    }

                    // 対応アバターのパスを変えてあげる
                    DatabaseUtils.ChangeAllItemPath(ref Items, prePath);

                    GenerateFilteredItem(searchFilter);
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                    DatabaseUtils.SaveItemsData(Items);
                }

                _leftWindow = LeftWindow.Default;

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
                ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("フォルダを開く", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.OpenIcon));
                EventHandler clickEvent1 = (_, _) => FileSystemUtils.OpenItemFolder(item, CurrentLanguage);

                toolStripMenuItem.Click += clickEvent1;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent1;

                contextMenuStrip.Items.Add(toolStripMenuItem);
            }

            if (item.BoothId != -1)
            {
                ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("Boothリンクのコピー", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent2 = (_, _) => BoothUtils.CopyItemBoothLink(item, CurrentLanguage);

                toolStripMenuItem.Click += clickEvent2;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent2;

                ToolStripMenuItem toolStripMenuItem1 = new(LanguageUtils.Translate("Boothリンクを開く", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent3 = (_, _) => BoothUtils.OpenItenBoothLink(item, CurrentLanguage);

                toolStripMenuItem1.Click += clickEvent3;
                toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent3;

                contextMenuStrip.Items.Add(toolStripMenuItem);
                contextMenuStrip.Items.Add(toolStripMenuItem1);
            }

            ToolStripMenuItem toolStripMenuItem2 = new(LanguageUtils.Translate("この作者の他のアイテムを表示", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.OpenIcon));
            EventHandler clickEvent4 = (_, _) =>
            {
                SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                SearchItems();
            };

            toolStripMenuItem2.Click += clickEvent4;
            toolStripMenuItem2.Disposed += (_, _) => toolStripMenuItem2.Click -= clickEvent4;

            ToolStripMenuItem toolStripMenuItem3 = new(LanguageUtils.Translate("サムネイル変更", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent5 = (_, _) =>
            {
                OpenFileDialog ofd = new()
                {
                    Filter = LanguageUtils.Translate("画像ファイル|*.png;*.jpg", CurrentLanguage),
                    Title = LanguageUtils.Translate("サムネイル変更", CurrentLanguage),
                    Multiselect = false
                };
                if (ofd.ShowDialog() != DialogResult.OK) return;

                item.ImagePath = ofd.FileName;

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("サムネイルを変更しました！", CurrentLanguage) + "\n\n" +
                    LanguageUtils.Translate("変更前: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                    LanguageUtils.Translate("変更後: ", CurrentLanguage) + ofd.FileName,
                    LanguageUtils.Translate("完了", CurrentLanguage)
                );

                GenerateFilteredItem(searchFilter);
                GenerateAvatarList();
                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem3.Click += clickEvent5;
            toolStripMenuItem3.Disposed += (_, _) => toolStripMenuItem3.Click -= clickEvent5;

            ToolStripMenuItem toolStripMenuItem4 = new(LanguageUtils.Translate("編集", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent6 = (_, _) =>
            {
                var prePath = item.ItemPath;
                AddItemForm addItem = new(this, item.Type, item.CustomCategory, true, item, null);
                addItem.ShowDialog();

                // 対応アバターのパスを変えてあげる
                DatabaseUtils.ChangeAllItemPath(ref Items, prePath);

                if (CurrentPath.CurrentSelectedAvatarPath == prePath)
                {
                    CurrentPath.CurrentSelectedAvatar = item.Title;
                    CurrentPath.CurrentSelectedAvatarPath = item.ItemPath;
                }

                GenerateFilteredItem(searchFilter);
                GenerateAvatarList();
                GenerateAuthorList();
                GenerateCategoryListLeft();
                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem4.Click += clickEvent6;
            toolStripMenuItem4.Disposed += (_, _) => toolStripMenuItem4.Click -= clickEvent6;

            ToolStripMenuItem toolStripMenuItem5 = new(LanguageUtils.Translate("メモの追加", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent7 = (_, _) =>
            {
                var previousMemo = item.ItemMemo;
                AddNoteForm addMemo = new(this, item);
                addMemo.ShowDialog();

                var memo = addMemo.Memo;
                if (string.IsNullOrEmpty(memo) || memo == previousMemo) return;

                item.ItemMemo = memo;

                GenerateFilteredItem(searchFilter);
                GenerateAvatarList();
                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem5.Click += clickEvent7;
            toolStripMenuItem5.Disposed += (_, _) => toolStripMenuItem5.Click -= clickEvent7;

            ToolStripMenuItem toolStripMenuItem6 = new(LanguageUtils.Translate("実装/未実装", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));

            foreach (var avatar in Items.Where(i => i.Type == ItemType.Avatar))
            {
                ToolStripMenuItem toolStripMenuItemTemp = new(DatabaseUtils.GetAvatarNameFromPath(Items, avatar.ItemPath))
                {
                    Tag = avatar.ItemPath,
                    Checked = item.ImplementedAvatars.Contains(avatar.ItemPath)
                };

                EventHandler clickEvent8 = (_, _) =>
                {
                    if (toolStripMenuItemTemp.Checked)
                    {
                        item.ImplementedAvatars.RemoveAll(avatarPath => avatarPath == (string)toolStripMenuItemTemp.Tag);
                        toolStripMenuItemTemp.Checked = false;
                    }
                    else
                    {
                        item.ImplementedAvatars.Add((string)toolStripMenuItemTemp.Tag);
                        toolStripMenuItemTemp.Checked = true;
                    }

                    DatabaseUtils.SaveItemsData(Items);
                };

                toolStripMenuItemTemp.Click += clickEvent8;
                toolStripMenuItemTemp.Click += FormUtils.ShowParentToolStrip;
                toolStripMenuItemTemp.Disposed += (_, _) =>
                {
                    toolStripMenuItemTemp.Click -= clickEvent8;
                    toolStripMenuItemTemp.Click -= FormUtils.ShowParentToolStrip;
                };
                toolStripMenuItem6.DropDownItems.Add(toolStripMenuItemTemp);
            }

            ToolStripMenuItem toolStripMenuItem7 = new(LanguageUtils.Translate("削除", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.TrashIcon));
            EventHandler clickEvent9 = (_, _) =>
            {
                bool result = FormUtils.ShowConfirmDialog(
                    LanguageUtils.Translate("本当に削除しますか？", CurrentLanguage),
                    LanguageUtils.Translate("確認", CurrentLanguage)
                );
                if (!result) return;

                if (item.Type == ItemType.Avatar)
                {
                    var result2 = FormUtils.ShowConfirmDialog(
                        LanguageUtils.Translate("このアバターを対応アバターとしているアイテムの対応アバターからこのアバターを削除しますか？", CurrentLanguage),
                        LanguageUtils.Translate("確認", CurrentLanguage)
                    );

                    DatabaseUtils.DeleteAvatarFromItem(ref Items, item.ItemPath, result2);

                    if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                    {
                        var result3 = FormUtils.ShowConfirmDialog(
                            LanguageUtils.Translate("このアバターを共通素体グループから削除しますか？", CurrentLanguage),
                            LanguageUtils.Translate("確認", CurrentLanguage)
                        );

                        if (result3)
                        {
                            DatabaseUtils.DeleteAvatarFromCommonAvatars(ref CommonAvatars, item.ItemPath);
                            DatabaseUtils.SaveCommonAvatarData(CommonAvatars);
                        }
                    }
                }

                Items.RemoveAll(i => i.ItemPath == item.ItemPath);

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("削除が完了しました。", CurrentLanguage),
                    LanguageUtils.Translate("完了", CurrentLanguage)
                );

                GenerateFilteredItem(searchFilter);
                GenerateAvatarList();
                GenerateAuthorList();
                GenerateCategoryListLeft();
                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem7.Click += clickEvent9;
            toolStripMenuItem7.Disposed += (_, _) => toolStripMenuItem7.Click -= clickEvent9;

            contextMenuStrip.Items.Add(toolStripMenuItem2);
            contextMenuStrip.Items.Add(toolStripMenuItem3);
            contextMenuStrip.Items.Add(toolStripMenuItem4);
            contextMenuStrip.Items.Add(toolStripMenuItem5);
            contextMenuStrip.Items.Add(toolStripMenuItem6);
            contextMenuStrip.Items.Add(toolStripMenuItem7);
            button.ContextMenuStrip = contextMenuStrip;
            AvatarItemExplorer.Controls.Add(button);
            index++;
        }

        AvatarItemExplorer.ResumeLayout();
        AvatarItemExplorer.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(AvatarItemExplorer);
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
            Window.ItemFolderItemsList => CurrentPath.CurrentSelectedItemFolderInfo.GetItems(CurrentPath.CurrentSelectedItemCategory),
            Window.ItemFolderCategoryList => CurrentPath.CurrentSelectedItemFolderInfo.GetAllItem(),
            _ => new List<FileData>()
        };

        var filteredFileData = fileDatas
            .Where(file => searchWords.SearchWords.All(word => file.FileName.Contains(word, StringComparison.CurrentCultureIgnoreCase)))
            .OrderByDescending(file => searchWords.SearchWords.Count(word => file.FileName.Contains(word, StringComparison.CurrentCultureIgnoreCase)));

        SearchResultLabel.Text =
            LanguageUtils.Translate("フォルダー内検索結果: ", CurrentLanguage) + filteredFileData.Count() + LanguageUtils.Translate("件", CurrentLanguage) +
            LanguageUtils.Translate(" (全", CurrentLanguage) + fileDatas.Count() + LanguageUtils.Translate("件)", CurrentLanguage);

        if (!filteredFileData.Any()) return;

        AvatarItemExplorer.SuspendLayout();
        AvatarItemExplorer.AutoScroll = false;

        var index = 0;
        foreach (var file in filteredFileData)
        {
            var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : "";
            Button button = AEUtils.CreateButton(imagePath, file.FileName, file.FileExtension.Replace(".", "") + LanguageUtils.Translate("ファイル", CurrentLanguage), false, LanguageUtils.Translate("開くファイルのパス: ", CurrentLanguage) + file.FilePath, GetItemExplorerListWidth);
            button.Location = new Point(0, (70 * index) + 2);
            button.MouseClick += OnMouseClick;

            ContextMenuStrip contextMenuStrip = new();

            ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("開く", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
            EventHandler clickEvent = (_, _) => FileSystemUtils.OpenItemFile(file, true, CurrentLanguage);

            toolStripMenuItem.Click += clickEvent;
            toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent;

            ToolStripMenuItem toolStripMenuItem1 = new(LanguageUtils.Translate("ファイルのパスを開く", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
            EventHandler clickEvent1 = (_, _) => FileSystemUtils.OpenItemFile(file, false, CurrentLanguage);

            toolStripMenuItem1.Click += clickEvent1;
            toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent1;

            contextMenuStrip.Items.Add(toolStripMenuItem);
            contextMenuStrip.Items.Add(toolStripMenuItem1);
            button.ContextMenuStrip = contextMenuStrip;

            EventHandler clickEvent2 = (_, _) => FileSystemUtils.OpenItemFile(file, true, CurrentLanguage);

            button.Click += clickEvent2;
            button.Disposed += (_, _) => button.Click -= clickEvent2;

            AvatarItemExplorer.Controls.Add(button);
            index++;
        }

        AvatarItemExplorer.ResumeLayout();
        AvatarItemExplorer.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(AvatarItemExplorer);
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
        AddItemForm addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, false, null, null);
        addItem.ShowDialog();
        RefleshWindow();
        DatabaseUtils.SaveItemsData(Items);
    }
    #endregion

    #region パス関連の処理
    /// <summary>
    /// 現在のパスを生成します。
    /// </summary>
    /// <returns></returns>
    private string GeneratePath()
    {
        string prefix = string.Empty;
        List<string> pathParts = new();

        switch (_leftWindow)
        {
            case LeftWindow.Author:
                {
                    prefix = LanguageUtils.Translate("作者", CurrentLanguage) + " | ";

                    var author = CurrentPath.CurrentSelectedAuthor;
                    if (author == null)
                        return LanguageUtils.Translate("ここには現在のパスが表示されます", CurrentLanguage);

                    pathParts.Add(AEUtils.RemoveFormat(author.AuthorName));
                    break;
                }
            case LeftWindow.Category:
                {
                    prefix = LanguageUtils.Translate("カテゴリ別", CurrentLanguage) + " | ";
                    break;
                }
            case LeftWindow.Default:
                {
                    prefix = LanguageUtils.Translate("アバター", CurrentLanguage) + " | ";

                    var avatar = CurrentPath.CurrentSelectedAvatar;
                    if (avatar == null)
                        return LanguageUtils.Translate("ここには現在のパスが表示されます", CurrentLanguage);

                    pathParts.Add(AEUtils.RemoveFormat(avatar));
                    break;
                }
            default:
                {
                    break;
                }
        }

        if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
            return prefix + (pathParts.Count > 0 ? AEUtils.GenerateSeparatedPath(pathParts.ToArray()) : LanguageUtils.Translate("ここには現在のパスが表示されます", CurrentLanguage));

        var categoryName = ItemUtils.GetCategoryName(
            CurrentPath.CurrentSelectedCategory,
            CurrentLanguage,
            CurrentPath.CurrentSelectedCustomCategory
        );
        pathParts.Add(categoryName);

        var item = CurrentPath.CurrentSelectedItem;
        if (item != null)
            pathParts.Add(AEUtils.RemoveFormat(item.Title));

        var itemCategory = CurrentPath.CurrentSelectedItemCategory;
        if (itemCategory != null)
            pathParts.Add(LanguageUtils.Translate(itemCategory, CurrentLanguage));

        return prefix + AEUtils.GenerateSeparatedPath(pathParts.ToArray());
    }

    /// <summary>
    /// 選択されたアイテムからパスを生成します。
    /// </summary>
    /// <param name="item"></param>
    private void GeneratePathFromItem(Item item)
    {
        var avatarPath = item.SupportedAvatar.FirstOrDefault();
        var avatarName = DatabaseUtils.GetAvatarName(Items, avatarPath);

        CurrentPath.CurrentSelectedAvatar = avatarName ?? "*";
        CurrentPath.CurrentSelectedAvatarPath = avatarPath;
        CurrentPath.CurrentSelectedCategory = item.Type;

        if (item.Type == ItemType.Custom)
        {
            CurrentPath.CurrentSelectedCustomCategory = item.CustomCategory;
        }

        CurrentPath.CurrentSelectedItem = item;
    }
    #endregion

    #region 戻るボタンの処理
    /// <summary>
    /// 戻るボタンが押された際の処理を行います。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UndoButton_Click(object? sender, EventArgs? e)
    {
        // 検索中だった場合は前の画面までとりあえず戻してあげる
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
            // エラー音を再生
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

        if (_leftWindow == LeftWindow.Author)
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
        else if (_leftWindow == LeftWindow.Default)
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

    /// <summary>
    /// コントロール上でサイドボタンが押された際の処理を行います。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMouseClick(object? sender, MouseEventArgs? e)
    {
        if (e?.Button != MouseButtons.XButton1) return;
        UndoButton_Click(null, null);
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
        SearchFilter searchFilter = AEUtils.GetSearchFilter(SearchBox.Text);

        if (_openingWindow is Window.ItemFolderCategoryList or Window.ItemFolderItemsList)
        {
            GenerateFilteredFolderItems(searchFilter);
        }
        else
        {
            GenerateFilteredItem(searchFilter);
        }

        var pathTextList = new List<string>();

        var filters = new (string label, string[] values)[]
        {
            ("作者", searchFilter.Author),
            ("タイトル", searchFilter.Title),
            ("BoothID", searchFilter.BoothId),
            ("アバター", searchFilter.Avatar),
            ("カテゴリ", searchFilter.Category),
            ("メモ", searchFilter.ItemMemo),
            ("フォルダ名", searchFilter.FolderName),
            ("ファイル名", searchFilter.FileName),
            ("実装アバター", searchFilter.ImplementedAvatars)
        };

        foreach (var (label, values) in filters)
        {
            if (values.Length > 0)
            {
                string translatedLabel = label == "BoothID" ? label : LanguageUtils.Translate(label, CurrentLanguage);
                pathTextList.Add($"{translatedLabel}: {string.Join(", ", values)}");
            }
        }

        if (searchFilter.SearchWords.Length > 0)
        {
            pathTextList.Add(string.Join(", ", searchFilter.SearchWords));
        }

        PathTextBox.Text = LanguageUtils.Translate("検索中... - ", CurrentLanguage) + string.Join(" / ", pathTextList);
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
            _leftWindow = LeftWindow.Default;
            CurrentPath = new CurrentPath();
            _openingWindow = Window.Nothing;
        }

        var controls = AvatarItemExplorer.Controls.Cast<Control>().ToList();
        controls.Reverse();

        AvatarItemExplorer.SuspendLayout();
        AvatarItemExplorer.AutoScroll = false;

        controls.ForEach(control =>
        {
            if (control is Label label)
            {
                label.Visible = startLabelVisible;
                return;
            }

            control.Visible = false;
            control.Dispose();
        });

        AvatarItemExplorer.ResumeLayout();
        AvatarItemExplorer.AutoScroll = true;
    }

    /// <summary>
    /// メイン画面左の画面をリセットします。
    /// </summary>
    /// <param name="page"></param>
    private static void ResetAvatarPage(TabPage page)
    {
        var controls = page.Controls.Cast<Control>().Reverse().ToList();

        page.SuspendLayout();
        page.AutoScroll = false;

        controls.ForEach(control =>
        {
            control.Visible = false;
            control.Dispose();
        });

        page.ResumeLayout();
        page.AutoScroll = true;
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
                break;
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

        AddItemForm addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, false, null, dragFilePathArr);
        EventHandler itemAdded = (_, _) =>
        {
            RefleshWindow();
            DatabaseUtils.SaveItemsData(Items);
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

        AddItemForm addItem = new(this, ItemType.Avatar, null, false, null, dragFilePathArr);
        addItem.ItemAdded += (_, _) =>
        {
            RefleshWindow();
            DatabaseUtils.SaveItemsData(Items);
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
                if (index > 60) break;
                fileName = currentTimeStr + $"_{index}.csv";
                index++;
            }

            if (index > 60)
            {
                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("エクスポートに失敗しました", CurrentLanguage),
                    LanguageUtils.Translate("エラー", CurrentLanguage),
                    true
                );
                return;
            }

            var commonAvatarResult = FormUtils.ShowConfirmDialog(
                LanguageUtils.Translate("対応アバターの欄に共通素体グループのアバターも追加しますか？", CurrentLanguage),
                LanguageUtils.Translate("確認", CurrentLanguage)
            );

            using var sw = new StreamWriter("./Output/" + fileName, false, Encoding.UTF8);
            sw.WriteLine("Title,AuthorName,AuthorImageFilePath,ImagePath,Type,Memo,SupportedAvatars,ImplementedAvatars,BoothId,ItemPath");

            foreach (var item in Items)
            {
                List<string> SupportedAvatarNames = new List<string>();
                List<string> SupportedAvatarPaths = new List<string>();

                foreach (var avatar in item.SupportedAvatar)
                {
                    var avatarName = DatabaseUtils.GetAvatarName(Items, avatar);
                    if (avatarName == null) continue;

                    SupportedAvatarNames.Add(avatarName);
                    SupportedAvatarPaths.Add(avatar);

                    if (!commonAvatarResult) continue;

                    var commonAvatarGroup = CommonAvatars.Where(commonAvatar => commonAvatar.Avatars.Contains(avatar));
                    foreach (var commonAvatar in commonAvatarGroup)
                    {
                        foreach (var commonAvatarPath in commonAvatar.Avatars)
                        {
                            if (SupportedAvatarPaths.Contains(commonAvatarPath)) continue;

                            var name = DatabaseUtils.GetAvatarName(Items, commonAvatarPath);
                            if (name == null) continue;

                            SupportedAvatarNames.Add(name);
                            SupportedAvatarPaths.Add(commonAvatarPath);
                        }
                    }
                }

                List<string> ImplementedAvatarNames = new List<string>();
                foreach (var avatar in item.ImplementedAvatars)
                {
                    var avatarName = DatabaseUtils.GetAvatarName(Items, avatar);
                    if (avatarName == null) continue;

                    ImplementedAvatarNames.Add(avatarName);
                }

                var itemTitle = CsvUtils.EscapeCsv(item.Title);
                var authorName = CsvUtils.EscapeCsv(item.AuthorName);
                var authorImageFilePath = CsvUtils.EscapeCsv(item.AuthorImageFilePath);
                var imagePath = CsvUtils.EscapeCsv(item.ImagePath);
                var type = CsvUtils.EscapeCsv(ItemUtils.GetCategoryName(item.Type, CurrentLanguage, item.CustomCategory));
                var memo = CsvUtils.EscapeCsv(item.ItemMemo);
                var SupportedAvatarList = CsvUtils.EscapeCsv(string.Join(Environment.NewLine, SupportedAvatarNames));
                var ImplementedAvatarList = CsvUtils.EscapeCsv(string.Join(Environment.NewLine, ImplementedAvatarNames));
                var boothId = CsvUtils.EscapeCsv(item.BoothId.ToString());
                var itemPath = CsvUtils.EscapeCsv(item.ItemPath);

                sw.WriteLine($"{itemTitle},{authorName},{authorImageFilePath},{imagePath},{type},{memo},{SupportedAvatarList},{ImplementedAvatarList},{boothId},{itemPath}");
            }

            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("Outputフォルダにエクスポートが完了しました！\nファイル名: ", CurrentLanguage) + fileName,
                LanguageUtils.Translate("完了", CurrentLanguage)
            );
            ExportButton.Enabled = true;
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("エクスポートに失敗しました", CurrentLanguage),
                LanguageUtils.Translate("エラー", CurrentLanguage),
                true
            );
            LogUtils.ErrorLogger("エクスポートに失敗しました。", ex);
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
                if (index > 60) break;
                fileName = currentTimeStr + $"_{index}.zip";
                index++;
            }

            if (index > 60)
            {
                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("バックアップに失敗しました", CurrentLanguage),
                    LanguageUtils.Translate("エラー", CurrentLanguage),
                    true
                );
                return;
            }

            ZipFile.CreateFromDirectory("./Datas", "./Backup/" + fileName);

            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("Backupフォルダにバックアップが完了しました！\n\n復元したい場合は、\"データを読み込む\"ボタンで現在作成されたファイルを展開したものを選択してください。\n\nファイル名: ", CurrentLanguage) + fileName,
                LanguageUtils.Translate("完了", CurrentLanguage)
            );
            MakeBackupButton.Enabled = true;
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("バックアップに失敗しました", CurrentLanguage),
                LanguageUtils.Translate("エラー", CurrentLanguage),
                true
            );
            LogUtils.ErrorLogger("バックアップに失敗しました。", ex);
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
            if (control.Name == "LanguageBox" || string.IsNullOrEmpty(control.Text)) continue;
            _controlNames.TryAdd(control.Name, control.Text);
            control.Text = LanguageUtils.Translate(_controlNames[control.Name], CurrentLanguage);
            ChangeControlFont(control);
        }

        string[] sortingItems = ["タイトル", "作者", "登録日時", "更新日時", "実装済み", "未実装"];
        var selected = SortingBox.SelectedIndex;
        SortingBox.Items.Clear();
        SortingBox.Items.AddRange(sortingItems.Select(item => LanguageUtils.Translate(item, CurrentLanguage)).ToArray());
        SortingBox.SelectedIndex = selected;

        foreach (Control control in AvatarSearchFilterList.Controls)
        {
            if (string.IsNullOrEmpty(control.Text)) continue;
            _controlNames.TryAdd(control.Name, control.Text);
            control.Text = LanguageUtils.Translate(_controlNames[control.Name], CurrentLanguage);
            ChangeControlFont(control);
        }

        foreach (Control control in ExplorerList.Controls)
        {
            if (string.IsNullOrEmpty(control.Text)) continue;
            _controlNames.TryAdd(control.Name, control.Text);
            control.Text = LanguageUtils.Translate(_controlNames[control.Name], CurrentLanguage);
            ChangeControlFont(control);
        }

        foreach (Control control in AvatarItemExplorer.Controls)
        {
            if (string.IsNullOrEmpty(control.Text)) continue;
            _controlNames.TryAdd(control.Name, control.Text);
            control.Text = LanguageUtils.Translate(_controlNames[control.Name], CurrentLanguage);
            ChangeControlFont(control);
        }

        ResizeControl();

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
        // 自動バックアップフォルダから復元するか聞く
        var result = FormUtils.ShowConfirmDialog(
            LanguageUtils.Translate("自動バックアップフォルダから復元しますか？", CurrentLanguage),
            LanguageUtils.Translate("確認", CurrentLanguage)
        );

        if (result)
        {
            var selectedBackupForm = new SelectAutoBackupForm(this);
            selectedBackupForm.ShowDialog();

            var backupPath = selectedBackupForm.SelectedBackupPath;

            if (string.IsNullOrEmpty(backupPath)) return;

            // バックアップフォルダが存在しない場合
            if (!Directory.Exists(backupPath))
            {
                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("バックアップフォルダが見つかりませんでした。", CurrentLanguage),
                    LanguageUtils.Translate("エラー", CurrentLanguage),
                    true
                );
                return;
            }

            try
            {
                var filePath = backupPath + "/ItemsData.json";
                if (!File.Exists(filePath))
                {
                    FormUtils.ShowMessageBox(
                        LanguageUtils.Translate("アイテムファイルが見つかりませんでした。", CurrentLanguage),
                        LanguageUtils.Translate("エラー", CurrentLanguage),
                        true
                    );
                }
                else
                {
                    Items = DatabaseUtils.LoadItemsData(filePath);
                    DatabaseUtils.FixSupportedAvatarPath(ref Items);
                    DatabaseUtils.UpdateEmptyDates(ref Items);
                    DatabaseUtils.FixItemDates(ref Items);
                    DatabaseUtils.FixRelativePathEscape(ref Items);
                    DatabaseUtils.SaveItemsData(Items);
                }

                var filePath2 = backupPath + "/CommonAvatar.json";
                if (!File.Exists(filePath2))
                {
                    FormUtils.ShowMessageBox(
                        LanguageUtils.Translate("共通素体ファイルが見つかりませんでした。", CurrentLanguage),
                        LanguageUtils.Translate("エラー", CurrentLanguage),
                        true
                    );
                }
                else
                {
                    CommonAvatars = DatabaseUtils.LoadCommonAvatarData(filePath2);
                    DatabaseUtils.SaveCommonAvatarData(CommonAvatars);
                }

                var customCategoryPath = backupPath + "/CustomCategory.txt";
                if (!File.Exists(customCategoryPath))
                {
                    FormUtils.ShowMessageBox(
                        LanguageUtils.Translate("カスタムカテゴリーファイルが見つかりませんでした。", CurrentLanguage),
                        LanguageUtils.Translate("エラー", CurrentLanguage),
                        true
                    );
                }
                else
                {
                    CustomCategories = DatabaseUtils.LoadCustomCategoriesData(customCategoryPath);
                    DatabaseUtils.SaveCustomCategoriesData(CustomCategories);
                }

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("復元が完了しました。", CurrentLanguage),
                    LanguageUtils.Translate("完了", CurrentLanguage)
                );
            }
            catch (Exception ex)
            {
                LogUtils.ErrorLogger("データの読み込みに失敗しました。", ex);
                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("データの読み込みに失敗しました。詳細はErrorLog.txtをご覧ください。", CurrentLanguage),
                    LanguageUtils.Translate("エラー", CurrentLanguage),
                    true
                );
            }
        }
        else
        {
            FolderBrowserDialog fbd = new()
            {
                UseDescriptionForTitle = true,
                Description = LanguageUtils.Translate("以前のバージョンのDatasフォルダ、もしくは展開したバックアップフォルダを選択してください", CurrentLanguage),
                ShowNewFolderButton = false
            };
            if (fbd.ShowDialog() != DialogResult.OK) return;

            try
            {
                if (Directory.Exists(Path.Combine(fbd.SelectedPath, "Datas")) && File.Exists(Path.Combine(fbd.SelectedPath, "Datas", "ItemsData.json")) && !File.Exists(Path.Combine(fbd.SelectedPath, "ItemsData.json")))
                {
                    fbd.SelectedPath += "/Datas";
                }

                var filePath = fbd.SelectedPath + "/ItemsData.json";
                if (!File.Exists(filePath))
                {
                    FormUtils.ShowMessageBox(
                        LanguageUtils.Translate("アイテムファイルが見つかりませんでした。", CurrentLanguage),
                        LanguageUtils.Translate("エラー", CurrentLanguage),
                        true
                    );
                }
                else
                {
                    Items = DatabaseUtils.LoadItemsData(filePath);
                    DatabaseUtils.FixSupportedAvatarPath(ref Items);
                    DatabaseUtils.UpdateEmptyDates(ref Items);
                    DatabaseUtils.FixItemDates(ref Items);
                    DatabaseUtils.FixRelativePathEscape(ref Items);
                    DatabaseUtils.SaveItemsData(Items);
                }

                var filePath2 = fbd.SelectedPath + "/CommonAvatar.json";
                if (!File.Exists(filePath2))
                {
                    FormUtils.ShowMessageBox(
                        LanguageUtils.Translate("共通素体ファイルが見つかりませんでした。", CurrentLanguage),
                        LanguageUtils.Translate("エラー", CurrentLanguage),
                        true
                    );
                }
                else
                {
                    CommonAvatars = DatabaseUtils.LoadCommonAvatarData(filePath2);
                    DatabaseUtils.SaveCommonAvatarData(CommonAvatars);
                }

                var customCategoryPath = fbd.SelectedPath + "/CustomCategory.txt";
                if (!File.Exists(customCategoryPath))
                {
                    FormUtils.ShowMessageBox(
                        LanguageUtils.Translate("カスタムカテゴリーファイルが見つかりませんでした。", CurrentLanguage),
                        LanguageUtils.Translate("エラー", CurrentLanguage),
                        true
                    );
                }
                else
                {
                    CustomCategories = DatabaseUtils.LoadCustomCategoriesData(customCategoryPath);
                    DatabaseUtils.SaveCustomCategoriesData(CustomCategories);
                }

                var result2 = FormUtils.ShowConfirmDialog(
                    LanguageUtils.Translate("Thumbnailフォルダ、AuthorImageフォルダ、Itemsフォルダもコピーしますか？", CurrentLanguage),
                    LanguageUtils.Translate("確認", CurrentLanguage)
                );

                if (result2)
                {
                    SearchBox.Text = "";
                    SearchResultLabel.Text = "";
                    _isSearching = false;
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                    ResetAvatarExplorer(true);
                    PathTextBox.Text = GeneratePath();

                    FormUtils.ShowMessageBox(
                        LanguageUtils.Translate("コピーが完了しました。", CurrentLanguage),
                        LanguageUtils.Translate("完了", CurrentLanguage)
                    );
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
                            LogUtils.ErrorLogger("サムネイルのコピーに失敗しました。", ex);
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
                            LogUtils.ErrorLogger("作者画像のコピーに失敗しました。", ex);
                            authorImageResult = false;
                        }
                    }
                }

                if (Directory.Exists(itemsPath))
                {
                    try
                    {
                        FileSystemUtils.CopyDirectory(itemsPath, "./Datas/Items");
                    }
                    catch (Exception ex)
                    {
                        LogUtils.ErrorLogger("Itemsのコピーに失敗しました。", ex);
                        itemsResult = false;
                    }
                }

                var thumbilResultText = thumbnailResult ? "" : "\n" + LanguageUtils.Translate("サムネイルのコピーに一部失敗しています。", CurrentLanguage);
                var authorImageResultText = authorImageResult ? "" : "\n" + LanguageUtils.Translate("作者画像のコピーに一部失敗しています。", CurrentLanguage);
                var itemsResultText = itemsResult ? "" : "\n" + LanguageUtils.Translate("Itemsのコピーに一部失敗しています。", CurrentLanguage);

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("コピーが完了しました。", CurrentLanguage) + "\n\n" + LanguageUtils.Translate("コピー失敗一覧: ", CurrentLanguage) +
                    thumbilResultText + authorImageResultText + itemsResultText,
                    LanguageUtils.Translate("完了", CurrentLanguage)
                );
            }
            catch (Exception ex)
            {
                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("データの読み込みに失敗しました。詳細はErrorLog.txtをご覧ください。", CurrentLanguage),
                    LanguageUtils.Translate("エラー", CurrentLanguage)
                );
                LogUtils.ErrorLogger("データの読み込みに失敗しました。", ex);
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
        ManageCommonAvatarsForm manageCommonAvatar = new(this);
        manageCommonAvatar.ShowDialog();
        RefleshWindow();
        PathTextBox.Text = GeneratePath();
        DatabaseUtils.SaveCommonAvatarData(CommonAvatars);
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
        if (previousSize is <= 0 or >= float.MaxValue) return;
        control.Font = new Font(GuiFont, previousSize, FontStyle.Bold);
    }

    /// <summary>
    /// フォームのリサイズ時にコントロールのサイズや位置を変更します。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Main_Resize(object sender, EventArgs e)
    {
        _resizeTimer.Stop();
        _resizeTimer.Start();
    }

    /// <summary>
    /// コントロールのサイズや位置を変更します。
    /// </summary>
    private void ResizeControl()
    {
        if (!_initialized) return;

        var labelControl = AvatarItemExplorer.Controls.OfType<Label>().First();
        var allControls = Controls.OfType<Control>().ToList();
        allControls.Add(labelControl);

        foreach (Control control in allControls)
        {
            // サイズのスケーリング
            if (!_defaultControlSize.TryGetValue(control.Name, out var defaultSize))
            {
                defaultSize = new ControlScale()
                {
                    ScreenLocationYRatio = (float)control.Location.Y / _initialFormSize.Height,
                    ScreenLocationXRatio = (float)control.Location.X / _initialFormSize.Width,
                    ScreenWidthRatio = (float)control.Size.Width / _initialFormSize.Width,
                    ScreenHeightRatio = (float)control.Size.Height / _initialFormSize.Height,
                    ScreenFontSize = control.Font.Size / _initialFormSize.Height
                };

                _defaultControlSize.Add(control.Name, defaultSize);
            }

            var newWidth = (int)(defaultSize.ScreenWidthRatio * ClientSize.Width);
            var newHeight = (int)(defaultSize.ScreenHeightRatio * ClientSize.Height);

            control.Size = new Size(newWidth, newHeight);

            var newX = (int)(defaultSize.ScreenLocationXRatio * ClientSize.Width);
            var newY = (int)(defaultSize.ScreenLocationYRatio * ClientSize.Height);

            control.Location = new Point(newX, newY);

            if (control is Label or TextBox or ComboBox or Button)
            {
                var newFontSize = defaultSize.ScreenFontSize * ClientSize.Height;
                if (newFontSize <= 0 || newFontSize >= float.MaxValue) continue;
                control.Font = new Font(control.Font.FontFamily, newFontSize, control.Font.Style);
            }
        }

        labelControl.Location = labelControl.Location with
        {
            X = (AvatarItemExplorer.Width - labelControl.Width) / 2,
            Y = (AvatarItemExplorer.Height - labelControl.Height) / 2
        };

        AdjustLabelPosition();
        ScaleItemButtons();

        AEUtils.UpdateExplorerThumbnails(AvatarItemExplorer);
        AEUtils.UpdateExplorerThumbnails(AvatarPage);
        AEUtils.UpdateExplorerThumbnails(AvatarAuthorPage);
        AEUtils.UpdateExplorerThumbnails(CategoryPage);
    }

    /// <summary>
    /// ラベルの位置を調整します。
    /// </summary>
    private void AdjustLabelPosition()
    {
        label2.Location = label2.Location with
        {
            X = LanguageBox.Location.X + (LanguageBox.Width / 2) - (label2.Width / 2),
            Y = label2.Location.Y
        };

        SortingLabel.Location = SortingLabel.Location with
        {
            X = SortingBox.Location.X + (SortingBox.Width / 2) - (SortingLabel.Width / 2),
            Y = SortingLabel.Location.Y
        };

        label1.Location = label1.Location with
        {
            X = SearchBox.Location.X - label1.Width - 8,
            Y = SearchBox.Location.Y + (SearchBox.Height / 2) - (label1.Height / 2)
        };

        SearchResultLabel.Location = SearchResultLabel.Location with
        {
            X = label1.Location.X,
            Y = SearchBox.Location.Y + SearchBox.Height + 2
        };
    }

    /// <summary>
    /// メインフォーム内のボタンサイズを変更します。
    /// </summary>
    private void ScaleItemButtons()
    {
        const int avatarItemExplorerBaseWidth = 874;
        const int avatarItemListBaseWidth = 303;

        var avatarItemExplorerWidth = avatarItemExplorerBaseWidth + GetItemExplorerListWidth;
        var avatarItemListWidth = avatarItemListBaseWidth + GetAvatarListWidth;

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
                if (_lastBackupError) Text = CurrentVersionFormText + " - " + LanguageUtils.Translate("バックアップエラー", CurrentLanguage);
                return;
            }

            var timeSpan = DateTime.Now - _lastBackupTime;
            var minutes = timeSpan.Minutes;
            Text = CurrentVersionFormText + $" - {LanguageUtils.Translate("最終自動バックアップ: ", CurrentLanguage) + minutes + LanguageUtils.Translate("分前", CurrentLanguage)}";

            if (_lastBackupError) Text += " - " + LanguageUtils.Translate("バックアップエラー", CurrentLanguage);
        };

        timer.Start();
    }

    private readonly static string[] BackupFiles = new[]
    {
        "./Datas/ItemsData.json",
        "./Datas/CommonAvatar.json",
        "./Datas/CustomCategory.txt"
    };

    /// <summary>
    /// ファイルのバックアップを行います。
    /// </summary>
    private void BackupFile()
    {
        try
        {
            BackupUtils.Backup(BackupFiles);
            _lastBackupError = false;
            _lastBackupTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            _lastBackupError = true;
            LogUtils.ErrorLogger("自動バックアップに失敗しました。", ex);
        }
    }
    #endregion

    #region フォルダーが閉じられる際の処理
    /// <summary>
    /// フォームが閉じられる際にデータを保存し、一時フォルダを削除します。
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
            LogUtils.ErrorLogger("一時フォルダの削除に失敗しました。", ex);
        }
    }
    #endregion
}