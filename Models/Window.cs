namespace Avatar_Explorer.Models;

/// <summary>
/// 現在開いているウィンドウの種類を表します。
/// </summary>
internal enum Window
{
    /// <summary>
    /// 何も開いていません。
    /// </summary>
    Nothing,

    /// <summary>
    /// アイテムカテゴリリスト
    /// </summary>
    ItemCategoryList,

    /// <summary>
    /// アイテムリスト
    /// </summary>
    ItemList,

    /// <summary>
    /// アイテム内のカテゴリリスト
    /// </summary>
    ItemFolderCategoryList,

    /// <summary>
    /// アイテム内のアイテムリスト
    /// </summary>
    ItemFolderItemsList
}
