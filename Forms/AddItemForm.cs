using Avatar_Explorer.Models;
using Avatar_Explorer.Utils;

namespace Avatar_Explorer.Forms;

internal sealed partial class AddItemForm : Form
{
    /// <summary>
    /// アイテムが追加されたときに発生するイベントです。
    /// </summary>
    internal event EventHandler? ItemAdded;

    /// <summary>
    /// メインフォームを取得または設定します。
    /// </summary>
    private readonly MainForm _mainForm;

    /// <summary>
    /// 編集モードで開かれているかどうかを取得します。
    /// </summary>
    private readonly bool _edit;

    /// <summary>
    /// 編集モード時にパスが存在しない状態で画面が開かれたかどうかを取得します。
    /// </summary>
    private readonly bool _directoryNotFound = false;

    /// <summary>
    /// 最後にBoothの情報を取得した時間を取得または設定します。
    /// </summary>
    private DateTime _lastGetTime;

    /// <summary>
    /// メインフォームに反映される予定のアイテムファイルです。
    /// </summary>
    private readonly Item _item = new();

    /// <summary>
    /// 渡されてるアイテムのパスです。
    /// </summary>
    internal string ItemPath => _item.ItemPath;

    /// <summary>
    /// 対応しているアバターのリストを取得または設定します。
    /// </summary>
    internal List<string> SupportedAvatar = new();
    
    private string[] _itemFolderPaths = Array.Empty<string>();

    /// <summary>
    /// アイテムのその他のフォルダのパスを取得または設定します。
    /// </summary>
    private string[] ItemFolderPaths
    {
        get => _itemFolderPaths;
        set
        {
            var validPaths = value
                .Where(path => (File.Exists(path) && path.EndsWith(".zip")) || Directory.Exists(path))
                .ToArray();

            var invalidPaths = value.Except(validPaths).ToArray();

            _itemFolderPaths = validPaths;

            if (invalidPaths.Length > 0)
            {
                ShowInvalidPathsMessage(invalidPaths, value.Length);
            }

            UpdateFolderUI();
        }
    }

    /// <summary>
    /// 不正なパスのメッセージを表示します。
    /// </summary>
    /// <param name="invalidPaths">不正なパスの配列</param>
    /// <param name="totalPaths">全パスの数</param>
    private void ShowInvalidPathsMessage(string[] invalidPaths, int totalPaths)
    {
        var invalidItems = invalidPaths.Select(file => "- " + Path.GetFileName(file)).ToArray();
        FormUtils.ShowMessageBox(
            LanguageUtils.Translate("以下のアイテムは対応していない、もしくは存在しないため追加されません {0}", _mainForm.CurrentLanguage, $"{invalidPaths.Length}/{totalPaths}\n\n") + string.Join("\n", invalidItems),
            LanguageUtils.Translate("エラー", _mainForm.CurrentLanguage),
            true
        );
    }

    /// <summary>
    /// フォルダ関連のUIを更新します。
    /// </summary>
    private void UpdateFolderUI()
    {
        if (ItemFolderPaths.Length > 0)
        {
            FolderTextBox.Text = ItemFolderPaths[0];
            OtherFolderCount.Text = "+" + " " + (ItemFolderPaths.Length - 1) + " " + LanguageUtils.Translate("個", _mainForm.CurrentLanguage);
        }
        else
        {
            OtherFolderCount.Text = "+" + " " + 0 + " " + LanguageUtils.Translate("個", _mainForm.CurrentLanguage);
        }
    }

    /// <summary>
    /// アイテムを追加または編集するフォームを初期化します。
    /// </summary>
    /// <param name="mainForm"></param>
    /// <param name="type"></param>
    /// <param name="customCategory"></param>
    /// <param name="edit"></param>
    /// <param name="item"></param>
    /// <param name="itemFolderPaths"></param>
    /// <param name="boothId"></param>
    internal AddItemForm(MainForm mainForm, ItemType type, string? customCategory, bool edit, Item? item, string[]? itemFolderPaths, string boothId = "")
    {
        _edit = edit;
        _mainForm = mainForm;

        InitializeComponent();
        TranslateControls();
        if (_mainForm.DarkMode) SetDarkMode();

        for (var i = 0; i < mainForm.CustomCategories.Count; i++)
        {
            TypeComboBox.Items.Add(mainForm.CustomCategories[i]);
        }

        SetTypeCombobox(type, customCategory);

        ItemFolderPaths = itemFolderPaths ?? Array.Empty<string>();
        if (boothId != string.Empty) BoothURLTextBox.Text = $"https://booth.pm/ja/items/{boothId}";

        Text = LanguageUtils.Translate("アイテムの追加", _mainForm.CurrentLanguage);

        if (edit && item != null)
        {
            _item = item;

            Text = LanguageUtils.Translate("アイテムの編集", _mainForm.CurrentLanguage);
            label3.Text = LanguageUtils.Translate("アイテムの編集", _mainForm.CurrentLanguage);
            AddButton.Text = LanguageUtils.Translate("編集", _mainForm.CurrentLanguage);

            FolderTextBox.Text = item.ItemPath;
            if (!Directory.Exists(FolderTextBox.Text))
            {
                FolderTextBox.ReadOnly = false;
                openFolderButton.Enabled = true;
                _directoryNotFound = true;
            }
            else
            {
                FolderTextBox.ReadOnly = true;
                openFolderButton.Enabled = false;
            }

            MaterialTextBox.Text = item.MaterialPath;

            BoothURLTextBox.Text = item.BoothId != -1 ? $"https://booth.pm/ja/items/{item.BoothId}" : string.Empty;

            TitleTextBox.Text = item.Title;
            TitleTextBox.Enabled = true;

            AuthorTextBox.Text = item.AuthorName;
            AuthorTextBox.Enabled = true;

            SetTypeCombobox(item.Type, item.CustomCategory);

            SupportedAvatar = item.SupportedAvatar;
            SelectAvatar.Text = LanguageUtils.Translate("選択中: {0}個", _mainForm.CurrentLanguage, SupportedAvatar.Count.ToString());

            CustomButton.Enabled = false;
        }

        ValidCheck();
    }

    private void SetDarkMode()
    {
        foreach (Control contorol in Controls)
        {
            DarkModeUtils.SetDarkMode(contorol);
        }
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

            if (control is TextBox textBox && !string.IsNullOrEmpty(textBox.PlaceholderText))
            {
                textBox.PlaceholderText = LanguageUtils.Translate(textBox.PlaceholderText, _mainForm.CurrentLanguage);
            }
        }

        for (var i = 0; i < TypeComboBox.Items.Count; i++)
        {
            var text = TypeComboBox.Items[i]?.ToString();
            if (text == null) continue;
            TypeComboBox.Items[i] = LanguageUtils.Translate(text, _mainForm.CurrentLanguage);
        }
    }

    /// <summary>
    /// アイテムタイプ、カスタムカテゴリから、TypeComboBoxのIndexを設定します。
    /// </summary>
    /// <param name="itemType"></param>
    /// <param name="customCategory"></param>
    private void SetTypeCombobox(ItemType itemType, string? customCategory)
    {
        if (itemType == ItemType.Custom)
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
            TypeComboBox.SelectedIndex = itemType == ItemType.Unknown ? 0 : (int)itemType;
        }
    }

    private (ItemType, string, bool) GetTypeCombobox(string categoryText)
    {
        ItemType itemType;
        var exists = true;

        var typeIndex = TypeComboBox.Items.IndexOf(categoryText);
        if (typeIndex > 9 || typeIndex == -1)
        {
            itemType = ItemType.Custom;
            if (typeIndex == -1) exists = false;
        }
        else
        {
            itemType = (ItemType)typeIndex;
        }

        return (itemType, categoryText, exists);
    }

    /// <summary>
    /// フォルダパスのテキストボックスが変更されたときの処理です。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckText(object sender, EventArgs e)
        => ValidCheck();

    /// <summary>
    /// パスなどのテキストボックス内のテキストが有効かどうかをチェックします。
    /// </summary>
    private void ValidCheck()
    {
        if (!(File.Exists(FolderTextBox.Text) && FolderTextBox.Text.EndsWith(".zip")) && !Directory.Exists(FolderTextBox.Text))
        {
            SetErrorState(LanguageUtils.Translate("エラー: フォルダパスが存在しません", _mainForm.CurrentLanguage));
            return;
        }

        if (File.Exists(FolderTextBox.Text) && !FolderTextBox.Text.EndsWith(".zip"))
        {
            SetErrorState(LanguageUtils.Translate("エラー: フォルダパスがファイルです", _mainForm.CurrentLanguage));
            return;
        }

        if (string.IsNullOrEmpty(FolderTextBox.Text))
        {
            SetErrorState(LanguageUtils.Translate("エラー: フォルダパスが入力されていません", _mainForm.CurrentLanguage));
            return;
        }

        if (_mainForm.Items.Any(i => i.ItemPath == FolderTextBox.Text) && (!_edit || _directoryNotFound))
        {
            SetErrorState(LanguageUtils.Translate("エラー: 同じパスのアイテムが既に存在します", _mainForm.CurrentLanguage));
            return;
        }

        if (!string.IsNullOrEmpty(MaterialTextBox.Text) && (!(File.Exists(MaterialTextBox.Text) && MaterialTextBox.Text.EndsWith(".zip")) && !Directory.Exists(MaterialTextBox.Text)))
        {
            SetErrorState(LanguageUtils.Translate("エラー: マテリアルフォルダパスが存在しません", _mainForm.CurrentLanguage));
            return;
        }

        if (!string.IsNullOrEmpty(MaterialTextBox.Text) && File.Exists(MaterialTextBox.Text) && !MaterialTextBox.Text.EndsWith(".zip"))
        {
            SetErrorState(LanguageUtils.Translate("エラー: マテリアルフォルダパスがファイルです", _mainForm.CurrentLanguage));
            return;
        }

        if (string.IsNullOrEmpty(TitleTextBox.Text))
        {
            SetErrorState(LanguageUtils.Translate("エラー: タイトルが入力されていません", _mainForm.CurrentLanguage));
            return;
        }

        if (TitleTextBox.Text == "*")
        {
            SetErrorState(LanguageUtils.Translate("エラー: タイトルを*にすることはできません", _mainForm.CurrentLanguage));
            return;
        }

        if (string.IsNullOrEmpty(AuthorTextBox.Text))
        {
            SetErrorState(LanguageUtils.Translate("エラー: 作者が入力されていません", _mainForm.CurrentLanguage));
            return;
        }

        if (string.IsNullOrEmpty(TypeComboBox.Text))
        {
            SetErrorState(LanguageUtils.Translate("エラー: タイプが入力されていません", _mainForm.CurrentLanguage));
            return;
        }

        if (!GetTypeCombobox(TypeComboBox.Text).Item3)
        {
            SetInfomationState(LanguageUtils.Translate("新規カスタムカテゴリが作成されます", _mainForm.CurrentLanguage));
            return;
        }

        ClearErrorState();
    }

    /// <summary>
    /// エラー状態を設定します。
    /// </summary>
    /// <param name="errorMessage"></param>
    private void SetErrorState(string errorMessage)
    {
        ErrorLabel.ForeColor = Color.Red;
        ErrorLabel.Text = errorMessage;
        AddButton.Enabled = false;
    }

    /// <summary>
    /// 通常メッセージを設定します。
    /// </summary>
    /// <param name="message"></param>
    private void SetInfomationState(string message)
    {
        ErrorLabel.ForeColor = _mainForm.DarkMode ? Color.LightGreen : Color.Green;
        ErrorLabel.Text = message;
        AddButton.Enabled = true;
    }

    /// <summary>
    /// エラー状態を解除します。
    /// </summary>
    private void ClearErrorState()
    {
        ErrorLabel.Text = string.Empty;
        AddButton.Enabled = true;
    }
    #endregion

    #region イベントハンドラ
    private void OpenFolderButton_Click(object sender, EventArgs e)
    {
        var fbd = new FolderBrowserDialog
        {
            Description = LanguageUtils.Translate("アイテムフォルダを選択してください", _mainForm.CurrentLanguage),
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            Multiselect = true
        };

        if (fbd.ShowDialog() != DialogResult.OK) return;
        ItemFolderPaths = fbd.SelectedPaths;
    }

    private void OpenMaterialFolderButton_Click(object sender, EventArgs e)
    {
        var fbd = new FolderBrowserDialog
        {
            Description = LanguageUtils.Translate("マテリアルフォルダを選択してください", _mainForm.CurrentLanguage),
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (fbd.ShowDialog() != DialogResult.OK) return;
        MaterialTextBox.Text = fbd.SelectedPath;
    }

    private void FolderTextBox_DragDrop(object sender, DragEventArgs e)
        => ItemFolderPaths = AEUtils.GetFileDropPaths(e);

    private void MaterialTextBox_DragDrop(object sender, DragEventArgs e)
    {
        var files = AEUtils.GetFileDropPaths(e);
        if (files.Length == 0) return;

        var folderPath = files[0];

        if (!(File.Exists(folderPath) && folderPath.EndsWith(".zip")) && !Directory.Exists(folderPath))
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("有効なフォルダ、またはzipファイルを選択してください", _mainForm.CurrentLanguage),
                LanguageUtils.Translate("エラー", _mainForm.CurrentLanguage),
                true
            );
            return;
        }
        MaterialTextBox.Text = folderPath;
    }

    private void BoothURLTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (!e.Control && e.KeyCode == Keys.Enter)
        {
            GetButton_Click(this, EventArgs.Empty);
        }
    }

    private void TypeComboBox_TextChanged(object sender, EventArgs e)
    {
        SelectAvatar.Enabled = GetTypeCombobox(TypeComboBox.Text).Item1 != ItemType.Avatar;
        ValidCheck();
    }

    private void SelectAvatar_Click(object sender, EventArgs e)
    {
        SelectSupportedAvatarForm selectSupportedAvatar = new(_mainForm, this);
        selectSupportedAvatar.ShowDialog();

        SelectAvatar.Text = LanguageUtils.Translate("選択中: {0}個", _mainForm.CurrentLanguage, SupportedAvatar.Count.ToString());
    }

    private void CustomButton_Click(object sender, EventArgs e)
    {
        BoothURLTextBox.Text = string.Empty;

        TitleTextBox.Text = string.Empty;
        TitleTextBox.Enabled = true;

        AuthorTextBox.Text = string.Empty;
        AuthorTextBox.Enabled = true;
    }

    private async void GetButton_Click(object sender, EventArgs e)
    {
        var rawBoothId = BoothURLTextBox.Text.Split('/')[^1];
        if (!int.TryParse(rawBoothId, out int boothId))
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("Booth URLが正しくありません", _mainForm.CurrentLanguage),
                LanguageUtils.Translate("エラー", _mainForm.CurrentLanguage),
                true
            );
            return;
        }

        var currentTime = DateTime.Now;
        if (_lastGetTime.AddSeconds(5) > currentTime)
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("情報取得の間隔が短すぎます。前回の取得から5秒以上空けてください", _mainForm.CurrentLanguage),
                LanguageUtils.Translate("エラー", _mainForm.CurrentLanguage),
                true
            );
            return;
        }
        _lastGetTime = currentTime;

        _item.BoothId = boothId;

        try
        {
            GetButton.Enabled = false;
            GetButton.Text = LanguageUtils.Translate("取得中...", _mainForm.CurrentLanguage);

            var newItem = await BoothUtils.GetBoothItemInfoAsync(rawBoothId);

            _item.Title = newItem.Title;
            _item.AuthorName = newItem.AuthorName;
            _item.ThumbnailUrl = newItem.ThumbnailUrl;
            _item.AuthorImageUrl = newItem.AuthorImageUrl;
            _item.AuthorId = newItem.AuthorId;
            _item.Type = newItem.Type;
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("Boothのアイテム情報を取得できませんでした", _mainForm.CurrentLanguage) + "\n" + ex.Message,
                LanguageUtils.Translate("エラー", _mainForm.CurrentLanguage),
                true
            );
        }
        finally
        {
            GetButton.Text = LanguageUtils.Translate("情報を取得", _mainForm.CurrentLanguage);
            GetButton.Enabled = true;
        }

        TitleTextBox.Text = _item.Title;
        TitleTextBox.Enabled = true;

        AuthorTextBox.Text = _item.AuthorName;
        AuthorTextBox.Enabled = true;

        if (_item.Type != ItemType.Unknown) TypeComboBox.SelectedIndex = (int)_item.Type;

        ValidCheck();
    }

    private async void AddButton_Click(object sender, EventArgs e)
    {
        if (ItemFolderPaths.Length == 0)
        {
            ItemFolderPaths = ItemFolderPaths.Append(FolderTextBox.Text).ToArray();
        }
        else
        {
            ItemFolderPaths[0] = FolderTextBox.Text;
        }

        if (ItemFolderPaths.Length == 0)
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("追加可能なフォルダ、またはファイルがありません", _mainForm.CurrentLanguage),
                LanguageUtils.Translate("エラー", _mainForm.CurrentLanguage),
                true
            );

            ValidCheck();
            return;
        }

        AddButton.Enabled = false;

        _item.Title = TitleTextBox.Text;
        _item.AuthorName = AuthorTextBox.Text;

        var typeInfo = GetTypeCombobox(TypeComboBox.Text);
        _item.Type = typeInfo.Item1;
        if (_item.Type == ItemType.Custom) _item.CustomCategory = typeInfo.Item2;

        var itemFolderArray = Array.Empty<string>();
        foreach (var itemFolderPath in ItemFolderPaths)
        {
            var result = ExtractZipWithHandling(itemFolderPath, Path.Combine("Datas", "Items"), _mainForm.RemoveOriginal && MaterialTextBox.Text != itemFolderPath);
            if (result == null) return;
            itemFolderArray = itemFolderArray.Append(result).ToArray();
        }

        var parentFolder = itemFolderArray[0];
        if (itemFolderArray.Length > 1)
        {
            for (var i = 1; i < itemFolderArray.Length; i++)
            {
                var folderName = Path.GetFileName(itemFolderArray[i]);
                var newPath = Path.Combine(parentFolder, "Others", folderName);

                await FileSystemUtils.CopyDirectoryWithProgress(Path.GetFullPath(itemFolderArray[i]), newPath);
            }
        }

        _item.ItemPath = parentFolder;

        var materialPath = ExtractZipWithHandling(MaterialTextBox.Text, Path.Combine(_item.ItemPath, "Materials"), _mainForm.RemoveOriginal);
        if (materialPath == null) return;

        _item.MaterialPath = materialPath;

        if (_item.Type != ItemType.Avatar) _item.SupportedAvatar = SupportedAvatar;

        if (_item.BoothId != -1)
        {
            var thumbnailFolderPath = Path.Combine("Datas", "Thumbnail");
            if (!Directory.Exists(thumbnailFolderPath))
            {
                Directory.CreateDirectory(thumbnailFolderPath);
            }

            var thumbnailPath = Path.Combine(thumbnailFolderPath, $"{_item.BoothId}.png");
            if (!File.Exists(thumbnailPath))
            {
                if (!string.IsNullOrEmpty(_item.ThumbnailUrl))
                {
                    try
                    {
                        var thumbnailData = await BoothUtils.GetImageBytes(_item.ThumbnailUrl);
                        await File.WriteAllBytesAsync(thumbnailPath, thumbnailData);
                        _item.ImagePath = thumbnailPath;
                    }
                    catch (Exception ex)
                    {
                        FormUtils.ShowMessageBox(
                            LanguageUtils.Translate("サムネイルのダウンロードに失敗しました。詳細はErrorLog.txtをご覧ください。", _mainForm.CurrentLanguage),
                            LanguageUtils.Translate("エラー", _mainForm.CurrentLanguage),
                            true
                        );
                        LogUtils.ErrorLogger("サムネイルのダウンロードに失敗しました。", ex);
                    }
                }
            }
            else
            {
                _item.ImagePath = thumbnailPath;
            }
        }

        if (!string.IsNullOrEmpty(_item.AuthorId))
        {
            var authorImageFolderPath = Path.Combine("Datas", "AuthorImage");
            if (!Directory.Exists(authorImageFolderPath))
            {
                Directory.CreateDirectory(authorImageFolderPath);
            }

            var authorImagePath = Path.Combine(authorImageFolderPath, $"{_item.AuthorId}.png");
            if (!File.Exists(authorImagePath))
            {
                if (!string.IsNullOrEmpty(_item.AuthorImageUrl))
                {
                    try
                    {
                        var authorImageData = await BoothUtils.GetImageBytes(_item.AuthorImageUrl);
                        await File.WriteAllBytesAsync(authorImagePath, authorImageData);
                        _item.AuthorImageFilePath = authorImagePath;
                    }
                    catch (Exception ex)
                    {
                        FormUtils.ShowMessageBox(
                            LanguageUtils.Translate("作者の画像のダウンロードに失敗しました。詳細はErrorLog.txtをご覧ください。", _mainForm.CurrentLanguage),
                            LanguageUtils.Translate("エラー", _mainForm.CurrentLanguage),
                            true
                        );
                        LogUtils.ErrorLogger("作者の画像のダウンロードに失敗しました。", ex);
                    }
                }
            }
            else
            {
                _item.AuthorImageFilePath = authorImagePath;
            }
        }

        // アイテムの更新日付の更新
        var now = DateUtils.GetUnixTime();

        if (!_edit) _item.CreatedDate = now;
        _item.UpdatedDate = now;

        if (_edit)
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("Boothのアイテムを編集しました!", _mainForm.CurrentLanguage) + "\n" +
                LanguageUtils.Translate("アイテム名: ", _mainForm.CurrentLanguage) + _item.Title + "\n" +
                LanguageUtils.Translate("作者: ", _mainForm.CurrentLanguage) + _item.AuthorName,
                LanguageUtils.Translate("編集完了", _mainForm.CurrentLanguage)
            );
        }
        else
        {
            _mainForm.Items.Add(_item);

            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("Boothのアイテムを追加しました!", _mainForm.CurrentLanguage) + "\n" +
                LanguageUtils.Translate("アイテム名: ", _mainForm.CurrentLanguage) + _item.Title + "\n" +
                LanguageUtils.Translate("作者: ", _mainForm.CurrentLanguage) + _item.AuthorName,
                LanguageUtils.Translate("追加完了", _mainForm.CurrentLanguage)
            );
        }

        ItemAdded?.Invoke(this, EventArgs.Empty);
        Close();
    }

    private void AddItem_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.Enter && AddButton.Enabled)
        {
            AddButton_Click(this, EventArgs.Empty);
        }
    }
    #endregion

    #region 処理関数
    /// <summary>
    /// zipファイルを指定したフォルダに展開します。
    /// </summary>
    /// <param name="path"></param>
    /// <param name="destination"></param>
    /// <param name="removeOriginal"></param>
    /// <returns></returns>
    private string? ExtractZipWithHandling(string path, string destination, bool removeOriginal)
    {
        if (!string.IsNullOrEmpty(path) && path.EndsWith(".zip"))
        {
            try
            {
                return FileSystemUtils.ExtractZip(path, destination, removeOriginal);
            }
            catch (Exception ex)
            {
                FormUtils.ShowMessageBox(
                    LanguageUtils.Translate("zipファイルの展開に失敗しました。詳細はErrorLog.txtをご覧ください。", _mainForm.CurrentLanguage),
                    LanguageUtils.Translate("エラー", _mainForm.CurrentLanguage),
                    true
                );
                LogUtils.ErrorLogger("zipファイルの展開に失敗しました。", ex);
                AddButton.Enabled = true;
                return null;
            }
        }

        return path;
    }
    #endregion
}
