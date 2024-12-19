using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public partial class SelectSupportedAvatar : Form
    {
        private readonly Main _mainForm;
        private readonly AddItem _addItem;

        public SelectSupportedAvatar(Main mainForm, AddItem addItem)
        {
            _mainForm = mainForm;
            _addItem = addItem;
            InitializeComponent();
            GenerateAvatarList();
        }

        private void GenerateAvatarList()
        {
            AvatarList.Controls.Clear();
            var index = 0;
            foreach (Item item in _mainForm.Items.Where(item => item.Type == ItemType.Avatar))
            {
                Button button = CreateAvatarButton(item);
                button.Location = new Point(0, (70 * index) + 3);
                button.BackColor = _addItem.SupportedAvatar.Contains(item.Title) ? Color.LightGreen : Color.FromKnownColor(KnownColor.Control);
                AvatarList.Controls.Add(button);
                index++;
            }
        }

        private static Button CreateAvatarButton(Item item)
        {
            Button button = new Button();
            button.Size = new Size(1009, 64);

            PictureBox pictureBox = new PictureBox();
            pictureBox.Size = new Size(56, 56);
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Image = File.Exists(item.ImagePath) ? Image.FromFile(item.ImagePath) : Image.FromFile("./Datas/FileIcon.png");
            pictureBox.Location = new Point(3, 3);
            button.Controls.Add(pictureBox);

            Label title = new Label();
            title.Text = item.Title;
            title.Location = new Point(60, 3);
            title.AutoSize = true;
            title.Font = new Font("Yu Gothic UI", 12F);
            button.Controls.Add(title);

            Label authorName = new Label();
            authorName.Text = "作者: " + item.AuthorName;
            authorName.Location = new Point(60, 25);
            authorName.Size = new Size(200, 20);

            button.Controls.Add(authorName);

            pictureBox.Click += (_, _) => button.PerformClick();
            title.Click += (_, _) => button.PerformClick();
            authorName.Click += (_, _) => button.PerformClick();

            button.Click += (_, _) =>
            {
                button.BackColor = button.BackColor == Color.LightGreen ? Color.FromKnownColor(KnownColor.Control) : Color.LightGreen;
            };

            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(button, item.Title);

            return button;
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            var selectedItems = AvatarList.Controls.OfType<Button>().Where(button => button.BackColor == Color.LightGreen)
                .Select(button => button.Controls.OfType<Label>().First().Text).ToArray();
            _addItem.SupportedAvatar = selectedItems;
            Close();
        }
    }
}
