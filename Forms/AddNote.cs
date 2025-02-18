
using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public partial class AddNote : Form
    {
        public string Memo { get; set; } = "";
        private readonly Main _mainForm;

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

        private void EditButton_Click(object sender, EventArgs e)
        {
            Memo = MemoTextBox.Text;
            Close();
        }
    }
}
