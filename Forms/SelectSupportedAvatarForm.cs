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

        Text = LanguageUtils.Translate("対応アバターの選択", _mainForm.CurrentLanguage);
        TranslateControls();

        GenerateAvatarList();
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
            Button button = CreateAvatarButton(_mainForm.ButtonSize, item, _mainForm.CurrentLanguage);
            button.Location = new Point(0, ((_mainForm.ButtonSize + 6) * index) + 2);
            button.BackColor = _addItem.SupportedAvatar.Contains(item.ItemPath) ? Color.LightGreen : Color.FromKnownColor(KnownColor.Control);
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
    /// <param name="buttonHeight"></param>
    /// <param name="item"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    private static CustomItemButton CreateAvatarButton(int buttonHeight, Item item, string language)
    {
        CustomItemButton button = new(992, buttonHeight)
        {
            ImagePath = item.ImagePath,
            TitleText = item.Title,
            AuthorName = LanguageUtils.Translate("作者: ", language) + item.AuthorName,
            ToolTipText = item.Title,
            Tag = item.ItemPath
        };

        button.Click += (_, _) =>
        {
            button.BackColor = button.BackColor == Color.LightGreen ? Color.FromKnownColor(KnownColor.Control) : Color.LightGreen;
        };

        return button;
    }
    #endregion

    #region イベントハンドラ
    private void ConfirmButton_Click(object sender, EventArgs e)
    {
        _addItem.SupportedAvatar = AvatarList.Controls.OfType<Button>()
            .Where(button => button.BackColor == Color.LightGreen)
            .Select(button => button.Tag?.ToString() ?? string.Empty)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .ToList();
        Close();
    }
    #endregion
}
