namespace Avatar_Explorer.Models;

/// <summary>
/// 共有画像を表します。
/// </summary>
internal static class SharedImages
{
    private static readonly Size DefaultImageSize = new(56, 56);

    /// <summary>
    /// ファイルの画像を取得します。
    /// </summary>
    private static readonly Image FileImage = new Bitmap(Image.FromStream(new MemoryStream(Properties.Resources.FileIcon)), DefaultImageSize);

    /// <summary>
    /// フォルダの画像を取得します。
    /// </summary>
    private static readonly Image FolderImage = new Bitmap(Image.FromStream(new MemoryStream(Properties.Resources.FolderIcon)), DefaultImageSize);

    /// <summary>
    /// コピーの画像を取得します。
    /// </summary>
    private static readonly Image CopyImage = new Bitmap(Image.FromStream(new MemoryStream(Properties.Resources.CopyIcon)), DefaultImageSize);

    /// <summary>
    /// ゴミ箱の画像を取得します。
    /// </summary>
    private static readonly Image TrashImage = new Bitmap(Image.FromStream(new MemoryStream(Properties.Resources.TrashIcon)), DefaultImageSize);

    /// <summary>
    /// 編集の画像を取得します。
    /// </summary>
    private static readonly Image EditImage = new Bitmap(Image.FromStream(new MemoryStream(Properties.Resources.EditIcon)), DefaultImageSize);

    /// <summary>
    /// 開くの画像を取得します。
    /// </summary>
    private static readonly Image OpenImage = new Bitmap(Image.FromStream(new MemoryStream(Properties.Resources.OpenIcon)), DefaultImageSize);

    /// <summary>
    /// 共有画像を取得します。
    /// </summary>
    internal enum Images
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
    }

    /// <summary>
    /// 共有画像を取得します。
    /// </summary>
    /// <param name="images"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static Image GetImage(Images images)
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
    internal static bool IsSharedImage(Image image)
    {
        return image == FileImage ||
            image == FolderImage ||
            image == CopyImage ||
            image == TrashImage ||
            image == EditImage ||
            image == OpenImage;
    }
}
