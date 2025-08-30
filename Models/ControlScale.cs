namespace Avatar_Explorer.Models;

/// <summary>
/// Formのコントロールのスケールを表します。
/// </summary>
internal class ControlScale
{
    /// <summary>
    /// 画面上の位置を表すX座標の比率を取得または設定します。
    /// </summary>
    internal float ScreenLocationXRatio { get; set; }

    /// <summary>
    /// 画面上の位置を表すY座標の比率を取得または設定します。
    /// </summary>
    internal float ScreenLocationYRatio { get; set; }

    /// <summary>
    /// 画面上の幅を表す比率を取得または設定します。
    /// </summary>
    internal float ScreenWidthRatio { get; set; }

    /// <summary>
    /// 画面上の高さを表す比率を取得または設定します。
    /// </summary>
    internal float ScreenHeightRatio { get; set; }

    /// <summary>
    /// 画面上のフォントサイズを表す比率を取得または設定します。
    /// </summary>
    internal float ScreenFontSize { get; set; }
}
