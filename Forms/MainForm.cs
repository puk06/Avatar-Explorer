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
    #region �t�H�[���̃e�L�X�g�֘A�̕ϐ�

    /// <summary>
    /// �\�t�g�̌��݂̃o�[�W����
    /// </summary>
    private const string CurrentVersion = "v1.1.1";

    /// <summary>
    /// �f�t�H���g�̃t�H�[���e�L�X�g
    /// </summary>
    private const string CurrentVersionFormText = $"VRChat Avatar Explorer {CurrentVersion} by �Ղ����";

    #endregion

    #region �\�t�g�̃f�[�^�x�[�X�֘A�̕ϐ�

    /// <summary>
    /// �A�C�e���f�[�^�x�[�X
    /// </summary>
    internal List<Item> Items;

    /// <summary>
    /// ���ʑf�̃f�[�^�x�[�X
    /// </summary>
    internal List<CommonAvatar> CommonAvatars;

    /// <summary>
    /// �J�X�^���J�e�S���[�f�[�^�x�[�X
    /// </summary>
    internal List<string> CustomCategories;

    #endregion

    #region �t�H���g�֘A�̕ϐ�
    /// <summary>
    /// �t�H���g�R���N�V����
    /// </summary>
    private readonly PrivateFontCollection _fontCollection = new();

    /// <summary>
    /// �t�H���g�t�@�~���[
    /// </summary>
    private readonly Dictionary<string, FontFamily> _fontFamilies = new();

    /// <summary>
    /// �t�H�[����GUI�t�H���g
    /// </summary>
    internal FontFamily? GuiFont;
    #endregion

    #region ���݂̃E�B���h�E�̎�ނɊւ���ϐ�
    /// <summary>
    /// ���݊J����Ă��鍶�E�B���h�E�̃^�C�v���擾�܂��͐ݒ肵�܂��B
    /// </summary>

    private LeftWindow _leftWindow = LeftWindow.Default;

    /// <summary>
    /// ���݊J���Ă��郁�C����ʃE�B���h�E�^�C�v
    /// </summary>
    private Window _openingWindow = Window.Nothing;
    #endregion

    #region �t�H�[�����T�C�Y�֘A�̕ϐ�
    /// <summary>
    /// �t�H�[�����T�C�Y���Ɏg�p�����R���g���[�����̃f�B�N�V���i���[
    /// </summary>
    private readonly Dictionary<string, string> _controlNames = new();

    /// <summary>
    /// �t�H�[�����T�C�Y���Ɏg�p�����R���g���[���̃f�t�H���g�T�C�Y
    /// </summary>
    private readonly Dictionary<string, ControlScale> _defaultControlSize = new();

    /// <summary>
    /// �t�H�[���̏����T�C�Y
    /// </summary>
    private readonly Size _initialFormSize;

    /// <summary>
    /// ���C����ʍ��̃A�o�^�[���̏�����
    /// </summary>
    private readonly int _baseAvatarSearchFilterListWidth;

    /// <summary>
    /// ���C����ʉE�̃A�C�e�����̏�����
    /// </summary>
    private readonly int _baseAvatarItemExplorerListWidth;

    /// <summary>
    /// ���T�C�Y�p�̃^�C�}�[
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

    #region �o�b�N�A�b�v�֘A�̕ϐ�
    /// <summary>
    /// �o�b�N�A�b�v����Ԋu(ms)
    /// </summary>
    private const int BackupInterval = 300000; // 5 Minutes

    /// <summary>
    /// �Ō�̃o�b�N�A�b�v�������擾�܂��͐ݒ肵�܂��B
    /// </summary>
    private DateTime _lastBackupTime;

    /// <summary>
    /// �Ō�̃o�b�N�A�b�v���ɃG���[�������������ǂ������擾�܂��͐ݒ肵�܂��B
    /// </summary>
    private bool _lastBackupError;
    #endregion

    #region �\�t�g�̃X�e�[�^�X�Ɋւ���ϐ�
    /// <summary>
    /// ���݂̃\�t�g�̌���
    /// </summary>
    internal string CurrentLanguage = "ja-JP";

    /// <summary>
    /// ���݂̃p�X
    /// </summary>
    internal CurrentPath CurrentPath = new();

    /// <summary>
    /// ���������ǂ������擾�܂��͐ݒ肵�܂��B
    /// </summary>
    private bool _isSearching;

    /// <summary>
    /// �t�H�[�������������ꂽ���ǂ������擾���܂��B
    /// </summary>
    private readonly bool _initialized;
    #endregion

    #region �t�H�[���̏�����
    /// <summary>
    /// ���C���t�H�[�������������܂��B
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

            Text = $"VRChat Avatar Explorer {CurrentVersion} by �Ղ����";

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
            FormUtils.ShowMessageBox("�\�t�g�̋N�����ɃG���[���������܂����B\n\n" + ex, "�G���[", true);
            Environment.Exit(0);
        }
    }

    /// <summary>
    /// �t�H���g�t�@�C�����\�t�g�ɒǉ����܂��B
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

    #region ���̃��X�g�֘A�̏���
    /// <summary>
    /// ���C����ʍ��̃A�o�^�[�����쐬���܂��B
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

            Button button = AEUtils.CreateButton(item.ImagePath, item.Title, LanguageUtils.Translate("���: ", CurrentLanguage) + item.AuthorName, true, description, GetAvatarListWidth);
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
                ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("Booth�����N�̃R�s�[", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent2 = (_, _) => BoothUtils.CopyItemBoothLink(item, CurrentLanguage);

                toolStripMenuItem.Click += clickEvent2;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent2;

                ToolStripMenuItem toolStripMenuItem1 = new(LanguageUtils.Translate("Booth�����N���J��", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent3 = (_, _) => BoothUtils.OpenItenBoothLink(item, CurrentLanguage);

                toolStripMenuItem1.Click += clickEvent3;
                toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent3;

                contextMenuStrip.Items.Add(toolStripMenuItem);
                contextMenuStrip.Items.Add(toolStripMenuItem1);
            }

            ToolStripMenuItem toolStripMenuItem2 = new(LanguageUtils.Translate("���̍�҂̑��̃A�C�e����\��", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.OpenIcon));
            EventHandler? clickEvent4 = (_, _) =>
            {
                SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                SearchItems();
            };

            toolStripMenuItem2.Click += clickEvent4;
            toolStripMenuItem2.Disposed += (_, _) => toolStripMenuItem2.Click -= clickEvent4;

            ToolStripMenuItem toolStripMenuItem3 = new(LanguageUtils.Translate("�T���l�C���ύX", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent5 = (_, _) =>
            {
                OpenFileDialog ofd = new()
                {
                    Filter = LanguageUtils.Translate("�摜�t�@�C��|*.png;*.jpg", CurrentLanguage),
                    Title = LanguageUtils.Translate("�T���l�C���ύX", CurrentLanguage),
                    Multiselect = false
                };
                if (ofd.ShowDialog() != DialogResult.OK) return;

                item.ImagePath = ofd.FileName;

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("�T���l�C����ύX���܂����I", CurrentLanguage) + "\n\n" +
                    LanguageUtils.Translate("�ύX�O: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                    LanguageUtils.Translate("�ύX��: ", CurrentLanguage) + ofd.FileName,
                    LanguageUtils.Translate("����", CurrentLanguage)
                );

                // �����A�o�^�[�̗����E�ŊJ���Ă�����A���̃T���l�C�����X�V���Ȃ��Ƃ����Ȃ����߁B
                if (_openingWindow == Window.ItemList && !_isSearching) GenerateItems();

                // ���������ƁA������ʂ��ēǍ����Ă�����
                if (_isSearching) SearchItems();

                GenerateAvatarList();
                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem3.Click += clickEvent5;
            toolStripMenuItem3.Disposed += (_, _) => toolStripMenuItem3.Click -= clickEvent5;

            ToolStripMenuItem toolStripMenuItem4 = new(LanguageUtils.Translate("�ҏW", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent6 = (_, _) =>
            {
                var prePath = item.ItemPath;

                AddItemForm addItem = new(this, item.Type, item.CustomCategory, true, item, null);
                addItem.ShowDialog();

                // �Ή��A�o�^�[�̃p�X��ς��Ă�����
                DatabaseUtils.ChangeAllItemPath(ref Items, prePath);

                // �����A�C�e���ŕҏW���ꂽ�A�C�e�����J���Ă�����A�p�X�ȂǂɎg�p����镶������X�V���Ȃ��Ƃ����Ȃ�����
                if (CurrentPath.CurrentSelectedAvatarPath == prePath)
                {
                    CurrentPath.CurrentSelectedAvatar = item.Title;
                    CurrentPath.CurrentSelectedAvatarPath = item.ItemPath;
                }

                // �����A�o�^�[�̗����E�ŊJ���Ă�����A���̃A�C�e���̏����X�V���Ȃ��Ƃ����Ȃ�����
                if (_openingWindow == Window.ItemList && !_isSearching) GenerateItems();

                // ���������ƁA������ʂ��ēǍ����Ă�����
                if (_isSearching) SearchItems();

                // �������̕�����������Ȃ��悤�ɂ��邽�߂�_isSearching�Ń`�F�b�N���Ă���
                if (!_isSearching) PathTextBox.Text = GeneratePath();

                RefleshWindow();
                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem4.Click += clickEvent6;
            toolStripMenuItem4.Disposed += (_, _) => toolStripMenuItem4.Click -= clickEvent6;

            ToolStripMenuItem toolStripMenuItem5 = new(LanguageUtils.Translate("�����̒ǉ�", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
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

            ToolStripMenuItem toolStripMenuItem6 = new(LanguageUtils.Translate("�폜", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.TrashIcon));
            EventHandler clickEvent8 = (_, _) =>
            {
                var result = FormUtils.ShowConfirmDialog(
                    LanguageUtils.Translate("�{���ɍ폜���܂����H", CurrentLanguage),
                    LanguageUtils.Translate("�m�F", CurrentLanguage)
                );
                if (!result) return;

                var undo = false; // �����폜�����A�C�e�����J����Ă�����
                if (CurrentPath.CurrentSelectedItem?.ItemPath == item.ItemPath)
                {
                    CurrentPath.CurrentSelectedItemCategory = null;
                    CurrentPath.CurrentSelectedItem = null;
                    undo = true;
                }

                var undo2 = false; // �A�o�^�[���[�h�ł����폜�����A�o�^�[���獡�܂ł̃A�C�e�����J����Ă�����
                if (CurrentPath.CurrentSelectedAvatarPath == item.ItemPath && _leftWindow == LeftWindow.Default)
                {
                    CurrentPath = new CurrentPath();
                    undo2 = true;
                }

                // �A�o�^�[�̂Ƃ��͑Ή��A�o�^�[�폜�A���ʑf�̃O���[�v����폜�p�̏��������s����
                if (item.Type == ItemType.Avatar)
                {
                    var result2 = FormUtils.ShowConfirmDialog(
                        LanguageUtils.Translate("���̃A�o�^�[��Ή��A�o�^�[�Ƃ��Ă���A�C�e���̑Ή��A�o�^�[���炱�̃A�o�^�[���폜���܂����H", CurrentLanguage),
                        LanguageUtils.Translate("�m�F", CurrentLanguage)
                    );

                    DatabaseUtils.DeleteAvatarFromItem(ref Items, item.ItemPath, result2);

                    if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                    {
                        var result3 = FormUtils.ShowConfirmDialog(
                            LanguageUtils.Translate("���̃A�o�^�[�����ʑf�̃O���[�v����폜���܂����H", CurrentLanguage),
                            LanguageUtils.Translate("�m�F", CurrentLanguage)
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
                    LanguageUtils.Translate("�폜���������܂����B", CurrentLanguage),
                    LanguageUtils.Translate("����", CurrentLanguage)
                );

                if (_isSearching)
                {
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();

                    // �t�H���_�[�������̎�
                    if (_openingWindow is Window.ItemFolderCategoryList or Window.ItemFolderItemsList)
                    {
                        // �I�����ꂽ�A�o�^�[���猻�݂̏��܂ŗ��Ă�ꍇ
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

                        // �A�C�e���Ƃ��đI������Ă���ꍇ
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

                    // �A�o�^�[���I�����ꂽ���(CurrentSelectedAvatarPath�Ƃ��Đݒ肳��Ă��鎞)
                    if (undo2)
                    {
                        ResetAvatarExplorer(true);
                        PathTextBox.Text = GeneratePath();
                        DatabaseUtils.SaveItemsData(Items);
                        return;
                    }

                    // �t�H���_�[���J���Ă����āA�A�C�e�����I�����ꂽ���(CurrentSelectedItem�Ƃ��Đݒ肳��Ă��鎞)
                    if (undo)
                    {
                        GenerateItems();
                        PathTextBox.Text = GeneratePath();
                        DatabaseUtils.SaveItemsData(Items);
                        return;
                    }

                    // �A�C�e����ʂɊ��ɂ���
                    if (_openingWindow == Window.ItemList)
                    {
                        GenerateItems();
                        DatabaseUtils.SaveItemsData(Items);
                        return;
                    }

                    // �A�C�e����ʂ̑O�ɂ���
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
    /// ���C����ʍ��̍�җ����쐬���܂��B
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
            Button button = AEUtils.CreateButton(author.AuthorImagePath, author.AuthorName,Items.Count(item => item.AuthorName == author.AuthorName) + LanguageUtils.Translate("�̍���", CurrentLanguage), true, author.AuthorName, GetAvatarListWidth);
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

            ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("�T���l�C���ύX", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent1 = (_, _) =>
            {
                OpenFileDialog ofd = new()
                {
                    Filter = LanguageUtils.Translate("�摜�t�@�C��|*.png;*.jpg", CurrentLanguage),
                    Title = LanguageUtils.Translate("�T���l�C���ύX", CurrentLanguage),
                    Multiselect = false
                };
                if (ofd.ShowDialog() != DialogResult.OK) return;

                foreach (var item in Items.Where(item => item.AuthorName == author.AuthorName))
                {
                    item.AuthorImageFilePath = ofd.FileName;
                }

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("�T���l�C����ύX���܂����I", CurrentLanguage) + "\n\n" +
                    LanguageUtils.Translate("�ύX�O: ", CurrentLanguage) + author.AuthorImagePath + "\n\n" +
                    LanguageUtils.Translate("�ύX��: ", CurrentLanguage) + ofd.FileName,
                    "����"
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
    /// ���C����ʍ��̃J�e�S���[�����쐬���܂��B
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

            Button button = AEUtils.CreateButton(null, ItemUtils.GetCategoryName(itemType, CurrentLanguage), itemCount + LanguageUtils.Translate("�̍���", CurrentLanguage), true, "", GetAvatarListWidth);
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

                Button button = AEUtils.CreateButton(null, customCategory, itemCount + LanguageUtils.Translate("�̍���", CurrentLanguage), true, "", GetAvatarListWidth);
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

    #region �E�̃��X�g�֘A�̏���
    /// <summary>
    /// ���C����ʉE�̃J�e�S�������쐬���܂��B
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

            Button button = AEUtils.CreateButton(null, ItemUtils.GetCategoryName(itemType, CurrentLanguage), itemCount + LanguageUtils.Translate("�̍���", CurrentLanguage), false, "", GetItemExplorerListWidth);
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

                Button button = AEUtils.CreateButton(null, customCategory, itemCount + LanguageUtils.Translate("�̍���", CurrentLanguage), false, "", GetItemExplorerListWidth);
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
    /// ���C����ʉE�̃A�C�e�������쐬���܂��B
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
            var authorText = LanguageUtils.Translate("���: ", CurrentLanguage) + item.AuthorName;

            var isSupportedOrCommon = ItemUtils.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath);

            if (isSupportedOrCommon.OnlyCommon && item.SupportedAvatar.Count != 0 && CurrentPath.CurrentSelectedAvatarPath != null && !item.SupportedAvatar.Contains(CurrentPath.CurrentSelectedAvatarPath))
            {
                var commonAvatarName = isSupportedOrCommon.CommonAvatarName;
                if (!string.IsNullOrEmpty(commonAvatarName))
                {
                    authorText += "\n" + LanguageUtils.Translate("���ʑf��: ", CurrentLanguage) + commonAvatarName;
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
                        LanguageUtils.Translate("�t�H���_��������܂���ł����B�ҏW���܂����H", CurrentLanguage),
                        LanguageUtils.Translate("�G���[", CurrentLanguage)
                    );
                    if (!result) return;

                    var prePath = item.ItemPath;

                    AddItemForm addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, true, item, null);
                    addItem.ShowDialog();

                    if (!Directory.Exists(item.ItemPath))
                    {
                        FormUtils.ShowMessageBox(
                            LanguageUtils.Translate("�t�H���_��������܂���ł����B", CurrentLanguage),
                            LanguageUtils.Translate("�G���[", CurrentLanguage),
                            true
                        );
                        return;
                    }

                    // �Ή��A�o�^�[�̃p�X��ς��Ă�����
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
                ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("�t�H���_���J��", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.OpenIcon));
                EventHandler clickEvent1 = (_, _) => FileSystemUtils.OpenItemFolder(item, CurrentLanguage);

                toolStripMenuItem.Click += clickEvent1;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent1;

                contextMenuStrip.Items.Add(toolStripMenuItem);
            }

            if (item.BoothId != -1)
            {
                ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("Booth�����N�̃R�s�[", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent2 = (_, _) => BoothUtils.CopyItemBoothLink(item, CurrentLanguage);

                toolStripMenuItem.Click += clickEvent2;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent2;

                ToolStripMenuItem toolStripMenuItem1 = new(LanguageUtils.Translate("Booth�����N���J��", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent3 = (_, _) => BoothUtils.OpenItenBoothLink(item, CurrentLanguage);

                toolStripMenuItem1.Click += clickEvent3;
                toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent3;

                contextMenuStrip.Items.Add(toolStripMenuItem);
                contextMenuStrip.Items.Add(toolStripMenuItem1);
            }

            ToolStripMenuItem toolStripMenuItem2 = new(LanguageUtils.Translate("���̍�҂̑��̃A�C�e����\��", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.OpenIcon));
            EventHandler clickEvent4 = (_, _) =>
            {
                SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                SearchItems();
            };

            toolStripMenuItem2.Click += clickEvent4;
            toolStripMenuItem2.Disposed += (_, _) => toolStripMenuItem2.Click -= clickEvent4;

            ToolStripMenuItem toolStripMenuItem3 = new(LanguageUtils.Translate("�T���l�C���ύX", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent5 = (_, _) =>
            {
                OpenFileDialog ofd = new()
                {
                    Filter = LanguageUtils.Translate("�摜�t�@�C��|*.png;*.jpg", CurrentLanguage),
                    Title = LanguageUtils.Translate("�T���l�C���ύX", CurrentLanguage),
                    Multiselect = false
                };
                if (ofd.ShowDialog() != DialogResult.OK) return;

                item.ImagePath = ofd.FileName;

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("�T���l�C����ύX���܂����I", CurrentLanguage) + "\n\n" +
                    LanguageUtils.Translate("�ύX�O: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                    LanguageUtils.Translate("�ύX��: ", CurrentLanguage) + ofd.FileName,
                    LanguageUtils.Translate("����", CurrentLanguage)
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

            ToolStripMenuItem toolStripMenuItem4 = new(LanguageUtils.Translate("�ҏW", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent6 = (_, _) =>
            {
                var prePath = item.ItemPath;

                AddItemForm addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, true, item, null);
                addItem.ShowDialog();

                // �Ή��A�o�^�[�̃p�X��ς��Ă�����
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

            ToolStripMenuItem toolStripMenuItem5 = new(LanguageUtils.Translate("�����̒ǉ�", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
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

            ToolStripMenuItem toolStripMenuItem6 = new(LanguageUtils.Translate("����/������", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));

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

            ToolStripMenuItem toolStripMenuItem7 = new(LanguageUtils.Translate("�폜", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.TrashIcon));
            EventHandler clickEvent9 = (_, _) =>
            {
                var result = FormUtils.ShowConfirmDialog(
                    LanguageUtils.Translate("�{���ɍ폜���܂����H", CurrentLanguage),
                    LanguageUtils.Translate("�m�F", CurrentLanguage)
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
                        LanguageUtils.Translate("���̃A�o�^�[��Ή��A�o�^�[�Ƃ��Ă���A�C�e���̑Ή��A�o�^�[���炱�̃A�o�^�[���폜���܂����H", CurrentLanguage),
                        LanguageUtils.Translate("�m�F", CurrentLanguage)
                    );

                    DatabaseUtils.DeleteAvatarFromItem(ref Items, item.ItemPath, result2);

                    if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                    {
                        var result3 = FormUtils.ShowConfirmDialog(
                            LanguageUtils.Translate("���̃A�o�^�[�����ʑf�̃O���[�v����폜���܂����H", CurrentLanguage),
                            LanguageUtils.Translate("�m�F", CurrentLanguage)
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
                    LanguageUtils.Translate("�폜���������܂����B", CurrentLanguage),
                    LanguageUtils.Translate("����", CurrentLanguage)
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
    /// ���C����ʉE�̃A�C�e�����̃t�H���_�[�J�e�S�������쐬���܂��B
    /// </summary>
    private void GenerateItemCategoryList()
    {
        _openingWindow = Window.ItemFolderCategoryList;
        var types = new[]
        {
            "���ϗp�f�[�^",
            "�e�N�X�`��",
            "�h�L�������g",
            "Unity�p�b�P�[�W",
            "�}�e���A��",
            "�s��"
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

            Button button = AEUtils.CreateButton(null, LanguageUtils.Translate(itemType, CurrentLanguage), itemCount + LanguageUtils.Translate("�̍���", CurrentLanguage), false, "", GetItemExplorerListWidth);
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
    /// ���C����ʉE�̃A�C�e�����̃t�@�C�������쐬���܂��B
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
            Button button = AEUtils.CreateButton(imagePath, file.FileName, file.FileExtension.Replace(".", "") + LanguageUtils.Translate("�t�@�C��", CurrentLanguage), false, LanguageUtils.Translate("�J���t�@�C���̃p�X: ", CurrentLanguage) + file.FilePath, GetItemExplorerListWidth);
            button.Location = new Point(0, (70 * index) + 2);
            button.MouseClick += OnMouseClick;

            ContextMenuStrip contextMenuStrip = new();

            ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("�J��", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
            EventHandler clickEvent = (_, _) => FileSystemUtils.OpenItemFile(file, true, CurrentLanguage);

            toolStripMenuItem.Click += clickEvent;
            toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent;

            ToolStripMenuItem toolStripMenuItem1 = new(LanguageUtils.Translate("�t�@�C���̃p�X���J��", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
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

    #region �����֘A�̏���
    /// <summary>
    /// �����{�b�N�X�ɓ��͂��ꂽ����������ɃA�C�e�����������܂��B
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
            LanguageUtils.Translate("��������: ", CurrentLanguage) + filteredItems.Count() + LanguageUtils.Translate("��", CurrentLanguage) +
            LanguageUtils.Translate(" (�S", CurrentLanguage) + Items.Count + LanguageUtils.Translate("��)", CurrentLanguage);

        if (!filteredItems.Any()) return;

        AvatarItemExplorer.SuspendLayout();
        AvatarItemExplorer.AutoScroll = false;

        var index = 0;
        foreach (Item item in filteredItems)
        {
            var description = ItemUtils.GetItemDescription(item, CurrentLanguage);

            Button button = AEUtils.CreateButton(item.ImagePath, item.Title, LanguageUtils.Translate("���: ", CurrentLanguage) + item.AuthorName, false, description, GetItemExplorerListWidth);
            button.Location = new Point(0, (70 * index) + 2);
            button.MouseClick += OnMouseClick;

            EventHandler clickEvent = (_, _) =>
            {
                if (!Directory.Exists(item.ItemPath))
                {
                    bool result = FormUtils.ShowConfirmDialog(
                        LanguageUtils.Translate("�t�H���_��������܂���ł����B�ҏW���܂����H", CurrentLanguage),
                        LanguageUtils.Translate("�G���[", CurrentLanguage)
                    );
                    if (!result) return;

                    var prePath = item.ItemPath;

                    AddItemForm addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, true, item, null);
                    addItem.ShowDialog();

                    if (!Directory.Exists(item.ItemPath))
                    {
                        FormUtils.ShowMessageBox(
                            LanguageUtils.Translate("�t�H���_��������܂���ł����B", CurrentLanguage),
                            LanguageUtils.Translate("�G���[", CurrentLanguage),
                            true
                        );
                        return;
                    }

                    // �Ή��A�o�^�[�̃p�X��ς��Ă�����
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
                ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("�t�H���_���J��", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.OpenIcon));
                EventHandler clickEvent1 = (_, _) => FileSystemUtils.OpenItemFolder(item, CurrentLanguage);

                toolStripMenuItem.Click += clickEvent1;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent1;

                contextMenuStrip.Items.Add(toolStripMenuItem);
            }

            if (item.BoothId != -1)
            {
                ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("Booth�����N�̃R�s�[", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent2 = (_, _) => BoothUtils.CopyItemBoothLink(item, CurrentLanguage);

                toolStripMenuItem.Click += clickEvent2;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent2;

                ToolStripMenuItem toolStripMenuItem1 = new(LanguageUtils.Translate("Booth�����N���J��", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent3 = (_, _) => BoothUtils.OpenItenBoothLink(item, CurrentLanguage);

                toolStripMenuItem1.Click += clickEvent3;
                toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent3;

                contextMenuStrip.Items.Add(toolStripMenuItem);
                contextMenuStrip.Items.Add(toolStripMenuItem1);
            }

            ToolStripMenuItem toolStripMenuItem2 = new(LanguageUtils.Translate("���̍�҂̑��̃A�C�e����\��", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.OpenIcon));
            EventHandler clickEvent4 = (_, _) =>
            {
                SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                SearchItems();
            };

            toolStripMenuItem2.Click += clickEvent4;
            toolStripMenuItem2.Disposed += (_, _) => toolStripMenuItem2.Click -= clickEvent4;

            ToolStripMenuItem toolStripMenuItem3 = new(LanguageUtils.Translate("�T���l�C���ύX", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent5 = (_, _) =>
            {
                OpenFileDialog ofd = new()
                {
                    Filter = LanguageUtils.Translate("�摜�t�@�C��|*.png;*.jpg", CurrentLanguage),
                    Title = LanguageUtils.Translate("�T���l�C���ύX", CurrentLanguage),
                    Multiselect = false
                };
                if (ofd.ShowDialog() != DialogResult.OK) return;

                item.ImagePath = ofd.FileName;

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("�T���l�C����ύX���܂����I", CurrentLanguage) + "\n\n" +
                    LanguageUtils.Translate("�ύX�O: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                    LanguageUtils.Translate("�ύX��: ", CurrentLanguage) + ofd.FileName,
                    LanguageUtils.Translate("����", CurrentLanguage)
                );

                GenerateFilteredItem(searchFilter);
                GenerateAvatarList();
                DatabaseUtils.SaveItemsData(Items);
            };

            toolStripMenuItem3.Click += clickEvent5;
            toolStripMenuItem3.Disposed += (_, _) => toolStripMenuItem3.Click -= clickEvent5;

            ToolStripMenuItem toolStripMenuItem4 = new(LanguageUtils.Translate("�ҏW", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
            EventHandler clickEvent6 = (_, _) =>
            {
                var prePath = item.ItemPath;
                AddItemForm addItem = new(this, item.Type, item.CustomCategory, true, item, null);
                addItem.ShowDialog();

                // �Ή��A�o�^�[�̃p�X��ς��Ă�����
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

            ToolStripMenuItem toolStripMenuItem5 = new(LanguageUtils.Translate("�����̒ǉ�", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));
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

            ToolStripMenuItem toolStripMenuItem6 = new(LanguageUtils.Translate("����/������", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.EditIcon));

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

            ToolStripMenuItem toolStripMenuItem7 = new(LanguageUtils.Translate("�폜", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.TrashIcon));
            EventHandler clickEvent9 = (_, _) =>
            {
                bool result = FormUtils.ShowConfirmDialog(
                    LanguageUtils.Translate("�{���ɍ폜���܂����H", CurrentLanguage),
                    LanguageUtils.Translate("�m�F", CurrentLanguage)
                );
                if (!result) return;

                if (item.Type == ItemType.Avatar)
                {
                    var result2 = FormUtils.ShowConfirmDialog(
                        LanguageUtils.Translate("���̃A�o�^�[��Ή��A�o�^�[�Ƃ��Ă���A�C�e���̑Ή��A�o�^�[���炱�̃A�o�^�[���폜���܂����H", CurrentLanguage),
                        LanguageUtils.Translate("�m�F", CurrentLanguage)
                    );

                    DatabaseUtils.DeleteAvatarFromItem(ref Items, item.ItemPath, result2);

                    if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                    {
                        var result3 = FormUtils.ShowConfirmDialog(
                            LanguageUtils.Translate("���̃A�o�^�[�����ʑf�̃O���[�v����폜���܂����H", CurrentLanguage),
                            LanguageUtils.Translate("�m�F", CurrentLanguage)
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
                    LanguageUtils.Translate("�폜���������܂����B", CurrentLanguage),
                    LanguageUtils.Translate("����", CurrentLanguage)
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
    /// �����{�b�N�X�ɓ��͂��ꂽ����������ɃA�C�e���t�H���_�[�����������܂��B
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
            LanguageUtils.Translate("�t�H���_�[����������: ", CurrentLanguage) + filteredFileData.Count() + LanguageUtils.Translate("��", CurrentLanguage) +
            LanguageUtils.Translate(" (�S", CurrentLanguage) + fileDatas.Count() + LanguageUtils.Translate("��)", CurrentLanguage);

        if (!filteredFileData.Any()) return;

        AvatarItemExplorer.SuspendLayout();
        AvatarItemExplorer.AutoScroll = false;

        var index = 0;
        foreach (var file in filteredFileData)
        {
            var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : "";
            Button button = AEUtils.CreateButton(imagePath, file.FileName, file.FileExtension.Replace(".", "") + LanguageUtils.Translate("�t�@�C��", CurrentLanguage), false, LanguageUtils.Translate("�J���t�@�C���̃p�X: ", CurrentLanguage) + file.FilePath, GetItemExplorerListWidth);
            button.Location = new Point(0, (70 * index) + 2);
            button.MouseClick += OnMouseClick;

            ContextMenuStrip contextMenuStrip = new();

            ToolStripMenuItem toolStripMenuItem = new(LanguageUtils.Translate("�J��", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
            EventHandler clickEvent = (_, _) => FileSystemUtils.OpenItemFile(file, true, CurrentLanguage);

            toolStripMenuItem.Click += clickEvent;
            toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent;

            ToolStripMenuItem toolStripMenuItem1 = new(LanguageUtils.Translate("�t�@�C���̃p�X���J��", CurrentLanguage), SharedImages.GetImage(SharedImages.Images.CopyIcon));
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

    #region �A�C�e���ǉ��֘A�̏���
    /// <summary>
    /// �A�C�e���ǉ��{�^���������ꂽ�ۂ̏������s���܂��B
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

    #region �p�X�֘A�̏���
    /// <summary>
    /// ���݂̃p�X�𐶐����܂��B
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
                    prefix = LanguageUtils.Translate("���", CurrentLanguage) + " | ";

                    var author = CurrentPath.CurrentSelectedAuthor;
                    if (author == null)
                        return LanguageUtils.Translate("�����ɂ͌��݂̃p�X���\������܂�", CurrentLanguage);

                    pathParts.Add(AEUtils.RemoveFormat(author.AuthorName));
                    break;
                }
            case LeftWindow.Category:
                {
                    prefix = LanguageUtils.Translate("�J�e�S����", CurrentLanguage) + " | ";
                    break;
                }
            case LeftWindow.Default:
                {
                    prefix = LanguageUtils.Translate("�A�o�^�[", CurrentLanguage) + " | ";

                    var avatar = CurrentPath.CurrentSelectedAvatar;
                    if (avatar == null)
                        return LanguageUtils.Translate("�����ɂ͌��݂̃p�X���\������܂�", CurrentLanguage);

                    pathParts.Add(AEUtils.RemoveFormat(avatar));
                    break;
                }
            default:
                {
                    break;
                }
        }

        if (CurrentPath.CurrentSelectedCategory == ItemType.Unknown)
            return prefix + (pathParts.Count > 0 ? AEUtils.GenerateSeparatedPath(pathParts.ToArray()) : LanguageUtils.Translate("�����ɂ͌��݂̃p�X���\������܂�", CurrentLanguage));

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
    /// �I�����ꂽ�A�C�e������p�X�𐶐����܂��B
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

    #region �߂�{�^���̏���
    /// <summary>
    /// �߂�{�^���������ꂽ�ۂ̏������s���܂��B
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UndoButton_Click(object? sender, EventArgs? e)
    {
        // �������������ꍇ�͑O�̉�ʂ܂łƂ肠�����߂��Ă�����
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
            // �G���[�����Đ�
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
    /// �R���g���[����ŃT�C�h�{�^���������ꂽ�ۂ̏������s���܂��B
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMouseClick(object? sender, MouseEventArgs? e)
    {
        if (e?.Button != MouseButtons.XButton1) return;
        UndoButton_Click(null, null);
    }
    #endregion

    #region �����{�b�N�X�̏���
    /// <summary>
    /// �����{�b�N�X�Ō����ΏۃL�[�������ꂽ�ۂ̏������s���܂��B
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
    /// �����{�b�N�X���̃e�L�X�g�����Ɍ����������s���܂��B
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
            ("���", searchFilter.Author),
            ("�^�C�g��", searchFilter.Title),
            ("BoothID", searchFilter.BoothId),
            ("�A�o�^�[", searchFilter.Avatar),
            ("�J�e�S��", searchFilter.Category),
            ("����", searchFilter.ItemMemo),
            ("�t�H���_��", searchFilter.FolderName),
            ("�t�@�C����", searchFilter.FileName),
            ("�����A�o�^�[", searchFilter.ImplementedAvatars)
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

        PathTextBox.Text = LanguageUtils.Translate("������... - ", CurrentLanguage) + string.Join(" / ", pathTextList);
    }
    #endregion

    #region ���Z�b�g�֘A�̏���
    /// <summary>
    /// ���C����ʉE�̉�ʂ����Z�b�g���܂��B
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
    /// ���C����ʍ��̉�ʂ����Z�b�g���܂��B
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
    /// ���C����ʂ̑S�Ẳ�ʂ�ǂݍ��ݒ����܂��B
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

    #region �h���b�O�A���h�h���b�v�֘A�̏���
    /// <summary>
    /// ���C����ʉE�̗��Ƀh���b�O���ꂽ�ۂ̏������s���܂��B
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
    /// ���C����ʍ��̗��Ƀh���b�O���ꂽ�ۂ̏������s���܂��B
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

    #region ��ʉ����̃{�^���֘A�̏���
    /// <summary>
    /// CSV�G�N�X�|�[�g�{�^���������ꂽ�ۂ̏������s���܂��B
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
                    LanguageUtils.Translate("�G�N�X�|�[�g�Ɏ��s���܂���", CurrentLanguage),
                    LanguageUtils.Translate("�G���[", CurrentLanguage),
                    true
                );
                return;
            }

            var commonAvatarResult = FormUtils.ShowConfirmDialog(
                LanguageUtils.Translate("�Ή��A�o�^�[�̗��ɋ��ʑf�̃O���[�v�̃A�o�^�[���ǉ����܂����H", CurrentLanguage),
                LanguageUtils.Translate("�m�F", CurrentLanguage)
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
                LanguageUtils.Translate("Output�t�H���_�ɃG�N�X�|�[�g���������܂����I\n�t�@�C����: ", CurrentLanguage) + fileName,
                LanguageUtils.Translate("����", CurrentLanguage)
            );
            ExportButton.Enabled = true;
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("�G�N�X�|�[�g�Ɏ��s���܂���", CurrentLanguage),
                LanguageUtils.Translate("�G���[", CurrentLanguage),
                true
            );
            LogUtils.ErrorLogger("�G�N�X�|�[�g�Ɏ��s���܂����B", ex);
            ExportButton.Enabled = true;
        }
    }

    /// <summary>
    /// �o�b�N�A�b�v�{�^���������ꂽ�ۂ̏������s���܂��B
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
                    LanguageUtils.Translate("�o�b�N�A�b�v�Ɏ��s���܂���", CurrentLanguage),
                    LanguageUtils.Translate("�G���[", CurrentLanguage),
                    true
                );
                return;
            }

            ZipFile.CreateFromDirectory("./Datas", "./Backup/" + fileName);

            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("Backup�t�H���_�Ƀo�b�N�A�b�v���������܂����I\n\n�����������ꍇ�́A\"�f�[�^��ǂݍ���\"�{�^���Ō��ݍ쐬���ꂽ�t�@�C����W�J�������̂�I�����Ă��������B\n\n�t�@�C����: ", CurrentLanguage) + fileName,
                LanguageUtils.Translate("����", CurrentLanguage)
            );
            MakeBackupButton.Enabled = true;
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("�o�b�N�A�b�v�Ɏ��s���܂���", CurrentLanguage),
                LanguageUtils.Translate("�G���[", CurrentLanguage),
                true
            );
            LogUtils.ErrorLogger("�o�b�N�A�b�v�Ɏ��s���܂����B", ex);
            MakeBackupButton.Enabled = true;
        }
    }

    /// <summary>
    /// ����ύX�{�b�N�X�̑I���󋵂��X�V���ꂽ�ۂ̏������s���܂��B
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

        string[] sortingItems = ["�^�C�g��", "���", "�o�^����", "�X�V����", "�����ς�", "������"];
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
    /// ���ёւ����̑I���󋵂��X�V���ꂽ�ۂ̏������s���܂��B
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SortingBox_SelectedIndexChanged(object sender, EventArgs e) => RefleshWindow();

    /// <summary>
    /// �f�[�^��ǂݍ��ރ{�^���������ꂽ�ۂ̏������s���܂��B
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LoadData_Click(object sender, EventArgs e) => LoadDataFromFolder();

    /// <summary>
    /// �t�H���_�I���_�C�A���O��\�����A�I�����ꂽ�t�H���_����f�[�^��ǂݍ��݂܂��B
    /// </summary>
    private void LoadDataFromFolder()
    {
        // �����o�b�N�A�b�v�t�H���_���畜�����邩����
        var result = FormUtils.ShowConfirmDialog(
            LanguageUtils.Translate("�����o�b�N�A�b�v�t�H���_���畜�����܂����H", CurrentLanguage),
            LanguageUtils.Translate("�m�F", CurrentLanguage)
        );

        if (result)
        {
            var selectedBackupForm = new SelectAutoBackupForm(this);
            selectedBackupForm.ShowDialog();

            var backupPath = selectedBackupForm.SelectedBackupPath;

            if (string.IsNullOrEmpty(backupPath)) return;

            // �o�b�N�A�b�v�t�H���_�����݂��Ȃ��ꍇ
            if (!Directory.Exists(backupPath))
            {
                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("�o�b�N�A�b�v�t�H���_��������܂���ł����B", CurrentLanguage),
                    LanguageUtils.Translate("�G���[", CurrentLanguage),
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
                        LanguageUtils.Translate("�A�C�e���t�@�C����������܂���ł����B", CurrentLanguage),
                        LanguageUtils.Translate("�G���[", CurrentLanguage),
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
                        LanguageUtils.Translate("���ʑf�̃t�@�C����������܂���ł����B", CurrentLanguage),
                        LanguageUtils.Translate("�G���[", CurrentLanguage),
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
                        LanguageUtils.Translate("�J�X�^���J�e�S���[�t�@�C����������܂���ł����B", CurrentLanguage),
                        LanguageUtils.Translate("�G���[", CurrentLanguage),
                        true
                    );
                }
                else
                {
                    CustomCategories = DatabaseUtils.LoadCustomCategoriesData(customCategoryPath);
                    DatabaseUtils.SaveCustomCategoriesData(CustomCategories);
                }

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("�������������܂����B", CurrentLanguage),
                    LanguageUtils.Translate("����", CurrentLanguage)
                );
            }
            catch (Exception ex)
            {
                LogUtils.ErrorLogger("�f�[�^�̓ǂݍ��݂Ɏ��s���܂����B", ex);
                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("�f�[�^�̓ǂݍ��݂Ɏ��s���܂����B�ڍׂ�ErrorLog.txt���������������B", CurrentLanguage),
                    LanguageUtils.Translate("�G���[", CurrentLanguage),
                    true
                );
            }
        }
        else
        {
            FolderBrowserDialog fbd = new()
            {
                UseDescriptionForTitle = true,
                Description = LanguageUtils.Translate("�ȑO�̃o�[�W������Datas�t�H���_�A�������͓W�J�����o�b�N�A�b�v�t�H���_��I�����Ă�������", CurrentLanguage),
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
                        LanguageUtils.Translate("�A�C�e���t�@�C����������܂���ł����B", CurrentLanguage),
                        LanguageUtils.Translate("�G���[", CurrentLanguage),
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
                        LanguageUtils.Translate("���ʑf�̃t�@�C����������܂���ł����B", CurrentLanguage),
                        LanguageUtils.Translate("�G���[", CurrentLanguage),
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
                        LanguageUtils.Translate("�J�X�^���J�e�S���[�t�@�C����������܂���ł����B", CurrentLanguage),
                        LanguageUtils.Translate("�G���[", CurrentLanguage),
                        true
                    );
                }
                else
                {
                    CustomCategories = DatabaseUtils.LoadCustomCategoriesData(customCategoryPath);
                    DatabaseUtils.SaveCustomCategoriesData(CustomCategories);
                }

                var result2 = FormUtils.ShowConfirmDialog(
                    LanguageUtils.Translate("Thumbnail�t�H���_�AAuthorImage�t�H���_�AItems�t�H���_���R�s�[���܂����H", CurrentLanguage),
                    LanguageUtils.Translate("�m�F", CurrentLanguage)
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
                        LanguageUtils.Translate("�R�s�[���������܂����B", CurrentLanguage),
                        LanguageUtils.Translate("����", CurrentLanguage)
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
                            LogUtils.ErrorLogger("�T���l�C���̃R�s�[�Ɏ��s���܂����B", ex);
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
                            LogUtils.ErrorLogger("��҉摜�̃R�s�[�Ɏ��s���܂����B", ex);
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
                        LogUtils.ErrorLogger("Items�̃R�s�[�Ɏ��s���܂����B", ex);
                        itemsResult = false;
                    }
                }

                var thumbilResultText = thumbnailResult ? "" : "\n" + LanguageUtils.Translate("�T���l�C���̃R�s�[�Ɉꕔ���s���Ă��܂��B", CurrentLanguage);
                var authorImageResultText = authorImageResult ? "" : "\n" + LanguageUtils.Translate("��҉摜�̃R�s�[�Ɉꕔ���s���Ă��܂��B", CurrentLanguage);
                var itemsResultText = itemsResult ? "" : "\n" + LanguageUtils.Translate("Items�̃R�s�[�Ɉꕔ���s���Ă��܂��B", CurrentLanguage);

                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("�R�s�[���������܂����B", CurrentLanguage) + "\n\n" + LanguageUtils.Translate("�R�s�[���s�ꗗ: ", CurrentLanguage) +
                    thumbilResultText + authorImageResultText + itemsResultText,
                    LanguageUtils.Translate("����", CurrentLanguage)
                );
            }
            catch (Exception ex)
            {
                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("�f�[�^�̓ǂݍ��݂Ɏ��s���܂����B�ڍׂ�ErrorLog.txt���������������B", CurrentLanguage),
                    LanguageUtils.Translate("�G���[", CurrentLanguage)
                );
                LogUtils.ErrorLogger("�f�[�^�̓ǂݍ��݂Ɏ��s���܂����B", ex);
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
    /// ���ʑf�̊Ǘ��{�^���������ꂽ�ۂ̏������s���܂��B
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

    #region ���T�C�Y�֘A�̏���
    /// <summary>
    /// �t�H�[���̃R���g���[���̃t�H���g��ύX���܂��B
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
    /// �t�H�[���̃��T�C�Y���ɃR���g���[���̃T�C�Y��ʒu��ύX���܂��B
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Main_Resize(object sender, EventArgs e)
    {
        _resizeTimer.Stop();
        _resizeTimer.Start();
    }

    /// <summary>
    /// �R���g���[���̃T�C�Y��ʒu��ύX���܂��B
    /// </summary>
    private void ResizeControl()
    {
        if (!_initialized) return;

        var labelControl = AvatarItemExplorer.Controls.OfType<Label>().First();
        var allControls = Controls.OfType<Control>().ToList();
        allControls.Add(labelControl);

        foreach (Control control in allControls)
        {
            // �T�C�Y�̃X�P�[�����O
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
    /// ���x���̈ʒu�𒲐����܂��B
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
    /// ���C���t�H�[�����̃{�^���T�C�Y��ύX���܂��B
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

    #region �o�b�N�A�b�v�֘A�̏���
    /// <summary>
    /// �t�@�C���̎����o�b�N�A�b�v���s���܂��B
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
    /// �ŏI�o�b�N�A�b�v�������t�H�[���^�C�g���ɕ\�����܂��B
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
                if (_lastBackupError) Text = CurrentVersionFormText + " - " + LanguageUtils.Translate("�o�b�N�A�b�v�G���[", CurrentLanguage);
                return;
            }

            var timeSpan = DateTime.Now - _lastBackupTime;
            var minutes = timeSpan.Minutes;
            Text = CurrentVersionFormText + $" - {LanguageUtils.Translate("�ŏI�����o�b�N�A�b�v: ", CurrentLanguage) + minutes + LanguageUtils.Translate("���O", CurrentLanguage)}";

            if (_lastBackupError) Text += " - " + LanguageUtils.Translate("�o�b�N�A�b�v�G���[", CurrentLanguage);
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
    /// �t�@�C���̃o�b�N�A�b�v���s���܂��B
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
            LogUtils.ErrorLogger("�����o�b�N�A�b�v�Ɏ��s���܂����B", ex);
        }
    }
    #endregion

    #region �t�H���_�[��������ۂ̏���
    /// <summary>
    /// �t�H�[����������ۂɃf�[�^��ۑ����A�ꎞ�t�H���_���폜���܂��B
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
            LogUtils.ErrorLogger("�ꎞ�t�H���_�̍폜�Ɏ��s���܂����B", ex);
        }
    }
    #endregion
}