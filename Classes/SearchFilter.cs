namespace Avatar_Explorer.Classes
{
    /// <summary>
    /// 検索フィルタを表します。
    /// </summary>
    public class SearchFilter
    {

        /// <summary>
        /// 検索する作者の名前を取得または設定します。
        /// </summary>
        public string[] Author { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 検索するアイテムのタイトルを取得または設定します。
        /// </summary>
        public string[] Title { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 検索するアイテムのIDを取得または設定します。
        /// </summary>
        public string[] BoothId { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 検索するアイテムのカスタムカテゴリを取得または設定します。
        /// </summary>
        public string[] Avatar { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 検索するアイテムのカテゴリ、またはカスタムカテゴリを取得または設定します。
        /// </summary>
        public string[] Category { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 検索するアイテムのメモを取得または設定します。
        /// </summary>
        public string[] ItemMemo { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 検索するアイテムのフォルダ名を取得または設定します。
        /// </summary>
        public string[] FolderName { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 検索するアイテムのファイル名を取得または設定します。
        /// </summary>
        public string[] FileName { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 検索するアイテムの文字列を取得または設定します。
        /// </summary>
        public string[] SearchWords { get; set; } = Array.Empty<string>();
    }
}
