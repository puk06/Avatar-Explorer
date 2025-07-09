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
        if (item.SupportedAvatar.Contains(path)) return new SupportedOrCommonAvatar { IsSupported = true };

        if (item.Type != ItemType.Clothing) return new SupportedOrCommonAvatar();
        var commonAvatarsArray = commonAvatars.Where(x => x.Avatars.Contains(path)).ToArray();
        var commonAvatarBool = item.SupportedAvatar.Any(supportedAvatar => commonAvatarsArray.Any(x => x.Avatars.Contains(supportedAvatar)));

        if (!commonAvatarBool) return new SupportedOrCommonAvatar();
        {
            var commonAvatar = item.SupportedAvatar
                .Select(supportedAvatar => commonAvatarsArray.FirstOrDefault(x => x.Avatars.Contains(supportedAvatar)))
                .FirstOrDefault(x => x != null);

            return new SupportedOrCommonAvatar
            {
                IsCommon = true,
                CommonAvatarName = commonAvatar?.Name ?? ""
            };
        }
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
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
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
                    itemFolderInfo.ModifyFiles.Add(item);
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
                    itemFolderInfo.UnkownFiles.Add(item);
                    break;
            }
        }

        if (string.IsNullOrEmpty(materialPath)) return itemFolderInfo;

        var materialFiles = Directory.GetFiles(materialPath, "*.*", SearchOption.AllDirectories);
        foreach (var file in materialFiles)
        {
            var item = new FileData
            {
                FileName = Path.GetFileName(file),
                FilePath = file
            };

            itemFolderInfo.MaterialFiles.Add(item);
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
        var description = item.Title;

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
            description += "\n\n" + LanguageUtils.Translate("メモ: ", CurrentLanguage) + item.ItemMemo;
        }

        return description;
    }
}
