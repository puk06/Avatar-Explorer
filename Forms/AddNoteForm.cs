using Avatar_Explorer.Models;
using Avatar_Explorer.Utils;

namespace Avatar_Explorer.Forms
{
    public partial class AddNoteForm : Form
    {
        /// <summary>
        /// アイテムのメモを取得または設定します。
        /// </summary>
        public string Memo { get; set; } = "";

        /// <summary>
        /// メインフォームを取得または設定します。
        /// </summary>
        private readonly MainForm _mainForm;

        /// <summary>
        /// アイテムのメモを追加または編集するフォームを初期化します。
        /// </summary>
        /// <param name="MainForm"></param>
        /// <param name="item"></param>
        public AddNoteForm(MainForm MainForm, Item item)
        {
            _mainForm = MainForm;
            InitializeComponent();

            Text = LanguageUtils.Translate(Text, _mainForm.CurrentLanguage);
            Text += " - " + item.Title;

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

            MemoTextBox.Text = item.ItemMemo;
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
}
