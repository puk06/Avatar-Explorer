using Avatar_Explorer.Models;
using System.Text;
using System.Text.Json;

namespace Avatar_Explorer.Utils;

internal static class DatabaseUtils
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// 指定されたパスからアイテムデータを取得します。
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static List<Item> LoadItemsData(string path = "./Datas/ItemsData.json")
    {
        try
        {
            if (!File.Exists(path)) return new List<Item>();
            using var sr = new StreamReader(path);
            var data = JsonSerializer.Deserialize<List<Item>>(sr.ReadToEnd());
            return data ?? new List<Item>();
        }
        catch
        {
            return new List<Item>();
        }
    }

    /// <summary>
    /// アイテムデータを保存します。
    /// </summary>
    /// <param name="items"></param>
    internal static void SaveItemsData(List<Item> items)
    {
        using var sw = new StreamWriter("./Datas/ItemsData.json");
        sw.Write(JsonSerializer.Serialize(items, jsonSerializerOptions));
    }

    /// <summary>
    /// 指定されたパスから共通素体データを取得します。
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static List<CommonAvatar> LoadCommonAvatarData(string path = "./Datas/CommonAvatar.json")
    {
        try
        {
            if (!File.Exists(path)) return new List<CommonAvatar>();
            using var sr = new StreamReader(path);
            var data = JsonSerializer.Deserialize<List<CommonAvatar>>(sr.ReadToEnd());
            return data ?? new List<CommonAvatar>();
        }
        catch
        {
            return new List<CommonAvatar>();
        }
    }

    /// <summary>
    /// 共通素体データを保存します。
    /// </summary>
    /// <param name="commonAvatars"></param>
    internal static void SaveCommonAvatarData(List<CommonAvatar> commonAvatars)
    {
        using var sw = new StreamWriter("./Datas/CommonAvatar.json");
        sw.Write(JsonSerializer.Serialize(commonAvatars, jsonSerializerOptions));
    }

    /// <summary>
    /// 指定されたパスからカスタムカテゴリーデータを取得します。
    /// </summary>
    /// <returns></returns>
    internal static List<string> LoadCustomCategoriesData(string path = "./Datas/CustomCategory.txt", bool createNewFile = true)
    {
        if (!File.Exists(path))
        {
            if (createNewFile) File.Create(path).Close();
            return new List<string>();
        }

        var categories = File.ReadAllLines(path, Encoding.UTF8).ToList();
        categories.RemoveAll(string.IsNullOrEmpty);

        return categories;
    }

    /// <summary>
    /// カスタムカテゴリーデータを保存します。
    /// </summary>
    /// <param name="customCategories"></param>
    internal static void SaveCustomCategoriesData(List<string> customCategories)
    {
        using var sw = new StreamWriter("./Datas/CustomCategory.txt", false, Encoding.UTF8);
        foreach (var category in customCategories)
        {
            sw.WriteLine(category);
        }
    }

    /// <summary>
    /// カスタムカテゴリーデータの数を取得します。
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static int GetCustomCategoryCount(string path)
    {
        var customCategoryDatas = LoadCustomCategoriesData(path + "/CustomCategory.txt", false);
        return customCategoryDatas.Count;
    }

    /// <summary>
    /// アイテムデータベースの数を取得します。
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static int GetItemDatabaseCount(string path)
    {
        var itemDatas = LoadItemsData(path + "/ItemsData.json");
        return itemDatas.Count;
    }

    /// <summary>
    /// 共通素体データベースの数を取得します。
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static int GetCommonAvatarDatabaseCount(string path)
    {
        var commonAvatars = LoadCommonAvatarData(path + "/CommonAvatar.json");
        return commonAvatars.Count;
    }

    /// <summary>
    ///　指定されたパスからアバター名を取得します。なければnullを返します。
    /// </summary>
    /// <param name="items"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static string? GetAvatarName(List<Item> items, string? path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        return items
            .Where(x => x.Type == ItemType.Avatar)
            .FirstOrDefault(x => x.ItemPath == path)?
            .Title;
    }

    /// <summary>
    ///　指定されたパスからアバター名を取得します。なければ空文字を返します。
    /// </summary>
    /// <param name="items"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static string GetAvatarNameFromPaths(List<Item> items, string? path)
    {
        if (string.IsNullOrEmpty(path)) return "";
        var item = items.FirstOrDefault(x => x.ItemPath == path);
        return item?.Title ?? "";
    }

    /// <summary>
    /// アイテムのパスを変更します。
    /// </summary>
    /// <param name="items"></param>
    /// <param name="oldPath"></param>
    internal static void ChangeAllItemPaths(ref List<Item> items, string oldPath)
    {
        foreach (var item in items)
        {
            if (item.SupportedAvatar.Contains(oldPath))
            {
                item.SupportedAvatar = item.SupportedAvatar
                    .Select(avatar => avatar == oldPath ? item.ItemPath : avatar)
                    .ToList();
            }

            if (item.ImplementedAvatars.Contains(oldPath))
            {
                item.ImplementedAvatars = item.ImplementedAvatars
                    .Select(avatar => avatar == oldPath ? item.ItemPath : avatar)
                    .ToList();
            }
        }
    }

    /// <summary>
    /// 指定されたアバターをアイテムから削除します。
    /// </summary>
    /// <param name="items"></param>
    /// <param name="avatarPath"></param>
    /// <param name="deleteFromSupported"></param>
    internal static void DeleteAvatarFromItems(ref List<Item> items, string avatarPath, bool deleteFromSupported)
    {
        foreach (var item in items)
        {
            if (deleteFromSupported && item.SupportedAvatar.Contains(avatarPath))
            {
                item.SupportedAvatar.RemoveAll(avatar => string.IsNullOrEmpty(avatar) || avatar == avatarPath);
            }

            if (item.ImplementedAvatars.Contains(avatarPath))
            {
                item.ImplementedAvatars.RemoveAll(avatar => string.IsNullOrEmpty(avatar) || avatar == avatarPath);
            }
        }
    }

    /// <summary>
    /// 指定されたアバターをCommonAvatarから削除します。
    /// </summary>
    /// <param name="commonAvatars"></param>
    /// <param name="avatarPath"></param>
    internal static void DeleteAvatarFromCommonAvatars(ref List<CommonAvatar> commonAvatars, string avatarPath)
    {
        foreach (var commonAvatar in commonAvatars)
        {
            commonAvatar.Avatars.RemoveAll(avatar => string.IsNullOrEmpty(avatar) || avatar == avatarPath);
        }
    }

    internal static void FixItemDates(ref List<Item> items)
    {
        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.CreatedDate))
            {
                var unixTime = new DateTimeOffset(DateUtils.GetDate(item.CreatedDate)).ToUnixTimeMilliseconds();
                item.CreatedDate = unixTime.ToString();
            }

            if (!string.IsNullOrEmpty(item.UpdatedDate))
            {
                var unixTime = new DateTimeOffset(DateUtils.GetDate(item.UpdatedDate)).ToUnixTimeMilliseconds();
                item.UpdatedDate = unixTime.ToString();
            }
        }
    }

    /// <summary>
    /// 空の登録日時と更新日時を現在の日時で埋めます。
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    internal static void UpdateEmptyDates(ref List<Item> items)
    {
        string now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.CreatedDate))
            {
                item.CreatedDate = now;
            }

            if (string.IsNullOrEmpty(item.UpdatedDate))
            {
                item.UpdatedDate = now;
            }
        }
    }

    /// <summary>
    /// アイテムパスの絶対パスが./Datasと同じだった場合、自動で相対パスに変換するものです。
    /// </summary>
    /// <param name="items"></param>
    internal static void FixItemRelativePaths(ref List<Item> items)
    {
        string currentDirectory = Path.GetFullPath(".");

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.ItemPath)) continue;

            string fullItemPath = Path.GetFullPath(item.ItemPath);

            string datasFolder = Path.Combine(currentDirectory, "Datas");
            string datasFolderFull = Path.GetFullPath(datasFolder);

            if (fullItemPath.StartsWith(datasFolderFull, StringComparison.OrdinalIgnoreCase))
            {
                string relativePath = Path.GetRelativePath(currentDirectory, fullItemPath);
                item.ItemPath = FixPath(relativePath);
            }
        }
    }

    /// <summary>
    /// 相対パスのエスケープを直してくれます。
    /// </summary>
    /// <param name="items"></param>
    internal static void FixRelativePathEscapes(ref List<Item> items)
    {
        foreach (var item in items)
        {
            item.ItemPath = FixPath(item.ItemPath);
            item.MaterialPath = FixPath(item.MaterialPath);
            item.ImagePath = FixPath(item.ImagePath);
            item.AuthorImageFilePath = FixPath(item.AuthorImageFilePath);
        }
    }

    private static string FixPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return "";

        if (path.StartsWith("./"))
        {
            path = path[2..];
        }

        return path.Replace('/', '\\');
    }

    /// <summary>
    /// アイテムの検索結果を取得します。
    /// </summary>
    /// <param name="items"></param>
    /// <param name="item"></param>
    /// <param name="searchFilter"></param>
    /// <param name="CurrentLanguage"></param>
    /// <returns></returns>
    internal static bool GetSearchResult(List<Item> items, Item item, SearchFilter searchFilter, string CurrentLanguage)
    {
        if (searchFilter.Author.Length != 0 && !searchFilter.Author.Contains(item.AuthorName))
            return false;

        if (searchFilter.Title.Length != 0 && !searchFilter.Title.Contains(item.Title))
            return false;

        if (searchFilter.BoothId.Length != 0 && !searchFilter.BoothId.Contains(item.BoothId.ToString()))
            return false;

        if (searchFilter.Avatar.Length != 0 && !searchFilter.Avatar.Any(avatar =>
        {
            return item.SupportedAvatar.Any(supportedAvatar =>
            {
                var supportedAvatarName = GetAvatarNameFromPaths(items, supportedAvatar);
                if (supportedAvatarName == "") return false;
                return supportedAvatarName.Contains(avatar, StringComparison.CurrentCultureIgnoreCase);
            });
        }))
        {
            return false;
        }

        if (searchFilter.Category.Length != 0 && !searchFilter.Category.Any(category =>
        {
            var translatedCategory = ItemUtils.GetCategoryName(item.Type, CurrentLanguage);
            return translatedCategory.Contains(category) || item.CustomCategory.Contains(category);
        }))
        {
            return false;
        }

        if (searchFilter.ItemMemo.Length != 0 && !searchFilter.ItemMemo.Any(memo =>
        {
            return item.ItemMemo.Contains(memo, StringComparison.CurrentCultureIgnoreCase);
        }))
        {
            return false;
        }

        if (searchFilter.FolderName.Length != 0 && !searchFilter.FolderName.Any(folderName =>
        {
            return Path.GetFileName(item.ItemPath).Contains(folderName, StringComparison.CurrentCultureIgnoreCase) ||
                    Path.GetFileName(item.MaterialPath).Contains(folderName, StringComparison.CurrentCultureIgnoreCase);
        }))
        {
            return false;
        }

        if (searchFilter.FileName.Length != 0 && !searchFilter.FileName.Any(fileName =>
        {
            return ItemUtils.GetItemFolderInfo(item.ItemPath, item.MaterialPath).GetAllItem()
                .Any(file =>
                    file.FileName.Contains(fileName, StringComparison.CurrentCultureIgnoreCase) ||
                    file.FileExtension.Contains(fileName, StringComparison.CurrentCultureIgnoreCase));
        }))
        {
            return false;
        }

        if (searchFilter.ImplementedAvatars.Length != 0 && !searchFilter.ImplementedAvatars.Any(avatar =>
        {
            return item.ImplementedAvatars.Any(implementedAvatar =>
            {
                var implementedAvatarName = GetAvatarNameFromPaths(items, implementedAvatar);
                if (implementedAvatarName == "") return false;
                return implementedAvatarName.Contains(avatar, StringComparison.CurrentCultureIgnoreCase);
            });
        }))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 対応アバターのパスを修正します(前のバージョンからの移行用)。
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    internal static void FixSupportedAvatarPaths(ref List<Item> items)
    {
        var avatars = items.Where(x => x.Type == ItemType.Avatar).ToArray();
        foreach (var item in items)
        {
            if (item.SupportedAvatar.Count == 0) continue;
            foreach (var supportedAvatar in item.SupportedAvatar)
            {
                var avatar = avatars.FirstOrDefault(x => x.Title == supportedAvatar);
                if (avatar == null) continue;
                item.SupportedAvatar = item.SupportedAvatar
                    .Where(x => x != supportedAvatar)
                    .Append(avatar.ItemPath)
                    .ToList();
            }
        }
    }
}
