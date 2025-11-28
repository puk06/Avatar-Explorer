using Avatar_Explorer.Models;
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
    internal static List<CommonAvatar> LoadCommonAvatarsData(string path = "./Datas/CommonAvatar.json")
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
    internal static void SaveCommonAvatarsData(List<CommonAvatar> commonAvatars)
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
        var commonAvatars = LoadCommonAvatarsData(path + "/CommonAvatar.json");
        return commonAvatars.Count;
    }

    /// <summary>
    /// アイテムデータベース内の全てのカスタムカテゴリを取得します。
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    internal static List<string> GetCustomCategories(List<Item> items)
    {
        return items
            .Where(item => item.Type == ItemType.Custom)
            .Select(item => item.CustomCategory)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// 指定されたパスからアバター名を取得します。なければnullを返します。
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
    /// 指定されたパスからアバター名を取得します。なければ空文字を返します。
    /// </summary>
    /// <param name="items"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static string GetAvatarNameFromPath(List<Item> items, string? path)
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
            if (item.SupportedAvatars.Contains(oldPath))
            {
                item.SupportedAvatar = item.SupportedAvatars
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
            if (deleteFromSupported && item.SupportedAvatars.Contains(avatarPath))
            {
                item.SupportedAvatars.RemoveAll(avatar => string.IsNullOrEmpty(avatar) || avatar == avatarPath);
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

            for (int i = 0; i < item.SupportedAvatars.Count; i++)
            {
                item.SupportedAvatars[i] = FixItemRelativePath(item.SupportedAvatars[i], currentDirectory);
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
    /// データベース内の破損したアイテムの対応 / 実装アバターパスをチェックします。
    /// </summary>
    /// <param name="items"></param>
    internal static void CheckBrokenItemPaths(List<Item> items, string currentLanguage)
    {
        int brokenItemsCount = items
            .Count(item => item.SupportedAvatars.Contains(item.ItemPath) || item.ImplementedAvatars.Contains(item.ItemPath));
        if (brokenItemsCount == 0) return;

        FormUtils.ShowMessageBox(
            LanguageUtils.Translate("対応 / 実装アバターのパスが壊れているアイテムが見つかりました。", currentLanguage) + "\n\n" +
            LanguageUtils.Translate("検索パネルで、\"BrokenItems=true\"と入力することで、壊れたアイテム一覧を表示することが出来ます。", currentLanguage) + "\n\n" +
            LanguageUtils.Translate("検索結果の画面からアイテムの対応アバター、実装アバターの修正が可能です。", currentLanguage) + "\n\n" +
            LanguageUtils.Translate("このバグは、アバターのパスが変更されたとき、そのアイテムを対応アバターとしていた時に、新しいパスではなく、そのアイテムのパスを割り当ててしまうというバグとなっています。", currentLanguage) + "\n" +
            LanguageUtils.Translate("なので、壊れたアバターはほぼ固定(アバターパスを変更した物のみ)となっています。", currentLanguage) + "\n\n" +
            LanguageUtils.Translate("この度はご迷惑をおかけし、誠に申し訳ございませんでした。", currentLanguage),
            LanguageUtils.Translate("データベースエラー", currentLanguage)
        );
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
        bool matchTitle = MatchesFilter(
            new[] { item.Title }, searchFilter.Titles,
            searchFilter.IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchAuthor = MatchesFilter(
            new[] { item.AuthorName }, searchFilter.Authors,
            searchFilter.IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchBooth = MatchesFilter(
            new[] { item.BoothId.ToString() }, searchFilter.BoothIds,
            searchFilter.IsOrSearch,
            (target, filter) => target == filter
        );

        bool matchAvatar = MatchesFilter(
            item.SupportedAvatars.Select(avatar => GetAvatarNameFromPath(items, avatar)), searchFilter.SupportedAvatars,
            searchFilter.IsOrSearch,
            (target, filter) => !string.IsNullOrEmpty(target) && target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchCategory = MatchesFilter(
            new[] { ItemUtils.GetCategoryName(item.Type, CurrentLanguage) }, searchFilter.Categories,
            searchFilter.IsOrSearch,
            (target, filter) => target.Contains(filter) || item.CustomCategory.Contains(filter)
        );

        bool matchMemo = MatchesFilter(
            new[] { item.ItemMemo }, searchFilter.ItemMemos,
            searchFilter.IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchPath = MatchesFilter(
            new[] { Path.GetFileName(item.ItemPath), Path.GetFileName(item.MaterialPath) }, searchFilter.FolderNames,
            searchFilter.IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchFile = searchFilter.FileNames.Count == 0
            || MatchesFilter(
                ItemUtils.GetItemFolderInfo(item.ItemPath, item.MaterialPath).GetAllItem().Select(file => file.FileName + file.FileExtension), searchFilter.FileNames,
                searchFilter.IsOrSearch,
                (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
            );
        
        var implementedAvatarNames = item.ImplementedAvatars.Select(avatar => GetAvatarNameFromPath(items, avatar));

        bool matchImplemented = MatchesFilter(
            implementedAvatarNames, searchFilter.ImplementedAvatars,
            searchFilter.IsOrSearch,
            (target, filter) => !string.IsNullOrEmpty(target) && target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchNotImplemented = searchFilter.NotImplementedAvatars.Count == 0
            || (searchFilter.NotImplementedAvatars.Count > 0 && searchFilter.IsOrSearch
                ? searchFilter.NotImplementedAvatars.Any(filter => !implementedAvatarNames.Any(name => name.Contains(filter, StringComparison.CurrentCultureIgnoreCase)))
                : searchFilter.NotImplementedAvatars.All(filter => !implementedAvatarNames.Any(name => name.Contains(filter, StringComparison.CurrentCultureIgnoreCase))));

        bool matchTag = MatchesFilter(
            item.Tags, searchFilter.Tags,
            searchFilter.IsOrSearch,
            (target, filter) => target.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

        bool matchBroken = !searchFilter.BrokenItems || (searchFilter.BrokenItems && !(item.SupportedAvatars.Contains(item.ItemPath) || item.ImplementedAvatars.Contains(item.ItemPath)));

        return matchTitle
            && matchAuthor
            && matchBooth
            && matchAvatar
            && matchCategory
            && matchMemo
            && matchPath
            && matchFile
            && matchImplemented
            && matchNotImplemented
            && matchTag
            && matchBroken;
    }

    private static bool MatchesFilter<T>(IEnumerable<T> targets, IEnumerable<T> filters, bool isOrSearch, Func<T, T, bool> comparer)
    {
        if (!filters.Any()) return true;

        if (isOrSearch)
        {
            return filters.Any(filter => targets.Any(target => comparer(target, filter)));
        }
        else
        {
            return filters.All(filter => targets.Any(target => comparer(target, filter)));
        }
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
            if (item.SupportedAvatars.Count == 0) continue;
            foreach (var supportedAvatar in item.SupportedAvatars)
            {
                var avatar = avatars.FirstOrDefault(x => x.Title == supportedAvatar);
                if (avatar == null) continue;

                item.SupportedAvatar = item.SupportedAvatars
                    .Where(x => x != supportedAvatar)
                    .Append(avatar.ItemPath)
                    .ToList();
            }
        }
    }
}
