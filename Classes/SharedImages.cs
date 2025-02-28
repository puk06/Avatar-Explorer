namespace Avatar_Explorer.Classes
{
    /// <summary>
    /// 共有画像を表します。
    /// </summary>
    public class SharedImages
    {
        /// <summary>
        /// ファイルの画像を取得します。
        /// </summary>
        private static readonly Image FileImage = Image.FromStream(new MemoryStream(Properties.Resources.FileIcon));

        /// <summary>
        /// フォルダの画像を取得します。
        /// </summary>
        private static readonly Image FolderImage = Image.FromStream(new MemoryStream(Properties.Resources.FolderIcon));

        /// <summary>
        /// コピーの画像を取得します。
        /// </summary>
        private static readonly Image CopyImage = Image.FromStream(new MemoryStream(Properties.Resources.CopyIcon));

        /// <summary>
        /// ゴミ箱の画像を取得します。
        /// </summary>
        private static readonly Image TrashImage = Image.FromStream(new MemoryStream(Properties.Resources.TrashIcon));

        /// <summary>
        /// 編集の画像を取得します。
        /// </summary>
        private static readonly Image EditImage = Image.FromStream(new MemoryStream(Properties.Resources.EditIcon));

        /// <summary>
        /// 開くの画像を取得します。
        /// </summary>
        private static readonly Image OpenImage = Image.FromStream(new MemoryStream(Properties.Resources.OpenIcon));

        /// <summary>
        /// 共有画像を取得します。
        /// </summary>
        public enum Images
        {
            /// <summary>
            /// ファイルの画像
            /// </summary>
            FileIcon,

            /// <summary>
            /// フォルダの画像
            /// </summary>
            FolderIcon,

            /// <summary>
            /// コピーの画像
            /// </summary>
            CopyIcon,

            /// <summary>
            /// ゴミ箱の画像
            /// </summary>
            TrashIcon,

            /// <summary>
            /// 編集の画像
            /// </summary>
            EditIcon,

            /// <summary>
            /// 開くの画像
            /// </summary>
            OpenIcon
        };

        /// <summary>
        /// 共有画像を取得します。
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Image GetImage(Images images)
        {
            Image sharedImage = images switch
            {
                Images.FileIcon => FileImage,
                Images.FolderIcon => FolderImage,
                Images.CopyIcon => CopyImage,
                Images.TrashIcon => TrashImage,
                Images.EditIcon => EditImage,
                Images.OpenIcon => OpenImage,
                _ => throw new ArgumentOutOfRangeException(nameof(images), images, "共有画像の定義がありません")
            };
            return sharedImage;
        }

        /// <summary>
        /// 共有画像かどうかを判定します。
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static bool IsSharedImage(Image image)
        {
            return image == FileImage
                   || image == FolderImage
                   || image == CopyImage
                   || image == TrashImage
                   || image == EditImage
                   || image == OpenImage;
        }
    }
}
