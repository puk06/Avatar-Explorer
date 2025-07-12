using Avatar_Explorer.Models;
using Avatar_Explorer.Utils;

namespace Avatar_Explorer.Forms;

internal partial class AddMemoForm : Form
{
    /// <summary>
    /// アイテムのメモを取得または設定します。
    /// </summary>
    internal string Memo { get; set; } = "";

    /// <summary>
    /// メインフォームを取得または設定します。
    /// </summary>
    private readonly MainForm _mainForm;

    /// <summary>
    /// アイテムのメモを追加または編集するフォームを初期化します。
    /// </summary>
    /// <param name="MainForm"></param>
    /// <param name="item"></param>
    internal AddMemoForm(MainForm MainForm, Item item)
    {
        _mainForm = MainForm;
        InitializeComponent();

        Text = LanguageUtils.Translate(Text, _mainForm.CurrentLanguage);
        Text += " - " + item.Title;

        TranslateControls();

        MemoTextBox.Text = item.ItemMemo;
    }

    private void TranslateControls()
    {
        if (_mainForm.CurrentLanguage != "ja-JP")
        {
            foreach (Control control in Controls)
            {
                if (!string.IsNullOrEmpty(control.Text))
                {
                    control.Text = LanguageUtils.Translate(control.Text, _mainForm.CurrentLanguage);
                }
            }
        }
    }

    /// <summary>
    /// メモを追加し、フォームを閉じます。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EditButton_Click(object sender, EventArgs e)
    {
        Memo = MemoTextBox.Text;
        Close();
    }
}
