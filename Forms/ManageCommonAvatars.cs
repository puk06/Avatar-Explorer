using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public sealed partial class ManageCommonAvatars : Form
    {
        /// <summary>
        /// メインフォームを取得または設定します。
        /// </summary>
        private readonly Main _mainForm;

        /// <summary>
        /// ファイルアイコンのイメージを取得します。
        /// </summary>
        private static readonly Image FileImage = Image.FromStream(new MemoryStream(Properties.Resources.FileIcon));

        /// <summary>
        /// 共通素体のリストを取得または設定します。
        /// </summary>
        private CommonAvatar[] _commonAvatars;

        /// <summary>
        /// 共通素体の管理フォームを初期化します。
        /// </summary>
        /// <param name="mainform"></param>
        public ManageCommonAvatars(Main mainform)
        {
            _mainForm = mainform;
            _commonAvatars = _mainForm.CommonAvatars;
            InitializeComponent();

            Text = Helper.Translate("共通素体の管理", _mainForm.CurrentLanguage);

            if (_mainForm.CurrentLanguage != "ja-JP")
            {
                foreach (Control control in Controls)
                {
                    if (!string.IsNullOrEmpty(control.Text))
                    {
                        control.Text = Helper.Translate(control.Text, _mainForm.CurrentLanguage);
                    }
                }

                AvatarList.Text = Helper.Translate(AvatarList.Text, _mainForm.CurrentLanguage);
            }

            foreach (var commonAvatar in _commonAvatars)
            {
                CommonAvatarsCombobox.Items.Add(commonAvatar.Name);
            }

            if (CommonAvatarsCombobox.Items.Count > 0)
            {
                CommonAvatarsCombobox.SelectedIndex = 0;
            }

            GenerateAvatarList();
            RefleshCommonAvatarButtonColor();
        }

        /// <summary>
        /// フォーム内のアバターリストを生成します。
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
            foreach (Item item in _mainForm.Items.Where(item => item.Type == ItemType.Avatar))
            {
                Button button = CreateAvatarButton(item, _mainForm.CurrentLanguage);
                button.Text = item.Title;
                button.Location = new Point(0, (70 * index) + 2);
                button.Tag = item.ItemPath;

                var commonAvatar = GetCommonAvatar(CommonAvatarsCombobox.Text);
                button.BackColor = commonAvatar != null
                    ? commonAvatar.Avatars.Contains(item.ItemPath)
                        ? Color.LightGreen
                        : Color.FromKnownColor(KnownColor.Control)
                    : Color.FromKnownColor(KnownColor.Control);

                AvatarList.Controls.Add(button);
                index++;
            }

            AvatarList.ResumeLayout();
            AvatarList.AutoScroll = true;

            Helper.UpdateExplorerThumbnails(AvatarList);
        }

        /// <summary>
        /// アバターのボタンを生成します。
        /// </summary>
        /// <param name="item"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        private static CustomItemButton CreateAvatarButton(Item item, string language)
        {
            CustomItemButton button = new CustomItemButton(875);
            button.ImagePath = item.ImagePath;
            button.TitleText = item.Title;
            button.AuthorName = Helper.Translate("作者: ", language) + item.AuthorName;
            button.ToolTipText = item.Title;

            button.Click += (_, _) =>
            {
                button.BackColor = button.BackColor == Color.LightGreen
                    ? Color.FromKnownColor(KnownColor.Control)
                    : Color.LightGreen;
            };

            return button;
        }

        /// <summary>
        /// 共通素体グループ名から共通素体のリストを取得します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private CommonAvatar? GetCommonAvatar(string? name) => string.IsNullOrWhiteSpace(name)
            ? null
            : _commonAvatars.FirstOrDefault(commonAvatar => commonAvatar.Name == name);

        /// <summary>
        /// 共通素体コンボボックスのテキストが変更されたときに呼び出されます。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommonAvatarsCombobox_TextChanged(object sender, EventArgs e) =>
            RefleshCommonAvatarButtonColor();

        /// <summary>
        /// 共通素体のボタンの色を現在の共通素体グループから更新します。
        /// </summary>
        private void RefleshCommonAvatarButtonColor()
        {

            foreach (Button button in AvatarList.Controls)
            {
                var commonAvatar = GetCommonAvatar(CommonAvatarsCombobox.Text);
                NewLabel.Visible = commonAvatar is null && !string.IsNullOrWhiteSpace(CommonAvatarsCombobox.Text);
                AddButton.Text = commonAvatar is null ? Helper.Translate("追加", _mainForm.CurrentLanguage) : Helper.Translate("更新", _mainForm.CurrentLanguage);
                AddButton.Enabled = !string.IsNullOrWhiteSpace(CommonAvatarsCombobox.Text);
                DeleteSelectedGroupButton.Enabled = !string.IsNullOrWhiteSpace(CommonAvatarsCombobox.Text) && commonAvatar != null;
                button.BackColor = commonAvatar != null
                    ? commonAvatar.Avatars.Contains(button.Tag?.ToString())
                        ? Color.LightGreen
                        : Color.FromKnownColor(KnownColor.Control)
                    : Color.FromKnownColor(KnownColor.Control);
            }
        }

        /// <summary>
        /// 共通素体グループを削除します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteSelectedGroupButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CommonAvatarsCombobox.Text))
            {
                MessageBox.Show(Helper.Translate("削除する共通素体を選択してください。", _mainForm.CurrentLanguage),
                    Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var commonAvatar = GetCommonAvatar(CommonAvatarsCombobox.Text);
            if (commonAvatar == null)
            {
                MessageBox.Show(Helper.Translate("削除する共通素体が見つかりませんでした。", _mainForm.CurrentLanguage),
                    Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show(Helper.Translate("本当に削除しますか？", _mainForm.CurrentLanguage),
                Helper.Translate("確認", _mainForm.CurrentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            _commonAvatars = _commonAvatars.Where(ca => ca.Name != commonAvatar.Name).ToArray();
            _mainForm.CommonAvatars = _commonAvatars;
            MessageBox.Show(Helper.Translate("削除が完了しました。", _mainForm.CurrentLanguage),
                Helper.Translate("完了", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefleshCommonAvatarButtonColor();
        }

        /// <summary>
        /// 共通素体グループを追加するボタンが押されたときに呼び出されます。
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object o, EventArgs e)
        {
            var name = CommonAvatarsCombobox.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show(Helper.Translate("追加、編集する共通素体を選択してください。", _mainForm.CurrentLanguage),
                    Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var commonAvatar = GetCommonAvatar(name);

            if (commonAvatar == null)
            {
#pragma warning disable CS8619 // 値における参照型の Null 許容性が、対象の型と一致しません。
                _commonAvatars = _commonAvatars.Append(new CommonAvatar
                {
                    Name = name,
                    Avatars = AvatarList.Controls.OfType<Button>()
                        .Where(button => button.BackColor == Color.LightGreen)
                        .Select(button => button.Tag as string)
                        .Where(tag => !string.IsNullOrWhiteSpace(tag))
                        .ToArray()
                }).ToArray();
#pragma warning restore CS8619 // 値における参照型の Null 許容性が、対象の型と一致しません。

                MessageBox.Show(Helper.Translate("共通素体名: ", _mainForm.CurrentLanguage) + name + "\n\n" + Helper.Translate("共通素体データの追加が完了しました。", _mainForm.CurrentLanguage),
                    Helper.Translate("完了", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
                CommonAvatarsCombobox.Items.Add(name);
            }
            else
            {
#pragma warning disable CS8619 // 値における参照型の Null 許容性が、対象の型と一致しません。
                commonAvatar.Avatars = AvatarList.Controls.OfType<Button>()
                    .Where(button => button.BackColor == Color.LightGreen)
                    .Select(button => button.Tag as string)
                    .Where(tag => !string.IsNullOrWhiteSpace(tag))
                    .ToArray();
#pragma warning restore CS8619 // 値における参照型の Null 許容性が、対象の型と一致しません。

                MessageBox.Show(Helper.Translate("共通素体名: ", _mainForm.CurrentLanguage) + name + "\n\n" + Helper.Translate("共通素体データの更新が完了しました。", _mainForm.CurrentLanguage),
                    Helper.Translate("完了", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            _mainForm.CommonAvatars = _commonAvatars;
            RefleshCommonAvatarButtonColor();
        }
    }
}
