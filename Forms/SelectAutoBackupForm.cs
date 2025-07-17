using Avatar_Explorer.Utils;

namespace Avatar_Explorer.Forms;

internal partial class SelectAutoBackupForm : Form
{
    /// <summary>
    /// メインフォームを取得または設定します。
    /// </summary>
    private readonly MainForm _mainForm;

    /// <summary>
    /// 自動でバックアップのパスを取得します。
    /// </summary>
    private readonly string AUTO_BACKUP_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Avatar Explorer", "Backup");

    /// <summary>
    /// バックアップフォルダと日付を紐づけてあるDictionaryを取得または設定します。
    /// </summary>
    private readonly Dictionary<string, string> _backupPaths;

    /// <summary>
    /// 選択されたフォルダパスを取得または設定します。
    /// </summary>
    internal string SelectedBackupPath { get; private set; } = string.Empty;

    internal SelectAutoBackupForm(MainForm mainForm)
    {
        _mainForm = mainForm;

        InitializeComponent();

        TranslateControls();

        _backupPaths = GetBackupPaths(AUTO_BACKUP_PATH);

        SelectBackup.Items.Clear();

        foreach (var backup in _backupPaths)
        {
            SelectBackup.Items.Add(backup.Key);
        }

        if (SelectBackup.Items.Count > 0) SelectBackup.SelectedIndex = 0;
    }

    #region フォーム関連の処理
    /// <summary>
    /// コントロールを翻訳します。
    /// </summary>
    private void TranslateControls()
    {
        if (_mainForm.CurrentLanguage == "ja-JP") return;

        foreach (Control control in Controls)
        {
            if (!string.IsNullOrEmpty(control.Text))
            {
                control.Text = LanguageUtils.Translate(control.Text, _mainForm.CurrentLanguage);
            }
        }

        Text = LanguageUtils.Translate("自動バックアップから復元", _mainForm.CurrentLanguage);
    }
    #endregion

    #region イベントハンドラ
    private void SelectBackup_SelectedIndexChanged(object sender, EventArgs e)
    {
        var backupPath = GetBackupPath();

        var customCategoryCount = 0;
        var itemDatabaseCount = 0;
        var commonAvatarDatabaseCount = 0;

        if (!string.IsNullOrEmpty(backupPath) && Directory.Exists(backupPath))
        {
            customCategoryCount = DatabaseUtils.GetCustomCategoryCount(backupPath);
            itemDatabaseCount = DatabaseUtils.GetItemDatabaseCount(backupPath);
            commonAvatarDatabaseCount = DatabaseUtils.GetCommonAvatarDatabaseCount(backupPath);
        }

        var countTranslate = LanguageUtils.Translate("個", _mainForm.CurrentLanguage);

        var itemDatabase = LanguageUtils.Translate("アイテムデータ", _mainForm.CurrentLanguage) + ": " + itemDatabaseCount + countTranslate;
        var commonAvatarDatabase = LanguageUtils.Translate("共通素体データ", _mainForm.CurrentLanguage) + ": " + commonAvatarDatabaseCount + countTranslate;
        var customCategory = LanguageUtils.Translate("カスタムカテゴリ", _mainForm.CurrentLanguage) + ": " + customCategoryCount + countTranslate;

        BackupInfo.Text = $"{itemDatabase}\n{commonAvatarDatabase}\n{customCategory}";
    }

    private void SelectButton_Click(object sender, EventArgs e)
    {
        SelectedBackupPath = GetBackupPath();
        Close();
    }
    #endregion

    #region 処理関数
    /// <summary>
    /// バックアップパスのリストを取得します。
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private Dictionary<string, string> GetBackupPaths(string path)
    {
        try
        {
            var backupPaths = new Dictionary<string, string>();
            var autoBackupFiles = Directory.GetDirectories(path).Reverse();

            foreach (var file in autoBackupFiles)
            {
                var fileName = Path.GetFileName(file);
                var dateTime = BackupUtils.GetBackupTime(fileName);
                backupPaths.Add(dateTime, file);
            }

            return backupPaths;
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("エラー", _mainForm.CurrentLanguage) + ": " + LanguageUtils.Translate("バックアップデータの取得に失敗しました" + ex.Message, _mainForm.CurrentLanguage),
                LanguageUtils.Translate("エラー", _mainForm.CurrentLanguage),
                true
            );
            return new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// 現在選択されているバックアップパスを取得します。
    /// </summary>
    /// <returns></returns>
    private string GetBackupPath()
    {
        if (SelectBackup.SelectedItem == null) return string.Empty;

        var selectedBackup = SelectBackup.SelectedItem.ToString();
        if (selectedBackup == null) return string.Empty;

        return _backupPaths.TryGetValue(selectedBackup, out string? backupPath) ? backupPath : string.Empty;
    }
    #endregion
}
