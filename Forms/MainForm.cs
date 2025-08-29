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
    private const string CurrentVersion = "v1.1.5";

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
    /// 現在開かれている左ウィンドウのタイプを取得または設定します。これはパスなどに使われます。フィルター用ではありません。
    /// </summary>
    private LeftWindow _leftWindow = LeftWindow.Default;

    /// <summary>
    /// 現在開かれている左ウィンドウのタイプを取得または設定します。これは現在のフィルターを取得するのに使用されます。
    /// </summary>
    private LeftWindow _leftWindowFilter = LeftWindow.Default;

    /// <summary>
    /// 現在開いているメイン画面ウィンドウタイプ
    /// </summary>
    private Window _openingWindow = Window.Nothing;

    /// <summary>
    /// 左のウィンドウ(アバター)でのページ数です。
    /// </summary>
    private int _currentPageAvatar = 0;

    /// <summary>
    /// 左のウィンドウ(アバター)でのページ数です。
    /// </summary>
    private int _currentPageAuthor = 0;

    /// <summary>
    /// 右のウィンドウでのページ数です。
    /// </summary>
    private int _currentPage = 0;
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
    private readonly int _baseFilterListWidth;

    /// <summary>
    /// メイン画面右のアイテム欄の初期幅
    /// </summary>
    private readonly int _baseExplorerListWidth;

    /// <summary>
    /// リサイズ用のタイマー
    /// </summary>
    private readonly Timer _resizeTimer = new()
    {
        Interval = 100
    };

    /// <summary>
    /// FilterListの横の長さを元の長さから計算します。
    /// </summary>
    /// <returns></returns>
    private int GetFilterListWidth
        => FilterList.Width - _baseFilterListWidth;

    /// <summary>
    /// ExplorerListの横の長さを元の長さから計算します。
    /// </summary>
    /// <returns>ItemExplorerList Width</returns>
    private int GetExplorerListWidth
        => ExplorerList.Width - _baseExplorerListWidth;
    #endregion

    #region バックアップ関連の変数
    /// <summary>
    /// バックアップする間隔(ms)
    /// </summary>
    private int _backupInterval = 300000; // 5 Minutes

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

    /// <summary>
    /// 最後にBoothの情報を取得した時間を取得または設定します。
    /// </summary>
    private DateTime _lastGetTime;
    #endregion

    #region 設定ファイル関連の変数
    /// <summary>
    /// 1ページあたりの表示数です。
    /// </summary>
    private int _itemsPerPage = 30;

    /// <summary>
    /// サムネイルのプレビュースケールです。
    /// </summary>
    private float _previewScale = 1.0f;

    /// <summary>
    /// デフォルトの言語です。
    /// </summary>
    private int _defaultLanguage = 1;

    /// <summary>
    /// デフォルトの並び替え順です。
    /// </summary>
    private int _defaultSortOrder = 1;

    /// <summary>
    /// 商品名の括弧を削除するか決めることが出来ます。
    /// </summary>
    private bool _removeBrackets = false;

    /// <summary>
    /// ボタンの高さです。
    /// </summary>
    internal int ButtonSize = 64;

    /// <summary>
    /// ダークモードかどうかを決めます。
    /// </summary>
    internal bool DarkMode = false;
    #endregion

    #region フォームの初期化
    /// <summary>
    /// メインフォームを初期化します。
    /// </summary>
    internal MainForm(LaunchInfo launchInfo, ConfigurationManager configurationManager)
    {
        try
        {
            // Load Configulation
            SetConfigulationValue(configurationManager);

            Items = DatabaseUtils.LoadItemsData();
            CommonAvatars = DatabaseUtils.LoadCommonAvatarData();
            CustomCategories = DatabaseUtils.LoadCustomCategoriesData();

            // Add Missing Custom Categories
            var added = DatabaseUtils.CheckMissingCustomCategories(Items, CustomCategories);
            if (added) DatabaseUtils.SaveCustomCategoriesData(CustomCategories);

            AddFontFile();
            InitializeComponent();
            AdditionalInitialize();

            if (DarkMode) SetDarkMode();

            // Save the default Size
            _initialFormSize = ClientSize;
            _baseFilterListWidth = FilterList.Width;
            _baseExplorerListWidth = ExplorerList.Width;
            _resizeTimer.Tick += (s, ev) =>
            {
                _resizeTimer.Stop();
                BeginInvoke(() => ResizeControl());
            };
            _initialized = true;

            // Render Window
            RefleshWindow();

            // Start AutoBackup
            AutoBackup();

            // Set Backup Title Loop
            BackupTimeTitle();

            // Render Filter
            RenderFilter();

            // Render Filter Text
            RedrawFilterName();

            Text = $"VRChat Avatar Explorer {CurrentVersion} by ぷこるふ";

            if (_defaultLanguage != 1 && _defaultLanguage > 0 && _defaultLanguage <= LanguageBox.Items.Count)
            {
                LanguageBox.SelectedIndex = _defaultLanguage - 1;
            }

            if (_defaultSortOrder != 1 && _defaultSortOrder > 0 && _defaultSortOrder <= SortingBox.Items.Count)
            {
                SortingBox.SelectedIndex = _defaultSortOrder - 1;
            }

            // Check if the software is launched with a URL
            if (launchInfo.launchedWithUrl && launchInfo.assetDirs.Length != 0 && !string.IsNullOrEmpty(launchInfo.assetId))
            {
                AddItemForm addItem = new(this, ItemType.Avatar, null, false, null, launchInfo.assetDirs, launchInfo.assetId);
                addItem.ShowDialog();

                DatabaseUtils.SaveItemsData(Items);

                RefleshWindow();
            }

            AdjustLabelPosition();

            // Check Broken Item Paths
            DatabaseUtils.CheckBrokenItemPaths(Items, CurrentLanguage);
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox("ソフトの起動中にエラーが発生しました。\n\n" + ex, "エラー", true);
            Environment.Exit(0);
        }
    }

    private void AdditionalInitialize()
    {
        FilterList.DragEnter += FormUtils.DragEnter;
        FilterList.DragDrop += FilterList_DragDrop;
        FilterList.MouseWheel += AEUtils.OnScroll;
        FilterList.Scroll += AEUtils.OnScroll;

        ExplorerList.DragEnter += FormUtils.DragEnter;
        ExplorerList.DragDrop += ExplorerList_DragDrop;
        ExplorerList.MouseWheel += AEUtils.OnScroll;
        ExplorerList.Scroll += AEUtils.OnScroll;

        LanguageBox.SelectedIndex = 0;
        SortingBox.SelectedIndex = 0;
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

        GuiFont = _fontFamilies.TryGetValue(CurrentLanguage, out var family) ? family : new FontFamily("Yu Gothic UI");
    }

    private void SetConfigulationValue(ConfigurationManager configuration)
    {
        int itemsPerPage = int.TryParse(configuration["ItemsPerPage"], out var ipp) ? Math.Clamp(ipp, 1, 1000) : 30;
        float previewScale = float.TryParse(configuration["PreviewScale"], out var ps) ? Math.Clamp(ps, 0.1f, 10f) : 1.0f;
        int defaultLanguage = int.TryParse(configuration["DefaultLanguage"], out var dl) ? dl : 1;
        int defaultSortOrder = int.TryParse(configuration["DefaultSortOrder"], out var dso) ? dso : 1;
        int thumbnailUpdateTimeout = int.TryParse(configuration["ThumbnailUpdateTimeout"], out var tut) ? Math.Clamp(tut, 0, 10000) : 200;
        int backupInterval = int.TryParse(configuration["BackupInterval"], out var bi) ? Math.Clamp(bi, 1, 1000) : 5;
        bool removeBrackets = configuration["RemoveBrackets"] == "true";
        int buttonSize = int.TryParse(configuration["ButtonSize"], out var bs) ? Math.Clamp(bs, 1, 500) : 64;
        bool darkMode = configuration["DarkMode"] == "true";

        _itemsPerPage = itemsPerPage;
        _previewScale = previewScale;
        _defaultLanguage = defaultLanguage;
        _defaultSortOrder = defaultSortOrder;
        AEUtils.ThumbnailUpdateTimer.Interval = thumbnailUpdateTimeout;
        _backupInterval = backupInterval * 60000; // ms -> min
        _removeBrackets = removeBrackets;
        ButtonSize = buttonSize;
        DarkMode = darkMode;
    }

    private void SetDarkMode()
    {
        foreach (Control contorol in Controls)
        {
            DarkModeUtils.SetDarkMode(contorol);
        }
    }
    #endregion

    #region 左のリスト関連の処理
    /// <summary>
    /// メイン画面左のアバター欄を作成します。
    /// </summary>
    /// <param name="pageReset"></param>
    private void GenerateAvatarList(bool pageReset = true)
    {
        if (pageReset) _currentPageAvatar = 0;
        ResetAvatarPage(FilterList);

        var items = Items.Where(item => item.Type == ItemType.Avatar);
        if (!items.Any()) return;

        items = SortingBox.SelectedIndex switch
        {
            0 => items.OrderBy(item => item.GetTitle(_removeBrackets)),
            1 => items.OrderBy(item => item.AuthorName),
            2 => items.OrderByDescending(item => item.CreatedDate),
            3 => items.OrderByDescending(item => item.UpdatedDate),
            _ => items.OrderBy(item => item.GetTitle(_removeBrackets)),
        };

        int totalCount = items.Count();
        _currentPageAvatar = Math.Clamp(_currentPageAvatar, 0, TabPageUtils.GetTotalPages(totalCount, _itemsPerPage) - 1);

        FilterList.SuspendLayout();
        FilterList.AutoScroll = false;

        var index = 0;
        foreach (Item item in items.Skip(_currentPageAvatar * _itemsPerPage).Take(_itemsPerPage))
        {
            var description = ItemUtils.GetItemDescription(item, CurrentLanguage);

            Button button = AEUtils.CreateButton(DarkMode, ButtonSize, _previewScale, item.ImagePath, item.GetTitle(_removeBrackets), LanguageUtils.Translate("作者: ", CurrentLanguage) + item.AuthorName, true, description, GetFilterListWidth);
            button.Location = new Point(0, ((ButtonSize + 6) * index) + 2);
            button.MouseClick += OnMouseClick;

            void ButtonClick(object? sender, EventArgs? e)
            {
                CurrentPath = new CurrentPath
                {
                    CurrentSelectedAvatar = item.Title,
                    CurrentSelectedAvatarPath = item.ItemPath
                };

                _leftWindow = LeftWindow.Default;

                SearchBox.Text = string.Empty;
                SearchResultLabel.Text = string.Empty;
                _isSearching = false;

                GenerateCategoryList();
                PathTextBox.Text = GeneratePath();
            }

            button.Click += ButtonClick;
            button.Disposed += (_, _) =>
            {
                button.Click -= ButtonClick;
                button.ContextMenuStrip?.Dispose();
            };

            var createContextMenu = new CreateContextMenu();

            if (item.BoothId != -1)
            {
                createContextMenu.AddItem(
                    LanguageUtils.Translate("Boothリンクのコピー", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon),
                    (_, _) => BoothUtils.CopyItemBoothLink(item, CurrentLanguage),
                    Keys.C
                );

                createContextMenu.AddItem(
                    LanguageUtils.Translate("Boothリンクを開く", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon),
                    (_, _) => BoothUtils.OpenItenBoothLink(item, CurrentLanguage),
                    Keys.B
                );
            }

            createContextMenu.AddItem(
                LanguageUtils.Translate("この作者の他のアイテムを表示", CurrentLanguage),
                SharedImages.GetImage(SharedImages.Images.OpenIcon),
                (_, _) => SearchByAuthorName(item),
                Keys.A
            );

            createContextMenu.AddItem(
                LanguageUtils.Translate("サムネイル変更", CurrentLanguage),
                SharedImages.GetImage(SharedImages.Images.EditIcon),
                (_, _) =>
                {
                    if (!ChangeThumbnail(item)) return;

                    DatabaseUtils.SaveItemsData(Items);

                    // もしアバターの欄を右で開いていたら、そのサムネイルも更新しないといけないため。
                    if (_openingWindow == Window.ItemList && !_isSearching) GenerateItems();

                    // 検索中だと、検索画面を再読込してあげる
                    if (_isSearching) SearchItems();

                    RenderFilter(false);
                },
                Keys.T
            );

            createContextMenu.AddItem(
                LanguageUtils.Translate("サムネイル再取得", CurrentLanguage),
                SharedImages.GetImage(SharedImages.Images.EditIcon),
                async (_, _) =>
                {
                    bool result = await ReacquisitionThumbnailImage(item);
                    if (!result) return;

                    DatabaseUtils.SaveItemsData(Items);

                    // もしアバターの欄を右で開いていたら、そのサムネイルも更新しないといけないため。
                    if (_openingWindow == Window.ItemList && !_isSearching) GenerateItems();

                    // 検索中だと、検索画面を再読込してあげる
                    if (_isSearching) SearchItems();

                    RenderFilter(false);
                },
                Keys.R
            );

            createContextMenu.AddItem(
                LanguageUtils.Translate("編集", CurrentLanguage),
                SharedImages.GetImage(SharedImages.Images.EditIcon),
                (_, _) =>
                {
                    var prePath = item.ItemPath;

                    AddItemForm addItem = new(this, item.Type, item.CustomCategory, true, item, null);
                    addItem.ShowDialog();

                    // 対応アバターのパスを変えてあげる
                    DatabaseUtils.ChangeAllItemPaths(Items, prePath, item.ItemPath);

                    DatabaseUtils.SaveItemsData(Items);

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
                },
                Keys.E
            );

            createContextMenu.AddItem(
                LanguageUtils.Translate("メモの追加", CurrentLanguage),
                SharedImages.GetImage(SharedImages.Images.EditIcon),
                (_, _) =>
                {
                    if (!AddMemoToItem(item)) return;

                    DatabaseUtils.SaveItemsData(Items);

                    RefleshWindow();
                },
                Keys.M
            );

            createContextMenu.AddItem(
                LanguageUtils.Translate("アイテムフォルダの追加", CurrentLanguage),
                SharedImages.GetImage(SharedImages.Images.EditIcon),
                (_, _) => AddFolderToItem(item),
                Keys.A
            );

            createContextMenu.AddItem(
                LanguageUtils.Translate("削除", CurrentLanguage),
                SharedImages.GetImage(SharedImages.Images.TrashIcon),
                (_, _) =>
                {
                    var result = FormUtils.ShowConfirmDialog(
                        LanguageUtils.Translate("本当に削除しますか？", CurrentLanguage) + "\n\n" + item.Title,
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
                    if (item.Type == ItemType.Avatar) DeleteAvatarFromSupported(item);

                    Items.RemoveAll(i => i.ItemPath == item.ItemPath);

                    DatabaseUtils.SaveItemsData(Items);

                    FormUtils.ShowMessageBox(
                        LanguageUtils.Translate("削除が完了しました。", CurrentLanguage),
                        LanguageUtils.Translate("完了", CurrentLanguage)
                    );

                    RenderFilter(false);

                    if (_isSearching)
                    {
                        // フォルダー内検索の時
                        if (_openingWindow is Window.ItemFolderCategoryList or Window.ItemFolderItemsList)
                        {
                            // 選択されたアバターから現在の所まで来てる場合
                            if (undo2)
                            {
                                SearchBox.Text = string.Empty;
                                SearchResultLabel.Text = string.Empty;
                                _isSearching = false;

                                ResetAvatarExplorer(true);
                                PathTextBox.Text = GeneratePath();
                                return;
                            }

                            // アイテムとして選択されている場合
                            if (undo)
                            {
                                SearchBox.Text = string.Empty;
                                SearchResultLabel.Text = string.Empty;
                                _isSearching = false;

                                GenerateItems();
                                PathTextBox.Text = GeneratePath();
                            }
                        }
                        else
                        {
                            SearchItems();
                        }
                    }
                    else
                    {
                        // アバターが選択された状態(CurrentSelectedAvatarPathとして設定されている時)
                        if (undo2)
                        {
                            ResetAvatarExplorer(true);
                            PathTextBox.Text = GeneratePath();
                            return;
                        }

                        // フォルダーを開いていって、アイテムが選択された状態(CurrentSelectedItemとして設定されている時)
                        if (undo)
                        {
                            GenerateItems();
                            PathTextBox.Text = GeneratePath();
                            return;
                        }

                        // アイテム画面に既にいる
                        if (_openingWindow == Window.ItemList)
                        {
                            GenerateItems();
                            return;
                        }

                        // アイテム画面の前にいる
                        RefleshWindow();
                    }
                },
                Keys.D
            );

            button.ContextMenuStrip = createContextMenu.ContextMenuStrip;
            FilterList.Controls.Add(button);
            index++;
        }

        TabPageUtils.AddNavigationButtons(
            FilterList,
            DarkMode,
            ((ButtonSize + 6) * index) + 2,
            _currentPageAvatar, _itemsPerPage, totalCount, true,
            CurrentLanguage,
            (_, _) => _currentPageAvatar--,
            (_, _) => _currentPageAvatar++,
            (_, _) => _currentPageAvatar = 0,
            (_, _) => _currentPageAvatar = TabPageUtils.GetTotalPages(totalCount, _itemsPerPage) - 1,
            (sender, _) =>
            {
                if (sender is int pageCount) _currentPageAvatar = pageCount;
                RenderFilter(false);
            }
        );

        FilterList.ResumeLayout();
        FilterList.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(FilterList);
    }

    /// <summary>
    /// メイン画面左の作者欄を作成します。
    /// </summary>
    /// <param name="pageReset"></param>
    private void GenerateAuthorList(bool pageReset = true)
    {
        if (pageReset) _currentPageAuthor = 0;
        ResetAvatarPage(FilterList);

        var authors = ItemUtils.GetAuthors(Items);
        if (authors.Count == 0) return;

        authors.Sort((a, b) => string.Compare(a.AuthorName, b.AuthorName, StringComparison.OrdinalIgnoreCase));
        int totalCount = authors.Count;
        _currentPageAuthor = Math.Clamp(_currentPageAuthor, 0, TabPageUtils.GetTotalPages(totalCount, _itemsPerPage) - 1);

        FilterList.SuspendLayout();
        FilterList.AutoScroll = false;

        var index = 0;
        foreach (var author in authors.Skip(_currentPageAuthor * _itemsPerPage).Take(_itemsPerPage))
        {
            try
            {
                Button button = AEUtils.CreateButton(DarkMode, ButtonSize, _previewScale, author.AuthorImagePath, author.AuthorName, Items.Count(item => item.AuthorName == author.AuthorName) + LanguageUtils.Translate("個の項目", CurrentLanguage), true, author.AuthorName, GetFilterListWidth);
                button.Location = new Point(0, ((ButtonSize + 6) * index) + 2);
                button.MouseClick += OnMouseClick;

                void ButtonClick(object? sender, EventArgs? e)
                {
                    CurrentPath = new CurrentPath
                    {
                        CurrentSelectedAuthor = author
                    };

                    _leftWindow = LeftWindow.Author;

                    SearchBox.Text = string.Empty;
                    SearchResultLabel.Text = string.Empty;
                    _isSearching = false;

                    GenerateCategoryList();
                    PathTextBox.Text = GeneratePath();
                }

                button.Click += ButtonClick;
                button.Disposed += (_, _) =>
                {
                    button.Click -= ButtonClick;
                    button.ContextMenuStrip?.Dispose();
                };

                var createContextMenu = new CreateContextMenu();

                createContextMenu.AddItem(
                    LanguageUtils.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon),
                    (_, _) =>
                    {
                        if (!ChangeThumbnail(author)) return;

                        DatabaseUtils.SaveItemsData(Items);

                        RenderFilter(false);
                    },
                    Keys.T
                );

                button.ContextMenuStrip = createContextMenu.ContextMenuStrip;
                FilterList.Controls.Add(button);
                index++;
            }
            catch (Exception ex)
            {
                FormUtils.ShowMessageBox("Error Occured while rendering item button\n\nError: " + ex, "Button Error", true);
            }
        }

        TabPageUtils.AddNavigationButtons(
            FilterList,
            DarkMode,
            ((ButtonSize + 6) * index) + 2,
            _currentPageAuthor, _itemsPerPage, totalCount, true,
            CurrentLanguage,
            (_, _) => _currentPageAuthor--,
            (_, _) => _currentPageAuthor++,
            (_, _) => _currentPageAuthor = 0,
            (_, _) => _currentPageAuthor = TabPageUtils.GetTotalPages(totalCount, _itemsPerPage) - 1,
            (sender, _) =>
            {
                if (sender is int pageCount) _currentPageAuthor = pageCount;
                RenderFilter(false);
            }
        );

        FilterList.ResumeLayout();
        FilterList.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(FilterList);
    }

    /// <summary>
    /// メイン画面左のカテゴリー欄を作成します。
    /// </summary>
    private void GenerateCategoryListLeft()
    {
        ResetAvatarPage(FilterList);

        FilterList.SuspendLayout();
        FilterList.AutoScroll = false;

        var index = 0;
        foreach (ItemType itemType in Enum.GetValues<ItemType>())
        {
            try
            {
                if (itemType is ItemType.Unknown or ItemType.Custom) continue;

                var items = Items.Where(item => item.Type == itemType);
                var itemCount = items.Count();

                CustomItemButton button = AEUtils.CreateButton(DarkMode, ButtonSize, _previewScale, null, ItemUtils.GetCategoryName(itemType, CurrentLanguage), itemCount + LanguageUtils.Translate("個の項目", CurrentLanguage), true, string.Empty, GetFilterListWidth);
                button.Location = new Point(0, ((ButtonSize + 6) * index) + 2);
                button.MouseClick += OnMouseClick;

                void ButtonClick(object? sender, EventArgs? e)
                {
                    CurrentPath = new CurrentPath
                    {
                        CurrentSelectedCategory = itemType
                    };

                    _leftWindow = LeftWindow.Category;
                    SearchBox.Text = string.Empty;
                    SearchResultLabel.Text = string.Empty;
                    _isSearching = false;

                    GenerateItems();
                    PathTextBox.Text = GeneratePath();
                }

                button.Click += ButtonClick;
                button.Disposed += (_, _) =>
                {
                    button.Click -= ButtonClick;
                    button.ContextMenuStrip?.Dispose();
                };

                FilterList.Controls.Add(button);
                index++;
            }
            catch (Exception ex)
            {
                FormUtils.ShowMessageBox("Error Occured while rendering item button\n\nError: " + ex, "Button Error", true);
            }
        }

        if (CustomCategories.Count != 0)
        {
            foreach (var customCategory in CustomCategories)
            {
                try
                {
                    var items = Items.Where(item => item.CustomCategory == customCategory);
                    var itemCount = items.Count();

                    Button button = AEUtils.CreateButton(DarkMode, ButtonSize, _previewScale, null, customCategory, itemCount + LanguageUtils.Translate("個の項目", CurrentLanguage), true, string.Empty, GetFilterListWidth);
                    button.Location = new Point(0, ((ButtonSize + 6) * index) + 2);
                    button.MouseClick += OnMouseClick;

                    void ButtonClick(object? sender, EventArgs? e)
                    {
                        CurrentPath = new CurrentPath
                        {
                            CurrentSelectedCategory = ItemType.Custom,
                            CurrentSelectedCustomCategory = customCategory
                        };

                        _leftWindow = LeftWindow.Category;

                        SearchBox.Text = string.Empty;
                        SearchResultLabel.Text = string.Empty;
                        _isSearching = false;

                        GenerateItems();
                        PathTextBox.Text = GeneratePath();
                    }

                    button.Click += ButtonClick;
                    button.Disposed += (_, _) =>
                    {
                        button.Click -= ButtonClick;
                        button.ContextMenuStrip?.Dispose();
                    };

                    FilterList.Controls.Add(button);
                    index++;
                }
                catch (Exception ex)
                {
                    FormUtils.ShowMessageBox("Error Occured while rendering item button\n\nError: " + ex, "Button Error", true);
                }
            }
        }

        FilterList.ResumeLayout();
        FilterList.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(FilterList);
    }

    /// <summary>
    /// 左のフィルター画面を変更します
    /// </summary>
    private void ChangeFilter()
    {
        if (_leftWindowFilter == LeftWindow.Nothing)
        {
            _leftWindowFilter = LeftWindow.Default;
        }
        else if (_leftWindowFilter == LeftWindow.Default)
        {
            _leftWindowFilter = LeftWindow.Author;
        }
        else if (_leftWindowFilter == LeftWindow.Author)
        {
            _leftWindowFilter = LeftWindow.Category;
        }
        else if (_leftWindowFilter == LeftWindow.Category)
        {
            _leftWindowFilter = LeftWindow.Nothing;
        }

        RenderFilter(false);
    }

    /// <summary>
    /// 左の画面を読み込みます。
    /// </summary>
    /// <param name="reset"></param>
    private void RenderFilter(bool reset = true)
    {
        var visibleChanged = false;
        if (_leftWindowFilter == LeftWindow.Nothing)
        {
            if (FilterList.Visible)
            {
                FilterList.Visible = false;
                visibleChanged = true;
            } 
        }
        else if (_leftWindowFilter == LeftWindow.Default)
        {
            if (!FilterList.Visible)
            {
                FilterList.Visible = true;
                visibleChanged = true;
            }
            GenerateAvatarList(reset);
        }
        else if (_leftWindowFilter == LeftWindow.Author)
        {
            if (!FilterList.Visible)
            {
                FilterList.Visible = true;
                visibleChanged = true;
            }
            GenerateAuthorList(reset);
        }
        else if (_leftWindowFilter == LeftWindow.Category)
        {
            if (!FilterList.Visible)
            {
                FilterList.Visible = true;
                visibleChanged = true;
            }
            GenerateCategoryListLeft();
        }

        if (visibleChanged) ResizeControl();
    }

    /// <summary>
    /// フィルターテキストを描画します。
    /// </summary>
    private void RedrawFilterName()
        => ChangeFilterButton.Text = LanguageUtils.Translate("フィルター: {0}", CurrentLanguage, FilterUtils.GetFilterName(_leftWindowFilter, CurrentLanguage));
    #endregion

    #region 右のリスト関連の処理
    /// <summary>
    /// メイン画面右のカテゴリ欄を作成します。
    /// </summary>
    private void GenerateCategoryList()
    {
        _openingWindow = Window.ItemCategoryList;
        ResetAvatarExplorer();

        ExplorerList.SuspendLayout();
        ExplorerList.AutoScroll = false;

        var index = 0;
        foreach (ItemType itemType in Enum.GetValues<ItemType>())
        {
            try
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

                Button button = AEUtils.CreateButton(DarkMode, ButtonSize, _previewScale, null, ItemUtils.GetCategoryName(itemType, CurrentLanguage), itemCount + LanguageUtils.Translate("個の項目", CurrentLanguage), false, string.Empty, GetExplorerListWidth);
                button.Location = new Point(0, ((ButtonSize + 6) * index) + 2);
                button.MouseClick += OnMouseClick;

                void ButtonClick(object? sender, EventArgs? e)
                {
                    CurrentPath.CurrentSelectedCategory = itemType;
                    GenerateItems();
                    PathTextBox.Text = GeneratePath();
                }

                button.Click += ButtonClick;
                button.Disposed += (_, _) =>
                {
                    button.Click -= ButtonClick;
                    button.ContextMenuStrip?.Dispose();
                };

                ExplorerList.Controls.Add(button);
                index++;
            }
            catch (Exception ex)
            {
                FormUtils.ShowMessageBox("Error Occured while rendering item button\n\nError: " + ex, "Button Error", true);
            }
        }

        if (CustomCategories.Count != 0)
        {
            foreach (var customCategory in CustomCategories)
            {
                try
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

                    Button button = AEUtils.CreateButton(DarkMode, ButtonSize, _previewScale, null, customCategory, itemCount + LanguageUtils.Translate("個の項目", CurrentLanguage), false, string.Empty, GetExplorerListWidth);
                    button.Location = new Point(0, ((ButtonSize + 6) * index) + 2);
                    button.MouseClick += OnMouseClick;

                    void ButtonClick(object? sender, EventArgs? e)
                    {
                        CurrentPath.CurrentSelectedCategory = ItemType.Custom;
                        CurrentPath.CurrentSelectedCustomCategory = customCategory;
                        GenerateItems();
                        PathTextBox.Text = GeneratePath();
                    }

                    button.Click += ButtonClick;
                    button.Disposed += (_, _) =>
                    {
                        button.Click -= ButtonClick;
                        button.ContextMenuStrip?.Dispose();
                    };

                    ExplorerList.Controls.Add(button);
                    index++;
                }
                catch (Exception ex)
                {
                    FormUtils.ShowMessageBox("Error Occured while rendering item button\n\nError: " + ex, "Button Error", true);
                }
            }
        }

        ExplorerList.ResumeLayout();
        ExplorerList.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(ExplorerList);
    }

    /// <summary>
    /// メイン画面右のアイテム欄を作成します。
    /// </summary>
    /// <param name="pageReset"></param>
    private void GenerateItems(bool pageReset = true)
    {
        if (pageReset) _currentPage = 0;
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
            0 => filteredItems.OrderBy(item => item.GetTitle(_removeBrackets)).ToArray(),
            1 => filteredItems.OrderBy(item => item.AuthorName).ToArray(),
            2 => filteredItems.OrderByDescending(item => item.CreatedDate).ToArray(),
            3 => filteredItems.OrderByDescending(item => item.UpdatedDate).ToArray(),
            4 => filteredItems.OrderBy(item => ItemUtils.ContainsSelectedAvatar(item, CurrentPath.CurrentSelectedAvatarPath) ? 0 : 1),
            5 => filteredItems.OrderBy(item => ItemUtils.ContainsSelectedAvatar(item, CurrentPath.CurrentSelectedAvatarPath) ? 1 : 0),
            _ => filteredItems.OrderBy(item => item.GetTitle(_removeBrackets)).ToArray(),
        };

        if (!filteredItems.Any()) return;

        int totalCount = filteredItems.Count();
        _currentPage = Math.Clamp(_currentPage, 0, TabPageUtils.GetTotalPages(totalCount, _itemsPerPage) - 1);

        ExplorerList.SuspendLayout();
        ExplorerList.AutoScroll = false;

        var index = 0;
        foreach (Item item in filteredItems.Skip(_currentPage * _itemsPerPage).Take(_itemsPerPage))
        {
            try
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

                Button button = AEUtils.CreateButton(DarkMode, ButtonSize, _previewScale, item.ImagePath, item.GetTitle(_removeBrackets), authorText, false, description, GetExplorerListWidth);
                button.Location = new Point(0, ((ButtonSize + 6) * index) + 2);
                button.MouseClick += OnMouseClick;

                if (SortingBox.SelectedIndex == 4 || SortingBox.SelectedIndex == 5)
                {
                    var currentAvatar = CurrentPath.CurrentSelectedAvatarPath;
                    if (!string.IsNullOrEmpty(currentAvatar))
                    {
                        button.BackColor = item.ImplementedAvatars.Contains(currentAvatar) ? Color.LightGreen : Color.LightPink;
                    }
                }

                void ButtonClick(object? sender, EventArgs? e)
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
                        DatabaseUtils.ChangeAllItemPaths(Items, prePath, item.ItemPath);

                        DatabaseUtils.SaveItemsData(Items);

                        if (CurrentPath.CurrentSelectedAvatarPath == prePath)
                        {
                            CurrentPath.CurrentSelectedAvatar = item.Title;
                            CurrentPath.CurrentSelectedAvatarPath = item.ItemPath;
                        }

                        RefleshWindow();
                    }

                    CurrentPath.CurrentSelectedItem = item;
                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                }

                button.Click += ButtonClick;
                button.Disposed += (_, _) =>
                {
                    button.Click -= ButtonClick;
                    button.ContextMenuStrip?.Dispose();
                };

                var createContextMenu = new CreateContextMenu();

                if (Directory.Exists(item.ItemPath))
                {
                    createContextMenu.AddItem(
                        LanguageUtils.Translate("フォルダを開く", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.OpenIcon),
                        (_, _) => FileSystemUtils.OpenItemFolder(item, CurrentLanguage),
                        Keys.O
                    );
                }

                if (item.BoothId != -1)
                {
                    createContextMenu.AddItem(
                        LanguageUtils.Translate("Boothリンクのコピー", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.CopyIcon),
                        (_, _) => BoothUtils.CopyItemBoothLink(item, CurrentLanguage),
                        Keys.C
                    );

                    createContextMenu.AddItem(
                        LanguageUtils.Translate("Boothリンクを開く", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.CopyIcon),
                        (_, _) => BoothUtils.OpenItenBoothLink(item, CurrentLanguage),
                        Keys.B
                    );
                }

                createContextMenu.AddItem(
                    LanguageUtils.Translate("この作者の他のアイテムを表示", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon),
                    (_, _) => SearchByAuthorName(item),
                    Keys.A
                );

                createContextMenu.AddItem(
                    LanguageUtils.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon),
                    (_, _) =>
                    {
                        if (!ChangeThumbnail(item)) return;

                        DatabaseUtils.SaveItemsData(Items);

                        if (_isSearching)
                        {
                            SearchItems();
                        }
                        else
                        {
                            GenerateItems(false);
                        }

                        RenderFilter(false);
                    },
                    Keys.T
                );

                createContextMenu.AddItem(
                    LanguageUtils.Translate("サムネイル再取得", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon),
                    async (_, _) =>
                    {
                        bool result = await ReacquisitionThumbnailImage(item);
                        if (!result) return;

                        DatabaseUtils.SaveItemsData(Items);

                        // もしアバターの欄を右で開いていたら、そのサムネイルも更新しないといけないため。
                        if (_openingWindow == Window.ItemList && !_isSearching) GenerateItems();

                        // 検索中だと、検索画面を再読込してあげる
                        if (_isSearching) SearchItems();

                        RenderFilter(false);
                    },
                    Keys.R
                );

                createContextMenu.AddItem(
                    LanguageUtils.Translate("編集", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon),
                    (_, _) =>
                    {
                        var prePath = item.ItemPath;

                        AddItemForm addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, true, item, null);
                        addItem.ShowDialog();

                        // 対応アバターのパスを変えてあげる
                        DatabaseUtils.ChangeAllItemPaths(Items, prePath, item.ItemPath);

                        DatabaseUtils.SaveItemsData(Items);

                        if (CurrentPath.CurrentSelectedAvatarPath == prePath)
                        {
                            CurrentPath.CurrentSelectedAvatar = item.Title;
                            CurrentPath.CurrentSelectedAvatarPath = item.ItemPath;
                        }

                        if (!_isSearching) PathTextBox.Text = GeneratePath();
                        RefleshWindow();
                    },
                    Keys.E
                );

                createContextMenu.AddItem(
                    LanguageUtils.Translate("メモの追加", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon),
                    (_, _) =>
                    {
                        if (!AddMemoToItem(item)) return;

                        DatabaseUtils.SaveItemsData(Items);

                        RenderFilter(false);
                        GenerateItems(false);
                    },
                    Keys.M
                );

                createContextMenu.AddItem(
                    LanguageUtils.Translate("アイテムフォルダの追加", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon),
                    (_, _) => AddFolderToItem(item),
                    Keys.A
                );

                var implementedMenu = createContextMenu.AddItem(LanguageUtils.Translate("実装/未実装", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
                foreach (var avatar in Items.Where(i => i.Type == ItemType.Avatar))
                {
                    ToolStripMenuItem avatarToolStripMenuItem = new()
                    {
                        Text = DatabaseUtils.GetAvatarNameFromPaths(Items, avatar.ItemPath),
                        Tag = avatar.ItemPath,
                        Checked = item.ImplementedAvatars.Contains(avatar.ItemPath)
                    };

                    CreateContextMenu.AddDropDownItem(
                        implementedMenu,
                        avatarToolStripMenuItem,
                        (sender, _) =>
                        {
                            if (sender is not ToolStripMenuItem toolStripMenuItem) return;
                            if (toolStripMenuItem.Tag == null) return;

                            if (toolStripMenuItem.Checked)
                            {
                                item.ImplementedAvatars.RemoveAll(avatarPath => avatarPath == (string)toolStripMenuItem.Tag);
                                toolStripMenuItem.Checked = false;
                            }
                            else
                            {
                                item.ImplementedAvatars.Add((string)toolStripMenuItem.Tag);
                                toolStripMenuItem.Checked = true;
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
                            FormUtils.ShowParentToolStrip(toolStripMenuItem, null!);
                        }
                    );
                }

                createContextMenu.AddItem(
                    LanguageUtils.Translate("削除", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon),
                    (_, _) =>
                    {
                        var result = FormUtils.ShowConfirmDialog(
                            LanguageUtils.Translate("本当に削除しますか？", CurrentLanguage) + "\n\n" + item.Title,
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

                        // アバターのときは対応アバター削除、共通素体グループから削除用の処理を実行する
                        if (item.Type == ItemType.Avatar) DeleteAvatarFromSupported(item);

                        Items.RemoveAll(i => i.ItemPath == item.ItemPath);

                        DatabaseUtils.SaveItemsData(Items);

                        FormUtils.ShowMessageBox(
                            LanguageUtils.Translate("削除が完了しました。", CurrentLanguage),
                            LanguageUtils.Translate("完了", CurrentLanguage)
                        );

                        if (undo)
                        {
                            SearchBox.Text = string.Empty;
                            SearchResultLabel.Text = string.Empty;
                            _isSearching = false;

                            RenderFilter(false);
                            ResetAvatarExplorer(true);
                        }
                        else
                        {
                            RefleshWindow();
                        }
                    },
                    Keys.D
                );

                button.ContextMenuStrip = createContextMenu.ContextMenuStrip;
                ExplorerList.Controls.Add(button);
                index++;
            }
            catch (Exception ex)
            {
                FormUtils.ShowMessageBox("Error Occured while rendering item button\n\nError: " + ex, "Button Error", true);
            }
        }

        TabPageUtils.AddNavigationButtons(
            ExplorerList,
            DarkMode,
            ((ButtonSize + 6) * index) + 2,
            _currentPage, _itemsPerPage, totalCount, false,
            CurrentLanguage,
            (_, _) => _currentPage--,
            (_, _) => _currentPage++,
            (_, _) => _currentPage = 0,
            (_, _) => _currentPage = TabPageUtils.GetTotalPages(totalCount, _itemsPerPage) - 1,
            (sender, _) =>
            {
                if (sender is int pageCount) _currentPage = pageCount;
                GenerateItems(false);
            }
        );

        ExplorerList.ResumeLayout();
        ExplorerList.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(ExplorerList);
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

        ExplorerList.SuspendLayout();
        ExplorerList.AutoScroll = false;

        var index = 0;
        foreach (var itemType in types)
        {
            try
            {
                var itemCount = itemFolderInfo.GetItemCount(itemType);
                if (itemCount == 0) continue;

                Button button = AEUtils.CreateButton(DarkMode, ButtonSize, _previewScale, null, LanguageUtils.Translate(itemType, CurrentLanguage), itemCount + LanguageUtils.Translate("個の項目", CurrentLanguage), false, string.Empty, GetExplorerListWidth);
                button.Location = new Point(0, ((ButtonSize + 6) * index) + 2);
                button.MouseClick += OnMouseClick;

                void ButtonClick(object? sender, EventArgs? e)
                {
                    CurrentPath.CurrentSelectedItemCategory = itemType;
                    GenerateItemFiles();
                    PathTextBox.Text = GeneratePath();
                }

                button.Click += ButtonClick;
                button.Disposed += (_, _) =>
                {
                    button.Click -= ButtonClick;
                    button.ContextMenuStrip?.Dispose();
                };

                ExplorerList.Controls.Add(button);
                index++;
            }
            catch (Exception ex)
            {
                FormUtils.ShowMessageBox("Error Occured while rendering item button\n\nError: " + ex, "Button Error", true);
            }
        }

        ExplorerList.ResumeLayout();
        ExplorerList.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(ExplorerList);
    }

    /// <summary>
    /// メイン画面右のアイテム内のファイル欄を作成します。
    /// </summary>
    /// <param name="pageReset"></param>
    private void GenerateItemFiles(bool pageReset = true)
    {
        if (pageReset) _currentPage = 0;
        _openingWindow = Window.ItemFolderItemsList;
        ResetAvatarExplorer();

        var files = CurrentPath.CurrentSelectedItemFolderInfo.GetItems(CurrentPath.CurrentSelectedItemCategory);
        if (!files.Any()) return;

        int totalCount = files.Count();
        _currentPage = Math.Clamp(_currentPage, 0, TabPageUtils.GetTotalPages(totalCount, _itemsPerPage) - 1);

        ExplorerList.SuspendLayout();
        ExplorerList.AutoScroll = false;

        var index = 0;
        foreach (var file in files.OrderBy(file => file.FileName).Skip(_currentPage * _itemsPerPage).Take(_itemsPerPage))
        {
            try
            {
                var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : string.Empty;
                Button button = AEUtils.CreateButton(DarkMode, ButtonSize, _previewScale, imagePath, file.FileName, file.FileExtension.Replace(".", string.Empty) + LanguageUtils.Translate("ファイル", CurrentLanguage), false, LanguageUtils.Translate("開くファイルのパス: ", CurrentLanguage) + file.FilePath, GetExplorerListWidth);
                button.Location = new Point(0, ((ButtonSize + 6) * index) + 2);
                button.MouseClick += OnMouseClick;

                void ButtonClick(object? sender, EventArgs? e)
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
                }

                button.Click += ButtonClick;
                button.Disposed += (_, _) =>
                {
                    button.Click -= ButtonClick;
                    button.ContextMenuStrip?.Dispose();
                };

                var createContextMenu = new CreateContextMenu();

                createContextMenu.AddItem(
                    LanguageUtils.Translate("開く", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon),
                    (_, _) => FileSystemUtils.OpenItemFile(file, true, CurrentLanguage),
                    Keys.O
                );

                createContextMenu.AddItem(
                    LanguageUtils.Translate("ファイルのパスを開く", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon),
                    (_, _) => FileSystemUtils.OpenItemFile(file, false, CurrentLanguage),
                    Keys.P
                );

                button.ContextMenuStrip = createContextMenu.ContextMenuStrip;
                ExplorerList.Controls.Add(button);
                index++;
            }
            catch (Exception ex)
            {
                FormUtils.ShowMessageBox("Error Occured while rendering item button\n\nError: " + ex, "Button Error", true);
            }
        }

        TabPageUtils.AddNavigationButtons(
            ExplorerList,
            DarkMode,
            ((ButtonSize + 6) * index) + 2,
            _currentPage, _itemsPerPage, totalCount, false,
            CurrentLanguage,
            (_, _) => _currentPage--,
            (_, _) => _currentPage++,
            (_, _) => _currentPage = 0,
            (_, _) => _currentPage = TabPageUtils.GetTotalPages(totalCount, _itemsPerPage) - 1,
            (sender, _) =>
            {
                if (sender is int pageCount) _currentPage = pageCount;
                GenerateItemFiles(false);
            }
        );

        ExplorerList.ResumeLayout();
        ExplorerList.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(ExplorerList);
    }
    #endregion

    #region ボタンの処理
    private void ChangeFilterButton_Click(object sender, EventArgs e)
    {
        ChangeFilter();
        RedrawFilterName();
    }

    private bool ChangeThumbnail(Item item)
    {
        var previousPath = item.ImagePath;
        OpenFileDialog ofd = new()
        {
            Filter = LanguageUtils.Translate("画像ファイル|*.png;*.jpg", CurrentLanguage),
            Title = LanguageUtils.Translate("サムネイル変更", CurrentLanguage),
            Multiselect = false
        };
        if (ofd.ShowDialog() != DialogResult.OK) return false;

        item.ImagePath = ofd.FileName;

        FormUtils.ShowMessageBox(
            LanguageUtils.Translate("サムネイルを変更しました！", CurrentLanguage) + "\n\n" +
            LanguageUtils.Translate("変更前: ", CurrentLanguage) + "\n" + previousPath + "\n\n" +
            LanguageUtils.Translate("変更後: ", CurrentLanguage) + "\n" + ofd.FileName,
            LanguageUtils.Translate("完了", CurrentLanguage)
        );

        return true;
    }
    private bool ChangeThumbnail(Author author)
    {
        var previousPath = author.AuthorImagePath;
        OpenFileDialog ofd = new()
        {
            Filter = LanguageUtils.Translate("画像ファイル|*.png;*.jpg", CurrentLanguage),
            Title = LanguageUtils.Translate("サムネイル変更", CurrentLanguage),
            Multiselect = false
        };
        if (ofd.ShowDialog() != DialogResult.OK) return false;

        foreach (var item in Items.Where(item => item.AuthorImageFilePath == previousPath))
        {
            item.AuthorImageFilePath = ofd.FileName;
        }

        FormUtils.ShowMessageBox(
            LanguageUtils.Translate("サムネイルを変更しました！", CurrentLanguage) + "\n\n" +
            LanguageUtils.Translate("変更前: ", CurrentLanguage) + "\n" + previousPath + "\n\n" +
            LanguageUtils.Translate("変更後: ", CurrentLanguage) + "\n" + ofd.FileName,
            "完了"
        );

        return true;
    }

    private async void AddFolderToItem(Item item)
    {
        var fbd = new FolderBrowserDialog
        {
            Description = LanguageUtils.Translate("アイテムフォルダを選択してください", CurrentLanguage),
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            Multiselect = true
        };

        if (fbd.ShowDialog() != DialogResult.OK) return;
        var itemFolderArray = fbd.SelectedPaths;

        var result = FormUtils.ShowConfirmDialog(LanguageUtils.Translate("アイテム: {0}\n\n追加予定のフォルダ一覧:\n{1}\n\n選択したフォルダをアイテムに追加してもよろしいですか？", CurrentLanguage, item.Title, string.Join("\n", itemFolderArray.Select(log => $"・{Path.GetFileName(log)}"))), LanguageUtils.Translate("アイテムフォルダの追加", CurrentLanguage));
        if (!result) return;

        var parentFolder = item.ItemPath;

        for (var i = 0; i < itemFolderArray.Length; i++)
        {
            var folderName = Path.GetFileName(itemFolderArray[i]);
            var newPath = Path.Combine(parentFolder, "Others", folderName);

            await FileSystemUtils.CopyDirectoryWithProgress(Path.GetFullPath(itemFolderArray[i]), newPath);
        }

        FormUtils.ShowMessageBox(LanguageUtils.Translate("フォルダの追加が完了しました。", CurrentLanguage), LanguageUtils.Translate("完了", CurrentLanguage));
    }

    private async Task<bool> ReacquisitionThumbnailImage(Item item)
    {
        if (item.BoothId != -1)
        {
            var thumbnailFolderPath = Path.Combine("Datas", "Thumbnail");
            if (!Directory.Exists(thumbnailFolderPath))
            {
                Directory.CreateDirectory(thumbnailFolderPath);
            }

            var currentTime = DateTime.Now;
            if (_lastGetTime.AddSeconds(5) > currentTime)
            {
                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("情報取得の間隔が短すぎます。前回の取得から5秒以上空けてください", CurrentLanguage),
                    LanguageUtils.Translate("エラー", CurrentLanguage),
                    true
                );
                return false;
            }
            _lastGetTime = currentTime;

            string newThumbnailUrl = await BoothUtils.GetThumbnailURL(item.BoothId);

            var thumbnailPath = Path.Combine(thumbnailFolderPath, $"{item.BoothId}.png");
            if (!string.IsNullOrEmpty(newThumbnailUrl))
            {
                try
                {
                    var thumbnailData = await BoothUtils.GetImageBytes(newThumbnailUrl);
                    await File.WriteAllBytesAsync(thumbnailPath, thumbnailData);

                    item.ThumbnailUrl = newThumbnailUrl;
                    item.UpdatedDate = DateUtils.GetUnixTime();

                    FormUtils.ShowMessageBox(
                        LanguageUtils.Translate("サムネイル画像の更新に成功しました。", CurrentLanguage),
                        LanguageUtils.Translate("完了", CurrentLanguage)
                    );
                    return true;
                }
                catch (Exception ex)
                {
                    FormUtils.ShowMessageBox(
                        LanguageUtils.Translate("サムネイルのダウンロードに失敗しました。詳細はErrorLog.txtをご覧ください。", CurrentLanguage),
                        LanguageUtils.Translate("エラー", CurrentLanguage),
                        true
                    );
                    LogUtils.ErrorLogger("サムネイルのダウンロードに失敗しました。", ex);
                    return false;
                }
            }
            else
            {
                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("サムネイル画像URLが見つかりませんでした。", CurrentLanguage),
                    LanguageUtils.Translate("エラー", CurrentLanguage),
                    true
                );

                return false;
            }
        }
        else
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("商品URLが見つかりませんでした。", CurrentLanguage),
                LanguageUtils.Translate("エラー", CurrentLanguage),
                true
            );

            return false;
        }
    }

    private bool AddMemoToItem(Item item)
    {
        var previousMemo = item.ItemMemo;
        AddMemoForm addMemo = new(this, item);
        addMemo.ShowDialog();

        var memo = addMemo.Memo;
        if (memo == previousMemo) return false;

        item.ItemMemo = memo;
        item.UpdatedDate = DateUtils.GetUnixTime();

        return true;
    }

    private void SearchByAuthorName(Item item)
    {
        SearchBox.Text = $"Author=\"{item.AuthorName}\"";
        SearchItems();
    }

    private void DeleteAvatarFromSupported(Item item)
    {
        var result = FormUtils.ShowConfirmDialog(
            LanguageUtils.Translate("このアバターを対応アバターとしているアイテムの対応アバターからこのアバターを削除しますか？", CurrentLanguage),
            LanguageUtils.Translate("確認", CurrentLanguage)
        );

        DatabaseUtils.DeleteAvatarFromItems(Items, item.ItemPath, result);

        if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
        {
            var result1 = FormUtils.ShowConfirmDialog(
                LanguageUtils.Translate("このアバターを共通素体グループから削除しますか？", CurrentLanguage),
                LanguageUtils.Translate("確認", CurrentLanguage)
            );

            if (result1)
            {
                DatabaseUtils.DeleteAvatarFromCommonAvatars(CommonAvatars, item.ItemPath);
                DatabaseUtils.SaveCommonAvatarData(CommonAvatars);
            }
        }
    }
    #endregion

    #region 検索関連の処理
    /// <summary>
    /// 検索ボックスに入力された文字列を元にアイテムを検索します。
    /// </summary>
    /// <param name="searchFilter"></param>
    /// <param name="pageReset"></param>
    private void GenerateFilteredItem(SearchFilter searchFilter, bool pageReset = true)
    {
        if (pageReset) _currentPage = 0;
        ResetAvatarExplorer();

        var filteredItems = Items
            .Where(item => DatabaseUtils.GetSearchResult(Items, item, searchFilter, CurrentLanguage))
            .Where(item =>
                searchFilter.SearchWords.All(word =>
                    item.Title.Contains(word, StringComparison.CurrentCultureIgnoreCase) ||
                    item.AuthorName.Contains(word, StringComparison.CurrentCultureIgnoreCase) ||
                    item.SupportedAvatar.Any(avatar =>
                    {
                        var supportedAvatarName = DatabaseUtils.GetAvatarNameFromPaths(Items, avatar);
                        if (supportedAvatarName == string.Empty) return false;
                        return supportedAvatarName.Contains(word, StringComparison.CurrentCultureIgnoreCase);
                    }) ||
                    item.BoothId.ToString().Contains(word, StringComparison.CurrentCultureIgnoreCase) ||
                    item.ItemMemo.Contains(word, StringComparison.CurrentCultureIgnoreCase)
                )
            )
            .OrderByDescending(item =>
            {
                var matchCount = 0;
                var fieldsToSearch = new List<string>
                {
                    item.Title,
                    item.AuthorName,
                    item.ItemMemo,
                    item.BoothId.ToString()
                };

                fieldsToSearch.AddRange(
                    item.SupportedAvatar
                        .Select(avatar => DatabaseUtils.GetAvatarNameFromPaths(Items, avatar))
                        .Where(name => !string.IsNullOrEmpty(name))
                );

                foreach (var word in searchFilter.SearchWords)
                {
                    matchCount += fieldsToSearch.Count(field =>
                        field.Contains(word, StringComparison.CurrentCultureIgnoreCase));
                }

                return matchCount;
            })
            .AsEnumerable();

        SearchResultLabel.Text = LanguageUtils.Translate("検索結果: {0}件 (全{1}件)", CurrentLanguage, filteredItems.Count().ToString(), Items.Count.ToString());

        if (!filteredItems.Any()) return;

        int totalCount = filteredItems.Count();
        _currentPage = Math.Clamp(_currentPage, 0, TabPageUtils.GetTotalPages(totalCount, _itemsPerPage) - 1);

        ExplorerList.SuspendLayout();
        ExplorerList.AutoScroll = false;

        var index = 0;
        foreach (Item item in filteredItems.Skip(_currentPage * _itemsPerPage).Take(_itemsPerPage))
        {
            try
            {
                var description = ItemUtils.GetItemDescription(item, CurrentLanguage);

                Button button = AEUtils.CreateButton(DarkMode, ButtonSize, _previewScale, item.ImagePath, item.GetTitle(_removeBrackets), LanguageUtils.Translate("作者: ", CurrentLanguage) + item.AuthorName, false, description, GetExplorerListWidth);
                button.Location = new Point(0, ((ButtonSize + 6) * index) + 2);
                button.MouseClick += OnMouseClick;

                void ButtonClick(object? sender, EventArgs? e)
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
                        DatabaseUtils.ChangeAllItemPaths(Items, prePath, item.ItemPath);

                        DatabaseUtils.SaveItemsData(Items);

                        GenerateFilteredItem(searchFilter, false);
                        RenderFilter(false);
                    }

                    _leftWindow = LeftWindow.Default;

                    GeneratePathFromItem(item);

                    SearchBox.Text = string.Empty;
                    SearchResultLabel.Text = string.Empty;
                    _isSearching = false;

                    GenerateItemCategoryList();
                    PathTextBox.Text = GeneratePath();
                }

                button.Click += ButtonClick;
                button.Disposed += (_, _) =>
                {
                    button.Click -= ButtonClick;
                    button.ContextMenuStrip?.Dispose();
                };

                var createContextMenu = new CreateContextMenu();

                if (Directory.Exists(item.ItemPath))
                {
                    createContextMenu.AddItem(
                        LanguageUtils.Translate("フォルダを開く", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.OpenIcon),
                        (_, _) => FileSystemUtils.OpenItemFolder(item, CurrentLanguage),
                        Keys.O
                    );
                }

                if (item.BoothId != -1)
                {
                    createContextMenu.AddItem(
                        LanguageUtils.Translate("Boothリンクのコピー", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.CopyIcon),
                        (_, _) => BoothUtils.CopyItemBoothLink(item, CurrentLanguage),
                        Keys.C
                    );

                    createContextMenu.AddItem(
                        LanguageUtils.Translate("Boothリンクを開く", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.CopyIcon),
                        (_, _) => BoothUtils.OpenItenBoothLink(item, CurrentLanguage),
                        Keys.B
                    );
                }

                createContextMenu.AddItem(
                    LanguageUtils.Translate("この作者の他のアイテムを表示", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon),
                    (_, _) => SearchByAuthorName(item),
                    Keys.A
                );

                createContextMenu.AddItem(
                    LanguageUtils.Translate("サムネイル変更", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon),
                    (_, _) =>
                    {
                        if (!ChangeThumbnail(item)) return;

                        DatabaseUtils.SaveItemsData(Items);

                        GenerateFilteredItem(searchFilter, false);
                        RenderFilter(false);
                    },
                    Keys.T
                );

                createContextMenu.AddItem(
                    LanguageUtils.Translate("サムネイル再取得", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon),
                    async (_, _) =>
                    {
                        bool result = await ReacquisitionThumbnailImage(item);
                        if (!result) return;

                        DatabaseUtils.SaveItemsData(Items);

                        // もしアバターの欄を右で開いていたら、そのサムネイルも更新しないといけないため。
                        if (_openingWindow == Window.ItemList && !_isSearching) GenerateItems();

                        // 検索中だと、検索画面を再読込してあげる
                        if (_isSearching) SearchItems();

                        RenderFilter(false);
                    },
                    Keys.R
                );

                createContextMenu.AddItem(
                    LanguageUtils.Translate("編集", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon),
                    (_, _) =>
                    {
                        var prePath = item.ItemPath;

                        AddItemForm addItem = new(this, item.Type, item.CustomCategory, true, item, null);
                        addItem.ShowDialog();

                        // 対応アバターのパスを変えてあげる
                        DatabaseUtils.ChangeAllItemPaths(Items, prePath, item.ItemPath);

                        DatabaseUtils.SaveItemsData(Items);

                        if (CurrentPath.CurrentSelectedAvatarPath == prePath)
                        {
                            CurrentPath.CurrentSelectedAvatar = item.Title;
                            CurrentPath.CurrentSelectedAvatarPath = item.ItemPath;
                        }

                        GenerateFilteredItem(searchFilter, false);
                        RenderFilter(false);
                    },
                    Keys.E
                );

                createContextMenu.AddItem(
                    LanguageUtils.Translate("メモの追加", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon),
                    (_, _) =>
                    {
                        if (!AddMemoToItem(item)) return;

                        DatabaseUtils.SaveItemsData(Items);

                        GenerateFilteredItem(searchFilter, false);
                        RenderFilter(false);
                    },
                    Keys.M
                );

                createContextMenu.AddItem(
                    LanguageUtils.Translate("アイテムフォルダの追加", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon),
                    (_, _) => AddFolderToItem(item),
                    Keys.A
                );

                var implementedMenu = createContextMenu.AddItem(LanguageUtils.Translate("実装/未実装", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
                foreach (var avatar in Items.Where(i => i.Type == ItemType.Avatar))
                {
                    ToolStripMenuItem avatarToolStripMenuItem = new()
                    {
                        Text = DatabaseUtils.GetAvatarNameFromPaths(Items, avatar.ItemPath),
                        Tag = avatar.ItemPath,
                        Checked = item.ImplementedAvatars.Contains(avatar.ItemPath)
                    };

                    CreateContextMenu.AddDropDownItem(
                        implementedMenu,
                        avatarToolStripMenuItem,
                        (sender, _) =>
                        {
                            if (sender is not ToolStripMenuItem toolStripMenuItem) return;
                            if (toolStripMenuItem.Tag == null) return;

                            if (toolStripMenuItem.Checked)
                            {
                                item.ImplementedAvatars.RemoveAll(avatarPath => avatarPath == (string)toolStripMenuItem.Tag);
                                toolStripMenuItem.Checked = false;
                            }
                            else
                            {
                                item.ImplementedAvatars.Add((string)toolStripMenuItem.Tag);
                                toolStripMenuItem.Checked = true;
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
                            FormUtils.ShowParentToolStrip(toolStripMenuItem, null!);
                        }
                    );
                }

                createContextMenu.AddItem(
                    LanguageUtils.Translate("削除", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon),
                    (_, _) =>
                    {
                        bool result = FormUtils.ShowConfirmDialog(
                            LanguageUtils.Translate("本当に削除しますか？", CurrentLanguage) + "\n\n" + item.Title,
                            LanguageUtils.Translate("確認", CurrentLanguage)
                        );
                        if (!result) return;

                        // アバターのときは対応アバター削除、共通素体グループから削除用の処理を実行する
                        if (item.Type == ItemType.Avatar) DeleteAvatarFromSupported(item);

                        Items.RemoveAll(i => i.ItemPath == item.ItemPath);

                        DatabaseUtils.SaveItemsData(Items);

                        FormUtils.ShowMessageBox(
                            LanguageUtils.Translate("削除が完了しました。", CurrentLanguage),
                            LanguageUtils.Translate("完了", CurrentLanguage)
                        );

                        GenerateFilteredItem(searchFilter, false);
                        RenderFilter(false);
                    },
                    Keys.D
                );

                button.ContextMenuStrip = createContextMenu.ContextMenuStrip;
                ExplorerList.Controls.Add(button);
                index++;
            }
            catch (Exception ex)
            {
                FormUtils.ShowMessageBox("Error Occured while rendering item button\n\nError: " + ex, "Button Error", true);
            }
        }

        TabPageUtils.AddNavigationButtons(
            ExplorerList,
            DarkMode,
            ((ButtonSize + 6) * index) + 2,
            _currentPage, _itemsPerPage, totalCount, false,
            CurrentLanguage,
            (_, _) => _currentPage--,
            (_, _) => _currentPage++,
            (_, _) => _currentPage = 0,
            (_, _) => _currentPage = TabPageUtils.GetTotalPages(totalCount, _itemsPerPage) - 1,
            (sender, _) =>
            {
                if (sender is int pageCount) _currentPage = pageCount;
                GenerateFilteredItem(searchFilter, false);
            }
        );

        ExplorerList.ResumeLayout();
        ExplorerList.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(ExplorerList);
    }

    /// <summary>
    /// 検索ボックスに入力された文字列を元にアイテムフォルダー内を検索します。
    /// </summary>
    /// <param name="searchWords"></param>
    /// <param name="pageReset"></param>
    private void GenerateFilteredFolderItems(SearchFilter searchWords, bool pageReset = true)
    {
        if (pageReset) _currentPage = 0;
        ResetAvatarExplorer();

        var fileDatas = _openingWindow switch
        {
            Window.ItemFolderItemsList => CurrentPath.CurrentSelectedItemFolderInfo.GetItems(CurrentPath.CurrentSelectedItemCategory),
            Window.ItemFolderCategoryList => CurrentPath.CurrentSelectedItemFolderInfo.GetAllItem(),
            _ => new List<FileData>()
        };

        var filteredFileData = fileDatas
            .Where(file => searchWords.SearchWords.All(word => file.FileName.Contains(word, StringComparison.CurrentCultureIgnoreCase)))
            .OrderByDescending(file => searchWords.SearchWords.Count(word => file.FileName.Contains(word, StringComparison.CurrentCultureIgnoreCase)))
            .AsEnumerable();

        SearchResultLabel.Text = LanguageUtils.Translate("フォルダー内検索結果: {0}件 (全{1}件)", CurrentLanguage, filteredFileData.Count().ToString(), fileDatas.Count().ToString());

        if (!filteredFileData.Any()) return;

        int totalCount = filteredFileData.Count();
        _currentPage = Math.Clamp(_currentPage, 0, TabPageUtils.GetTotalPages(totalCount, _itemsPerPage) - 1);

        ExplorerList.SuspendLayout();
        ExplorerList.AutoScroll = false;

        var index = 0;
        foreach (var file in filteredFileData.Skip(_currentPage * _itemsPerPage).Take(_itemsPerPage))
        {
            try
            {
                var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : string.Empty;
                Button button = AEUtils.CreateButton(DarkMode, ButtonSize, _previewScale, imagePath, file.FileName, file.FileExtension.Replace(".", string.Empty) + LanguageUtils.Translate("ファイル", CurrentLanguage), false, LanguageUtils.Translate("開くファイルのパス: ", CurrentLanguage) + file.FilePath, GetExplorerListWidth);
                button.Location = new Point(0, ((ButtonSize + 6) * index) + 2);
                button.MouseClick += OnMouseClick;

                void ButtonClick(object? sender, EventArgs? e)
                {
                    FileSystemUtils.OpenItemFile(file, true, CurrentLanguage);
                }

                button.Click += ButtonClick;
                button.Disposed += (_, _) =>
                {
                    button.Click -= ButtonClick;
                    button.ContextMenuStrip?.Dispose();
                };

                var createContextMenu = new CreateContextMenu();

                createContextMenu.AddItem(
                    LanguageUtils.Translate("開く", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon),
                    (_, _) => FileSystemUtils.OpenItemFile(file, true, CurrentLanguage),
                    Keys.O
                );

                createContextMenu.AddItem(
                    LanguageUtils.Translate("ファイルのパスを開く", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon),
                    (_, _) => FileSystemUtils.OpenItemFile(file, false, CurrentLanguage),
                    Keys.P
                );

                button.ContextMenuStrip = createContextMenu.ContextMenuStrip;
                ExplorerList.Controls.Add(button);
                index++;
            }
            catch (Exception ex)
            {
                FormUtils.ShowMessageBox("Error Occured while rendering item button\n\nError: " + ex, "Button Error", true);
            }
        }

        TabPageUtils.AddNavigationButtons(
            ExplorerList,
            DarkMode,
            ((ButtonSize + 6) * index) + 2,
            _currentPage, _itemsPerPage, totalCount, false,
            CurrentLanguage,
            (_, _) => _currentPage--,
            (_, _) => _currentPage++,
            (_, _) => _currentPage = 0,
            (_, _) => _currentPage = TabPageUtils.GetTotalPages(totalCount, _itemsPerPage) - 1,
            (sender, _) =>
            {
                if (sender is int pageCount) _currentPage = pageCount;
                GenerateFilteredFolderItems(searchWords, false);
            }
        );

        ExplorerList.ResumeLayout();
        ExplorerList.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(ExplorerList);
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

        DatabaseUtils.SaveItemsData(Items);

        RefleshWindow();
    }
    #endregion

    #region パス関連の処理
    /// <summary>
    /// 現在のパスを生成します。
    /// </summary>
    /// <returns></returns>
    private string GeneratePath()
    {
        List<string> pathParts = new();

        switch (_leftWindow)
        {
            case LeftWindow.Author:
                {
                    pathParts.Add(LanguageUtils.Translate("作者", CurrentLanguage));

                    var author = CurrentPath.CurrentSelectedAuthor;
                    if (author == null)
                        return LanguageUtils.Translate("ここには現在のパスが表示されます", CurrentLanguage);

                    pathParts.Add(author.AuthorName);
                    break;
                }
            case LeftWindow.Category:
                {
                    pathParts.Add(LanguageUtils.Translate("カテゴリ別", CurrentLanguage));
                    break;
                }
            case LeftWindow.Default:
                {
                    pathParts.Add(LanguageUtils.Translate("アバター", CurrentLanguage));

                    var avatar = CurrentPath.CurrentSelectedAvatar;
                    if (avatar == null)
                        return LanguageUtils.Translate("ここには現在のパスが表示されます", CurrentLanguage);

                    pathParts.Add(avatar);
                    break;
                }
            default:
                {
                    break;
                }
        }

        if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
            return pathParts.Count > 1 ? AEUtils.GenerateSeparatedPath(pathParts.ToArray()) : LanguageUtils.Translate("ここには現在のパスが表示されます", CurrentLanguage);

        var categoryName = ItemUtils.GetCategoryName(
            CurrentPath.CurrentSelectedCategory,
            CurrentLanguage,
            CurrentPath.CurrentSelectedCustomCategory
        );
        pathParts.Add(categoryName);

        var item = CurrentPath.CurrentSelectedItem;
        if (item != null)
            pathParts.Add(item.Title);

        var itemCategory = CurrentPath.CurrentSelectedItemCategory;
        if (itemCategory != null)
            pathParts.Add(LanguageUtils.Translate(itemCategory, CurrentLanguage));

        return AEUtils.GenerateSeparatedPath(pathParts.ToArray());
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
            SearchBox.Text = string.Empty;
            SearchResultLabel.Text = string.Empty;
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

        SearchBox.Text = string.Empty;
        SearchResultLabel.Text = string.Empty;
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
            SearchResultLabel.Text = string.Empty;
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
                string translatedLabel = LanguageUtils.Translate(label, CurrentLanguage);
                pathTextList.Add($"{translatedLabel}: {string.Join(", ", values)}");
            }
        }

        if (searchFilter.BrokenItems)
        {
            string translatedLabel = LanguageUtils.Translate("パスが壊れているアイテム", CurrentLanguage);
            pathTextList.Add(translatedLabel);
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
            _openingWindow = Window.Nothing;
            CurrentPath = new CurrentPath();
        }

        var controls = ExplorerList.Controls.Cast<Control>().Reverse().ToList();

        ExplorerList.SuspendLayout();
        ExplorerList.AutoScroll = false;

        controls.ForEach(control =>
        {
            if (control is Label label && label.Name == "StartLabel")
            {
                label.Visible = startLabelVisible;
                return;
            }

            control.Visible = false;
            control.Dispose();
        });

        ExplorerList.ResumeLayout();
        ExplorerList.AutoScroll = true;
    }

    /// <summary>
    /// メイン画面左の画面をリセットします。
    /// </summary>
    /// <param name="panel"></param>
    private static void ResetAvatarPage(Panel panel)
    {
        var controls = panel.Controls.Cast<Control>().Reverse().ToList();

        panel.SuspendLayout();
        panel.AutoScroll = false;

        controls.ForEach(control =>
        {
            control.Visible = false;
            control.Dispose();
        });

        panel.ResumeLayout();
        panel.AutoScroll = true;
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
            RenderFilter(false);
            return;
        }

        switch (_openingWindow)
        {
            case Window.ItemList:
                GenerateItems(false);
                break;
            case Window.ItemCategoryList:
                GenerateCategoryList();
                break;
            case Window.ItemFolderCategoryList:
                GenerateItemCategoryList();
                break;
            case Window.ItemFolderItemsList:
                GenerateItemFiles(false);
                break;
            case Window.Nothing:
                break;
            default:
                break;
        }

        if (!reloadLeft) return;
        RenderFilter(false);
    }
    #endregion

    #region ドラッグアンドドロップ関連の処理
    /// <summary>
    /// メイン画面右の欄にドラッグされた際の処理を行います。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ExplorerList_DragDrop(object? sender, DragEventArgs e)
    {
        var files = AEUtils.GetFileDropPaths(e);

        AddItemForm addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, false, null, AEUtils.GetFileDropPaths(e));

        void ItemAdded(object? sender, EventArgs? e)
        {
            DatabaseUtils.SaveItemsData(Items);
            RefleshWindow();
        }

        addItem.ItemAdded += ItemAdded;
        addItem.FormClosed += (_, _) =>
        {
            addItem.ItemAdded -= ItemAdded;
            Enabled = true;
        };

        addItem.Show();
        Enabled = false;
    }

    /// <summary>
    /// メイン画面左の欄にドラッグされた際の処理を行います。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FilterList_DragDrop(object? sender, DragEventArgs e)
    {
        var files = AEUtils.GetFileDropPaths(e);

        AddItemForm addItem = new(this, ItemType.Avatar, null, false, null, files);

        void ItemAdded(object? sender, EventArgs? e)
        {
            DatabaseUtils.SaveItemsData(Items);
            RefleshWindow();
        }

        addItem.ItemAdded += ItemAdded;
        addItem.FormClosed += (_, _) =>
        {
            addItem.ItemAdded -= ItemAdded;
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
                List<string> SupportedAvatarNames = new();
                List<string> SupportedAvatarPaths = new();

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

                List<string> ImplementedAvatarNames = new();
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

        GuiFont = _fontFamilies.TryGetValue(CurrentLanguage, out var family) ? family : new FontFamily("Yu Gothic UI");

        foreach (Control control in Controls)
        {
            if (control.Name == "LanguageBox" || string.IsNullOrEmpty(control.Text)) continue;
            _controlNames.TryAdd(control.Name, control.Text);
            control.Text = LanguageUtils.Translate(_controlNames[control.Name], CurrentLanguage);
            ChangeControlFont(control);
        }

        RedrawFilterName();

        string[] sortingItems = ["タイトル", "作者", "登録日時", "更新日時", "実装済み", "未実装"];
        var selected = SortingBox.SelectedIndex;
        SortingBox.Items.Clear();
        SortingBox.Items.AddRange(sortingItems.Select(item => LanguageUtils.Translate(item, CurrentLanguage)).ToArray());
        SortingBox.SelectedIndex = selected;

        var labelControl = ExplorerList.Controls.OfType<Label>().FirstOrDefault(label => label.Name == "StartLabel");
        if (labelControl != null)
        {
            _controlNames.TryAdd(labelControl.Name, labelControl.Text);
            labelControl.Text = LanguageUtils.Translate(_controlNames[labelControl.Name], CurrentLanguage);
            ChangeControlFont(labelControl);
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
    private async void LoadDataFromFolder()
    {
        // 自動バックアップフォルダから復元するか聞く
        var result = FormUtils.ShowConfirmDialog(
            LanguageUtils.Translate("自動バックアップフォルダから復元しますか？", CurrentLanguage),
            LanguageUtils.Translate("確認", CurrentLanguage)
        );

        if (result)
        {
            var selectBackupForm = new SelectAutoBackupForm(this);
            selectBackupForm.ShowDialog();

            var backupPath = selectBackupForm.SelectedBackupPath;

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

                if (!result2)
                {
                    SearchBox.Text = string.Empty;
                    SearchResultLabel.Text = string.Empty;
                    _isSearching = false;

                    RenderFilter();
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

                Enabled = false;
                Visible = false;
                if (Directory.Exists(thumbnailPath))
                {
                    try
                    {
                        await FileSystemUtils.CopyDirectoryWithProgress(thumbnailPath, "./Datas/Thumbnail", CurrentLanguage, LanguageUtils.Translate("データの移行中", CurrentLanguage) + " (Thumbnail)", true);
                    }
                    catch (Exception ex)
                    {
                        LogUtils.ErrorLogger("サムネイルのコピーに失敗しました。", ex);
                        thumbnailResult = false;
                    }
                }

                if (Directory.Exists(authorImagePath))
                {
                    try
                    {
                        await FileSystemUtils.CopyDirectoryWithProgress(authorImagePath, "./Datas/AuthorImage", CurrentLanguage, LanguageUtils.Translate("データの移行中", CurrentLanguage) + " (Author Image)", true);
                    }
                    catch (Exception ex)
                    {
                        LogUtils.ErrorLogger("作者画像のコピーに失敗しました。", ex);
                        authorImageResult = false;
                    }
                }

                if (Directory.Exists(itemsPath))
                {
                    try
                    {
                        Enabled = false;
                        await FileSystemUtils.CopyDirectoryWithProgress(itemsPath, "./Datas/Items", CurrentLanguage, LanguageUtils.Translate("データの移行中", CurrentLanguage) + " (Items)", true);
                    }
                    catch (Exception ex)
                    {
                        LogUtils.ErrorLogger("Itemsのコピーに失敗しました。", ex);
                        itemsResult = false;
                    }
                }

                var thumbilResultText = thumbnailResult ? string.Empty : "\n" + LanguageUtils.Translate("サムネイルのコピーに一部失敗しています。", CurrentLanguage);
                var authorImageResultText = authorImageResult ? string.Empty : "\n" + LanguageUtils.Translate("作者画像のコピーに一部失敗しています。", CurrentLanguage);
                var itemsResultText = itemsResult ? string.Empty : "\n" + LanguageUtils.Translate("Itemsのコピーに一部失敗しています。", CurrentLanguage);

                var resultMessage = LanguageUtils.Translate("コピーが完了しました。", CurrentLanguage);
                if (!thumbnailResult || !authorImageResult || !itemsResult)
                {
                    resultMessage += "\n\n" + LanguageUtils.Translate("コピー失敗一覧: ", CurrentLanguage) +
                    thumbilResultText + authorImageResultText + itemsResultText;
                }

                FormUtils.ShowMessageBox(
                    resultMessage,
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
            finally
            {
                Enabled = true;
                Visible = true;
            }
        }

        // Add Missing Custom Categories
        var added = DatabaseUtils.CheckMissingCustomCategories(Items, CustomCategories);
        if (added) DatabaseUtils.SaveCustomCategoriesData(CustomCategories);

        // Check Broken Item Paths
        DatabaseUtils.CheckBrokenItemPaths(Items, CurrentLanguage);

        SearchBox.Text = string.Empty;
        SearchResultLabel.Text = string.Empty;
        _isSearching = false;
        RenderFilter();
        ResetAvatarExplorer(true);
        PathTextBox.Text = GeneratePath();

        BringToFront();
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

        var previousSize = previousFont.Size;
        if (previousSize is <= 0 or >= float.MaxValue) return;

        control.Font = new Font(GuiFont, previousSize, previousFont.Style);
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

        SuspendLayout();
        ExplorerList.SuspendLayout();

        var labelControl = ExplorerList.Controls.OfType<Label>().FirstOrDefault(label => label.Name == "StartLabel");
        var allControls = Controls.OfType<Control>().ToList();
        if (labelControl != null) allControls.Add(labelControl);

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
            else if (_leftWindowFilter == LeftWindow.Nothing && control is Panel panel && panel.Name == "ExplorerList")
            {
                var leftWindowPosition = FilterList.Location;
                var mainWindowPosition = ExplorerList.Location;

                var diffWidth = mainWindowPosition.X - leftWindowPosition.X;

                ExplorerList.Location = ExplorerList.Location with { X = mainWindowPosition.X - diffWidth };
                ExplorerList.Size = new Size(ExplorerList.Size.Width + diffWidth, ExplorerList.Size.Height);
            }
        }

        if (labelControl != null)
        {
            labelControl.Location = labelControl.Location with
            {
                X = (ExplorerList.Width - labelControl.Width) / 2,
                Y = (ExplorerList.Height - labelControl.Height) / 2
            };
        }

        AdjustLabelPosition();
        ScaleItemButtons();

        ExplorerList.ResumeLayout();
        ResumeLayout();

        AEUtils.UpdateExplorerThumbnails(ExplorerList);
        AEUtils.UpdateExplorerThumbnails(FilterList);
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
            Y = SearchBox.Location.Y + SearchBox.Height
        };
    }

    /// <summary>
    /// メインフォーム内のボタンサイズを変更します。
    /// </summary>
    private void ScaleItemButtons()
    {
        const int baseItemExplorerWidth = 874;
        const int baseItemListWidth = 303;

        int explorerWidth = baseItemExplorerWidth + GetExplorerListWidth;
        int listWidth = baseItemListWidth + GetFilterListWidth;

        ScaleItemButtonsInContainer(ExplorerList, explorerWidth, ExplorerList.Width);
        ScaleItemButtonsInContainer(FilterList, listWidth, FilterList.Width, true);
    }

    private static void ScaleItemButtonsInContainer(Control container, int buttonWidth, int listWidth = 0, bool small = false)
    {
        Label? pageInfoLabel = container.Controls.OfType<Label>().FirstOrDefault(label => label.Name == "PageInfoLabel");
        Size labelSize = pageInfoLabel != null ? TextRenderer.MeasureText(pageInfoLabel.Text, pageInfoLabel.Font) : Size.Empty;

        foreach (Control control in container.Controls)
        {
            switch (control)
            {
                case CustomItemButton customButton:
                    customButton.Size = customButton.Size with { Width = buttonWidth };
                    break;

                case Button navButton:
                    navButton.Location = navButton.Location with { X = GetUpdatedX(navButton.Name, listWidth, labelSize.Width, small) };
                    break;

                case Label label when label.Name == "PageInfoLabel":
                    label.Location = label.Location with { X = TabPageUtils.GetLabelLocation(listWidth, label.Size).X };
                    break;
            }
        }
    }

    private static int GetUpdatedX(string name, int containerWidth, int labelWidth, bool small = false)
    {
        if (small)
        {
            return name switch
            {
                "BackPageButton" => TabPageUtils.GetFirstButtonLocation(containerWidth, labelWidth, 0, true).X
                                         + TabPageUtils.SmallButtonSpacing
                                         + TabPageUtils.SmallButtonSize.Width,
                "NextPageButton" => TabPageUtils.GetLastButtonLocation(containerWidth, labelWidth, 0, true).X
                                         - TabPageUtils.SmallButtonSpacing
                                         - TabPageUtils.SmallButtonSize.Width,
                "FirstPageButton" => TabPageUtils.GetFirstButtonLocation(containerWidth, labelWidth, 0, true).X,
                "LastPageButton" => TabPageUtils.GetLastButtonLocation(containerWidth, labelWidth, 0, true).X,
                _ => 0,
            };
        }
        else
        {
            return name switch
            {
                "BackPageButton" => TabPageUtils.GetFirstButtonLocation(containerWidth, labelWidth, 0, false).X
                                         + TabPageUtils.ButtonSpacing
                                         + TabPageUtils.ButtonSize.Width,
                "NextPageButton" => TabPageUtils.GetLastButtonLocation(containerWidth, labelWidth, 0, false).X
                                         - TabPageUtils.ButtonSpacing
                                         - TabPageUtils.ButtonSize.Width,
                "FirstPageButton" => TabPageUtils.GetFirstButtonLocation(containerWidth, labelWidth, 0, false).X,
                "LastPageButton" => TabPageUtils.GetLastButtonLocation(containerWidth, labelWidth, 0, false).X,
                _ => 0,
            };
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
            Interval = _backupInterval
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