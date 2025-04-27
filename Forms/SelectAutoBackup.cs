using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public partial class SelectAutoBackup : Form
    {
        /// <summary>
        /// メインフォームを取得または設定します。
        /// </summary>
        private readonly Main _mainForm;

        /// <summary>
        /// 自動でバックアップのパスを取得します。
        /// </summary>
        private readonly string AUTO_BACKUP_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Avatar Explorer", "Backup");

        /// <summary>
        /// バックアップフォルダと日付を紐づけてあるDictionaryを取得または設定します。
        /// </summary>
        private readonly Dictionary<string, string> _backupPaths = new();

        /// <summary>
        /// 選択されたフォルダパスを取得または設定します。
        /// </summary>
        public string SelectedBackupPath { get; private set; } = string.Empty;

        public SelectAutoBackup(Main mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();

            if (_mainForm.CurrentLanguage != "ja-JP")
            {
                foreach (Control control in Controls)
                {
                    if (!string.IsNullOrEmpty(control.Text))
                    {
                        control.Text = Helper.Translate(control.Text, _mainForm.CurrentLanguage);
                    }
                }

                Text = Helper.Translate("自動バックアップから復元", _mainForm.CurrentLanguage);
            }

            _backupPaths = GetBackupPaths(AUTO_BACKUP_PATH);
            SelectBackup.Items.Clear();

            foreach (var backup in _backupPaths)
            {
                SelectBackup.Items.Add(backup.Key);
            }

            if (SelectBackup.Items.Count > 0) SelectBackup.SelectedIndex = 0;
        }

        private void SelectBackup_SelectedIndexChanged(object sender, EventArgs e)
        {
            var backupPath = GetBackupPath();

            var customCategoryCount = 0;
            var itemDatabaseCount = 0;
            var commonAvatarDatabaseCount = 0;

            if (!string.IsNullOrEmpty(backupPath) && Directory.Exists(backupPath))
            {
                customCategoryCount = Helper.GetCustomCategoryCount(backupPath);
                itemDatabaseCount = Helper.GetItemDatabaseCount(backupPath);
                commonAvatarDatabaseCount = Helper.GetCommonAvatarDatabaseCount(backupPath);
            }

            var countTranslate = Helper.Translate("個", _mainForm.CurrentLanguage);

            var itemDatabase = Helper.Translate("アイテムデータ", _mainForm.CurrentLanguage) + ": " + itemDatabaseCount + countTranslate;
            var commonAvatarDatabase = Helper.Translate("共通素体データ", _mainForm.CurrentLanguage) + ": " + commonAvatarDatabaseCount + countTranslate;
            var customCategory = Helper.Translate("カスタムカテゴリ", _mainForm.CurrentLanguage) + ": " + customCategoryCount + countTranslate;

            BackupInfo.Text = $"{itemDatabase}\n{commonAvatarDatabase}\n{customCategory}";
        }

        private Dictionary<string, string> GetBackupPaths(string path)
        {
            try
            {
                var backupPaths = new Dictionary<string, string>();
                var autoBackupFiles = Directory.GetDirectories(path);
                autoBackupFiles = autoBackupFiles.Reverse().ToArray();

                foreach (var file in autoBackupFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var dateTime = Helper.GetBackupTime(fileName);
                    backupPaths.Add(dateTime, file);
                }

                return backupPaths;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helper.Translate("エラー", _mainForm.CurrentLanguage) + ": " + Helper.Translate("バックアップデータの取得に失敗しました" + ex.Message, _mainForm.CurrentLanguage),
                    Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new Dictionary<string, string>();
            }
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            SelectedBackupPath = GetBackupPath();
            Close();
        }

        private string GetBackupPath()
        {
            if (SelectBackup.SelectedItem == null) return string.Empty;

            var selectedBackup = SelectBackup.SelectedItem.ToString();
            if (selectedBackup == null) return string.Empty;

            return _backupPaths.TryGetValue(selectedBackup, out string? backupPath) ? backupPath : string.Empty;
        }
    }
}
