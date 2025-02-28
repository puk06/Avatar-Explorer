namespace Avatar_Explorer.Classes
{
    /// <summary>
    /// 現在のパスを表します。
    /// </summary>
    public class CurrentPath
    {
        /// <summary>
        /// 現在選択されているアバターの名前を取得または設定します。
        /// </summary>
        public string? CurrentSelectedAvatar;

        /// <summary>
        /// 現在選択されているアバターのパスを取得または設定します。
        /// </summary>
        public string? CurrentSelectedAvatarPath;

        /// <summary>
        /// 現在選択されている作者を取得または設定します。
        /// </summary>
        public Author? CurrentSelectedAuthor;

        /// <summary>
        /// 現在選択されているカテゴリを取得または設定します。
        /// </summary>
        public ItemType CurrentSelectedCategory = ItemType.Unknown;

        /// <summary>
        /// 現在選択されているカスタムカテゴリを取得または設定します。
        /// </summary>
        public string CurrentSelectedCustomCategory = "";

        /// <summary>
        /// 現在選択されているアイテムを取得または設定します。
        /// </summary>
        public Item? CurrentSelectedItem;

        /// <summary>
        /// 現在選択されているアイテム内のカテゴリを取得または設定します。
        /// </summary>
        public string? CurrentSelectedItemCategory;

        /// <summary>
        /// 現在選択されているアイテムフォルダ情報を取得または設定します。
        /// </summary>
        public ItemFolderInfo CurrentSelectedItemFolderInfo = new();

        /// <summary>
        /// パス全体が空かどうかを取得します。
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return CurrentSelectedAvatar == null &&
                   CurrentSelectedAvatarPath == null &&
                   CurrentSelectedAuthor == null &&
                   CurrentSelectedCategory == ItemType.Unknown &&
                   CurrentSelectedItem == null &&
                   CurrentSelectedItemCategory == null;
        }
    }
}
