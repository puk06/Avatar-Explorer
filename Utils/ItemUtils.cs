using Avatar_Explorer.Models;

namespace Avatar_Explorer.Utils;

internal static class ItemUtils
{
    /// <summary>
    /// 指定されたアイテムが対応アバターかどうか、共通素体グループに入っているかどうかを取得します。
    /// </summary>
    /// <param name="item"></param>
    /// <param name="commonAvatars"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static SupportedOrCommonAvatar IsSupportedAvatarOrCommon(Item item, List<CommonAvatar> commonAvatars, string? path)
    {
        if (string.IsNullOrEmpty(path)) return new SupportedOrCommonAvatar();
        if (item.SupportedAvatars.Contains(path)) return new SupportedOrCommonAvatar { IsSupported = true };

        if (item.Type != ItemType.Clothing) return new SupportedOrCommonAvatar();
        var commonAvatarsArray = commonAvatars.Where(x => x.Avatars.Contains(path));
        var commonAvatarBool = item.SupportedAvatars.Any(supportedAvatar => commonAvatarsArray.Any(x => x.Avatars.Contains(supportedAvatar)));

        if (!commonAvatarBool) return new SupportedOrCommonAvatar();

        var commonAvatar = item.SupportedAvatars
            .Select(supportedAvatar => commonAvatarsArray.FirstOrDefault(x => x.Avatars.Contains(supportedAvatar)))
            .FirstOrDefault(x => x != null);

        return new SupportedOrCommonAvatar
        {
            IsCommon = true,
            CommonAvatarName = commonAvatar?.Name ?? string.Empty
        };
    }

    /// <summary>
    /// 翻訳されたカテゴリー名を取得します。
    /// </summary>
    /// <param name="itemType"></param>
    /// <param name="lang"></param>
    /// <returns></returns>
    internal static string GetCategoryName(ItemType itemType, string lang, string customCategory = "")
    {
        return itemType switch
        {
            ItemType.Avatar => LanguageUtils.Translate("アバター", lang),
            ItemType.Clothing => LanguageUtils.Translate("衣装", lang),
            ItemType.Texture => LanguageUtils.Translate("テクスチャ", lang),
            ItemType.Gimmick => LanguageUtils.Translate("ギミック", lang),
            ItemType.Accessory => LanguageUtils.Translate("アクセサリー", lang),
            ItemType.HairStyle => LanguageUtils.Translate("髪型", lang),
            ItemType.Animation => LanguageUtils.Translate("アニメーション", lang),
            ItemType.Tool => LanguageUtils.Translate("ツール", lang),
            ItemType.Shader => LanguageUtils.Translate("シェーダー", lang),
            ItemType.Custom => customCategory,
            _ => LanguageUtils.Translate("不明", lang)
        };
    }

    /// <summary>
    /// すべての作者情報を取得します。
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    internal static List<Author> GetAuthors(List<Item> items)
    {
        var authors = new List<Author>();

        foreach (Item item in items)
        {
            if (authors.Any(author => author.AuthorName == item.AuthorName)) continue;
            authors.Add(new Author
            {
                AuthorName = item.AuthorName,
                AuthorImagePath = item.AuthorImageFilePath
            });
        }

        return authors;
    }


    /// <summary>
    /// 与えられたアバターがアイテムの実装済みリストに含まれているかどうかを確認します。
    /// </summary>
    /// <param name="item"></param>
    /// <param name="selectedAvatar"></param>
    /// <returns></returns>
    internal static bool ContainsSelectedAvatar(Item item, string? selectedAvatar)
    {
        if (string.IsNullOrEmpty(selectedAvatar)) return false;
        return item.ImplementedAvatars.Contains(selectedAvatar);
    }


    /// <summary>
    /// 指定されたパスからアイテムフォルダー情報を取得します。
    /// </summary>
    /// <param name="path"></param>
    /// <param name="materialPath"></param>
    /// <returns></returns>
    internal static ItemFolderInfo GetItemFolderInfo(string path, string materialPath)
    {
        var itemFolderInfo = new ItemFolderInfo();
        if (!Directory.Exists(path)) return itemFolderInfo;

        try
        {
            foreach (var file in FileSystemUtils.FastEnumerateFiles(path))
            {
                var extension = Path.GetExtension(file);
                var item = new FileData
                {
                    FileName = Path.GetFileName(file),
                    FilePath = file
                };

                switch (extension.ToLower())
                {
                    case ".psd":
                    case ".clip":
                    case ".blend":
                    case ".fbx":
                        itemFolderInfo.FilesForModification.Add(item);
                        break;
                    case ".png":
                    case ".jpg":
                        itemFolderInfo.TextureFiles.Add(item);
                        break;
                    case ".txt":
                    case ".md":
                    case ".pdf":
                        itemFolderInfo.DocumentFiles.Add(item);
                        break;
                    case ".unitypackage":
                        itemFolderInfo.UnityPackageFiles.Add(item);
                        break;
                    default:
                        itemFolderInfo.UnknownFiles.Add(item);
                        break;
                }
            }

            if (string.IsNullOrEmpty(materialPath)) return itemFolderInfo;

            foreach (var file in FileSystemUtils.FastEnumerateFiles(materialPath))
            {
                var item = new FileData
                {
                    FileName = Path.GetFileName(file),
                    FilePath = file
                };

                itemFolderInfo.MaterialFiles.Add(item);
            }
        }
        catch
        {
            // Ignored
        }
        
        return itemFolderInfo;
    }


    /// <summary>
    /// アイテムの説明を取得します。
    /// </summary>
    /// <param name="item"></param>
    /// <param name="CurrentLanguage"></param>
    /// <returns></returns>
    internal static string GetItemDescription(Item item, string CurrentLanguage)
    {
        var description = item.Title + "\n";

        if (!string.IsNullOrEmpty(item.CreatedDate))
        {
            description += "\n" + LanguageUtils.Translate("登録日時", CurrentLanguage) + ": " + DateUtils.GetDateStringFromUnixTime(item.CreatedDate);
        }

        if (!string.IsNullOrEmpty(item.UpdatedDate))
        {
            description += "\n" + LanguageUtils.Translate("更新日時", CurrentLanguage) + ": " + DateUtils.GetDateStringFromUnixTime(item.UpdatedDate);
        }

        if (!string.IsNullOrEmpty(item.ItemMemo))
        {
            description += $"\n\n- {LanguageUtils.Translate("メモ", CurrentLanguage)} -" + "\n" + item.ItemMemo;
        }

        return description;
    }

    /// <summary>
    /// アイテムのフォルダに新しくフォルダを追加します。
    /// </summary>
    /// <param name="item"></param>
    internal static async Task AddFolderToItem(Item item, string currentLanguage)
    {
        var fbd = new FolderBrowserDialog
        {
            Description = LanguageUtils.Translate("アイテムフォルダを選択してください", currentLanguage),
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            Multiselect = true
        };

        if (fbd.ShowDialog() != DialogResult.OK) return;
        var itemFolderArray = fbd.SelectedPaths;

        var result = FormUtils.ShowConfirmDialog(LanguageUtils.Translate("アイテム: {0}\n\n追加予定のフォルダ一覧:\n{1}\n\n選択したフォルダをアイテムに追加してもよろしいですか？", currentLanguage, item.Title, string.Join("\n", itemFolderArray.Select(log => $"・{Path.GetFileName(log)}"))), LanguageUtils.Translate("アイテムフォルダの追加", currentLanguage));
        if (!result) return;

        var parentFolder = item.ItemPath;

        for (var i = 0; i < itemFolderArray.Length; i++)
        {
            var folderName = Path.GetFileName(itemFolderArray[i]);
            var newPath = Path.Combine(parentFolder, "Others", folderName);

            await FileSystemUtils.CopyDirectoryWithProgress(Path.GetFullPath(itemFolderArray[i]), newPath);
        }

        FormUtils.ShowMessageBox(LanguageUtils.Translate("フォルダの追加が完了しました。", currentLanguage), LanguageUtils.Translate("完了", currentLanguage));
    }

    /// <summary>
    /// 対応アバター
    /// </summary>
    /// <param name="items"></param>
    /// <param name="item"></param>
    /// <param name="commonAvatars"></param>
    /// <param name="currentLanguage"></param>
    internal static void DeleteAvatarFromSupported(List<Item> items, Item item, List<CommonAvatar> commonAvatars, string currentLanguage)
    {
        var result = FormUtils.ShowConfirmDialog(
            LanguageUtils.Translate("このアバターを対応アバターとしているアイテムの対応アバターからこのアバターを削除しますか？", currentLanguage),
            LanguageUtils.Translate("確認", currentLanguage)
        );

        DatabaseUtils.DeleteAvatarFromItems(items, item.ItemPath, result);

        if (commonAvatars.Any(commonAvatar => commonAvatar.Avatars.Contains(item.ItemPath)))
        {
            var result1 = FormUtils.ShowConfirmDialog(
                LanguageUtils.Translate("このアバターを共通素体グループから削除しますか？", currentLanguage),
                LanguageUtils.Translate("確認", currentLanguage)
            );

            if (result1)
            {
                DatabaseUtils.DeleteAvatarFromCommonAvatars(commonAvatars, item.ItemPath);
                DatabaseUtils.SaveCommonAvatarsData(commonAvatars);
            }
        }
    }
}
