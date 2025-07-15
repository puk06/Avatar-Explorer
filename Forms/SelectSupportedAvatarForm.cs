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

    private void TranslateControls()
    {
        if (_mainForm.CurrentLanguage != "ja-JP")
        {
            foreach (Control control in Controls)
            {
                if (control.Text != string.Empty)
                {
                    control.Text = LanguageUtils.Translate(control.Text, _mainForm.CurrentLanguage);
                }
            }

            AvatarList.Text = LanguageUtils.Translate(AvatarList.Text, _mainForm.CurrentLanguage);
        }
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
            if (item.ItemPath == _addItem.GetEditItem.ItemPath) continue;
            Button button = CreateAvatarButton(item, _mainForm.CurrentLanguage);
            button.Location = new Point(0, (70 * index) + 2);
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
    /// <param name="item"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    private static CustomItemButton CreateAvatarButton(Item item, string language)
    {
        CustomItemButton button = new CustomItemButton(992);
        button.ImagePath = item.ImagePath;
        button.TitleText = item.Title;
        button.AuthorName = LanguageUtils.Translate("作者: ", language) + item.AuthorName;
        button.ToolTipText = item.Title;
        button.Tag = item.ItemPath;

        button.Click += (_, _) =>
        {
            button.BackColor = button.BackColor == Color.LightGreen ? Color.FromKnownColor(KnownColor.Control) : Color.LightGreen;
        };

        return button;
    }

    /// <summary>
    /// 選択を確定し、フォームを閉じます。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ConfirmButton_Click(object sender, EventArgs e)
    {
#pragma warning disable CS8619 // 値における参照型の Null 許容性が、対象の型と一致しません。
        _addItem.SupportedAvatar = AvatarList.Controls.OfType<Button>()
            .Where(button => button.BackColor == Color.LightGreen)
            .Select(button => button.Tag as string)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .ToList();
#pragma warning restore CS8619 // 値における参照型の Null 許容性が、対象の型と一致しません。
        Close();
    }
}
