namespace Avatar_Explorer.Models;

/// <summary>
/// アイテムのフォルダ情報を表します。
/// </summary>
internal class ItemFolderInfo
{
    /// <summary>
    /// 改変用のデータが入ったパス配列を取得または設定します。
    /// </summary>
    internal List<FileData> FilesForModification { get; set; } = new List<FileData>();

    /// <summary>
    /// テクスチャが入ったパス配列を取得または設定します。
    /// </summary>
    internal List<FileData> TextureFiles { get; set; } = new List<FileData>();

    /// <summary>
    /// ドキュメントが入ったパス配列を取得または設定します。
    /// </summary>
    internal List<FileData> DocumentFiles { get; set; } = new List<FileData>();

    /// <summary>
    /// Unityパッケージが入ったパス配列を取得または設定します。
    /// </summary>
    internal List<FileData> UnityPackageFiles { get; set; } = new List<FileData>();

    /// <summary>
    /// マテリアルが入ったパス配列を取得または設定します。
    /// </summary>
    internal List<FileData> MaterialFiles { get; set; } = new List<FileData>();
    /// <summary>
    /// 不明なファイルが入ったパス配列を取得または設定します。
    /// </summary>
    internal List<FileData> UnknownFiles { get; set; } = new List<FileData>();

    /// <summary>
    /// フォルダ内の指定されたタイプのアイテムの数を取得します。
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    internal int GetItemCount(string type)
    {
        return type switch
        {
            "改変用データ" => FilesForModification.Count,
            "テクスチャ" => TextureFiles.Count,
            "ドキュメント" => DocumentFiles.Count,
            "Unityパッケージ" => UnityPackageFiles.Count,
            "マテリアル" => MaterialFiles.Count,
            "不明" => UnknownFiles.Count,
            _ => 0
        };
    }

    /// <summary>
    /// フォルダ内の指定されたタイプのアイテムを取得します。
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    internal IEnumerable<FileData> GetItems(string? type)
    {
        return type switch
        {
            "改変用データ" => FilesForModification,
            "テクスチャ" => TextureFiles,
            "ドキュメント" => DocumentFiles,
            "Unityパッケージ" => UnityPackageFiles,
            "マテリアル" => MaterialFiles,
            "不明" => UnknownFiles,
            _ => new List<FileData>()
        };
    }

    /// <summary>
    /// フォルダ内の全てのアイテムを取得します。
    /// </summary>
    /// <returns></returns>
    internal IEnumerable<FileData> GetAllItem()
    {
        return FilesForModification
            .Concat(TextureFiles)
            .Concat(DocumentFiles)
            .Concat(UnityPackageFiles)
            .Concat(MaterialFiles)
            .Concat(UnknownFiles);
    }
}
