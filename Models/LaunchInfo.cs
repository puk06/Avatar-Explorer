namespace Avatar_Explorer.Models
{
    /// <summary>
    /// アプリケーションの起動情報を表します。
    /// </summary>
    internal class LaunchInfo
    {
        /// <summary>
        /// アプリケーションがURLで起動されたかどうかを示す値を取得または設定します。
        /// </summary>
        internal bool launchedWithUrl;

        /// <summary>
        /// URLで起動された場合の、アセットのパスを取得または設定します。
        /// </summary>
        internal string[] assetDirs = Array.Empty<string>();

        /// <summary>
        /// URLで起動された場合の、アセットのIDを取得または設定します。
        /// </summary>
        internal string assetId = "";
    }
}
