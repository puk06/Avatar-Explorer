using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public partial class AddNote : Form
    {
        /// <summary>
        /// アイテムのメモを取得または設定します。
        /// </summary>
        public string Memo { get; set; } = "";

        /// <summary>
        /// メインフォームを取得または設定します。
        /// </summary>
        private readonly Main _mainForm;

        /// <summary>
        /// アイテムのメモを追加または編集するフォームを初期化します。
        /// </summary>
        /// <param name="MainForm"></param>
        /// <param name="item"></param>
        public AddNote(Main MainForm, Item item)
        {
            _mainForm = MainForm;
            InitializeComponent();

            Text = Helper.Translate(Text, _mainForm.CurrentLanguage);
            Text += " - " + item.Title;

            if (_mainForm.CurrentLanguage != "ja-JP")
            {
                foreach (Control control in Controls)
                {
                    if (!string.IsNullOrEmpty(control.Text))
                    {
                        control.Text = Helper.Translate(control.Text, _mainForm.CurrentLanguage);
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
