namespace Avatar_Explorer.Models;

/// <summary>
/// 現在のパスを表します。
/// </summary>
internal class CurrentPath
{
    /// <summary>
    /// 現在選択されているアバターの名前を取得または設定します。
    /// </summary>
    internal string? CurrentSelectedAvatar;

    /// <summary>
    /// 現在選択されているアバターのパスを取得または設定します。
    /// </summary>
    internal string? CurrentSelectedAvatarPath;

    /// <summary>
    /// 現在選択されている作者を取得または設定します。
    /// </summary>
    internal Author? CurrentSelectedAuthor;

    /// <summary>
    /// 現在選択されているカテゴリを取得または設定します。
    /// </summary>
    internal ItemType CurrentSelectedCategory = ItemType.Unknown;

    /// <summary>
    /// 現在選択されているカスタムカテゴリを取得または設定します。
    /// </summary>
    internal string CurrentSelectedCustomCategory = "";

    /// <summary>
    /// 現在選択されているアイテムを取得または設定します。
    /// </summary>
    internal Item? CurrentSelectedItem;

    /// <summary>
    /// 現在選択されているアイテム内のカテゴリを取得または設定します。
    /// </summary>
    internal string? CurrentSelectedItemCategory;

    /// <summary>
    /// 現在選択されているアイテムフォルダ情報を取得または設定します。
    /// </summary>
    internal ItemFolderInfo CurrentSelectedItemFolderInfo = new();

    /// <summary>
    /// パス全体が空かどうかを取得します。
    /// </summary>
    /// <returns></returns>
    internal bool IsEmpty()
    {
        return CurrentSelectedAvatar == null &&
               CurrentSelectedAvatarPath == null &&
               CurrentSelectedAuthor == null &&
               CurrentSelectedCategory == ItemType.Unknown &&
               CurrentSelectedItem == null &&
               CurrentSelectedItemCategory == null;
    }
}
