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
        if (!File.Exists(path))
            return [];

        try
        {
            string json = File.ReadAllText(path);
            var items =  JsonSerializer.Deserialize<List<Item>>(json) ?? [];

            // Fix Item Relative Path
            FixItemRelativePaths(items);

            // Fix Supported Avatar Path (Title => Path)
            FixSupportedAvatarPaths(items);

            // Update Empty Dates
            UpdateEmptyDates(items);

            // Fix Item Dates
            FixItemDates(items);

            // Fix Relative Path Escape
            FixRelativePathEscapes(items);

            return items;
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox("アイテムデータの読み込みに失敗しました。詳細はErrorLog.txtをご覧ください。", "エラー", true);
            LogUtils.ErrorLogger("アイテムデータの読み込みに失敗しました", ex);
            return [];
        }
    }

    /// <summary>
    /// アイテムデータを保存します。
    /// </summary>
    /// <param name="items"></param>
    internal static void SaveItemsData(List<Item> items)
    {
        try
        {
            // Fix Item Relative Path
            FixItemRelativePaths(items);

            // Fix Supported Avatar Path (Title => Path)
            FixSupportedAvatarPaths(items);

            // Update Empty Dates
            UpdateEmptyDates(items);

            // Fix Item Dates
            FixItemDates(items);

            // Fix Relative Path Escape
            FixRelativePathEscapes(items);

            string json = JsonSerializer.Serialize(items, jsonSerializerOptions);
            File.WriteAllText("./Datas/ItemsData.json", json);
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox("アイテムデータの保存に失敗しました。詳細はErrorLog.txtをご覧ください。", "エラー", true);
            LogUtils.ErrorLogger("アイテムデータの保存に失敗しました", ex);
        }
    }

    /// <summary>
    /// 指定されたパスから共通素体データを取得します。
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static List<CommonAvatar> LoadCommonAvatarData(string path = "./Datas/CommonAvatar.json")
    {
        if (!File.Exists(path))
            return [];

        try
        {
            string json = File.ReadAllText(path);
            var commonAvatars = JsonSerializer.Deserialize<List<CommonAvatar>>(json) ?? [];

            // Fix Item Relative Path
            FixItemRelativePaths(commonAvatars);

            // Fix Relative Path Escape
            FixRelativePathEscapes(commonAvatars);

            return commonAvatars;
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox("共通素体データの読み込みに失敗しました。詳細はErrorLog.txtをご覧ください。", "エラー", true);
            LogUtils.ErrorLogger("共通素体データの読み込みに失敗しました", ex);
            return [];
        }
    }

    /// <summary>
    /// 共通素体データを保存します。
    /// </summary>
    /// <param name="commonAvatars"></param>
    internal static void SaveCommonAvatarData(List<CommonAvatar> commonAvatars)
    {
        try
        {
            // Fix Item Relative Path
            FixItemRelativePaths(commonAvatars);

            // Fix Relative Path Escape
            FixRelativePathEscapes(commonAvatars);

            string json = JsonSerializer.Serialize(commonAvatars, jsonSerializerOptions);
            File.WriteAllText("./Datas/CommonAvatar.json", json);
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox("共通素体データの保存に失敗しました。詳細はErrorLog.txtをご覧ください。", "エラー", true);
            LogUtils.ErrorLogger("共通素体データの保存に失敗しました", ex);
        }
    }

    /// <summary>
    /// 指定されたパスからカスタムカテゴリーデータを取得します。
    /// </summary>
    /// <returns></returns>
    internal static List<string> LoadCustomCategoriesData(string path = "./Datas/CustomCategory.txt", bool createNewFile = true)
    {
        try
        {
            if (!File.Exists(path))
            {
                if (createNewFile)
                {
                    File.WriteAllText(path, string.Empty, Encoding.UTF8);
                }

                return [];
            }

            var categories = File.ReadAllLines(path, Encoding.UTF8)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line))
                .Distinct()
                .ToList();

            return categories;
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox("カスタムカテゴリデータの読み込みに失敗しました。詳細はErrorLog.txtをご覧ください。", "エラー", true);
            LogUtils.ErrorLogger("カスタムカテゴリデータの読み込みに失敗しました", ex);
            return [];
        }
    }

    /// <summary>
    /// カスタムカテゴリーデータを保存します。
    /// </summary>
    /// <param name="customCategories"></param>
    internal static void SaveCustomCategoriesData(List<string> customCategories)
    {
        try
        {
            Directory.CreateDirectory("./Datas");

            File.WriteAllLines(
                "./Datas/CustomCategory.txt",
                customCategories.Distinct().Where(c => !string.IsNullOrWhiteSpace(c)),
                Encoding.UTF8
            );
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox("カスタムカテゴリデータの保存に失敗しました。詳細はErrorLog.txtをご覧ください。", "エラー", true);
            LogUtils.ErrorLogger("カスタムカテゴリデータの保存に失敗しました", ex);
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
        if (string.IsNullOrEmpty(path)) return string.Empty;
        var item = items.FirstOrDefault(x => x.ItemPath == path);
        return item?.Title ?? string.Empty;
    }

    /// <summary>
    /// アイテムのパスを変更します。
    /// </summary>
    /// <param name="items"></param>
    /// <param name="oldPath"></param>
    /// <param name="newPath"></param>
    internal static void ChangeAllItemPaths(List<Item> items, string oldPath, string newPath)
    {
        foreach (var item in items)
        {
            if (item.SupportedAvatar.Contains(oldPath))
            {
                item.SupportedAvatar = item.SupportedAvatar
                    .Select(avatar => avatar == oldPath ? newPath : avatar)
                    .ToList();
            }

            if (item.ImplementedAvatars.Contains(oldPath))
            {
                item.ImplementedAvatars = item.ImplementedAvatars
                    .Select(avatar => avatar == oldPath ? newPath : avatar)
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
    internal static void DeleteAvatarFromItems(List<Item> items, string avatarPath, bool deleteFromSupported)
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
    internal static void DeleteAvatarFromCommonAvatars(List<CommonAvatar> commonAvatars, string avatarPath)
    {
        foreach (var commonAvatar in commonAvatars)
        {
            commonAvatar.Avatars.RemoveAll(avatar => string.IsNullOrEmpty(avatar) || avatar == avatarPath);
        }
    }

    /// <summary>
    /// アイテムの日付を修正します。
    /// </summary>
    /// <param name="items"></param>
    private static void FixItemDates(List<Item> items)
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
    private static void UpdateEmptyDates(List<Item> items)
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
    private static void FixItemRelativePaths(List<Item> items)
    {
        string currentDirectory = Path.GetFullPath(".");

        foreach (var item in items)
        {
            item.ItemPath = FixItemRelativePath(item.ItemPath, currentDirectory);

            for (int i = 0; i < item.SupportedAvatar.Count; i++)
            {
                item.SupportedAvatar[i] = FixItemRelativePath(item.SupportedAvatar[i], currentDirectory);
            }

            for (int i = 0; i < item.ImplementedAvatars.Count; i++)
            {
                item.ImplementedAvatars[i] = FixItemRelativePath(item.ImplementedAvatars[i], currentDirectory);
            }
        }
    }

    /// <summary>
    /// アイテムパスの絶対パスが./Datasと同じだった場合、自動で相対パスに変換するものです。
    /// </summary>
    /// <param name="commonAvatars"></param>
    private static void FixItemRelativePaths(List<CommonAvatar> commonAvatars)
    {
        string currentDirectory = Path.GetFullPath(".");

        foreach (var commonAvatar in commonAvatars)
        {
            for (int i = 0; i < commonAvatar.Avatars.Count; i++)
            {
                commonAvatar.Avatars[i] = FixItemRelativePath(commonAvatar.Avatars[i], currentDirectory);
            }
        }
    }

    /// <summary>
    /// アイテムパスの絶対パスが./Datasと同じだった場合、自動で相対パスに変換するものです。
    /// </summary>
    /// <param name="path"></param>
    /// <param name="currentDirectory"></param>
    /// <returns></returns>
    private static string FixItemRelativePath(string path, string currentDirectory)
    {
        if (string.IsNullOrEmpty(path)) return path;

        string fullItemPath = Path.GetFullPath(path);

        string datasFolder = Path.Combine(currentDirectory, "Datas");
        string datasFolderFull = Path.GetFullPath(datasFolder);

        if (fullItemPath.StartsWith(datasFolderFull, StringComparison.OrdinalIgnoreCase))
        {
            string relativePath = Path.GetRelativePath(currentDirectory, fullItemPath);
            return FixPath(relativePath);
        }

        return path;
    }

    /// <summary>
    /// 相対パスのエスケープを直してくれます。
    /// </summary>
    /// <param name="items"></param>
    private static void FixRelativePathEscapes(List<Item> items)
    {
        foreach (var item in items)
        {
            item.ItemPath = FixPath(item.ItemPath);
            item.MaterialPath = FixPath(item.MaterialPath);
            item.ImagePath = FixPath(item.ImagePath);
            item.AuthorImageFilePath = FixPath(item.AuthorImageFilePath);
        }
    }

    /// <summary>
    /// 相対パスのエスケープを直してくれます。
    /// </summary>
    /// <param name="commonAvatars"></param>
    private static void FixRelativePathEscapes(List<CommonAvatar> commonAvatars)
    {
        foreach (var commonAvatar in commonAvatars)
        {
            for (int i = 0; i < commonAvatar.Avatars.Count; i++)
            {
                commonAvatar.Avatars[i] = FixPath(commonAvatar.Avatars[i]);
            }
        }
    }

    /// <summary>
    /// 相対パスの./の文字を削除し、/を\\へ変換します。
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static string FixPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;

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
                if (supportedAvatarName == string.Empty) return false;
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
                if (implementedAvatarName == string.Empty) return false;
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
    private static void FixSupportedAvatarPaths(List<Item> items)
    {
        var avatars = items.Where(x => x.Type == ItemType.Avatar);

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

    /// <summary>
    /// 不足しているカスタムカテゴリを検知して、自動で追加します。
    /// </summary>
    /// <param name="items"></param>
    /// <param name="categories"></param>
    /// <returns></returns>
    internal static bool CheckMissingCustomCategories(List<Item> items, List<string> categories)
    {
        List<string> missingCategories = new();

        foreach (var item in items)
        {
            if (item.Type != ItemType.Custom || string.IsNullOrEmpty(item.CustomCategory)) continue;
            if (missingCategories.Contains(item.CustomCategory) || categories.Contains(item.CustomCategory)) continue;

            missingCategories.Add(item.CustomCategory);
        }

        if (missingCategories.Count == 0) return false;

        foreach (var item in missingCategories)
        {
            categories.Add(item);
        }

        return true;
    }
}
