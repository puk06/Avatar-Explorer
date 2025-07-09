namespace Avatar_Explorer.Models;

/// <summary>
/// ファイルのデータを表します。
/// </summary>
internal class FileData
{
    /// <summary>
    /// ファイル名を取得または設定します。
    /// </summary>
    internal string FileName { get; set; } = "";

    /// <summary>
    /// ファイルのパスを取得または設定します。
    /// </summary>
    internal string FilePath { get; set; } = "";

    /// <summary>
    /// ファイルの拡張子を取得します。
    /// </summary>
    internal string FileExtension
        => Path.GetExtension(FilePath);
}
