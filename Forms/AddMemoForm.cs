using Avatar_Explorer.Models;
using Avatar_Explorer.Utils;

namespace Avatar_Explorer.Forms;

internal partial class AddMemoForm : Form
{
    /// <summary>
    /// アイテムのメモを取得または設定します。
    /// </summary>
    internal string Memo { get; private set; } = string.Empty;

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
    }
    #endregion

    #region イベントハンドラ
    private void EditButton_Click(object sender, EventArgs e)
    {
        Memo = MemoTextBox.Text;
        Close();
    }
    #endregion
}
