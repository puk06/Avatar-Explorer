namespace Avatar_Explorer.Classes
{
    /// <summary>
    /// アイテムのフォルダ情報を表します。
    /// </summary>
    public class ItemFolderInfo
    {
        /// <summary>
        /// 改変用のデータが入ったパス配列を取得または設定します。
        /// </summary>
        public FileData[] ModifyFiles { get; set; } = Array.Empty<FileData>();

        /// <summary>
        /// テクスチャが入ったパス配列を取得または設定します。
        /// </summary>
        public FileData[] TextureFiles { get; set; } = Array.Empty<FileData>();

        /// <summary>
        /// ドキュメントが入ったパス配列を取得または設定します。
        /// </summary>
        public FileData[] DocumentFiles { get; set; } = Array.Empty<FileData>();

        /// <summary>
        /// Unityパッケージが入ったパス配列を取得または設定します。
        /// </summary>
        public FileData[] UnityPackageFiles { get; set; } = Array.Empty<FileData>();

        /// <summary>
        /// マテリアルが入ったパス配列を取得または設定します。
        /// </summary>
        public FileData[] MaterialFiles { get; set; } = Array.Empty<FileData>();

        /// <summary>
        /// 不明なファイルが入ったパス配列を取得または設定します。
        /// </summary>
        public FileData[] UnkownFiles { get; set; } = Array.Empty<FileData>();

        /// <summary>
        /// フォルダ内の指定されたタイプのアイテムの数を取得します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetItemCount(string type)
        {
            return type switch
            {
                "改変用データ" => ModifyFiles.Length,
                "テクスチャ" => TextureFiles.Length,
                "ドキュメント" => DocumentFiles.Length,
                "Unityパッケージ" => UnityPackageFiles.Length,
                "マテリアル" => MaterialFiles.Length,
                "不明" => UnkownFiles.Length,
                _ => 0
            };
        }

        /// <summary>
        /// フォルダ内の指定されたタイプのアイテムを取得します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public FileData[] GetItems(string? type)
        {
            return type switch
            {
                "改変用データ" => ModifyFiles,
                "テクスチャ" => TextureFiles,
                "ドキュメント" => DocumentFiles,
                "Unityパッケージ" => UnityPackageFiles,
                "マテリアル" => MaterialFiles,
                "不明" => UnkownFiles,
                _ => Array.Empty<FileData>()
            };
        }

        /// <summary>
        /// フォルダ内の全てのアイテムを取得します。
        /// </summary>
        /// <returns></returns>
        public FileData[] GetAllItem()
        {
            return ModifyFiles.Concat(TextureFiles).Concat(DocumentFiles).Concat(UnityPackageFiles).Concat(MaterialFiles).Concat(UnkownFiles).ToArray();
        }
    }

    /// <summary>
    /// ファイルのデータを表します。
    /// </summary>
    public class FileData
    {
        /// <summary>
        /// ファイル名を取得または設定します。
        /// </summary>
        public string FileName { get; set; } = "";

        /// <summary>
        /// ファイルのパスを取得または設定します。
        /// </summary>
        public string FilePath { get; set; } = "";

        /// <summary>
        /// ファイルの拡張子を取得します。
        /// </summary>
        public string FileExtension => Path.GetExtension(FilePath);
    }
}
