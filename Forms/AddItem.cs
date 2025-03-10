using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    public sealed partial class AddItem : Form
    {
        /// <summary>
        /// メインフォームを取得または設定します。
        /// </summary>
        private readonly Main _mainForm;

        /// <summary>
        /// 編集モードで開かれているかどうかを取得または設定します。
        /// </summary>
        private readonly bool _edit;

        /// <summary>
        /// HTTPクライアントを取得または設定します。
        /// </summary>
        private static readonly HttpClient HttpClient = new();

        /// <summary>
        /// 追加ボタンが有効になれるかどうかを取得または設定します。
        /// </summary>
        private bool _addButtonEnabled;

        /// <summary>
        /// アイテムが追加されたときに発生するイベントです。
        /// </summary>
        public event EventHandler? ItemAdded;

        /// <summary>
        /// メインフォームに反映される予定のアイテムファイルです。
        /// </summary>
        public Item Item = new();

        /// <summary>
        /// 対応しているアバターのリストを取得または設定します。
        /// </summary>
        public string[] SupportedAvatar = Array.Empty<string>();

        /// <summary>
        /// アイテムを追加または編集するフォームを初期化します。
        /// </summary>
        /// <param name="mainForm"></param>
        /// <param name="type"></param>
        /// <param name="customCategory"></param>
        /// <param name="edit"></param>
        /// <param name="item"></param>
        /// <param name="folderPath"></param>
        /// <param name="boothId"></param>
        public AddItem(Main mainForm, ItemType type, string? customCategory, bool edit, Item? item, string? folderPath, string boothId = "")
        {
            _edit = edit;
            _mainForm = mainForm;
            InitializeComponent();

            ValidCheck();

            if (_mainForm.CurrentLanguage != "ja-JP")
            {
                foreach (Control control in Controls)
                {
                    if (!string.IsNullOrEmpty(control.Text))
                    {
                        control.Text = Helper.Translate(control.Text, _mainForm.CurrentLanguage);
                    }
                }

                for (var i = 0; i < TypeComboBox.Items.Count; i++)
                {
                    var text = TypeComboBox.Items[i]?.ToString();
                    if (text == null) continue;
                    TypeComboBox.Items[i] = Helper.Translate(text, _mainForm.CurrentLanguage);
                }
            }

            for (var i = 0; i < mainForm.CustomCategories.Length; i++)
            {
                TypeComboBox.Items.Add(mainForm.CustomCategories[i]);
            }

            if (folderPath != null) FolderTextBox.Text = folderPath;
            if (boothId != "") BoothURLTextBox.Text = "https://booth.pm/ja/items/" + boothId;

            ItemType itemType = type;

            if (type == ItemType.Custom)
            {
                if (!string.IsNullOrEmpty(customCategory))
                {
                    var typeIndex = TypeComboBox.Items.IndexOf(customCategory);
                    TypeComboBox.SelectedIndex = typeIndex == -1 ? 0 : typeIndex;
                }
                else
                {
                    TypeComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                TypeComboBox.SelectedIndex = (int)type == 10 ? 0 : (int)type;
            }

            Text = Helper.Translate("アイテムの追加", _mainForm.CurrentLanguage);

            if (!(edit && item != null)) return;
            Item = item;
            Text = Helper.Translate("アイテムの編集", _mainForm.CurrentLanguage);
            label3.Text = Helper.Translate("アイテムの編集", _mainForm.CurrentLanguage);
            AddButton.Text = Helper.Translate("編集", _mainForm.CurrentLanguage);

            AddButton.Enabled = true;
            TitleTextBox.Enabled = true;
            AuthorTextBox.Enabled = true;
            CustomButton.Enabled = false;
            _addButtonEnabled = true;

            BoothURLTextBox.Text = item.BoothId != -1 ? $"https://booth.pm/ja/items/{item.BoothId}" : "";
            FolderTextBox.Text = item.ItemPath;
            MaterialTextBox.Text = item.MaterialPath;
            FolderTextBox.Enabled = false;
            openFolderButton.Enabled = false;

            if (item.Type == ItemType.Custom)
            {
                var typeIndex = TypeComboBox.Items.IndexOf(item.CustomCategory);
                TypeComboBox.SelectedIndex = typeIndex == -1 ? 0 : typeIndex;
            }
            else
            {
                TypeComboBox.SelectedIndex = (int)item.Type;
            }

            SupportedAvatar = item.SupportedAvatar;
            TitleTextBox.Text = item.Title;
            AuthorTextBox.Text = item.AuthorName;
            SelectAvatar.Text = Helper.Translate("選択中: ", _mainForm.CurrentLanguage) + SupportedAvatar.Length +
                                Helper.Translate("個", _mainForm.CurrentLanguage);

            if (Directory.Exists(FolderTextBox.Text)) return;
            FolderTextBox.Enabled = true;
            openFolderButton.Enabled = true;
        }

        /// <summary>
        /// カスタムで追加するボタンがクリックされたときの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomButton_Click(object sender, EventArgs e)
        {
            BoothURLTextBox.Text = "";
            TitleTextBox.Text = "";
            AuthorTextBox.Text = "";
            TitleTextBox.Enabled = true;
            AuthorTextBox.Enabled = true;
            _addButtonEnabled = true;
        }

        /// <summary>
        /// Boothのアイテム情報を取得するボタンがクリックされたときの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetButton_Click(object sender, EventArgs e)
        {
            var boothId = BoothURLTextBox.Text.Split('/').Last();
            if (!int.TryParse(boothId, out _))
            {
                MessageBox.Show(Helper.Translate("Booth URLが正しくありません", _mainForm.CurrentLanguage),
                    Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                GetButton.Enabled = false;
                GetButton.Text = Helper.Translate("取得中...", _mainForm.CurrentLanguage);
                Item = await Helper.GetBoothItemInfoAsync(boothId);
                GetButton.Text = Helper.Translate("情報を取得", _mainForm.CurrentLanguage);
                GetButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Helper.Translate("Boothのアイテム情報を取得できませんでした", _mainForm.CurrentLanguage) + "\n" + ex.Message,
                    Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                TitleTextBox.Enabled = true;
                AuthorTextBox.Enabled = true;
                GetButton.Enabled = true;
                GetButton.Text = Helper.Translate("情報を取得", _mainForm.CurrentLanguage);
                Item = new Item();
            }

            Item.BoothId = int.Parse(boothId);

            AddButton.Enabled = true;
            TitleTextBox.Text = Item.Title;
            AuthorTextBox.Text = Item.AuthorName;
            if (Item.Type != ItemType.Unknown) TypeComboBox.SelectedIndex = (int)Item.Type;
            TitleTextBox.Enabled = true;
            AuthorTextBox.Enabled = true;

            _addButtonEnabled = true;
        }

        /// <summary>
        /// 対応アバターを選択するボタンがクリックされたときの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAvatar_Click(object sender, EventArgs e)
        {
            SelectSupportedAvatar selectSupportedAvatar = new(_mainForm, this);
            selectSupportedAvatar.ShowDialog();
            SelectAvatar.Text = Helper.Translate("選択中: ", _mainForm.CurrentLanguage) + SupportedAvatar.Length +
                                Helper.Translate("個", _mainForm.CurrentLanguage);
        }

        /// <summary>
        /// アイテムのタイプのコンボボックスが変更されたときの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectAvatar.Enabled = TypeComboBox.SelectedIndex != (int)ItemType.Avatar;
        }

        /// <summary>
        /// アイテムを追加または編集します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddButton_Click(object sender, EventArgs e)
        {
            ItemType type;
            if (TypeComboBox.SelectedIndex >= 9)
            {
                type = ItemType.Custom;
            }
            else
            {
                type = (ItemType)TypeComboBox.SelectedIndex;
            }

            AddButton.Enabled = false;
            Item.Title = TitleTextBox.Text;
            Item.AuthorName = AuthorTextBox.Text;
            Item.Type = type;
            if (type == ItemType.Custom)
            {
                Item.CustomCategory = TypeComboBox.Text;
            }

            var folderPath = ExtractZipWithHandling(FolderTextBox.Text, Path.Combine("Datas", "Items", Item.Title));
            if (folderPath == null) return;
            Item.ItemPath = folderPath;

            var materialPath = ExtractZipWithHandling(MaterialTextBox.Text, Path.Combine(folderPath, "Materials"));
            if (materialPath == null) return;
            Item.MaterialPath = materialPath;

            if (Item.Type != ItemType.Avatar) Item.SupportedAvatar = SupportedAvatar;

            if (string.IsNullOrEmpty(Item.Title) || string.IsNullOrEmpty(Item.AuthorName) || string.IsNullOrEmpty(Item.ItemPath))
            {
                MessageBox.Show(Helper.Translate("タイトル、作者、フォルダパスのどれかが入力されていません", _mainForm.CurrentLanguage),
                    Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                AddButton.Enabled = true;
                return;
            }


            if (Item.BoothId != -1)
            {
                var thumbnailFolderPath = Path.Combine("Datas", "Thumbnail");
                if (!Directory.Exists(thumbnailFolderPath))
                {
                    Directory.CreateDirectory(thumbnailFolderPath);
                }

                var thumbnailPath = Path.Combine(thumbnailFolderPath, $"{Item.BoothId}.png");
                if (!File.Exists(thumbnailPath))
                {
                    if (!string.IsNullOrEmpty(Item.ThumbnailUrl))
                    {
                        try
                        {
                            var thumbnailData = await HttpClient.GetByteArrayAsync(Item.ThumbnailUrl);
                            await File.WriteAllBytesAsync(thumbnailPath, thumbnailData);
                            Item.ImagePath = thumbnailPath;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                Helper.Translate("サムネイルのダウンロードに失敗しました。詳細はErrorLog.txtをご覧ください。", _mainForm.CurrentLanguage),
                                Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            Helper.ErrorLogger("サムネイルのダウンロードに失敗しました。", ex);
                        }
                    }
                }
                else
                {
                    Item.ImagePath = thumbnailPath;
                }
            }

            if (!string.IsNullOrEmpty(Item.AuthorId))
            {
                var authorImageFolderPath = Path.Combine("Datas", "AuthorImage");
                if (!Directory.Exists(authorImageFolderPath))
                {
                    Directory.CreateDirectory(authorImageFolderPath);
                }

                var authorImagePath = Path.Combine(authorImageFolderPath, $"{Item.AuthorId}.png");
                if (!File.Exists(authorImagePath))
                {
                    if (!string.IsNullOrEmpty(Item.AuthorImageUrl))
                    {
                        try
                        {
                            var authorImageData = await HttpClient.GetByteArrayAsync(Item.AuthorImageUrl);
                            await File.WriteAllBytesAsync(authorImagePath, authorImageData);
                            Item.AuthorImageFilePath = authorImagePath;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                Helper.Translate("作者の画像のダウンロードに失敗しました。詳細はErrorLog.txtをご覧ください。", _mainForm.CurrentLanguage),
                                Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            Helper.ErrorLogger("作者の画像のダウンロードに失敗しました。", ex);
                        }
                    }
                }
                else
                {
                    Item.AuthorImageFilePath = authorImagePath;
                }
            }

            // 日付更新
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (_edit)
            {
                Item.UpdatedDate = now;
            }
            else
            {
                Item.CreatedDate = now;
                Item.UpdatedDate = now;
            }

            if (_edit)
            {
                // 同じパスのものを削除してから追加
                MessageBox.Show(
                    Helper.Translate("Boothのアイテムを編集しました!", _mainForm.CurrentLanguage) + "\n" +
                    Helper.Translate("アイテム名: ", _mainForm.CurrentLanguage) + Item.Title + "\n" +
                    Helper.Translate("作者: ", _mainForm.CurrentLanguage) + Item.AuthorName,
                    Helper.Translate("編集完了", _mainForm.CurrentLanguage),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                _mainForm.Items = _mainForm.Items.Where(i => i.ItemPath != Item.ItemPath).ToArray();
                _mainForm.Items = _mainForm.Items.Append(Item).ToArray();
            }
            else
            {
                MessageBox.Show(
                    Helper.Translate("Boothのアイテムを追加しました!", _mainForm.CurrentLanguage) + "\n" +
                    Helper.Translate("アイテム名: ", _mainForm.CurrentLanguage) + Item.Title + "\n" +
                    Helper.Translate("作者: ", _mainForm.CurrentLanguage) + Item.AuthorName,
                    Helper.Translate("追加完了", _mainForm.CurrentLanguage),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                _mainForm.Items = _mainForm.Items.Append(Item).ToArray();
            }

            Close();
        }

        /// <summary>
        /// zipファイルを指定されたフォルダに展開します。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        private string? ExtractZipWithHandling(string path, string destination)
        {
            if (!string.IsNullOrEmpty(path) && path.EndsWith(".zip"))
            {
                try
                {
                    return Helper.ExtractZip(path, destination);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        Helper.Translate("zipファイルの展開に失敗しました。詳細はErrorLog.txtをご覧ください。", _mainForm.CurrentLanguage),
                        Helper.Translate("エラー", _mainForm.CurrentLanguage),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    Helper.ErrorLogger("zipファイルの展開に失敗しました。", ex);
                    AddButton.Enabled = true;
                    return null;
                }
            }

            return path;
        }

        /// <summary>
        /// アイテムフォルダを開くボタンがクリックされたときの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFolderButton_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                FolderTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        /// <summary>
        /// マテリアルフォルダを開くボタンがクリックされたときの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openMaterialFolderButton_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                MaterialTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        /// <summary>
        /// アイテムフォルダ欄にドラッグされたときの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderTextBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? dragFilePathArr = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (dragFilePathArr == null) return;
            var folderPath = dragFilePathArr[0];

            if (!(File.Exists(folderPath) && folderPath.EndsWith(".zip")) && !Directory.Exists(folderPath))
            {
                MessageBox.Show(Helper.Translate("フォルダを選択してください", _mainForm.CurrentLanguage),
                    Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            FolderTextBox.Text = folderPath;
        }

        /// <summary>
        /// マテリアルフォルダ欄にドラッグされたときの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaterialTextBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? dragFilePathArr = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);
            if (dragFilePathArr == null) return;
            var folderPath = dragFilePathArr[0];

            if (!(File.Exists(folderPath) && folderPath.EndsWith(".zip")) && !Directory.Exists(folderPath))
            {
                MessageBox.Show(Helper.Translate("フォルダを選択してください", _mainForm.CurrentLanguage),
                    Helper.Translate("エラー", _mainForm.CurrentLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MaterialTextBox.Text = folderPath;
        }

        /// <summary>
        /// エラー状態を設定します。
        /// </summary>
        /// <param name="errorMessage"></param>
        private void SetErrorState(string errorMessage)
        {
            AddButton.Enabled = false;
            ErrorLabel.Text = errorMessage;
        }

        /// <summary>
        /// エラー状態を解除します。
        /// </summary>
        private void ClearErrorState()
        {
            if (_addButtonEnabled) AddButton.Enabled = true;
            ErrorLabel.Text = "";
        }

        /// <summary>
        /// フォルダパスのテキストボックスが変更されたときの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckText(object sender, EventArgs e) => ValidCheck();

        /// <summary>
        /// パスなどのテキストボックス内のテキストが有効かどうかをチェックします。
        /// </summary>
        private void ValidCheck()
        {
            if (!(File.Exists(FolderTextBox.Text) && FolderTextBox.Text.EndsWith(".zip")) && !Directory.Exists(FolderTextBox.Text))
            {
                SetErrorState(Helper.Translate("エラー: フォルダパスが存在しません", _mainForm.CurrentLanguage));
                return;
            }

            if (File.Exists(FolderTextBox.Text) && !FolderTextBox.Text.EndsWith(".zip"))
            {
                SetErrorState(Helper.Translate("エラー: フォルダパスがファイルです", _mainForm.CurrentLanguage));
                return;
            }

            if (string.IsNullOrEmpty(FolderTextBox.Text))
            {
                SetErrorState(Helper.Translate("エラー: フォルダパスが入力されていません", _mainForm.CurrentLanguage));
                return;
            }

            if (_mainForm.Items.Any(i => i.ItemPath == FolderTextBox.Text) && !_edit)
            {
                SetErrorState(Helper.Translate("エラー: 同じパスのアイテムが既に存在します", _mainForm.CurrentLanguage));
                return;
            }

            if (!string.IsNullOrEmpty(MaterialTextBox.Text) && (!(File.Exists(MaterialTextBox.Text) && MaterialTextBox.Text.EndsWith(".zip")) && !Directory.Exists(MaterialTextBox.Text)))
            {
                SetErrorState(Helper.Translate("エラー: マテリアルフォルダパスが存在しません", _mainForm.CurrentLanguage));
                return;
            }

            if (!string.IsNullOrEmpty(MaterialTextBox.Text) && File.Exists(MaterialTextBox.Text) && !MaterialTextBox.Text.EndsWith(".zip"))
            {
                SetErrorState(Helper.Translate("エラー: マテリアルフォルダパスがファイルです", _mainForm.CurrentLanguage));
                return;
            }

            if (string.IsNullOrEmpty(TitleTextBox.Text))
            {
                SetErrorState(Helper.Translate("エラー: タイトルが入力されていません", _mainForm.CurrentLanguage));
                return;
            }

            if (TitleTextBox.Text == "*")
            {
                SetErrorState(Helper.Translate("エラー: タイトルを*にすることはできません", _mainForm.CurrentLanguage));
                return;
            }

            if (string.IsNullOrEmpty(AuthorTextBox.Text))
            {
                SetErrorState(Helper.Translate("エラー: 作者が入力されていません", _mainForm.CurrentLanguage));
                return;
            }

            ClearErrorState();
        }

        /// <summary>
        /// フォームが閉じられるときの処理です。イベントを発火します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddItem_FormClosing(object sender, FormClosingEventArgs e) => ItemAdded?.Invoke(this, EventArgs.Empty);
    }
}
