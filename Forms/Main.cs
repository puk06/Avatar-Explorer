using Avatar_Explorer.Classes;
using System.Drawing.Text;
using System.IO.Compression;
using System.Media;
using System.Text;
using Timer = System.Windows.Forms.Timer;

namespace Avatar_Explorer.Forms
{
    public sealed partial class Main : Form
    {
        #region �t�H�[���̃e�L�X�g�֘A�̕ϐ�
        /// <summary>
        /// �\�t�g�̌��݂̃o�[�W����
        /// </summary>
        private const string CurrentVersion = "v1.0.10";

        /// <summary>
        /// �f�t�H���g�̃t�H�[���e�L�X�g
        /// </summary>
        private const string CurrentVersionFormText = $"VRChat Avatar Explorer {CurrentVersion} by �Ղ����";
        #endregion

        #region �\�t�g�̃f�[�^�x�[�X�֘A�̕ϐ�
        /// <summary>
        /// �A�C�e���f�[�^�x�[�X
        /// </summary>
        public Item[] Items;

        /// <summary>
        /// ���ʑf�̃f�[�^�x�[�X
        /// </summary>
        public CommonAvatar[] CommonAvatars;

        /// <summary>
        /// �J�X�^���J�e�S���[�f�[�^�x�[�X
        /// </summary>
        public string[] CustomCategories;
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
        public FontFamily? GuiFont;
        #endregion

        #region ���݂̃E�B���h�E�̎�ނɊւ���ϐ�

        /// <summary>
        /// ���݊J����Ă���E�B���h�E����҃��[�h���ǂ������擾�܂��͐ݒ肵�܂��B
        /// </summary>
        private bool _authorMode;

        /// <summary>
        /// ���݊J����Ă���E�B���h�E���J�e�S���[���[�h���ǂ������擾�܂��͐ݒ肵�܂��B
        /// </summary>
        private bool _categoryMode;

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
        ///�@���C����ʍ��̃A�o�^�[���̏�����
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
        private int GetAvatarListWidth() => AvatarSearchFilterList.Width - _baseAvatarSearchFilterListWidth;

        /// <summary>
        /// Get ItemExplorerList Width
        /// </summary>
        /// <returns>ItemExplorerList Width</returns>
        private int GetItemExplorerListWidth() => AvatarItemExplorer.Width - _baseAvatarItemExplorerListWidth;
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
        public string CurrentLanguage = "ja-JP";

        /// <summary>
        /// ���݂̃p�X
        /// </summary>
        public CurrentPath CurrentPath = new();

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
        public Main(LaunchInfo launchInfo)
        {
            try
            {
                Items = Helper.LoadItemsData();
                CommonAvatars = Helper.LoadCommonAvatarData();

                // Fix Supported Avatar Path (Title => Path)
                Helper.FixSupportedAvatarPath(ref Items);

                // Update Empty Dates
                Helper.UpdateEmptyDates(ref Items);

                // Fix Item Dates
                Helper.FixItemDates(ref Items);

                // Fix Relative Path Escape
                Helper.FixRelativePathEscape(ref Items);

                AddFontFile();
                CustomCategories = Helper.LoadCustomCategoriesData();
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
                if (launchInfo.launchedWithUrl)
                {
                    if (launchInfo.assetDirs.Length != 0 && !string.IsNullOrEmpty(launchInfo.assetId))
                    {
                        AddItem addItem = new(this, ItemType.Avatar, null, false, null, launchInfo.assetDirs, launchInfo.assetId);
                        addItem.ShowDialog();

                        RefleshWindow();
                        Helper.SaveItemsData(Items);
                    }
                }

                AdjustLabelPosition();
            }
            catch (Exception ex)
            {
                MessageBox.Show("�\�t�g�̋N�����ɃG���[���������܂����B\n\n" + ex,
                    "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            AvatarPage.SuspendLayout();
            AvatarPage.AutoScroll = false;

            var index = 0;
            foreach (Item item in items)
            {
                var description = Helper.GetItemDescription(item, CurrentLanguage);

                Button button = Helper.CreateButton(item.ImagePath, item.Title,
                    Helper.Translate("���: ", CurrentLanguage) + item.AuthorName, true,
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
                        new(Helper.Translate("Booth�����N�̃R�s�[", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    EventHandler clickEvent2 = (_, _) => Helper.CopyItemBoothLink(item, CurrentLanguage);

                    toolStripMenuItem.Click += clickEvent2;
                    toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent2;

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Booth�����N���J��", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    EventHandler clickEvent3 = (_, _) => Helper.OpenItenBoothLink(item, CurrentLanguage);

                    toolStripMenuItem1.Click += clickEvent3;
                    toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent3;

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("���̍�҂̑��̃A�C�e����\��", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                EventHandler? clickEvent4 = (_, _) =>
                {
                    SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                    SearchItems();
                };

                toolStripMenuItem2.Click += clickEvent4;
                toolStripMenuItem2.Disposed += (_, _) => toolStripMenuItem2.Click -= clickEvent4;

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent5 = (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("�摜�t�@�C��|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("�T���l�C����ύX���܂����I", CurrentLanguage) + "\n\n" +
                        Helper.Translate("�ύX�O: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                        Helper.Translate("�ύX��: ", CurrentLanguage) + ofd.FileName,
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    item.ImagePath = ofd.FileName;

                    // �����A�o�^�[�̗����E�ŊJ���Ă�����A���̃T���l�C�����X�V���Ȃ��Ƃ����Ȃ����߁B
                    if (_openingWindow == Window.ItemList && !_isSearching) GenerateItems();

                    //���������ƁA������ʂ��ēǍ����Ă�����
                    if (_isSearching) SearchItems();

                    GenerateAvatarList();
                    Helper.SaveItemsData(Items);
                };

                toolStripMenuItem3.Click += clickEvent5;
                toolStripMenuItem3.Disposed += (_, _) => toolStripMenuItem3.Click -= clickEvent5;

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("�ҏW", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent6 = (_, _) =>
                {
                    var prePath = item.ItemPath;

                    AddItem addItem = new(this, item.Type, item.CustomCategory, true, item, null);
                    addItem.ShowDialog();

                    //�Ή��A�o�^�[�̃p�X��ς��Ă�����
                    Helper.ChangeAllItemPath(ref Items, prePath);

                    // �����A�C�e���ŕҏW���ꂽ�A�C�e�����J���Ă�����A�p�X�ȂǂɎg�p����镶������X�V���Ȃ��Ƃ����Ȃ�����
                    if (CurrentPath.CurrentSelectedAvatarPath == prePath)
                    {
                        CurrentPath.CurrentSelectedAvatar = item.Title;
                        CurrentPath.CurrentSelectedAvatarPath = item.ItemPath;
                    }

                    // �����A�o�^�[�̗����E�ŊJ���Ă�����A���̃A�C�e���̏����X�V���Ȃ��Ƃ����Ȃ�����
                    if (_openingWindow == Window.ItemList && !_isSearching) GenerateItems();

                    //���������ƁA������ʂ��ēǍ����Ă�����
                    if (_isSearching) SearchItems();

                    // �������̕�����������Ȃ��悤�ɂ��邽�߂�_isSearching�Ń`�F�b�N���Ă���
                    if (!_isSearching) PathTextBox.Text = GeneratePath();

                    RefleshWindow();
                    Helper.SaveItemsData(Items);
                };

                toolStripMenuItem4.Click += clickEvent6;
                toolStripMenuItem4.Disposed += (_, _) => toolStripMenuItem4.Click -= clickEvent6;

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("�����̒ǉ�", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent7 = (_, _) =>
                {
                    var previouseMemo = item.ItemMemo;
                    AddNote addMemo = new(this, item);
                    addMemo.ShowDialog();

                    var memo = addMemo.Memo;
                    if (string.IsNullOrEmpty(memo) || memo == previouseMemo) return;

                    item.ItemMemo = memo;
                    item.UpdatedDate = Helper.GetUnixTime();

                    RefleshWindow();
                    Helper.SaveItemsData(Items);
                };

                toolStripMenuItem5.Click += clickEvent7;
                toolStripMenuItem5.Disposed += (_, _) => toolStripMenuItem5.Click -= clickEvent7;

                ToolStripMenuItem toolStripMenuItem6 = new(Helper.Translate("�폜", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                EventHandler clickEvent8 = (_, _) =>
                {
                    DialogResult result = MessageBox.Show(Helper.Translate("�{���ɍ폜���܂����H", CurrentLanguage),
                        Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;

                    var undo = false; //�����폜�����A�C�e�����J����Ă�����
                    if (CurrentPath.CurrentSelectedItem?.ItemPath == item.ItemPath)
                    {
                        CurrentPath.CurrentSelectedItemCategory = null;
                        CurrentPath.CurrentSelectedItem = null;
                        undo = true;
                    }

                    var undo2 = false; //�A�o�^�[���[�h�ł����폜�����A�o�^�[���獡�܂ł̃A�C�e�����J����Ă�����
                    if (CurrentPath.CurrentSelectedAvatarPath == item.ItemPath && !_authorMode && !_categoryMode)
                    {
                        CurrentPath = new CurrentPath();
                        undo2 = true;
                    }

                    // �A�o�^�[�̂Ƃ��͑Ή��A�o�^�[�폜�A���ʑf�̃O���[�v����폜�p�̏��������s����
                    if (item.Type == ItemType.Avatar)
                    {
                        var result2 = MessageBox.Show(
                            Helper.Translate("���̃A�o�^�[��Ή��A�o�^�[�Ƃ��Ă���A�C�e���̑Ή��A�o�^�[���炱�̃A�o�^�[���폜���܂����H", CurrentLanguage),
                            Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        Helper.DeleteAvatarFromItem(ref Items, item.ItemPath, result2 == DialogResult.Yes);

                        if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                        {
                            var result3 = MessageBox.Show(Helper.Translate("���̃A�o�^�[�����ʑf�̃O���[�v����폜���܂����H", CurrentLanguage),
                                Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (result3 == DialogResult.Yes)
                            {
                                Helper.DeleteAvatarFromCommonAvatars(ref CommonAvatars, item.ItemPath);

                                Helper.SaveCommonAvatarData(CommonAvatars);
                            }
                        }
                    }

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();

                    MessageBox.Show(Helper.Translate("�폜���������܂����B", CurrentLanguage),
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);

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
                                Helper.SaveItemsData(Items);
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

                        // �A�o�^�[���I�����ꂽ���(CurrentSelectedAvatarPath�Ƃ��Đݒ肳��Ă��鎞)
                        if (undo2)
                        {
                            ResetAvatarExplorer(true);
                            PathTextBox.Text = GeneratePath();
                            Helper.SaveItemsData(Items);
                            return;
                        }

                        // �t�H���_�[���J���Ă����āA�A�C�e�����I�����ꂽ���(CurrentSelectedItem�Ƃ��Đݒ肳��Ă��鎞)
                        if (undo)
                        {
                            GenerateItems();
                            PathTextBox.Text = GeneratePath();
                            Helper.SaveItemsData(Items);
                            return;
                        }

                        // �A�C�e����ʂɊ��ɂ���
                        if (_openingWindow == Window.ItemList)
                        {
                            GenerateItems();
                            Helper.SaveItemsData(Items);
                            return;
                        }

                        // �A�C�e����ʂ̑O�ɂ���
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

            AvatarPage.ResumeLayout();
            AvatarPage.AutoScroll = true;

            Helper.UpdateExplorerThumbnails(AvatarPage);
        }

        /// <summary>
        /// ���C����ʍ��̍�җ����쐬���܂��B
        /// </summary>
        private void GenerateAuthorList()
        {
            ResetAvatarPage(AvatarAuthorPage);

            var index = 0;

            var authors = Helper.GetAuthors(Items);

            if (authors.Length == 0) return;
            authors = authors.OrderBy(author => author.AuthorName).ToArray();

            AvatarAuthorPage.SuspendLayout();
            AvatarAuthorPage.AutoScroll = false;

            foreach (var author in authors)
            {
                Button button = Helper.CreateButton(author.AuthorImagePath, author.AuthorName,
                    Items.Count(item => item.AuthorName == author.AuthorName) +
                    Helper.Translate("�̍���", CurrentLanguage), true, author.AuthorName, GetAvatarListWidth());
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

                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent1 = (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("�摜�t�@�C��|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("�T���l�C����ύX���܂����I", CurrentLanguage) + "\n\n" +
                        Helper.Translate("�ύX�O: ", CurrentLanguage) + author.AuthorImagePath + "\n\n" +
                        Helper.Translate("�ύX��: ", CurrentLanguage) + ofd.FileName,
                        "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            AvatarAuthorPage.ResumeLayout();
            AvatarAuthorPage.AutoScroll = true;

            Helper.UpdateExplorerThumbnails(AvatarAuthorPage);
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
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                if (itemType is ItemType.Unknown or ItemType.Custom) continue;

                var items = Items.Where(item => item.Type == itemType);
                var itemCount = items.Count();
                Button button = Helper.CreateButton(null,
                    Helper.GetCategoryName(itemType, CurrentLanguage),
                    itemCount + Helper.Translate("�̍���", CurrentLanguage), true, "", GetAvatarListWidth());
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

            if (CustomCategories.Length != 0)
            {
                foreach (var customCategory in CustomCategories)
                {
                    var items = Items.Where(item => item.CustomCategory == customCategory);
                    var itemCount = items.Count();

                    Button button = Helper.CreateButton(null, customCategory,
                        itemCount + Helper.Translate("�̍���", CurrentLanguage), true, "", GetAvatarListWidth());
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

            CategoryPage.ResumeLayout();
            CategoryPage.AutoScroll = true;

            Helper.UpdateExplorerThumbnails(CategoryPage);
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
                    itemCount + Helper.Translate("�̍���", CurrentLanguage), false, "", GetItemExplorerListWidth());
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

            if (CustomCategories.Length != 0)
            {
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
                        itemCount + Helper.Translate("�̍���", CurrentLanguage), false, "", GetItemExplorerListWidth());
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

            AvatarItemExplorer.ResumeLayout();
            AvatarItemExplorer.AutoScroll = true;

            Helper.UpdateExplorerThumbnails(AvatarItemExplorer);
        }

        /// <summary>
        /// ���C����ʉE�̃A�C�e�������쐬���܂��B
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
                4 => filteredItems.OrderBy(item =>
                {
                    return Helper.ContainsSelectedAvatar(item, CurrentPath.CurrentSelectedAvatarPath) ? 0 : 1;
                }),
                5 => filteredItems.OrderBy(item =>
                {
                    return Helper.ContainsSelectedAvatar(item, CurrentPath.CurrentSelectedAvatarPath) ? 1 : 0;
                }),
                _ => filteredItems.OrderBy(item => item.Title).ToArray(),
            };

            if (!filteredItems.Any()) return;

            AvatarItemExplorer.SuspendLayout();
            AvatarItemExplorer.AutoScroll = false;

            var index = 0;
            foreach (Item item in filteredItems)
            {
                var authorText = Helper.Translate("���: ", CurrentLanguage) + item.AuthorName;

                var isSupportedOrCommon =
                    Helper.IsSupportedAvatarOrCommon(item, CommonAvatars, CurrentPath.CurrentSelectedAvatarPath);

                if (isSupportedOrCommon.OnlyCommon && item.SupportedAvatar.Length != 0 &&
                    !item.SupportedAvatar.Contains(CurrentPath.CurrentSelectedAvatarPath))
                {
                    var commonAvatarName = isSupportedOrCommon.CommonAvatarName;
                    if (!string.IsNullOrEmpty(commonAvatarName))
                    {
                        authorText += "\n" + Helper.Translate("���ʑf��: ", CurrentLanguage) + commonAvatarName;
                    }
                }

                var description = Helper.GetItemDescription(item, CurrentLanguage);

                Button button = Helper.CreateButton(item.ImagePath, item.Title, authorText, false, description,
                    GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                if (SortingBox.SelectedIndex == 4 || SortingBox.SelectedIndex == 5)
                {
                    var currentAvatar = CurrentPath.CurrentSelectedAvatarPath;
                    if (!string.IsNullOrEmpty(currentAvatar))
                    {
                        button.BackColor = item.ImplementationAvatars.Contains(currentAvatar)
                            ? Color.LightGreen
                            : Color.LightPink;
                    }
                }

                EventHandler clickEvent = (_, _) =>
                {
                    if (!Directory.Exists(item.ItemPath))
                    {
                        var prePath = item.ItemPath;
                        DialogResult result =
                            MessageBox.Show(Helper.Translate("�t�H���_��������܂���ł����B�ҏW���܂����H", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Error);
                        if (result != DialogResult.Yes) return;

                        AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, true, item, null);
                        addItem.ShowDialog();

                        if (!Directory.Exists(item.ItemPath))
                        {
                            MessageBox.Show(Helper.Translate("�t�H���_��������܂���ł����B", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        //�Ή��A�o�^�[�̃p�X��ς��Ă�����
                        Helper.ChangeAllItemPath(ref Items, prePath);

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
                    ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("�t�H���_���J��", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.OpenIcon));
                    EventHandler clickEvent1 = (_, _) => Helper.OpenItemFolder(item, CurrentLanguage);

                    toolStripMenuItem.Click += clickEvent1;
                    toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent1;

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                }

                if (item.BoothId != -1)
                {
                    ToolStripMenuItem toolStripMenuItem =
                        new(Helper.Translate("Booth�����N�̃R�s�[", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    EventHandler clickEvent2 = (_, _) => Helper.CopyItemBoothLink(item, CurrentLanguage);

                    toolStripMenuItem.Click += clickEvent2;
                    toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent2;

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Booth�����N���J��", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    EventHandler clickEvent3 = (_, _) => Helper.OpenItenBoothLink(item, CurrentLanguage);

                    toolStripMenuItem1.Click += clickEvent3;
                    toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent3;

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("���̍�҂̑��̃A�C�e����\��", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                EventHandler clickEvent4 = (_, _) =>
                {
                    SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                    SearchItems();
                };

                toolStripMenuItem2.Click += clickEvent4;
                toolStripMenuItem2.Disposed += (_, _) => toolStripMenuItem2.Click -= clickEvent4;

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent5 = (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("�摜�t�@�C��|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("�T���l�C����ύX���܂����I", CurrentLanguage) + "\n\n" +
                        Helper.Translate("�ύX�O: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                        Helper.Translate("�ύX��: ", CurrentLanguage) + ofd.FileName,
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("�ҏW", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent6 = (_, _) =>
                {
                    var prePath = item.ItemPath;

                    AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, true, item, null);
                    addItem.ShowDialog();

                    //�Ή��A�o�^�[�̃p�X��ς��Ă�����
                    Helper.ChangeAllItemPath(ref Items, prePath);

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

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("�����̒ǉ�", CurrentLanguage),
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

                ToolStripMenuItem toolStripMenuItem6 = new(Helper.Translate("����/������", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));

                foreach (var avatar in Items.Where(i => i.Type == ItemType.Avatar))
                {
                    ToolStripMenuItem toolStripMenuItemTemp = new(Helper.GetAvatarNameFromPath(Items, avatar.ItemPath))
                    {
                        Tag = avatar.ItemPath,
                        Checked = item.ImplementationAvatars.Contains(avatar.ItemPath)
                    };

                    EventHandler clickEvent8 = (_, _) =>
                    {
                        if (toolStripMenuItemTemp.Checked)
                        {
                            item.ImplementationAvatars = item.ImplementationAvatars.Where(avatarPath => avatarPath != (string)toolStripMenuItemTemp.Tag).ToArray();
                            toolStripMenuItemTemp.Checked = false;
                        }
                        else
                        {
                            item.ImplementationAvatars = item.ImplementationAvatars.Append((string)toolStripMenuItemTemp.Tag).ToArray();
                            toolStripMenuItemTemp.Checked = true;
                        }

                        if (SortingBox.SelectedIndex == 4 || SortingBox.SelectedIndex == 5)
                        {
                            var currentAvatar = CurrentPath.CurrentSelectedAvatarPath;
                            if (!string.IsNullOrEmpty(currentAvatar))
                            {
                                button.BackColor = item.ImplementationAvatars.Contains(currentAvatar)
                                    ? Color.LightGreen
                                    : Color.LightPink;
                            }
                        }

                        Helper.SaveItemsData(Items);
                    };

                    toolStripMenuItemTemp.Click += clickEvent8;
                    toolStripMenuItemTemp.Click += Helper.ShowParentToolStrip;
                    toolStripMenuItemTemp.Disposed += (_, _) =>
                    {
                        toolStripMenuItemTemp.Click -= clickEvent8;
                        toolStripMenuItemTemp.Click -= Helper.ShowParentToolStrip;
                    };
                    toolStripMenuItem6.DropDownItems.Add(toolStripMenuItemTemp);
                }

                ToolStripMenuItem toolStripMenuItem7 = new(Helper.Translate("�폜", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                EventHandler clickEvent9 = (_, _) =>
                {
                    DialogResult result = MessageBox.Show(Helper.Translate("�{���ɍ폜���܂����H", CurrentLanguage),
                        Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                            Helper.Translate("���̃A�o�^�[��Ή��A�o�^�[�Ƃ��Ă���A�C�e���̑Ή��A�o�^�[���炱�̃A�o�^�[���폜���܂����H", CurrentLanguage),
                            Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        Helper.DeleteAvatarFromItem(ref Items, item.ItemPath, result2 == DialogResult.Yes);

                        if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                        {
                            var result3 = MessageBox.Show(Helper.Translate("���̃A�o�^�[�����ʑf�̃O���[�v����폜���܂����H", CurrentLanguage),
                                Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (result3 == DialogResult.Yes)
                            {
                                Helper.DeleteAvatarFromCommonAvatars(ref CommonAvatars, item.ItemPath);

                                Helper.SaveCommonAvatarData(CommonAvatars);
                            }
                        }
                    }

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();

                    MessageBox.Show(Helper.Translate("�폜���������܂����B", CurrentLanguage),
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);

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

            Helper.UpdateExplorerThumbnails(AvatarItemExplorer);
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
            ItemFolderInfo itemFolderInfo = Helper.GetItemFolderInfo(CurrentPath.CurrentSelectedItem.ItemPath,
                CurrentPath.CurrentSelectedItem.MaterialPath);
            CurrentPath.CurrentSelectedItemFolderInfo = itemFolderInfo;

            ResetAvatarExplorer();

            AvatarItemExplorer.SuspendLayout();
            AvatarItemExplorer.AutoScroll = false;

            var index = 0;
            foreach (var itemType in types)
            {
                var itemCount = itemFolderInfo.GetItemCount(itemType);
                if (itemCount == 0) continue;

                Button button = Helper.CreateButton(null,
                    Helper.Translate(itemType, CurrentLanguage), itemCount + Helper.Translate("�̍���", CurrentLanguage),
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

            AvatarItemExplorer.ResumeLayout();
            AvatarItemExplorer.AutoScroll = true;

            Helper.UpdateExplorerThumbnails(AvatarItemExplorer);
        }

        /// <summary>
        /// ���C����ʉE�̃A�C�e�����̃t�@�C�������쐬���܂��B
        /// </summary>
        private void GenerateItemFiles()
        {
            _openingWindow = Window.ItemFolderItemsList;
            ResetAvatarExplorer();

            var files = CurrentPath.CurrentSelectedItemFolderInfo.GetItems(CurrentPath.CurrentSelectedItemCategory);
            if (files.Length == 0) return;

            files = files.OrderBy(file => file.FileName).ToArray();

            AvatarItemExplorer.SuspendLayout();
            AvatarItemExplorer.AutoScroll = false;

            var index = 0;
            foreach (var file in files)
            {
                var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : "";
                Button button = Helper.CreateButton(imagePath, file.FileName,
                    file.FileExtension.Replace(".", "") + Helper.Translate("�t�@�C��", CurrentLanguage), false,
                    Helper.Translate("�J���t�@�C���̃p�X: ", CurrentLanguage) + file.FilePath, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);

                ContextMenuStrip contextMenuStrip = new();

                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("�J��", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent = (_, _) => Helper.OpenItemFile(file, true, CurrentLanguage);

                toolStripMenuItem.Click += clickEvent;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent;

                ToolStripMenuItem toolStripMenuItem1 = new(Helper.Translate("�t�@�C���̃p�X���J��", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent1 = (_, _) => Helper.OpenItemFile(file, false, CurrentLanguage);

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
                            Helper.OpenItemFile(file, true, CurrentLanguage);
                        }
                    }
                    catch
                    {
                        Helper.OpenItemFile(file, false, CurrentLanguage);
                    }
                };

                button.Click += clickEvent2;
                button.Disposed += (_, _) => button.Click -= clickEvent2;

                AvatarItemExplorer.Controls.Add(button);
                index++;
            }

            AvatarItemExplorer.ResumeLayout();
            AvatarItemExplorer.AutoScroll = true;

            Helper.UpdateExplorerThumbnails(AvatarItemExplorer);
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

            var filteredItems = Items.Where(item => Helper.GetSearchResult(Items, item, searchFilter, CurrentLanguage));

            filteredItems = filteredItems
                .Where(item =>
                    searchFilter.SearchWords.All(word =>
                        item.Title.Contains(word, StringComparison.CurrentCultureIgnoreCase) ||
                        item.AuthorName.Contains(word, StringComparison.CurrentCultureIgnoreCase) ||
                        item.SupportedAvatar.Any(avatar =>
                        {
                            var supportedAvatarName = Helper.GetAvatarNameFromPath(Items, avatar);
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
                            var supportedAvatarName = Helper.GetAvatarNameFromPath(Items, avatar);
                            if (supportedAvatarName == "") return false;
                            return supportedAvatarName.Contains(word, StringComparison.CurrentCultureIgnoreCase);
                        })) matchCount++;
                        if (item.BoothId.ToString().Contains(word, StringComparison.CurrentCultureIgnoreCase)) matchCount++;
                        if (item.ItemMemo.Contains(word, StringComparison.CurrentCultureIgnoreCase)) matchCount++;
                    }

                    return matchCount;
                })
                .ToList();

            SearchResultLabel.Text = Helper.Translate("��������: ", CurrentLanguage) + filteredItems.Count() +
                                     Helper.Translate("��", CurrentLanguage) + Helper.Translate(" (�S", CurrentLanguage) +
                                     Items.Length + Helper.Translate("��)", CurrentLanguage);
            if (!filteredItems.Any()) return;

            AvatarItemExplorer.SuspendLayout();
            AvatarItemExplorer.AutoScroll = false;

            var index = 0;
            foreach (Item item in filteredItems)
            {
                var description = Helper.GetItemDescription(item, CurrentLanguage);

                Button button = Helper.CreateButton(item.ImagePath, item.Title,
                    Helper.Translate("���: ", CurrentLanguage) + item.AuthorName, false,
                    description, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);
                EventHandler clickEvent = (_, _) =>
                {
                    if (!Directory.Exists(item.ItemPath))
                    {
                        var prePath = item.ItemPath;
                        DialogResult result =
                            MessageBox.Show(Helper.Translate("�t�H���_��������܂���ł����B�ҏW���܂����H", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Error);
                        if (result != DialogResult.Yes) return;

                        AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, true, item, null);
                        addItem.ShowDialog();

                        if (!Directory.Exists(item.ItemPath))
                        {
                            MessageBox.Show(Helper.Translate("�t�H���_��������܂���ł����B", CurrentLanguage),
                                Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        //�Ή��A�o�^�[�̃p�X��ς��Ă�����
                        Helper.ChangeAllItemPath(ref Items, prePath);

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
                    ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("�t�H���_���J��", CurrentLanguage),
                        SharedImages.GetImage(SharedImages.Images.OpenIcon));
                    EventHandler clickEvent1 = (_, _) => Helper.OpenItemFolder(item, CurrentLanguage);

                    toolStripMenuItem.Click += clickEvent1;
                    toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent1;

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                }

                if (item.BoothId != -1)
                {
                    ToolStripMenuItem toolStripMenuItem =
                        new(Helper.Translate("Booth�����N�̃R�s�[", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    EventHandler clickEvent2 = (_, _) => Helper.CopyItemBoothLink(item, CurrentLanguage);

                    toolStripMenuItem.Click += clickEvent2;
                    toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent2;

                    ToolStripMenuItem toolStripMenuItem1 =
                        new(Helper.Translate("Booth�����N���J��", CurrentLanguage),
                            SharedImages.GetImage(SharedImages.Images.CopyIcon));
                    EventHandler clickEvent3 = (_, _) => Helper.OpenItenBoothLink(item, CurrentLanguage);

                    toolStripMenuItem1.Click += clickEvent3;
                    toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent3;

                    contextMenuStrip.Items.Add(toolStripMenuItem);
                    contextMenuStrip.Items.Add(toolStripMenuItem1);
                }

                ToolStripMenuItem toolStripMenuItem2 = new(Helper.Translate("���̍�҂̑��̃A�C�e����\��", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.OpenIcon));
                EventHandler clickEvent4 = (_, _) =>
                {
                    SearchBox.Text = $"Author=\"{item.AuthorName}\"";
                    SearchItems();
                };

                toolStripMenuItem2.Click += clickEvent4;
                toolStripMenuItem2.Disposed += (_, _) => toolStripMenuItem2.Click -= clickEvent4;

                ToolStripMenuItem toolStripMenuItem3 = new(Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent5 = (_, _) =>
                {
                    OpenFileDialog ofd = new()
                    {
                        Filter = Helper.Translate("�摜�t�@�C��|*.png;*.jpg", CurrentLanguage),
                        Title = Helper.Translate("�T���l�C���ύX", CurrentLanguage),
                        Multiselect = false
                    };

                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    MessageBox.Show(
                        Helper.Translate("�T���l�C����ύX���܂����I", CurrentLanguage) + "\n\n" +
                        Helper.Translate("�ύX�O: ", CurrentLanguage) + item.ImagePath + "\n\n" +
                        Helper.Translate("�ύX��: ", CurrentLanguage) + ofd.FileName,
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    item.ImagePath = ofd.FileName;
                    GenerateFilteredItem(searchFilter);
                    GenerateAvatarList();
                    Helper.SaveItemsData(Items);
                };

                toolStripMenuItem3.Click += clickEvent5;
                toolStripMenuItem3.Disposed += (_, _) => toolStripMenuItem3.Click -= clickEvent5;

                ToolStripMenuItem toolStripMenuItem4 = new(Helper.Translate("�ҏW", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));
                EventHandler clickEvent6 = (_, _) =>
                {
                    var prePath = item.ItemPath;
                    AddItem addItem = new(this, item.Type, item.CustomCategory, true, item, null);
                    addItem.ShowDialog();

                    //�Ή��A�o�^�[�̃p�X��ς��Ă�����
                    Helper.ChangeAllItemPath(ref Items, prePath);

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

                ToolStripMenuItem toolStripMenuItem5 = new(Helper.Translate("�����̒ǉ�", CurrentLanguage),
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

                ToolStripMenuItem toolStripMenuItem6 = new(Helper.Translate("����/������", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.EditIcon));

                foreach (var avatar in Items.Where(i => i.Type == ItemType.Avatar))
                {
                    ToolStripMenuItem toolStripMenuItemTemp = new(Helper.GetAvatarNameFromPath(Items, avatar.ItemPath))
                    {
                        Tag = avatar.ItemPath,
                        Checked = item.ImplementationAvatars.Contains(avatar.ItemPath)
                    };

                    EventHandler clickEvent8 = (_, _) =>
                    {
                        if (toolStripMenuItemTemp.Checked)
                        {
                            item.ImplementationAvatars = item.ImplementationAvatars.Where(avatarPath => avatarPath != (string)toolStripMenuItemTemp.Tag).ToArray();
                            toolStripMenuItemTemp.Checked = false;
                        }
                        else
                        {
                            item.ImplementationAvatars = item.ImplementationAvatars.Append((string)toolStripMenuItemTemp.Tag).ToArray();
                            toolStripMenuItemTemp.Checked = true;
                        }

                        Helper.SaveItemsData(Items);
                    };

                    toolStripMenuItemTemp.Click += clickEvent8;
                    toolStripMenuItemTemp.Click += Helper.ShowParentToolStrip;
                    toolStripMenuItemTemp.Disposed += (_, _) =>
                    {
                        toolStripMenuItemTemp.Click -= clickEvent8;
                        toolStripMenuItemTemp.Click -= Helper.ShowParentToolStrip;
                    };
                    toolStripMenuItem6.DropDownItems.Add(toolStripMenuItemTemp);
                }

                ToolStripMenuItem toolStripMenuItem7 = new(Helper.Translate("�폜", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.TrashIcon));
                EventHandler clickEvent9 = (_, _) =>
                {
                    DialogResult result = MessageBox.Show(Helper.Translate("�{���ɍ폜���܂����H", CurrentLanguage),
                        Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();

                    if (item.Type == ItemType.Avatar)
                    {
                        var result2 = MessageBox.Show(
                            Helper.Translate("���̃A�o�^�[��Ή��A�o�^�[�Ƃ��Ă���A�C�e���̑Ή��A�o�^�[���炱�̃A�o�^�[���폜���܂����H", CurrentLanguage),
                            Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        Helper.DeleteAvatarFromItem(ref Items, item.ItemPath, result2 == DialogResult.Yes);

                        if (CommonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
                        {
                            var result3 = MessageBox.Show(Helper.Translate("���̃A�o�^�[�����ʑf�̃O���[�v����폜���܂����H", CurrentLanguage),
                                Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (result3 == DialogResult.Yes)
                            {
                                Helper.DeleteAvatarFromCommonAvatars(ref CommonAvatars, item.ItemPath);

                                Helper.SaveCommonAvatarData(CommonAvatars);
                            }
                        }
                    }

                    Items = Items.Where(i => i.ItemPath != item.ItemPath).ToArray();

                    MessageBox.Show(Helper.Translate("�폜���������܂����B", CurrentLanguage),
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);

                    GenerateFilteredItem(searchFilter);
                    GenerateAvatarList();
                    GenerateAuthorList();
                    GenerateCategoryListLeft();
                    Helper.SaveItemsData(Items);
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

            Helper.UpdateExplorerThumbnails(AvatarItemExplorer);
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
                Window.ItemFolderItemsList => CurrentPath.CurrentSelectedItemFolderInfo.GetItems(CurrentPath
                    .CurrentSelectedItemCategory),
                Window.ItemFolderCategoryList => CurrentPath.CurrentSelectedItemFolderInfo.GetAllItem(),
                _ => Array.Empty<FileData>()
            };

            var filteredFileData = fileDatas
                .Where(file =>
                    searchWords.SearchWords.All(word =>
                        file.FileName.Contains(word, StringComparison.CurrentCultureIgnoreCase)
                    )
                )
                .OrderByDescending(file =>
                {
                    return searchWords.SearchWords.Count(word => file.FileName.Contains(word, StringComparison.CurrentCultureIgnoreCase));
                })
                .ToList();

            SearchResultLabel.Text = Helper.Translate("�t�H���_�[����������: ", CurrentLanguage) + filteredFileData.Count +
                                     Helper.Translate("��", CurrentLanguage) + Helper.Translate(" (�S", CurrentLanguage) +
                                     fileDatas.Length + Helper.Translate("��)", CurrentLanguage);
            if (filteredFileData.Count == 0) return;

            AvatarItemExplorer.SuspendLayout();
            AvatarItemExplorer.AutoScroll = false;

            var index = 0;
            foreach (var file in filteredFileData)
            {
                var imagePath = file.FileExtension is ".png" or ".jpg" ? file.FilePath : "";
                Button button = Helper.CreateButton(imagePath, file.FileName,
                    file.FileExtension.Replace(".", "") + Helper.Translate("�t�@�C��", CurrentLanguage), false,
                    Helper.Translate("�J���t�@�C���̃p�X: ", CurrentLanguage) + file.FilePath, GetItemExplorerListWidth());
                button.Location = new Point(0, (70 * index) + 2);

                ContextMenuStrip contextMenuStrip = new();

                ToolStripMenuItem toolStripMenuItem = new(Helper.Translate("�J��", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent = (_, _) => Helper.OpenItemFile(file, true, CurrentLanguage);

                toolStripMenuItem.Click += clickEvent;
                toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= clickEvent;

                ToolStripMenuItem toolStripMenuItem1 = new(Helper.Translate("�t�@�C���̃p�X���J��", CurrentLanguage),
                    SharedImages.GetImage(SharedImages.Images.CopyIcon));
                EventHandler clickEvent1 = (_, _) => Helper.OpenItemFile(file, false, CurrentLanguage);

                toolStripMenuItem1.Click += clickEvent1;
                toolStripMenuItem1.Disposed += (_, _) => toolStripMenuItem1.Click -= clickEvent1;

                contextMenuStrip.Items.Add(toolStripMenuItem);
                contextMenuStrip.Items.Add(toolStripMenuItem1);
                button.ContextMenuStrip = contextMenuStrip;

                EventHandler clickEvent2 = (_, _) => Helper.OpenItemFile(file, true, CurrentLanguage);

                button.Click += clickEvent2;
                button.Disposed += (_, _) => button.Click -= clickEvent2;

                AvatarItemExplorer.Controls.Add(button);
                index++;
            }

            AvatarItemExplorer.ResumeLayout();
            AvatarItemExplorer.AutoScroll = true;

            Helper.UpdateExplorerThumbnails(AvatarItemExplorer);
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
            AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, false, null, null);
            addItem.ShowDialog();
            RefleshWindow();
            Helper.SaveItemsData(Items);
        }
        #endregion

        #region �p�X�֘A�̏���

        /// <summary>
        /// ���݂̃p�X�𐶐����܂��B
        /// </summary>
        /// <returns></returns>
        private string GeneratePath()
        {
            var categoryName = Helper.GetCategoryName(CurrentPath.CurrentSelectedCategory, CurrentLanguage, CurrentPath.CurrentSelectedCustomCategory);

            if (_authorMode)
            {
                if (CurrentPath.CurrentSelectedAuthor == null)
                    return Helper.Translate("�����ɂ͌��݂̃p�X���\������܂�", CurrentLanguage);
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
                    return Helper.Translate("�����ɂ͌��݂̃p�X���\������܂�", CurrentLanguage);
                if (CurrentPath.CurrentSelectedItem == null)
                    return categoryName;
                if (CurrentPath.CurrentSelectedItemCategory == null)
                    return categoryName + " / " +
                           Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title);

                return categoryName + " / " +
                       Helper.RemoveFormat(CurrentPath.CurrentSelectedItem.Title) + " / " +
                       Helper.Translate(CurrentPath.CurrentSelectedItemCategory, CurrentLanguage);
            }

            if (CurrentPath.CurrentSelectedAvatar == null) return Helper.Translate("�����ɂ͌��݂̃p�X���\������܂�", CurrentLanguage);
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
        /// �I�����ꂽ�A�C�e������p�X�𐶐����܂��B
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

        #region �߂�{�^���̏���

        /// <summary>
        /// �߂�{�^���������ꂽ�ۂ̏������s���܂��B
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UndoButton_Click(object sender, EventArgs e)
        {
            //�������������ꍇ�͑O�̉�ʂ܂łƂ肠�����߂��Ă�����
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
                //�G���[�����Đ�
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
                pathTextArr = pathTextArr.Append(Helper.Translate("���", CurrentLanguage) + ": " +
                                                 string.Join(", ", searchFilter.Author))
                    .ToArray();
            }

            if (searchFilter.Title.Length != 0)
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("�^�C�g��", CurrentLanguage) + ": " +
                                                 string.Join(", ", searchFilter.Title))
                    .ToArray();
            }

            if (searchFilter.BoothId.Length != 0)
            {
                pathTextArr = pathTextArr.Append("BoothID: " + string.Join(", ", searchFilter.BoothId)).ToArray();
            }

            if (searchFilter.Avatar.Length != 0)
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("�A�o�^�[", CurrentLanguage) + ": " +
                                                 string.Join(", ", searchFilter.Avatar))
                    .ToArray();
            }

            if (searchFilter.Category.Length != 0)
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("�J�e�S��", CurrentLanguage) + ": " +
                                                 string.Join(", ", searchFilter.Category))
                    .ToArray();
            }

            if (searchFilter.ItemMemo.Length != 0)
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("����", CurrentLanguage) + ": " +
                                                 string.Join(", ", searchFilter.ItemMemo))
                    .ToArray();
            }

            if (searchFilter.FolderName.Length != 0)
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("�t�H���_��", CurrentLanguage) + ": " +
                                                 string.Join(", ", searchFilter.FolderName))
                    .ToArray();
            }

            if (searchFilter.FileName.Length != 0)
            {
                pathTextArr = pathTextArr.Append(Helper.Translate("�t�@�C����", CurrentLanguage) + ": " +
                                                 string.Join(", ", searchFilter.FileName))
                    .ToArray();
            }

            pathTextArr = pathTextArr.Append(string.Join(", ", searchFilter.SearchWords)).ToArray();

            PathTextBox.Text = Helper.Translate("������... - ", CurrentLanguage) + string.Join(" / ", pathTextArr);
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
                _authorMode = false;
                _categoryMode = false;
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
            var controls = page.Controls.Cast<Control>().ToList();
            controls.Reverse();

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

            AddItem addItem = new(this, CurrentPath.CurrentSelectedCategory, CurrentPath.CurrentSelectedCustomCategory, false, null, dragFilePathArr);
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

            AddItem addItem = new(this, ItemType.Avatar, null, false, null, dragFilePathArr);
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
                    if (index > 60) throw new Exception("Too many exports.");
                    fileName = currentTimeStr + $"_{index}.csv";
                    index++;
                }

                var commonAvatarResult = MessageBox.Show(Helper.Translate("�Ή��A�o�^�[�̗��ɋ��ʑf�̃O���[�v�̃A�o�^�[���ǉ����܂����H", CurrentLanguage), Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                using var sw = new StreamWriter("./Output/" + fileName, false, Encoding.UTF8);
                sw.WriteLine("Title,AuthorName,AuthorImageFilePath,ImagePath,Type,Memo,SupportedAvatars,ImplementedAvatars,BoothId,ItemPath");

                foreach (var item in Items)
                {
                    string[] SupportedAvatarNames = Array.Empty<string>();
                    string[] SupportedAvatarPaths = Array.Empty<string>();

                    foreach (var avatar in item.SupportedAvatar)
                    {
                        var avatarName = Helper.GetAvatarName(Items, avatar);
                        if (avatarName == null) continue;
                        SupportedAvatarNames = SupportedAvatarNames.Append(avatarName).ToArray();
                        SupportedAvatarPaths = SupportedAvatarPaths.Append(avatar).ToArray();

                        if (commonAvatarResult != DialogResult.Yes) continue;
                        var commonAvatarGroup = CommonAvatars.Where(commonAvatar => commonAvatar.Avatars.Contains(avatar));
                        foreach (var commonAvatar in commonAvatarGroup)
                        {
                            foreach (var commonAvatarPath in commonAvatar.Avatars)
                            {
                                if (SupportedAvatarPaths.Contains(commonAvatarPath)) continue;
                                var name = Helper.GetAvatarName(Items, commonAvatarPath);
                                if (name == null) continue;
                                SupportedAvatarNames = SupportedAvatarNames.Append(name).ToArray();
                                SupportedAvatarPaths = SupportedAvatarPaths.Append(commonAvatarPath).ToArray();
                            }
                        }
                    }

                    string[] ImplementedAvatarNames = Array.Empty<string>();
                    foreach (var avatar in item.ImplementationAvatars)
                    {
                        var avatarName = Helper.GetAvatarName(Items, avatar);
                        if (avatarName == null) continue;
                        ImplementedAvatarNames = ImplementedAvatarNames.Append(avatarName).ToArray();
                    }

                    var itemTitle = Helper.EscapeCsv(item.Title);
                    var authorName = Helper.EscapeCsv(item.AuthorName);
                    var authorImageFilePath = Helper.EscapeCsv(item.AuthorImageFilePath);
                    var imagePath = Helper.EscapeCsv(item.ImagePath);
                    var type = Helper.EscapeCsv(Helper.GetCategoryName(item.Type, CurrentLanguage, item.CustomCategory));
                    var memo = Helper.EscapeCsv(item.ItemMemo);
                    var SupportedAvatarList = Helper.EscapeCsv(string.Join(Environment.NewLine, SupportedAvatarNames));
                    var ImplementedAvatarList = Helper.EscapeCsv(string.Join(Environment.NewLine, ImplementedAvatarNames));
                    var boothId = Helper.EscapeCsv(item.BoothId.ToString());
                    var itemPath = Helper.EscapeCsv(item.ItemPath);

                    sw.WriteLine($"{itemTitle},{authorName},{authorImageFilePath},{imagePath},{type},{memo},{SupportedAvatarList},{ImplementedAvatarList},{boothId},{itemPath}");
                }

                MessageBox.Show(Helper.Translate("Output�t�H���_�ɃG�N�X�|�[�g���������܂����I\n�t�@�C����: ", CurrentLanguage) + fileName,
                    Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                ExportButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helper.Translate("�G�N�X�|�[�g�Ɏ��s���܂���", CurrentLanguage),
                    Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Helper.ErrorLogger("�G�N�X�|�[�g�Ɏ��s���܂����B", ex);
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
                    if (index > 60) throw new Exception("Too many backups");
                    fileName = currentTimeStr + $"_{index}.zip";
                    index++;
                }

                ZipFile.CreateFromDirectory("./Datas", "./Backup/" + fileName);

                MessageBox.Show(
                    Helper.Translate(
                        "Backup�t�H���_�Ƀo�b�N�A�b�v���������܂����I\n\n�����������ꍇ�́A\"�f�[�^��ǂݍ���\"�{�^���Ō��ݍ쐬���ꂽ�t�@�C����W�J�������̂�I�����Ă��������B\n\n�t�@�C����: ",
                        CurrentLanguage) + fileName, Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                MakeBackupButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helper.Translate("�o�b�N�A�b�v�Ɏ��s���܂���", CurrentLanguage),
                    Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Helper.ErrorLogger("�o�b�N�A�b�v�Ɏ��s���܂����B", ex);
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
                if (control.Name == "LanguageBox") continue;
                if (string.IsNullOrEmpty(control.Text)) continue;
                _controlNames.TryAdd(control.Name, control.Text);
                control.Text = Helper.Translate(_controlNames[control.Name], CurrentLanguage);
                ChangeControlFont(control);
            }

            string[] sortingItems = ["�^�C�g��", "���", "�o�^����", "�X�V����", "�����ς�", "������"];
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
            //�����o�b�N�A�b�v�t�H���_���畜�����邩����
            var result = MessageBox.Show(Helper.Translate("�����o�b�N�A�b�v�t�H���_���畜�����܂����H", CurrentLanguage),
                Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                //�o�b�N�A�b�v��̃t�H���_
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var backupPath = Path.Combine(appDataPath, "Avatar Explorer", "Backup");

                //�o�b�N�A�b�v�t�H���_�����݂��Ȃ��ꍇ
                if (!Directory.Exists(backupPath))
                {
                    MessageBox.Show(Helper.Translate("�o�b�N�A�b�v�t�H���_��������܂���ł����B", CurrentLanguage),
                        Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //�ŏ��̃t�H���_
                var firstFolder = Directory.GetDirectories(backupPath).MaxBy(d => new DirectoryInfo(d).CreationTime) ??
                                  backupPath;

                FolderBrowserDialog fbd = new()
                {
                    UseDescriptionForTitle = true,
                    Description = Helper.Translate("�������鎞�Ԃ̃o�b�N�A�b�v�t�H���_��I�����Ă�������", CurrentLanguage),
                    ShowNewFolderButton = false,
                    SelectedPath = firstFolder
                };

                if (fbd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var filePath = fbd.SelectedPath + "/ItemsData.json";
                    if (!File.Exists(filePath))
                    {
                        MessageBox.Show(Helper.Translate("�A�C�e���t�@�C����������܂���ł����B", CurrentLanguage),
                            Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        Items = Helper.LoadItemsData(filePath);
                        Helper.FixSupportedAvatarPath(ref Items);
                        Helper.UpdateEmptyDates(ref Items);
                        Helper.FixItemDates(ref Items);
                        Helper.FixRelativePathEscape(ref Items);
                        Helper.SaveItemsData(Items);
                    }

                    var filePath2 = fbd.SelectedPath + "/CommonAvatar.json";
                    if (!File.Exists(filePath2))
                    {
                        MessageBox.Show(Helper.Translate("���ʑf�̃t�@�C����������܂���ł����B", CurrentLanguage),
                            Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        CommonAvatars = Helper.LoadCommonAvatarData(filePath2);
                        Helper.SaveCommonAvatarData(CommonAvatars);
                    }

                    var customCategoryPath = fbd.SelectedPath + "/CustomCategory.txt";
                    if (!File.Exists(customCategoryPath))
                    {
                        MessageBox.Show(Helper.Translate("�J�X�^���J�e�S���[�t�@�C����������܂���ł����B", CurrentLanguage),
                            Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        CustomCategories = Helper.LoadCustomCategoriesData(customCategoryPath);
                        Helper.SaveCustomCategoriesData(CustomCategories);
                    }

                    MessageBox.Show(Helper.Translate("�������������܂����B", CurrentLanguage),
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Helper.ErrorLogger("�f�[�^�̓ǂݍ��݂Ɏ��s���܂����B", ex);
                    MessageBox.Show(Helper.Translate("�f�[�^�̓ǂݍ��݂Ɏ��s���܂����B�ڍׂ�ErrorLog.txt���������������B", CurrentLanguage),
                        Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                FolderBrowserDialog fbd = new()
                {
                    UseDescriptionForTitle = true,
                    Description = Helper.Translate("�ȑO�̃o�[�W������Datas�t�H���_�A�������͓W�J�����o�b�N�A�b�v�t�H���_��I�����Ă�������", CurrentLanguage),
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
                        MessageBox.Show(Helper.Translate("�A�C�e���t�@�C����������܂���ł����B", CurrentLanguage),
                            Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        Items = Helper.LoadItemsData(filePath);
                        Helper.FixSupportedAvatarPath(ref Items);
                        Helper.UpdateEmptyDates(ref Items);
                        Helper.FixItemDates(ref Items);
                        Helper.FixRelativePathEscape(ref Items);
                        Helper.SaveItemsData(Items);
                    }

                    var filePath2 = fbd.SelectedPath + "/CommonAvatar.json";
                    if (!File.Exists(filePath2))
                    {
                        MessageBox.Show(Helper.Translate("���ʑf�̃t�@�C����������܂���ł����B", CurrentLanguage),
                            Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        CommonAvatars = Helper.LoadCommonAvatarData(filePath2);
                        Helper.SaveCommonAvatarData(CommonAvatars);
                    }

                    var customCategoryPath = fbd.SelectedPath + "/CustomCategory.txt";
                    if (!File.Exists(customCategoryPath))
                    {
                        MessageBox.Show(Helper.Translate("�J�X�^���J�e�S���[�t�@�C����������܂���ł����B", CurrentLanguage),
                            Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        CustomCategories = Helper.LoadCustomCategoriesData(customCategoryPath);
                        Helper.SaveCustomCategoriesData(CustomCategories);
                    }

                    var result2 = MessageBox.Show(
                        Helper.Translate("Thumbnail�t�H���_�AAuthorImage�t�H���_�AItems�t�H���_���R�s�[���܂����H", CurrentLanguage),
                        Helper.Translate("�m�F", CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                        MessageBox.Show(Helper.Translate("�R�s�[���������܂����B", CurrentLanguage),
                            Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                                Helper.ErrorLogger("�T���l�C���̃R�s�[�Ɏ��s���܂����B", ex);
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
                                Helper.ErrorLogger("��҉摜�̃R�s�[�Ɏ��s���܂����B", ex);
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
                            Helper.ErrorLogger("Items�̃R�s�[�Ɏ��s���܂����B", ex);
                            itemsResult = false;
                        }
                    }

                    MessageBox.Show(Helper.Translate("�R�s�[���������܂����B", CurrentLanguage) + "\n\n" + Helper.Translate("�R�s�[���s�ꗗ: ", CurrentLanguage) +
                                    (thumbnailResult ? "" : "\n" + Helper.Translate("�T���l�C���̃R�s�[�Ɉꕔ���s���Ă��܂��B", CurrentLanguage)) +
                                    (authorImageResult ? "" : "\n" + Helper.Translate("��҉摜�̃R�s�[�Ɉꕔ���s���Ă��܂��B", CurrentLanguage)) +
                                    (itemsResult ? "" : "\n" + Helper.Translate("Items�̃R�s�[�Ɉꕔ���s���Ă��܂��B", CurrentLanguage)),
                        Helper.Translate("����", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Helper.ErrorLogger("�f�[�^�̓ǂݍ��݂Ɏ��s���܂����B", ex);
                    MessageBox.Show(Helper.Translate("�f�[�^�̓ǂݍ��݂Ɏ��s���܂����B�ڍׂ�ErrorLog.txt���������������B", CurrentLanguage),
                        Helper.Translate("�G���[", CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            ManageCommonAvatars manageCommonAvatar = new(this);
            manageCommonAvatar.ShowDialog();
            RefleshWindow();
            PathTextBox.Text = GeneratePath();
            Helper.SaveCommonAvatarData(CommonAvatars);
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
            control.Font = new Font(GuiFont, previousSize);
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

            Helper.UpdateExplorerThumbnails(AvatarItemExplorer);
            Helper.UpdateExplorerThumbnails(AvatarPage);
            Helper.UpdateExplorerThumbnails(AvatarAuthorPage);
            Helper.UpdateExplorerThumbnails(CategoryPage);
        }

        /// <summary>
        /// ���x���̈ʒu�𒲐����܂��B
        /// </summary>
        private void AdjustLabelPosition()
        {
            label2.Location = label2.Location with
            {
                X = LanguageBox.Location.X + LanguageBox.Width / 2 - label2.Width / 2,
                Y = label2.Location.Y
            };

            SortingLabel.Location = SortingLabel.Location with
            {
                X = SortingBox.Location.X + SortingBox.Width / 2 - SortingLabel.Width / 2,
                Y = SortingLabel.Location.Y
            };

            label1.Location = label1.Location with
            {
                X = SearchBox.Location.X - label1.Width - 8,
                Y = SearchBox.Location.Y + SearchBox.Height / 2 - label1.Height / 2
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
                    if (_lastBackupError)
                        Text = CurrentVersionFormText + " - " + Helper.Translate("�o�b�N�A�b�v�G���[", CurrentLanguage);
                    return;
                }

                var timeSpan = DateTime.Now - _lastBackupTime;
                var minutes = timeSpan.Minutes;
                Text = CurrentVersionFormText +
                       $" - {Helper.Translate("�ŏI�����o�b�N�A�b�v: ", CurrentLanguage) + minutes + Helper.Translate("���O", CurrentLanguage)}";

                if (_lastBackupError) Text += " - " + Helper.Translate("�o�b�N�A�b�v�G���[", CurrentLanguage);
            };
            timer.Start();
        }

        /// <summary>
        /// �t�@�C���̃o�b�N�A�b�v���s���܂��B
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
                Helper.ErrorLogger("�����o�b�N�A�b�v�Ɏ��s���܂����B", ex);
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
                Helper.ErrorLogger("�ꎞ�t�H���_�̍폜�Ɏ��s���܂����B", ex);
            }
        }
        #endregion
    }
}