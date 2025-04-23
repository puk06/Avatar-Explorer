namespace Avatar_Explorer.Classes
{
    /// <summary>
    /// アイテム情報を表します。
    /// </summary>
    public class Item
    {
        /// <summary>
        /// アイテムのタイトルを取得または設定します。
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// アイテムの作者の名前を取得また設定します。
        /// </summary>
        public string AuthorName { get; set; } = "";

        /// <summary>
        /// アイテムのメモを取得または設定します。
        /// </summary>
        public string ItemMemo { get; set; } = "";

        /// <summary>
        /// アイテムの作者のIDを取得または設定します。
        /// </summary>
        public string AuthorId { get; set; } = "";

        /// <summary>
        /// アイテムのBooth IDを取得または設定します。
        /// </summary>
        public int BoothId { get; set; } = -1;

        /// <summary>
        /// アイテムフォルダがあるパスを取得または設定します。
        /// </summary>
        public string ItemPath { get; set; } = "";

        /// <summary>
        /// アイテムのマテリアル用のフォルダのパスを取得または設定します。
        /// </summary>
        public string MaterialPath { get; set; } = "";

        /// <summary>
        /// アイテムのサムネイルのURLを取得または設定します。
        /// </summary>
        public string ThumbnailUrl { get; set; } = "";

        /// <summary>
        /// アイテムのサムネイルのファイルパスを取得または設定します。
        /// </summary>
        public string ImagePath { get; set; } = "";

        /// <summary>
        /// アイテムの作者のアイコンのURLを取得または設定します。
        /// </summary>
        public string AuthorImageUrl { get; set; } = "";

        /// <summary>
        /// アイテムの作者のアイコンのファイルパスを取得または設定します。
        /// </summary>
        public string AuthorImageFilePath { get; set; } = "";

        /// <summary>
        /// アイテムのタイプを取得または設定します。
        /// </summary>
        public ItemType Type { get; set; }

        /// <summary>
        /// もしタイプがカスタムカテゴリだった場合の、そのカスタムカテゴリ名を取得または設定します。
        /// </summary>
        public string CustomCategory { get; set; } = "";

        /// <summary>
        /// アイテムの対応アバターを取得また設定します。
        /// </summary>
        public string[] SupportedAvatar { get; set; } = Array.Empty<string>();

        /// <summary>
        /// アイテムの作成日時を取得または設定します。
        /// </summary>
        public string CreatedDate { get; set; } = "";

        /// <summary>
        /// アイテムの更新日時を取得または設定します。
        /// </summary>
        public string UpdatedDate { get; set; } = "";

        /// <summary>
        /// アイテムが実装済みかどうかを管理する配列を取得または設定します。
        /// </summary>
        public string[] ImplementationAvatars { get; set; } = Array.Empty<string>();
    }
}
