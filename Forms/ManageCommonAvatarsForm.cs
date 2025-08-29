using Avatar_Explorer.Models;
using Avatar_Explorer.Utils;

namespace Avatar_Explorer.Forms;

internal sealed partial class ManageCommonAvatarsForm : Form
{
    /// <summary>
    /// メインフォームを取得または設定します。
    /// </summary>
    private readonly MainForm _mainForm;

    /// <summary>
    /// 共通素体のリストを取得または設定します。
    /// </summary>
    private readonly List<CommonAvatar> _commonAvatars;

    /// <summary>
    /// 共通素体の管理フォームを初期化します。
    /// </summary>
    /// <param name="mainform"></param>
    internal ManageCommonAvatarsForm(MainForm mainform)
    {
        _mainForm = mainform;
        _commonAvatars = _mainForm.CommonAvatars;
        InitializeComponent();
        if (_mainForm.DarkMode) SetDarkMode();

        Text = LanguageUtils.Translate("共通素体の管理", _mainForm.CurrentLanguage);
        TranslateControls();

        foreach (var commonAvatar in _commonAvatars)
        {
            CommonAvatarsCombobox.Items.Add(commonAvatar.Name);
        }

        if (CommonAvatarsCombobox.Items.Count > 0)
        {
            CommonAvatarsCombobox.SelectedIndex = 0;
        }

        GenerateAvatarList();
        RefleshCommonAvatarButtonColor();
    }

    private void SetDarkMode()
    {
        foreach (Control contorol in Controls)
        {
            DarkModeUtils.SetDarkMode(contorol);
        }
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

        AvatarList.Text = LanguageUtils.Translate(AvatarList.Text, _mainForm.CurrentLanguage);
    }

    /// <summary>
    /// フォーム内のアバターリストを生成します。
    /// </summary>
    private void GenerateAvatarList()
    {
        AvatarList.Controls.Clear();
        var items = _mainForm.Items.Where(item => item.Type == ItemType.Avatar).ToList();
        if (items.Count == 0) return;
        items = items.OrderBy(item => item.Title).ToList();

        AvatarList.SuspendLayout();
        AvatarList.AutoScroll = false;

        var index = 0;
        foreach (Item item in _mainForm.Items.Where(item => item.Type == ItemType.Avatar))
        {
            Button button = CreateAvatarButton(_mainForm.DarkMode, _mainForm.ButtonSize, item, _mainForm.CurrentLanguage);
            button.Location = new Point(0, ((_mainForm.ButtonSize + 6) * index) + 2);
            button.Tag = item.ItemPath;

            var commonAvatar = GetCommonAvatar(CommonAvatarsCombobox.Text);
            button.BackColor = commonAvatar != null
                ? commonAvatar.Avatars.Contains(item.ItemPath)
                    ? Color.LightGreen
                    : Color.FromKnownColor(KnownColor.Control)
                : Color.FromKnownColor(KnownColor.Control);

            AvatarList.Controls.Add(button);
            index++;
        }

        AvatarList.ResumeLayout();
        AvatarList.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(AvatarList);
    }

    /// <summary>
    /// アバターのボタンを生成します。
    /// </summary>
    /// <param name="buttonHeight"></param>
    /// <param name="item"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    private static CustomItemButton CreateAvatarButton(bool darkMode, int buttonHeight, Item item, string language)
    {
        CustomItemButton button = new(875, buttonHeight, darkMode)
        {
            ImagePath = item.ImagePath,
            TitleText = item.Title,
            AuthorName = LanguageUtils.Translate("作者: ", language) + item.AuthorName,
            ToolTipText = item.Title
        };

        button.Click += (_, _) =>
        {
            button.BackColor = button.BackColor == Color.LightGreen
                ? Color.FromKnownColor(KnownColor.Control)
                : Color.LightGreen;
        };

        return button;
    }
    #endregion

    #region イベントハンドラ
    private void DeleteSelectedGroupButton_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CommonAvatarsCombobox.Text))
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("削除する共通素体を選択してください。", _mainForm.CurrentLanguage),
                LanguageUtils.Translate("エラー", _mainForm.CurrentLanguage),
                true
            );
            return;
        }

        var commonAvatar = GetCommonAvatar(CommonAvatarsCombobox.Text);
        if (commonAvatar == null)
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("削除する共通素体が見つかりませんでした。", _mainForm.CurrentLanguage),
                LanguageUtils.Translate("エラー", _mainForm.CurrentLanguage),
                true
            );
            return;
        }

        var result = FormUtils.ShowConfirmDialog(
            LanguageUtils.Translate("本当に削除しますか？", _mainForm.CurrentLanguage),
            LanguageUtils.Translate("確認", _mainForm.CurrentLanguage)
        );
        if (result) return;

        _commonAvatars.RemoveAll(_commonAvatar => _commonAvatar.Name == commonAvatar.Name);
        _mainForm.CommonAvatars = _commonAvatars;

        FormUtils.ShowMessageBox(
            LanguageUtils.Translate("削除が完了しました。", _mainForm.CurrentLanguage),
            LanguageUtils.Translate("完了", _mainForm.CurrentLanguage)
        );
        RefleshCommonAvatarButtonColor();
    }

    private void CommonAvatarsCombobox_TextChanged(object sender, EventArgs e) =>
        RefleshCommonAvatarButtonColor();

    private void AddButton_Click(object o, EventArgs e)
    {
        var name = CommonAvatarsCombobox.Text;
        if (string.IsNullOrWhiteSpace(name))
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("追加、編集する共通素体を選択してください。", _mainForm.CurrentLanguage),
                LanguageUtils.Translate("エラー", _mainForm.CurrentLanguage),
                true
            );
            return;
        }

        var commonAvatar = GetCommonAvatar(name);

        if (commonAvatar == null)
        {
            _commonAvatars.Add(new CommonAvatar
            {
                Name = name,
                Avatars = AvatarList.Controls.OfType<Button>()
                    .Where(button => button.BackColor == Color.LightGreen)
                    .Select(button => button.Tag?.ToString() ?? string.Empty)
                    .Where(tag => !string.IsNullOrWhiteSpace(tag))
                    .ToList()
            });

            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("共通素体名: ", _mainForm.CurrentLanguage) + name + "\n\n" + LanguageUtils.Translate("共通素体データの追加が完了しました。", _mainForm.CurrentLanguage),
                LanguageUtils.Translate("完了", _mainForm.CurrentLanguage)
            );
            CommonAvatarsCombobox.Items.Add(name);
        }
        else
        {
            commonAvatar.Avatars = AvatarList.Controls.OfType<Button>()
                .Where(button => button.BackColor == Color.LightGreen)
                .Select(button => button.Tag?.ToString() ?? string.Empty)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .ToList();

            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("共通素体名: ", _mainForm.CurrentLanguage) + name + "\n\n" + LanguageUtils.Translate("共通素体データの更新が完了しました。", _mainForm.CurrentLanguage),
                LanguageUtils.Translate("完了", _mainForm.CurrentLanguage)
            );
        }

        _mainForm.CommonAvatars = _commonAvatars;
        RefleshCommonAvatarButtonColor();
    }
    #endregion

    #region 処理関数
    /// <summary>
    /// 共通素体グループ名から共通素体のリストを取得します。
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private CommonAvatar? GetCommonAvatar(string? name) 
        => string.IsNullOrWhiteSpace(name) ? null : _commonAvatars.FirstOrDefault(commonAvatar => commonAvatar.Name == name);

    /// <summary>
    /// 共通素体のボタンの色を現在の共通素体グループから更新します。
    /// </summary>
    private void RefleshCommonAvatarButtonColor()
    {
        foreach (Button button in AvatarList.Controls)
        {
            var commonAvatar = GetCommonAvatar(CommonAvatarsCombobox.Text);
            NewLabel.Visible = commonAvatar is null && !string.IsNullOrWhiteSpace(CommonAvatarsCombobox.Text);
            AddButton.Text = commonAvatar is null ? LanguageUtils.Translate("追加", _mainForm.CurrentLanguage) : LanguageUtils.Translate("更新", _mainForm.CurrentLanguage);
            AddButton.Enabled = !string.IsNullOrWhiteSpace(CommonAvatarsCombobox.Text);
            DeleteSelectedGroupButton.Enabled = !string.IsNullOrWhiteSpace(CommonAvatarsCombobox.Text) && commonAvatar != null;
            button.BackColor = commonAvatar != null
                ? commonAvatar.Avatars.Contains(button.Tag?.ToString() ?? string.Empty)
                    ? Color.LightGreen
                    : Color.FromKnownColor(KnownColor.Control)
                : Color.FromKnownColor(KnownColor.Control);
        }
    }
    #endregion
}
