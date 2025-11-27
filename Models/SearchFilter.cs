namespace Avatar_Explorer.Models;

/// <summary>
/// 検索フィルタを表します。
/// </summary>
internal class SearchFilter
{
    /// <summary>
    /// 検索する作者の名前を取得または設定します。
    /// </summary>
    internal List<string> Authors { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのタイトルを取得または設定します。
    /// </summary>
    internal List<string> Titles { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのIDを取得または設定します。
    /// </summary>
    internal List<string> BoothIds { get; set; } = new List<string>();

    /// <summary>
    /// 検索する対応アバターを取得または設定します。
    /// </summary>
    internal List<string> SupportedAvatars { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのカテゴリ、またはカスタムカテゴリを取得または設定します。
    /// </summary>
    internal List<string> Categories { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのメモを取得または設定します。
    /// </summary>
    internal List<string> ItemMemos { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのフォルダ名を取得または設定します。
    /// </summary>
    internal List<string> FolderNames { get; set; } = new List<string>();

    /// <summary>
    /// 検索するアイテムのファイル名を取得または設定します。
    /// </summary>
    internal List<string> FileNames { get; set; } = new List<string>();

    /// <summary>
    /// 検索する実装済みのアバターを取得または設定します。
    /// </summary>
    internal List<string> ImplementedAvatars { get; set; } = new List<string>();

    /// <summary>
    /// 検索する未実装のアバターを取得または設定します。
    /// </summary>
    internal List<string> NotImplementedAvatars { get; set; } = new List<string>();

    /// <summary>
    /// 検索するタグを取得または設定します。
    /// </summary>
    internal List<string> Tags { get; set; } = new List<string>();
    
    /// <summary>
    /// OR検索かどうかを取得または設定します。
    /// </summary>
    internal bool IsOrSearch { get; set; } = false;

    /// <summary>
    /// アイテムの対応パスなどが破損しているかどうかを取得または設定します。
    /// </summary>
    internal bool BrokenItems { get; set; } = false;

    /// <summary>
    /// 検索するアイテムの文字列を取得または設定します。
    /// </summary>
    internal List<string> SearchWords { get; set; } = new List<string>();
}
