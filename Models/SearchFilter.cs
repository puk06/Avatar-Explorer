namespace Avatar_Explorer.Models;

/// <summary>
/// 検索フィルタを表します。
/// </summary>
internal class SearchFilter
{
    /// <summary>
    /// 検索する作者の名前を取得または設定します。
    /// </summary>
    internal List<string> Author { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのタイトルを取得または設定します。
    /// </summary>
    internal List<string> Title { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのIDを取得または設定します。
    /// </summary>
    internal List<string> BoothId { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのカスタムカテゴリを取得または設定します。
    /// </summary>
    internal List<string> Avatar { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのカテゴリ、またはカスタムカテゴリを取得または設定します。
    /// </summary>
    internal List<string> Category { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのメモを取得または設定します。
    /// </summary>
    internal List<string> ItemMemo { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのフォルダ名を取得または設定します。
    /// </summary>
    internal List<string> FolderName { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのファイル名を取得または設定します。
    /// </summary>
    internal List<string> FileName { get; set; } = new List<string>();

    /// <summary>
    /// 検索する実装済みのアバターを取得または設定します。
    /// </summary>
    internal List<string> ImplementedAvatars { get; set; } = new List<string>();

    /// <summary>
    /// 検索するタグを取得または設定します。
    /// </summary>
    internal List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// アイテムの対応パスなどが破損しているかどうかを取得または設定します。
    /// </summary>
    internal bool BrokenItems { get; set; } = false;

    /// <summary>
    /// OR検索かどうかを取得または設定します。
    /// </summary>
    internal bool IsOrSearch { get; set; } = false;

    /// <summary>
    /// 検索するアイテムの文字列を取得または設定します。
    /// </summary>
    internal List<string> SearchWords { get; set; } = new List<string>();
}
