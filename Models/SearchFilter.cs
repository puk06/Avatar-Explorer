namespace Avatar_Explorer.Models;

/// <summary>
/// 検索フィルタを表します。
/// </summary>
internal class SearchFilter
{
    /// <summary>
    /// 検索する作者の名前を取得または設定します。
    /// </summary>
    internal string[] Author { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 検索するアイテムのタイトルを取得または設定します。
    /// </summary>
    internal string[] Title { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 検索するアイテムのIDを取得または設定します。
    /// </summary>
    internal string[] BoothId { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 検索するアイテムのカスタムカテゴリを取得または設定します。
    /// </summary>
    internal string[] Avatar { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 検索するアイテムのカテゴリ、またはカスタムカテゴリを取得または設定します。
    /// </summary>
    internal string[] Category { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 検索するアイテムのメモを取得または設定します。
    /// </summary>
    internal string[] ItemMemo { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 検索するアイテムのフォルダ名を取得または設定します。
    /// </summary>
    internal string[] FolderName { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 検索するアイテムのファイル名を取得または設定します。
    /// </summary>
    internal string[] FileName { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 検索する実装済みのアバターを取得または設定します。
    /// </summary>
    internal string[] ImplementedAvatars { get; set; } = Array.Empty<string>();

    /// <summary>
    /// アイテムの対応パスなどが破損しているかどうかを取得または設定します。
    /// </summary>
    internal bool BrokenItems { get; set; } = false;

    /// <summary>
    /// 検索するアイテムの文字列を取得または設定します。
    /// </summary>
    internal string[] SearchWords { get; set; } = Array.Empty<string>();
}
