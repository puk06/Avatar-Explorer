using Avatar_Explorer.Models;
using Avatar_Explorer.Utils;

namespace Avatar_Explorer.Forms;

internal sealed partial class SelectSupportedAvatarForm : Form
{
    /// <summary>
    /// メインフォームを取得または設定します。
    /// </summary>
    private readonly MainForm _mainForm;

    /// <summary>
    /// アイテムを追加するフォームを取得または設定します。
    /// </summary>
    private readonly AddItemForm _addItem;

    /// <summary>
    /// 対応アバターの選択フォームを初期化します。
    /// </summary>
    /// <param name="mainForm"></param>
    /// <param name="addItem"></param>
    internal SelectSupportedAvatarForm(MainForm mainForm, AddItemForm addItem)
    {
        _mainForm = mainForm;
        _addItem = addItem;
        InitializeComponent();
        AdditionalInitialize();
        if (_mainForm.DarkMode) SetDarkMode();

        Text = LanguageUtils.Translate("対応アバターの選択", _mainForm.CurrentLanguage);
        TranslateControls();

        GenerateAvatarList();
    }

    private void AdditionalInitialize()
    {
        AvatarList.MouseWheel += AEUtils.OnScroll;
        AvatarList.Scroll += AEUtils.OnScroll;
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
    /// アバターのリストを生成します。
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
        foreach (Item item in items)
        {
            if (item.ItemPath == _addItem.ItemPath) continue;
            Button button = CreateAvatarButton(_mainForm.DarkMode, _mainForm.ButtonSize, item, _mainForm.CurrentLanguage);
            button.Location = new Point(0, ((_mainForm.ButtonSize + 6) * index) + 2);
            button.BackColor = _addItem.SupportedAvatar.Contains(item.ItemPath) ? DarkModeUtils.GetSelectedButtonColor(_mainForm.DarkMode) : DarkModeUtils.GetNormalButtonColor(_mainForm.DarkMode);
            AvatarList.Controls.Add(button);
            index++;
        }

        AvatarList.ResumeLayout();
        AvatarList.AutoScroll = true;

        AEUtils.UpdateExplorerThumbnails(AvatarList);
    }

    /// <summary>
    /// フォーム内のアバターのボタンを生成します。
    /// </summary>
    /// <param name="darkMode"></param>
    /// <param name="buttonHeight"></param>
    /// <param name="item"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    private CustomItemButton CreateAvatarButton(bool darkMode, int buttonHeight, Item item, string language)
    {
        CustomItemButton button = new(992, buttonHeight, darkMode)
        {
            ImagePath = item.ImagePath,
            TitleText = item.Title,
            AuthorName = LanguageUtils.Translate("作者: ", language) + item.AuthorName,
            ToolTipText = item.Title,
            Tag = item.ItemPath
        };

        button.Click += (_, _) =>
        {
            ActiveControl = null;

            button.BackColor = button.BackColor == DarkModeUtils.GetSelectedButtonColor(darkMode)
                ? DarkModeUtils.GetNormalButtonColor(darkMode)
                : DarkModeUtils.GetSelectedButtonColor(darkMode);
        };

        return button;
    }
    #endregion

    #region イベントハンドラ
    private void ConfirmButton_Click(object sender, EventArgs e)
    {
        _addItem.SupportedAvatar = AvatarList.Controls.OfType<Button>()
            .Where(button => button.BackColor == DarkModeUtils.GetSelectedButtonColor(_mainForm.DarkMode))
            .Select(button => button.Tag?.ToString() ?? string.Empty)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .ToList();
        Close();
    }
    #endregion
}
