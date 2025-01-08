using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public sealed partial class SelectSupportedAvatar : Form
    {
        private readonly Main _mainForm;
        private readonly AddItem _addItem;
        private static readonly Image FileImage = SharedImages.GetImage(SharedImages.Images.FileIcon);

        public SelectSupportedAvatar(Main mainForm, AddItem addItem)
        {
            _mainForm = mainForm;
            _addItem = addItem;
            InitializeComponent();

            Text = Helper.Translate("対応アバターの選択", _mainForm.CurrentLanguage);

            if (_mainForm.CurrentLanguage != "ja-JP")
            {
                foreach (Control control in Controls)
                {
                    if (control.Text != "")
                    {
                        control.Text = Helper.Translate(control.Text, _mainForm.CurrentLanguage);
                    }
                }

                AvatarList.Text = Helper.Translate(AvatarList.Text, _mainForm.CurrentLanguage);
            }

            GenerateAvatarList();
        }

        private void GenerateAvatarList()
        {
            AvatarList.Controls.Clear();

            var items = _mainForm.Items.Where(item => item.Type == ItemType.Avatar).ToList();
            if (items.Count == 0) return;
            items = items.OrderBy(item => item.Title).ToList();

            var index = 0;
            foreach (Item item in items)
            {
                if (item.ItemPath == _addItem.Item.ItemPath) continue;
                Button button = CreateAvatarButton(item, _mainForm.CurrentLanguage);
                button.Location = new Point(0, (70 * index) + 2);
                button.BackColor = _addItem.SupportedAvatar.Contains(item.ItemPath) ? Color.LightGreen : Color.FromKnownColor(KnownColor.Control);
                AvatarList.Controls.Add(button);
                index++;
            }
        }

        private static Button CreateAvatarButton(Item item, string language)
        {
            CustomItemButton button = new CustomItemButton(1009);
            button.ImagePath = item.ImagePath;
            button.Picture = File.Exists(item.ImagePath) ? Image.FromFile(item.ImagePath) : FileImage;
            button.TitleText = item.Title;
            button.AuthorName = Helper.Translate("作者: ", language) + item.AuthorName;
            button.ToolTipText = item.Title;
            button.Tag = item.ItemPath;

            button.Click += (_, _) =>
            {
                button.BackColor = button.BackColor == Color.LightGreen ? Color.FromKnownColor(KnownColor.Control) : Color.LightGreen;
            };

            return button;
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            var selectedItems = AvatarList.Controls.OfType<Button>()
                .Where(button => button.BackColor == Color.LightGreen)
                .Select(button => button.Tag as string)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .ToArray();
            _addItem.SupportedAvatar = selectedItems;
            Close();
        }
    }
}
