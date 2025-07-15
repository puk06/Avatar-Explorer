namespace Avatar_Explorer.Models;

/// <summary>
/// アイテムの作者を表します。
/// </summary>
internal class Author
{
    /// <summary>
    /// アイテムの作者の名前を取得または設定します。
    /// </summary>
    internal string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// アイテムの作者の画像パスを取得または設定します。
    /// </summary>
    internal string AuthorImagePath { get; set; } = string.Empty;
}
